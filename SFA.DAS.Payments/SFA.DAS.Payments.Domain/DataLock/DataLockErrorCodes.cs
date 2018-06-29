namespace SFA.DAS.Payments.Domain.DataLock
{
    public class DataLockErrorCodes
    {
        public const string MismatchingUkprn = "DLOCK_01";
        public const string MismatchingUln = "DLOCK_02";
        public const string MismatchingStandard = "DLOCK_03";
        public const string MismatchingFramework = "DLOCK_04";
        public const string MismatchingProgramme = "DLOCK_05";
        public const string MismatchingPathway = "DLOCK_06";
        public const string MismatchingPrice = "DLOCK_07";
        public const string MultipleMatches = "DLOCK_08";
        public const string EarlierStartDate = "DLOCK_09";
        public const string NotPayable = "DLOCK_10";
        public const string NotLevyPayer = "DLOCK_11";

    }

}