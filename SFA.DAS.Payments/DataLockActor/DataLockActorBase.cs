using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using DataLockActor.Interfaces;
using SFA.DAS.Payments.Domain;
using SFA.DAS.Payments.Domain.DataLock.Matcher;
using SFA.DAS.Payments.TestDataGenerator;

namespace DataLockActor
{
    [StatePersistence(StatePersistence.Persisted)]
    internal abstract class DataLockActorBase : Actor, IDataLockActor
    {
        protected  long Ukprn { get; set; }
            
        private static int _actorCounter = 0;
        private readonly int _actorNumber;

        protected DataLockActorBase(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
            Ukprn = actorId.GetLongId();            
            _actorNumber = ++_actorCounter;
            Debug.WriteLine($"======================================== actor {_actorNumber}-{Ukprn} created ========================================");
            TestDataGenerator.Log("DataLockActorBase", $"actor {_actorNumber}-{Ukprn} created");
        }

        protected abstract ILocalCommitmentCache GetLocalCache();

        public async Task<Metric> ProcessEarning(Earning earning)
        {
            var sw = Stopwatch.StartNew();

            var cache = GetLocalCache();

            var metric = new Metric {Actor = $"#{_actorNumber}({earning.Ukprn})"};
            var payableEarnings = new List<PayableEarning>();
            var nonPayableEarnings = new List<NonPayableEarning>();
            var key = string.Concat(earning.Ukprn, "-", earning.LearnerReferenceNumber);
            var commitments = (List<Commitment>) await cache.Get(key);

            metric.InsideRead = sw.ElapsedTicks;
            sw.Restart();

            //if (commitments != null && commitments.Count > 0)
            //{
                // compare
                var matcher = MatcherFactory.CreateMatcher();
                var accounts = new List<Account>();
                var matchResult = matcher.Match(commitments, earning, accounts);
                var payable = matchResult.ErrorCodes.Count > 0;

                // create (non)payable earning
                if (payable)
                    payableEarnings.Add(new PayableEarning
                    {
                        Commitment = commitments[0],
                        Earning = earning
                    });
                else
                    nonPayableEarnings.Add(new NonPayableEarning
                    {
                        Earning = earning,
                        Errors = matchResult.ErrorCodes
                    });
            //}
            //else
            //{
            //    nonPayableEarnings.Add(new NonPayableEarning
            //    {
            //        Earning = earning,
            //        Errors = new[] { "DLOCK_02" }
            //    });
            //}

            metric.InsideCalc = sw.ElapsedTicks;
            sw.Restart();

            //if (commitments != null)
            //{
                commitments[0].NegotiatedPrice += 100;
                metric.Other = await cache.Update(key, commitments);
            //}

            metric.InsideWrite = sw.ElapsedTicks;
            return metric;
        }
    }
}
