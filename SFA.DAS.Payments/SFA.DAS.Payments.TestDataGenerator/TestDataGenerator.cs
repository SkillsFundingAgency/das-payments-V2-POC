using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dapper;
using ESFA.DC.ILR.FundingService.ALB.FundingOutput.Model;
using ESFA.DC.ILR.FundingService.ALB.FundingOutput.Model.Interface;
using ESFA.DC.ILR.FundingService.ALB.FundingOutput.Model.Interface.Attribute;
using FastMember;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using SFA.DAS.Payments.Domain;
using SFA.DAS.Payments.Domain.Config;

namespace SFA.DAS.Payments.TestDataGenerator
{
    public static class TestDataGenerator
    {
        private static readonly Dictionary<int, PropertyInfo> _props = typeof(ILearningDeliveryPeriodisedAttribute).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.Name.StartsWith("Period"))
            .ToDictionary(p => int.Parse(p.Name.Replace("Period", null)), p => p);

        public static IFundingOutputs Create1000Learners()
        {
            var jsonSerializerSettings = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Auto};
            var directory = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath);
            var file = "ALBOutput1000.json"; //string.Concat(directory.Contains("netcoreapp2.0") ? string.Empty : "..\\", "ALBOutput1000.json");
            var path = Path.Combine(directory, file);
            var output = JsonConvert.DeserializeObject<FundingOutputs>(File.ReadAllText(path), jsonSerializerSettings);
            var learners = new List<ILearnerAttribute>();
            for (var i = 0; i < Configuration.AmplifyEarnings; i++)
            {
                learners.AddRange(output.Learners);
            }
            var result = new FundingOutputs {Global = output.Global, Learners = learners.ToArray()};
            return result;
        }

        public static List<Earning> CreateEarningsFromLearners(IFundingOutputs fundingOutputs = null, string collectionPeriod = "1718-R11")
        {
            var deliveryPeriodPrefix = collectionPeriod.Substring(0, 6);

            var result = new List<Earning>();

            if (fundingOutputs == null)
                fundingOutputs = Create1000Learners();

            var ukprnIncrement = 0;

            foreach (var learner in fundingOutputs.Learners)
            {
                if (++ukprnIncrement >= Configuration.NumberOfProviders)
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

        public static IList<Commitment> CreateCommitmentsFromEarnings(IList<Earning> earnings = null)
        {
            if (earnings == null)
                earnings = CreateEarningsFromLearners();

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
                NegotiatedPrice = Convert.ToInt64(e.Amount)
            }).ToList();
        }

        private static decimal GetAttributeValue(ILearningDeliveryPeriodisedAttribute attr, int index)
        {
            return (decimal)_props[index].GetValue(attr);
        }

        public static async Task ResetAndPopulateTableStorage()
        {
            var table = CloudStorageAccount.Parse(Configuration.TableStorageServerConnectionString).CreateCloudTableClient().GetTableReference("Commitment");
            await table.DeleteIfExistsAsync();
            await table.CreateIfNotExistsAsync();

            var commitments = CreateCommitmentsFromEarnings()
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
            var commitments = CreateCommitmentsFromEarnings();
            using (var cnn = new SqlConnection(Configuration.SqlServerConnectionString))
            {
                cnn.Execute(@"truncate table Commitment");
                using (var bulkCopy = new SqlBulkCopy(cnn))
                using (var reader = ObjectReader.Create(commitments,
                    "Id",
                    "ProgrammeType",
                    "StandardCode",
                    "FrameworkCode",
                    "PathwayCode",
                    "Ukprn",
                    "LearnerReferenceNumber",
                    "TransferSenderAccountId",
                    "EmployerAccountId",
                    "PaymentStatus",
                    "NegotiatedPrice",
                    "StartDate",
                    "EndDate",
                    "EffectiveFrom",
                    "EffectiveTo",
                    "Uln"))
                {
                    bulkCopy.BatchSize = 4000;
                    bulkCopy.DestinationTableName = "Commitment";

                    //var table = new DataTable();
                    //table.Columns.Add("Id", typeof(long));
                    //table.Columns.Add("ProgrammeType", typeof(int));
                    //table.Columns.Add("StandardCode", typeof(long));
                    //table.Columns.Add("FrameworkCode", typeof(int));
                    //table.Columns.Add("PathwayCode", typeof(int));
                    //table.Columns.Add("Ukprn", typeof(long));
                    //table.Columns.Add("LearnerReferenceNumber", typeof(string));
                    //table.Columns.Add("TransferSenderAccountId", typeof(long));
                    //table.Columns.Add("EmployerAccountId", typeof(long));
                    //table.Columns.Add("PaymentStatus", typeof(int));
                    //table.Columns.Add("NegotiatedPrice", typeof(long));
                    //table.Columns.Add("StartDate", typeof(DateTime));
                    //table.Columns.Add("EndDate", typeof(DateTime));
                    //table.Columns.Add("EffectiveFrom", typeof(DateTime));
                    //table.Columns.Add("EffectiveTo", typeof(DateTime));
                    //table.Columns.Add("Uln", typeof(long));

                    bulkCopy.ColumnMappings.Add("Id", "Id");
                    bulkCopy.ColumnMappings.Add("ProgrammeType", "ProgrammeType");
                    bulkCopy.ColumnMappings.Add("StandardCode", "StandardCode");
                    bulkCopy.ColumnMappings.Add("FrameworkCode", "FrameworkCode");
                    bulkCopy.ColumnMappings.Add("PathwayCode", "PathwayCode");
                    bulkCopy.ColumnMappings.Add("Ukprn", "Ukprn");
                    bulkCopy.ColumnMappings.Add("LearnerReferenceNumber", "LearnerReferenceNumber");
                    bulkCopy.ColumnMappings.Add("TransferSenderAccountId", "TransferSenderAccountId");
                    bulkCopy.ColumnMappings.Add("EmployerAccountId", "EmployerAccountId");
                    bulkCopy.ColumnMappings.Add("PaymentStatus", "PaymentStatus");
                    bulkCopy.ColumnMappings.Add("NegotiatedPrice", "NegotiatedPrice");
                    bulkCopy.ColumnMappings.Add("StartDate", "StartDate");
                    bulkCopy.ColumnMappings.Add("EndDate", "EndDate");
                    bulkCopy.ColumnMappings.Add("EffectiveFrom", "EffectiveFrom");
                    bulkCopy.ColumnMappings.Add("EffectiveTo", "EffectiveTo");
                    bulkCopy.ColumnMappings.Add("Uln", "Uln");


                    //foreach (var c in commitments)
                    //{
                    //    table.Rows.Add(c);
                    //}

                    cnn.Open();
                    await bulkCopy.WriteToServerAsync(reader);
                }

                //await cnn.ExecuteAsync(@"INSERT INTO [dbo].[Commitment]
                //                   ([Id]
                //                   ,[ProgrammeType]
                //                   ,[StandardCode]
                //                   ,[FrameworkCode]
                //                   ,[PathwayCode]
                //                   ,[Ukprn]
                //                   ,[LearnerReferenceNumber]
                //                   ,[TransferSenderAccountId]
                //                   ,[EmployerAccountId]
                //                   ,[PaymentStatus]
                //                   ,[NegotiatedPrice]
                //                   ,[StartDate]
                //                   ,[EndDate]
                //                   ,[EffectiveFrom]
                //                   ,[EffectiveTo]
                //                   ,[Uln])
                //             VALUES
                //                   (@Id
                //                   ,@ProgrammeType
                //                   ,@StandardCode
                //                   ,@FrameworkCode
                //                   ,@PathwayCode
                //                   ,@Ukprn
                //                   ,@LearnerReferenceNumber
                //                   ,@TransferSenderAccountId
                //                   ,@EmployerAccountId
                //                   ,@PaymentStatus
                //                   ,@NegotiatedPrice
                //                   ,@StartDate
                //                   ,@EndDate
                //                   ,@EffectiveFrom
                //                   ,@EffectiveTo
                //                   ,@Uln)", commitments);
            }
        }

        public static async Task WriteMetric(Metric metric)
        {
            using (var cnn = new SqlConnection(Configuration.MetricsConnectionString))
            {
                await cnn.ExecuteAsync(@"INSERT INTO [dbo].[Metric]
                                           ([BatchId]
                                           ,[Number]
                                           ,[InsideRead]
                                           ,[InsideCalc]
                                           ,[InsideWrite]
                                           ,[OutsideProxy]
                                           ,[OutsideCall]
                                           ,[Actor]
                                           ,[Progress]
                                           ,[WriteMetrics]
                                           ,[Other])
                                     VALUES
                                           (@BatchId,
                                           @Number,
                                           @InsideRead,
                                           @InsideCalc,
                                           @InsideWrite,
                                           @OutsideProxy,
                                           @OutsideCall,
                                           @Actor,
                                           @Progress,
                                           @WriteMetrics,
                                           @Other)"
                    , metric);
            }

        }

        public static async void Log(string logger, string log)
        {
            //using (var cnn = new SqlConnection(Configuration.MetricsConnectionString))
            //{
            //    await cnn.ExecuteAsync(@"INSERT INTO [dbo].[Log]
            //                               ([Logger]
            //                               ,[Text])
            //                         VALUES
            //                               (@logger,
            //                               @log)"
            //        , new {logger, log});
            //}
        }

    }
}
