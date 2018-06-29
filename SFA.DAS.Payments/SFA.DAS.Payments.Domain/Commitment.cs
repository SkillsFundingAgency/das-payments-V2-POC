using System;

namespace SFA.DAS.Payments.Domain
{
    public class Commitment
    {
        public long Id { get; set; }

        public int? ProgrammeType { get; set; }

        public long? StandardCode { get; set; }

        public int? FrameworkCode { get; set; }

        public int? PathwayCode { get; set; }

        public long Ukprn { get; set; }

        public string LearnerReferenceNumber { get; set; }

        public long? TransferSenderAccountId { get; set; }

        public long EmployerAccountId { get; set; }
        public int PaymentStatus { get; set; }
        public long? NegotiatedPrice { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime EffectiveFrom { get; set; }
        public DateTime? EffectiveTo { get; set; }
        public long Uln { get; set; }
    }
}
