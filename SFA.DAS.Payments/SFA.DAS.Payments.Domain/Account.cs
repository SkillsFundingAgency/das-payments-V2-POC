namespace SFA.DAS.Payments.Domain
{
    public class Account
    {
        public long Id { get; set; }

        public decimal LevyBalance { get; set; }

        public decimal TransferBalance { get; set; }
    }
}