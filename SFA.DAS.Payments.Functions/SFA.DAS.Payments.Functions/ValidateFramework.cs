using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using SFA.DAS.Payments.Domain;

namespace SFA.DAS.Payments.Functions
{
    public static class ValidateFramework
    {
        [FunctionName("ValidateFramework")]
        public static Task<PayableEarning> Run(Earning content)
        {
            return Task.FromResult(new PayableEarning());
        }
    }
}