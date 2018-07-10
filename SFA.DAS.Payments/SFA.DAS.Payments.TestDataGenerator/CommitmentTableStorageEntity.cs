using Microsoft.WindowsAzure.Storage.Table;

namespace SFA.DAS.Payments.TestDataGenerator
{
    public class CommitmentTableStorageEntity : TableEntity
    {
        public CommitmentTableStorageEntity()
        {
            PartitionKey = "LOCAL";
        }
        public string Commitments { get; set; }
    }
}