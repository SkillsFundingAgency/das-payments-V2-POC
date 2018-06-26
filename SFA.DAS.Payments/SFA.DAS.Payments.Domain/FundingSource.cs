namespace SFA.DAS.Payments.Domain
{
    public class FundingSource
    {
        public Account Account { get; set; }

        public FundingSourceType Type { get; set; }
    }
}