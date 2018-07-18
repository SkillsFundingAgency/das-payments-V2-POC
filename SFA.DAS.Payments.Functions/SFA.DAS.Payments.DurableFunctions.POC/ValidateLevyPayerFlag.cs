using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using SFA.DAS.Payments.Domain;
using SFA.DAS.Payments.Domain.DataLock;

namespace SFA.DAS.Payments.DurableFunctions.POC
{
    public static class ValidateLevyPayerFlag
    {
        [FunctionName("ValidateLevyPayerFlag")]
        public static Task Run([ActivityTrigger]EarningValidation content)
        {
            var matchResult = content.MatchResult;
            var accounts = content.Accounts;
            var commitments = matchResult.Commitments;
            var accountsMatch = accounts == null || commitments == null ? new List<Account>() : accounts.Where(a => commitments.Any(c => c.Id == a.Id && a.IsLevyPayer));

            if (!accountsMatch.Any())
            {
                matchResult.ErrorCodes.Add(DataLockErrorCodes.NotLevyPayer);
            }

            return Task.FromResult(true);
        }
    }
}