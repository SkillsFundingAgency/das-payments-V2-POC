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

        public long? StandardCode { get; set; }

        public int? FrameworkCode { get; set; }
        public int? PathwayCode { get; set; }
        public decimal? NegotiatedPrice { get; set; }
        public int? ProgrammeType { get; set; }
        public long? Uln { get; set; }

        public DateTime EnqueueTime { get; set; }
    }
}