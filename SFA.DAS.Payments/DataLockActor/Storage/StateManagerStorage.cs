﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors.Runtime;
using SFA.DAS.Payments.Domain;

namespace DataLockActor.Storage
{
    public class StateManagerStorage : ILocalCommitmentCache
    {
        private readonly IActorStateManager _stateManager;

        public StateManagerStorage(IActorStateManager stateManager)
        {
            _stateManager = stateManager;
        }

        public async Task Reset()
        {
            await _stateManager.ClearCacheAsync();
        }

        public async Task Add(string key, IList<Commitment> commitments)
        {
            await _stateManager.TryAddStateAsync(key, commitments);
        }

        public async Task<IList<Commitment>> Get(string key)
        {
            try
            {
                return await _stateManager.GetStateAsync<List<Commitment>>(key);
            }
            catch (KeyNotFoundException)
            {
                return null;
            }
        }

        public async Task Update(string key, List<Commitment> commitments)
        {
            await _stateManager.SetStateAsync(key, commitments);
            await _stateManager.SaveStateAsync();
        }
    }
}
