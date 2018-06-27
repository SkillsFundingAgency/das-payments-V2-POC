using System;

namespace SFA.DAS.Payments.Domain
{
    public class Earning
    {
        public long Ukprn { get; set; }

        public string LearnerReferenceNumber { get; set; }

        public decimal Amount { get; set; }

        public int TransactionType { get; set; }

        public string DeliveryPeriod { get; set; }

        public string PriceEpisodeIdentifier { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}