﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using DataLockActor.Interfaces;
using DataLockActor.Storage;
using Newtonsoft.Json;
using SFA.DAS.Payments.Application.Interfaces;
using SFA.DAS.Payments.Domain;
using SFA.DAS.Payments.Domain.DataLock.Matcher;

namespace DataLockActor
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class DataLockActor : Actor, IDataLockActor
    {
        public DataLockActor(ActorService actorService, ActorId actorId)
            : base(actorService, actorId)
        {
        }
        
        private async Task InitialiseState(long ukprn)
        {
            var initialised = await StateManager.ContainsStateAsync("initialised");
            if (initialised) return;

            // load commitments
            var sw = Stopwatch.StartNew();

            ICommitmentProvider commitmentProvider = new CommitmentProvider();

            var commitments = commitmentProvider.GetCommitments(ukprn)
                .GroupBy(c => string.Concat(c.Ukprn, "-", c.LearnerReferenceNumber))
                .ToDictionary(c => c.Key, c => c.ToList());

            Debug.WriteLine($"read from provider in {sw.ElapsedMilliseconds.ToString("##,###")}ms");
            sw.Restart();

            var cache = GetLocalCache();

            await cache.Reset();

            foreach (var c in commitments)
            {
                await cache.Add(c.Key, c.Value);
            }

            Debug.WriteLine($"saved in state in {sw.ElapsedMilliseconds.ToString("##,###")}ms");
            await this.StateManager.AddStateAsync("initialised", true);
        }

        private ILocalCommitmentCache GetLocalCache()
        {
            ILocalCommitmentCache cache = new StateManagerStorage(StateManager);
            //ILocalCommitmentCache cache = new TableStorage();
            return cache;
        }

        public async Task<Metric> ProcessEarning(Earning earning)
        {
            await InitialiseState(earning.Ukprn);
            var sw = Stopwatch.StartNew();

            var cache = GetLocalCache();

            var metric = new Metric();
            var payableEarnings = new List<PayableEarning>();
            var nonPayableEarnings = new List<NonPayableEarning>();

            var key = string.Concat(earning.Ukprn, "-", earning.LearnerReferenceNumber);
            var commitments = (List<Commitment>) await cache.Get(key);

            metric.InsideRead = sw.ElapsedTicks;
            sw.Restart();

            if (commitments != null)
            {
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

            commitments[0].NegotiatedPrice += 100;
            await cache.Update(key, commitments);

            metric.InsideWrite = sw.ElapsedTicks;
            return metric;
        }

        //private void WritePayableEarnings(List<PayableEarning> payableEarnings, List<NonPayableEarning> nonPayableEarnings)
        //{
        //    foreach (var payableEarning in payableEarnings)
        //    {
        //        var serializeObject = JsonConvert.SerializeObject(payableEarning);
        //        //Debug.Write("+");
        //    }

        //    foreach (var nonPayableEarning in nonPayableEarnings)
        //    {
        //        var serializeObject = JsonConvert.SerializeObject(nonPayableEarning);
        //        //Debug.Write("-");
        //    }
        //}

        public Task ProcessCommitment(Commitment commitment)
        {
            throw new NotImplementedException();
        }
    }
}
