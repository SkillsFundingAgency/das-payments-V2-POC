using Hangfire.Annotations;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;

namespace SFA.DAS.Payments.PocWebClient
{
    public class HangFireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize([NotNull] DashboardContext context)
        {
            //can add some more logic here...
            return true;
        }
    }
}