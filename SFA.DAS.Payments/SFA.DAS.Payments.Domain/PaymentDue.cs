namespace SFA.DAS.Payments.Domain
{
    public class PaymentDue
    {
        public PayableEarning PayableEarning { get; set; }

        public decimal AmountDue { get; set; }

        public string CollectionPeriod { get; set; }
    }
}