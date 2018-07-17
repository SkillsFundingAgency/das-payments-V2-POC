using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.Payments.Domain;
using SFA.DAS.Payments.Domain.Config;

namespace DataLockActor.Storage
{
    public class SqlStorage : ILocalCommitmentCache
    {
        public async Task Reset()
        {
            await Task.FromResult(0);
        }

        public async Task Add(string key, IList<Commitment> commitments)
        {
            await Task.FromResult(0);
        }

        public async Task<IList<Commitment>> Get(string key)
        {
            using (var cnn = new SqlConnection(Configuration.SqlServerConnectionString))
            {
                var bits = key.Split("-");
                return (await cnn.QueryAsync<Commitment>("select * from Commitment with(nolock) where Ukprn = @ukprn and LearnerReferenceNumber = @LearnerReferenceNumber", new { ukprn = bits[0], LearnerReferenceNumber = bits[1] })).ToList();
            }
        }

        public async Task Update(string key, List<Commitment> commitments)
        {
            using (var cnn = new SqlConnection(Configuration.SqlServerConnectionString))
            {
                await cnn.ExecuteAsync(@"UPDATE [dbo].[Commitment]
                                           SET [ProgrammeType] = @ProgrammeType
                                              ,[StandardCode] = @StandardCode
                                              ,[FrameworkCode] = @FrameworkCode
                                              ,[PathwayCode] = @PathwayCode
                                              ,[TransferSenderAccountId] = @TransferSenderAccountId
                                              ,[EmployerAccountId] = @EmployerAccountId
                                              ,[PaymentStatus] = @PaymentStatus
                                              ,[NegotiatedPrice] = @NegotiatedPrice
                                              ,[StartDate] = @StartDate
                                              ,[EndDate] = @EndDate
                                              ,[EffectiveFrom] = @EffectiveFrom
                                              ,[EffectiveTo] = @EffectiveTo
                                              ,[Uln] = @Uln
                                         WHERE Ukprn = @Ukprn and LearnerReferenceNumber = @LearnerReferenceNumber", 
                    commitments);
            }
        }
    }
}
