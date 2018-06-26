namespace SFA.DAS.Payments.Domain
{
    public class PayableEarning
    {
        public Earning Earning { get; set; }

        public Commitment Commitment { get; set; }
    }
}