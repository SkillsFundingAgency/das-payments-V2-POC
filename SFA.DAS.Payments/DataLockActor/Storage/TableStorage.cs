using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using SFA.DAS.Payments.Domain;
using SFA.DAS.Payments.Domain.Config;

namespace DataLockActor.Storage
{
    public class TableStorage : ILocalCommitmentCache
    {
        private readonly CloudStorageAccount _storageAccount;

        public TableStorage()
        {
            this._storageAccount = CloudStorageAccount.Parse(Configuration.TableStorageServerConnectionString);
        }

        public async Task Reset()
        {
            //var table = GetTable();
            //await table.DeleteIfExistsAsync();
            //await table.CreateIfNotExistsAsync();
        }

        public async Task Add(string key, IList<Commitment> commitments)
        {
            var tableOperation = TableOperation.Insert(new CommitmentEntity
            {
                PartitionKey = PartitionKey(key),
                RowKey = key,
                Commitments = JsonConvert.SerializeObject(commitments)
            });
            await GetTable().ExecuteAsync(tableOperation);
        }

        public async Task<IList<Commitment>> Get(string key)
        {
            var json = await this.GetTable().ExecuteAsync(this.GetOperation(key), (TableRequestOptions) null, (OperationContext) null);
            if (json.HttpStatusCode != (int) HttpStatusCode.OK)
                return null;
            return JsonConvert.DeserializeObject<List<Commitment>>(((CommitmentEntity)json.Result).Commitments);
        }

        public async Task<string> Update(string key, List<Commitment> commitments)
        {
            var tableOperation = TableOperation.InsertOrReplace(new CommitmentEntity()
            {
                RowKey = key,
                Commitments = JsonConvert.SerializeObject(commitments)
            });
            await GetTable().ExecuteAsync(tableOperation);
            return null;
        }

        private CloudTable GetTable()
        {
            return this._storageAccount.CreateCloudTableClient().GetTableReference("Commitment");
        }

        private TableOperation GetOperation(string key)
        {
            return TableOperation.Retrieve<CommitmentEntity>(PartitionKey(key), key);
        }

        private static string PartitionKey(string key)
        {
            var indexOf = key.IndexOf('-');
            if (indexOf <= 0)
                return "LOCAL";
            return key.Substring(0, indexOf);
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
