using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using Microsoft.ServiceFabric.Actors.Client;
using DataLockActor.Interfaces;
using Microsoft.ServiceFabric.Data.Collections;
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

        public DataLockActor(ActorService actorService, ActorId actorId) 
            : base(actorService, actorId)
        {
            _ukprn = actorId.GetLongId();
        }

        protected override async Task OnActivateAsync()
        {
            ActorEventSource.Current.ActorMessage(this, "Actor activated.");



            // load commitments

            ICommitmentProvider commitmentProvider = new CommitmentProvider();

            var commitments = commitmentProvider.GetCommitments(_ukprn)
                .GroupBy(c => string.Concat(c.Ukprn, "-", c.LearnerReferenceNumber))
                .ToDictionary(c => c.Key, c => c.ToList());

            await StateManager.GetOrAddStateAsync("commitments", commitments);
            


            // load earnings

            //IEarningProvider earningProvider = new EarningProvider();

            //var earnings = earningProvider.GetEarnings()
            //    .GroupBy(e => string.Concat(e.Ukprn, "-", e.LearnerReferenceNumber))
            //    .ToDictionary(e => e.Key, e => e.ToList());

            //await StateManager.GetOrAddStateAsync("earnings", commitments);
        }

        public async Task ProcessEarning(Earning earning)
        {
            // find commitment
            var allCommitments = await StateManager.GetStateAsync<IDictionary<string, List<Commitment>>>("commitments");
            List<Commitment> commitments;
            bool payable;

            var payableEarnings = new List<PayableEarning>();
            var nonPayableEarnings = new List<NonPayableEarning>();
            
            if (allCommitments.TryGetValue(string.Concat(earning.Ukprn, "-", earning.LearnerReferenceNumber), out commitments))
            {
                // compare
                var matcher = MatcherFactory.CreateMatcher();
                var accounts = new List<Account>();
                var matchResult = matcher.Match(commitments, earning, accounts);
                payable = matchResult.ErrorCodes.Count > 0;

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
                    Errors = new[] {"DLOCK_02"}
                });

            }

            WritePayableEarnings(payableEarnings, nonPayableEarnings);


        }

        private void WritePayableEarnings(List<PayableEarning> payableEarnings, List<NonPayableEarning> nonPayableEarnings)
        {
            Debug.WriteLine($"============================== {payableEarnings.Count} Payable Earnings ==============================");
            foreach (var payableEarning in payableEarnings)
            {
                Debug.WriteLine(JsonConvert.SerializeObject(payableEarning));
            }

            Debug.WriteLine($"============================== {nonPayableEarnings.Count} Non-Payable Earnings ==============================");
            foreach (var nonPayableEarning in nonPayableEarnings)
            {
                Debug.WriteLine(JsonConvert.SerializeObject(nonPayableEarning));
            }
        }

        public Task ProcessCommitment(Commitment commitment)
        {
            throw new NotImplementedException();
        }
    }
}
