using System;
using System.Runtime.Serialization;

namespace SFA.DAS.Payments.Domain
{
    [DataContract]
    public class Commitment
    {
        [DataMember]
        public long Id { get; set; }

        [DataMember]
        public int? ProgrammeType { get; set; }

        [DataMember]
        public long? StandardCode { get; set; }

        [DataMember]
        public int? FrameworkCode { get; set; }

        [DataMember]
        public int? PathwayCode { get; set; }

        [DataMember]
        public long Ukprn { get; set; }

        [DataMember]
        public string LearnerReferenceNumber { get; set; }

        [DataMember]
        public long? TransferSenderAccountId { get; set; }

        [DataMember]
        public long EmployerAccountId { get; set; }

        [DataMember]
        public int PaymentStatus { get; set; }

        [DataMember]
        public long? NegotiatedPrice { get; set; }

        [DataMember]
        public DateTime StartDate { get; set; }

        [DataMember]
        public DateTime EndDate { get; set; }

        [DataMember]
        public DateTime EffectiveFrom { get; set; }

        [DataMember]
        public DateTime? EffectiveTo { get; set; }

        [DataMember]
        public long Uln { get; set; }
    }
}
