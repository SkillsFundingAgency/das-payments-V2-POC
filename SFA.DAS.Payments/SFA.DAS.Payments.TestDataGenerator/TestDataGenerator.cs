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
            return JsonConvert.DeserializeObject<FundingOutputs>(File.ReadAllText("ALBOutput1000.json"), jsonSerializerSettings);
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

        private static decimal GetAttributeValue(ILearningDeliveryPeriodisedAttribute attr, int index)
        {
            return (decimal)_props[index].GetValue(attr);
        }
    }
}
