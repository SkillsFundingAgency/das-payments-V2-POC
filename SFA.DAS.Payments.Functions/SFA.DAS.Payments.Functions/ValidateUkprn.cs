using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using SFA.DAS.Payments.Domain;

namespace SFA.DAS.Payments.Functions
{
    public static class ValidateUkprn
    {
        [FunctionName("ValidateUkprn")]
        public static Task<PayableEarning> Run(string content)
        {
            var learner = JsonConvert.DeserializeObject<List<Earning>>(content);
            return Task.FromResult(new PayableEarning());
        }
    }
}