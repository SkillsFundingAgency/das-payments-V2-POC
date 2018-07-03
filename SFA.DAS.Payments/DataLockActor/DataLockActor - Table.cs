using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using DataLockActor.Interfaces;
using Newtonsoft.Json;
using SFA.DAS.Payments.Application.Interfaces;
using SFA.DAS.Payments.Domain;
using SFA.DAS.Payments.Domain.DataLock.Matcher;

namespace DataLockActor
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class DataLockActor : Actor, IDataLockActor
    {
        private long _ukprn;
        private IEarningProvider _earningProvider;
        private List<long> _speed = new List<long>();

        public DataLockActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
            _ukprn = actorId.GetLongId();
        }

        protected override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");


            // load commitments
            var sw = Stopwatch.StartNew();

            ICommitmentProvider commitmentProvider = new CommitmentProvider();

            var commitments = commitmentProvider.GetCommitments(_ukprn)
                .GroupBy(c => string.Concat(c.Ukprn, "-", c.LearnerReferenceNumber))
                .ToDictionary(c => c.Key, c => c.ToList());

            Debug.WriteLine($"read from provider in {sw.ElapsedMilliseconds.ToString("##,###")}ms");
            sw.Restart();

            ILocalCommitmentCache cache = new TableStorage();
            await cache.Reset();

            foreach (var c in commitments)
            {
                await cache.Add(c.Key, c.Value);
            }

            //await StateManager.GetOrAddStateAsync("commitments", commitments);

            Debug.WriteLine($"saved in state in {sw.ElapsedMilliseconds.ToString("##,###")}ms");


            // load earnings

            //IEarningProvider earningProvider = new EarningProvider();

            //var earnings = earningProvider.GetEarnings()
            //    .GroupBy(e => string.Concat(e.Ukprn, "-", e.LearnerReferenceNumber))
            //    .ToDictionary(e => e.Key, e => e.ToList());

            //await StateManager.GetOrAddStateAsync("earnings", commitments);
        }

        public async Task<Metric> ProcessEarning(Earning earning)
        {
            var sw = Stopwatch.StartNew();

            ILocalCommitmentCache cache = new TableStorage();

            var metric = new Metric();
            var payableEarnings = new List<PayableEarning>();
            var nonPayableEarnings = new List<NonPayableEarning>();

            var key = string.Concat(earning.Ukprn, "-", earning.LearnerReferenceNumber);
            var commitments = (List<Commitment>) await cache.Get(key);

            metric.InsideRead = sw.ElapsedTicks;
            sw.Restart();

            if (commitments != null)
            {
                //Debug.WriteLine($"got one item in {sw.ElapsedMilliseconds.ToString("##,##0")}ms");

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
            }
            else
            {
                nonPayableEarnings.Add(new NonPayableEarning
                {
                    Earning = earning,
                    Errors = new[] { "DLOCK_02" }
                });

            }

            metric.InsideCalc = sw.ElapsedTicks;
            sw.Restart();

            //WritePayableEarnings(payableEarnings, nonPayableEarnings);

            //log.Add(sw.ElapsedTicks);
            //sw.Restart();

            commitments[0].NegotiatedPrice += 100;
            await cache.Update(key, commitments);

            //var timing1 = sw.ElapsedTicks;
            //_speed.Add(timing1);
            //Debug.Write($"inside: {timing1:##,##0}t, avg of {_speed.Count}: {_speed.Average():#,##0.##}t. ");
            //Debug.Write(string.Join(",", log));

            metric.InsideWrite = sw.ElapsedTicks;
            return metric;
        }

        private void WritePayableEarnings(List<PayableEarning> payableEarnings, List<NonPayableEarning> nonPayableEarnings)
        {
            foreach (var payableEarning in payableEarnings)
            {
                var serializeObject = JsonConvert.SerializeObject(payableEarning);
                //Debug.Write("+");
            }

            foreach (var nonPayableEarning in nonPayableEarnings)
            {
                var serializeObject = JsonConvert.SerializeObject(nonPayableEarning);
                //Debug.Write("-");
            }
        }

        public Task ProcessCommitment(Commitment commitment)
        {
            throw new NotImplementedException();
        }
    }
}
