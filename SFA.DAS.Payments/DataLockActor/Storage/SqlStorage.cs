using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.Payments.Domain;

namespace DataLockActor.Storage
{
    public class SqlStorage : ILocalCommitmentCache
    {
        private const string ConnectionString = "Server=.;Database=SFA.DAS.Payments.POC;Trusted_Connection=False;User ID=SFActor;Password=SFActor";
        //private const string ConnectionString = "Server=tcp:sfa-das-payments-poc.database.windows.net,1433;Initial Catalog=SFA.DAS.Payments.POC;Persist Security Info=False;User ID=SFActor;Password=Vladimir+Dmitry=King_of_all_swaps;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";

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
            using (var cnn = new SqlConnection(ConnectionString))
            {
                var bits = key.Split("-");
                return (await cnn.QueryAsync<Commitment>("select * from Commitment where Ukprn = @ukprn and LearnerReferenceNumber = @LearnerReferenceNumber", new { ukprn = bits[0], LearnerReferenceNumber = bits[1] })).ToList();
            }
        }

        public async Task Update(string key, List<Commitment> commitments)
        {
            using (var cnn = new SqlConnection(ConnectionString))
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
