using System;
using SFA.DAS.Payments.Domain;

namespace SFA.DAS.Payments.DataLock.Messages.Commands
{
    public class ProcessEarning
    {
        public DateTimeOffset RequestTime { get; set; }
        public Earning Earning { get; set; }
    }
}