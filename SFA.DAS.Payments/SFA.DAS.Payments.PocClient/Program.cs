﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataLockActor.Interfaces;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using SFA.DAS.Payments.Application.Interfaces;

namespace SFA.DAS.Payments.ServiceFabric.PocClient
{
    class Program
    {
        static void Main(string[] args)
        {

            var earnings = TestDataGenerator.TestDataGenerator.CreateEarningsFromLearners();
            //EarningProvider.SetEarnings(earnings);
            //CommitmentProvider.SetCommitments(TestDataGenerator.TestDataGenerator.CreateCommitmentsFromEarnings(earnings));
            Debug.WriteLine("#,time inside,incl read,incl calc,incl output,incl write,time outside,incl getproxy,incl call");
            Task.Run(async () =>
            {
                try
                {
                    var sw1 = Stopwatch.StartNew();
                    //var avg = new List<long>();

                    for (var i = 0; i < earnings.Count; i++)
                    {
                        var sw2 = Stopwatch.StartNew();
                        var earning = earnings[i];
                        var actor = ActorProxy.Create<IDataLockActor>(new ActorId(earning.Ukprn), new Uri("fabric:/SFA.DAS.Payments.DataLock/DataLockActorService"));

                        var proxyTime = sw2.ElapsedTicks;
                        sw2.Restart();

                        var metrics = await actor.ProcessEarning(earning);
                        metrics.OutsideProxy = proxyTime;
                        metrics.OutsideCall = sw2.ElapsedTicks;

                        Debug.WriteLine($"#{i},outside,{metrics.OutsideProxy},{metrics.OutsideCall}, inside, {metrics.InsideRead},{metrics.InsideCalc},{metrics.InsideWrite}");
                    }

                    Debug.WriteLine($"Done full list in {sw1.ElapsedMilliseconds:##,##0}ms");
                }
                catch (Exception ex)
                {
                    throw;
                }
            }).Wait();

            Debug.WriteLine("*********************************** finished processing earnings ***********************************");
            Console.ReadLine();
        }
    }
}
