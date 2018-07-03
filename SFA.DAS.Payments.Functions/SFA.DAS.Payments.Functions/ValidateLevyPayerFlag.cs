using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using SFA.DAS.Payments.Domain;

namespace SFA.DAS.Payments.Functions
{
    public static class ValidateLevyPayerFlag
    {
        [FunctionName("ValidateLevyPayerFlag")]
        public static Task<PayableEarning> Run(Earning content)
        {
            return Task.FromResult(new PayableEarning());
        }
    }
}