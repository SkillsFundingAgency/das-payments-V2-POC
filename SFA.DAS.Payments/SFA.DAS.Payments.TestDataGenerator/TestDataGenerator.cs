using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.ILR.FundingService.ALB.FundingOutput.Model;
using ESFA.DC.ILR.FundingService.ALB.FundingOutput.Model.Interface;
using ESFA.DC.ILR.FundingService.ALB.FundingOutput.Model.Interface.Attribute;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using SFA.DAS.Payments.Domain;

namespace SFA.DAS.Payments.TestDataGenerator
{
    public static class TestDataGenerator
    {
        private const string TableStorageConnectionString = "UseDevelopmentStorage=true";
        private const string SqlStorageConnectionString = "Server=.;Database=SFA.DAS.Payments.POC;Trusted_Connection=True";
        private const string MetricConnectionString = SqlStorageConnectionString;

        private static readonly Dictionary<int, PropertyInfo> _props = typeof(ILearningDeliveryPeriodisedAttribute).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.Name.StartsWith("Period"))
            .ToDictionary(p => int.Parse(p.Name.Replace("Period", null)), p => p);

        public static IFundingOutputs Create1000Learners()
        {
            var jsonSerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            var directory = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath);
            var path = Path.Combine(directory, "ALBOutput1000.json");
            return JsonConvert.DeserializeObject<FundingOutputs>(File.ReadAllText(path), jsonSerializerSettings);
        }

        public static List<Earning> CreateEarningsFromLearners(IFundingOutputs fundingOutputs = null, string collectionPeriod = "1718-R11", int spreadUkprnAcross = 1)
        {
            var deliveryPeriodPrefix = collectionPeriod.Substring(0, 6);

            var result = new List<Earning>();

            if (fundingOutputs == null)
                fundingOutputs = Create1000Learners();

            var ukprnIncrement = 0;

            foreach (var learner in fundingOutputs.Learners)
            {
                if (++ukprnIncrement >= spreadUkprnAcross)
                    ukprnIncrement = 0;

                foreach (var learningDeliveryAttribute in learner.LearningDeliveryAttributes)
                {
                    var earnings = new Earning[12];

                    for (var i = 1; i < 13; i++)
                    {
                        var earning = new Earning
                        {
                            Ukprn = fundingOutputs.Global.UKPRN + ukprnIncrement,
                            LearnerReferenceNumber = learner.LearnRefNumber,
                            DeliveryPeriod = deliveryPeriodPrefix + i.ToString("d2"),
                            Uln = new Random(100000).Next()
                        };

                        foreach (var attribute in learningDeliveryAttribute.LearningDeliveryPeriodisedAttributes)
                        {
                            if (attribute.AttributeName.EndsWith("Payment"))
                                earning.Amount += GetAttributeValue(attribute, i);
                        }

                        earnings[i - 1] = earning;
                    }

                    result.AddRange(earnings);
                }
            }

            return result;
        }

        public static IList<Commitment> CreateCommitmentsFromEarnings(IList<Earning> earnings = null, int spreadUkprnAcross = 1)
        {
            if (earnings == null)
                earnings = CreateEarningsFromLearners(spreadUkprnAcross: spreadUkprnAcross);

            var random = new Random();
            var i = 0;
            return earnings.Select(e => new Commitment
            {
                LearnerReferenceNumber = e.LearnerReferenceNumber,
                Ukprn = e.Ukprn,
                EmployerAccountId = random.Next(1, 5),
                Id = ++i,
                FrameworkCode = random.Next(10),
                PathwayCode = random.Next(5),
                ProgrammeType = 1,
                StandardCode = 25,
                TransferSenderAccountId = random.Next(10) == 10 ? random.Next(1, 5) : (long?) null,
                EffectiveFrom = DateTime.Today,
                EffectiveTo = DateTime.Today,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today,
                Uln = e.Uln ?? 0,
                NegotiatedPrice = e.Amount
            }).ToList();
        }

        private static decimal GetAttributeValue(ILearningDeliveryPeriodisedAttribute attr, int index)
        {
            return (decimal)_props[index].GetValue(attr);
        }

        public static async Task ResetAndPopulateTableStorage()
        {
            var table = CloudStorageAccount.Parse(TableStorageConnectionString).CreateCloudTableClient().GetTableReference("Commitment");
            await table.DeleteIfExistsAsync();
            await table.CreateIfNotExistsAsync();

            var commitments = CreateCommitmentsFromEarnings(null, 5)
                .GroupBy(c => string.Concat(c.Ukprn, "-", c.LearnerReferenceNumber))
                .ToDictionary(c => c.Key, c => c.ToList());


            foreach (var commitment in commitments)
            {
                var tableOperation = TableOperation.Insert(new CommitmentTableStorageEntity
                {
                    RowKey = commitment.Key,
                    Commitments = JsonConvert.SerializeObject(commitment.Value)
                });

                await table.ExecuteAsync(tableOperation);
            }
        }

        public static async Task ResetAndPopulateSqlStorage()
        {
            var commitments = CreateCommitmentsFromEarnings(null, 5);
            using (var cnn = new SqlConnection(SqlStorageConnectionString))
            {
                cnn.Execute(@"truncate table Commitment");
                await cnn.ExecuteAsync(@"INSERT INTO [dbo].[Commitment]
                                   ([Id]
                                   ,[ProgrammeType]
                                   ,[StandardCode]
                                   ,[FrameworkCode]
                                   ,[PathwayCode]
                                   ,[Ukprn]
                                   ,[LearnerReferenceNumber]
                                   ,[TransferSenderAccountId]
                                   ,[EmployerAccountId]
                                   ,[PaymentStatus]
                                   ,[NegotiatedPrice]
                                   ,[StartDate]
                                   ,[EndDate]
                                   ,[EffectiveFrom]
                                   ,[EffectiveTo]
                                   ,[Uln])
                             VALUES
                                   (@Id
                                   ,@ProgrammeType
                                   ,@StandardCode
                                   ,@FrameworkCode
                                   ,@PathwayCode
                                   ,@Ukprn
                                   ,@LearnerReferenceNumber
                                   ,@TransferSenderAccountId
                                   ,@EmployerAccountId
                                   ,@PaymentStatus
                                   ,@NegotiatedPrice
                                   ,@StartDate
                                   ,@EndDate
                                   ,@EffectiveFrom
                                   ,@EffectiveTo
                                   ,@Uln)", commitments);
            }
        }

        public static async Task WriteMetric(Metric metric)
        {
            using (var cnn = new SqlConnection(MetricConnectionString))
            {
                await cnn.ExecuteAsync(@"INSERT INTO [dbo].[Metric]
                                           ([BatchId]
                                           ,[Number]
                                           ,[InsideRead]
                                           ,[InsideCalc]
                                           ,[InsideWrite]
                                           ,[OutsideProxy]
                                           ,[OutsideCall]
                                           ,[Actor])
                                     VALUES
                                           (@BatchId,
                                           @Number,
                                           @InsideRead,
                                           @InsideCalc,
                                           @InsideWrite,
                                           @OutsideProxy,
                                           @OutsideCall,
                                           @Actor)"
                    , metric);
            }

        }

    }
}
