namespace SFA.DAS.Payments.Domain
{
    public class Payment
    {
        public PaymentDue PaymentDue { get; set; }

        public FundingSource FundingSource { get; set; }

        public decimal Amount { get; set; }
    }
}