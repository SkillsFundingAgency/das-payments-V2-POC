using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ESFA.DC.ILR.FundingService.ALB.FundingOutput.Model;
using ESFA.DC.ILR.FundingService.ALB.FundingOutput.Model.Interface;
using ESFA.DC.ILR.FundingService.ALB.FundingOutput.Model.Interface.Attribute;
using Newtonsoft.Json;
using SFA.DAS.Payments.Domain;

namespace SFA.DAS.Payments.TestDataGenerator
{
    public static class TestDataGenerator
    {
        private static readonly Dictionary<int, PropertyInfo> _props = typeof(ILearningDeliveryPeriodisedAttribute).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.Name.StartsWith("Period"))
            .ToDictionary(p => int.Parse(p.Name.Replace("Period", null)), p => p);

        public static IFundingOutputs Create1000Learners()
        {
            var jsonSerializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            var path = Path.Combine(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath), "ALBOutput1000.json");
            return JsonConvert.DeserializeObject<FundingOutputs>(File.ReadAllText(path), jsonSerializerSettings);
        }

        public static IList<Earning> CreateEarningsFromLearners(IFundingOutputs fundingOutputs = null, string collectionPeriod = "1718-R11")
        {
            var deliveryPeriodPrefix = collectionPeriod.Substring(0, 6);

            var result = new List<Earning>();

            if (fundingOutputs == null)
                fundingOutputs = Create1000Learners();

            foreach (var learner in fundingOutputs.Learners)
            {
                foreach (var learningDeliveryAttribute in learner.LearningDeliveryAttributes)
                {
                    var earnings = new Earning[12];

                    for (var i = 1; i < 13; i++)
                    {
                        var earning = new Earning
                        {
                            Ukprn = fundingOutputs.Global.UKPRN,
                            LearnerReferenceNumber = learner.LearnRefNumber,
                            DeliveryPeriod = deliveryPeriodPrefix + i.ToString("d2")

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
            return earnings.Skip(10).Take(earnings.Count - 100).Select(e => new Commitment
            {
                LearnerReferenceNumber = e.LearnerReferenceNumber,
                Ukprn = e.Ukprn,
                EmployerAccountId = random.Next(1, 5),
                Id = ++i,
                FrameworkCode = random.Next(10),
                PathwayCode = random.Next(5),
                ProgrammeType = 1,
                StandardCode = 25,
                TransferSenderAccountId = random.Next(10) == 10 ? random.Next(1, 5) : (long?)null                
            }).ToList();
        }

        private static decimal GetAttributeValue(ILearningDeliveryPeriodisedAttribute attr, int index)
        {
            return (decimal)_props[index].GetValue(attr);
        }
    }
}
