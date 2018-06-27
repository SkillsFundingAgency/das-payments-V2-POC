using System;
using System.IO;
using Bogus;
using ESFA.DC.ILR.FundingService.ALB.FundingOutput.Model;
using ESFA.DC.ILR.FundingService.ALB.FundingOutput.Model.Attribute;
using Newtonsoft.Json;

namespace SFA.DAS.Payments.PocClient
{
    class Program
    {
        static void Main(string[] args)
        {
            //IDataLockActor actor = ActorProxy.Create<IDataLockActor>(ActorId.CreateRandom(), new Uri("fabric:/MyApplication/DataLockActorService"));
            //Task retval = actor.GetCountAsync(new CancellationTokenSource().Token);
            //Console.Write(retval);
            PopulateTestData();
            Console.ReadLine();
        }

        static void PopulateTestData()
        {
            var learners = TestDataGenerator.TestDataGenerator.CreateEarningsFromLearners();
        }


    }
}
