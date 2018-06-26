namespace SFA.DAS.Payments.Domain
{
    public class Commitment
    {
        public long Id { get; set; }

        public string ProgrammeType { get; set; }

        public long StandardCode { get; set; }

        public int FrameworkCode { get; set; }

        public int PathwayCode { get; set; }

        public long Ukprn { get; set; }

        public string LearnerReferenceNumber { get; set; }

        public long? TransferSenderAccountId { get; set; }

        public long EmployerAccountId { get; set; }
    }
}