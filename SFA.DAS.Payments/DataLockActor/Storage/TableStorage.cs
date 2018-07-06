﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using SFA.DAS.Payments.Domain;

namespace DataLockActor
{
    public class TableStorage : ILocalCommitmentCache
    {
        private readonly CloudStorageAccount _storageAccount;

        public TableStorage()
        {
            this._storageAccount = CloudStorageAccount.Parse("UseDevelopmentStorage=true");
        }


        public async Task Reset()
        {
            var table = GetTable();
            await table.DeleteIfExistsAsync();
            await table.CreateIfNotExistsAsync();
        }

        public async Task Add(string key, IList<Commitment> commitments)
        {
            var tableOperation = TableOperation.Insert(new CommitmentEntity()
            {
                RowKey = key,
                Commitments = JsonConvert.SerializeObject(commitments)
            });
            await GetTable().ExecuteAsync(tableOperation);
        }

        public async Task<IList<Commitment>> Get(string key)
        {
            var json = await this.GetTable().ExecuteAsync(this.GetOperation(key), (TableRequestOptions) null, (OperationContext) null);
            return JsonConvert.DeserializeObject<List<Commitment>>(((CommitmentEntity)json.Result).Commitments);
        }

        public async Task Update(string key, List<Commitment> commitments)
        {
            var tableOperation = TableOperation.InsertOrReplace(new CommitmentEntity()
            {
                RowKey = key,
                Commitments = JsonConvert.SerializeObject(commitments)
            });
            await GetTable().ExecuteAsync(tableOperation);
        }

        private CloudTable GetTable()
        {
            return this._storageAccount.CreateCloudTableClient().GetTableReference("Commitment");
        }

        private TableOperation GetOperation(string key)
        {
            return TableOperation.Retrieve<CommitmentEntity>("LOCAL", key);
        }
    }

    public class CommitmentEntity : TableEntity
    {
        public CommitmentEntity()
        {
            PartitionKey = "LOCAL";
        }

        public string Commitments { get; set; }
    }
}