using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.Payments.Domain.Config
{
    public class Configuration
    {
        public const string SqlServerConnectionString = "Server=.;Database=SFA.DAS.Payments.POC;Trusted_Connection=False;User ID=SFActor;Password=SFActor";
        public const string TableStorageServerConnectionString = "UseDevelopmentStorage=true";
        public const string ActorConnectionString = "fabric:/SFA.DAS.Payments.DataLock/{0}";
    }
}
