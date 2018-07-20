using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Microsoft.SqlServer.Server;
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

        public async Task<string> Update(string key, List<Commitment> commitments)
        {
            var sw = Stopwatch.StartNew();
            var timing = new List<long>();

            using (var cnn = new SqlConnection(Configuration.SqlServerConnectionString))
            {
                timing.Add(sw.ElapsedTicks);
                sw.Restart();

                cnn.Open();

                timing.Add(sw.ElapsedTicks);
                sw.Restart();

                //var tvp = new CommitmentTvp(commitments);
                //await cnn.ExecuteAsync("[dbo].[UpdateCommitments]", tvp, commandType: CommandType.StoredProcedure);

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
                    commitments[0]);

                timing.Add(sw.ElapsedTicks);
                sw.Restart();
            }

            timing.Add(sw.ElapsedTicks);

            return string.Join(',', timing) + 'x' + commitments.Count;
        }
    }

    public class CommitmentTvp : SqlMapper.IDynamicParameters
    {
        private readonly IEnumerable<Commitment> _parameters;

        private static readonly SqlMetaData _programmeTypeMetaData = new SqlMetaData("ProgrammeType", SqlDbType.Int);
        private static readonly SqlMetaData _standardCodeMetaData = new SqlMetaData("StandardCode", SqlDbType.BigInt);
        private static readonly SqlMetaData _frameworkCodeMetaData = new SqlMetaData("FrameworkCode", SqlDbType.Int);
        private static readonly SqlMetaData _pathwayCodeMetaData = new SqlMetaData("PathwayCode", SqlDbType.Int);
        private static readonly SqlMetaData _ukprnMetaData = new SqlMetaData("UKPRN", SqlDbType.BigInt);
        private static readonly SqlMetaData _learnRefNumberMetaData = new SqlMetaData("LearnerReferenceNumber", SqlDbType.VarChar, 12);
        private static readonly SqlMetaData _transferSenderAccountIdMetaData = new SqlMetaData("TransferSenderAccountId", SqlDbType.BigInt);
        private static readonly SqlMetaData _employerAccountIdMetaData = new SqlMetaData("EmployerAccountId", SqlDbType.BigInt);
        private static readonly SqlMetaData _paymentStatusMetaData = new SqlMetaData("PaymentStatus", SqlDbType.Int);
        private static readonly SqlMetaData _negotiatedPriceMetaData = new SqlMetaData("NegotiatedPrice", SqlDbType.Decimal);
        private static readonly SqlMetaData _startDateMetaData = new SqlMetaData("StartDate", SqlDbType.DateTime);
        private static readonly SqlMetaData _endDateMetaData = new SqlMetaData("EndDate", SqlDbType.DateTime);
        private static readonly SqlMetaData _effectiveFromMetaData = new SqlMetaData("EffectiveFrom", SqlDbType.DateTime);
        private static readonly SqlMetaData _effectiveToMetaData = new SqlMetaData("EffectiveTo", SqlDbType.DateTime);
        private static readonly SqlMetaData _ulnMetaData = new SqlMetaData("Uln", SqlDbType.BigInt);

        public CommitmentTvp(IList<Commitment> commitments)
        {
            _parameters = commitments;
        }

        public void AddParameters(IDbCommand command, SqlMapper.Identity identity)
        {
            var sqlCommand = (SqlCommand) command;
            sqlCommand.CommandType = CommandType.StoredProcedure;
            var items = new List<SqlDataRecord>();
            foreach (var param in _parameters)
            {
                var rec = new SqlDataRecord(
                    _programmeTypeMetaData,
                    _standardCodeMetaData,
                    _frameworkCodeMetaData,
                    _pathwayCodeMetaData,
                    _ukprnMetaData,
                    _learnRefNumberMetaData,
                    _transferSenderAccountIdMetaData,
                    _employerAccountIdMetaData,
                    _paymentStatusMetaData,
                    _negotiatedPriceMetaData,
                    _startDateMetaData,
                    _endDateMetaData,
                    _effectiveFromMetaData,
                    _effectiveToMetaData,
                    _ulnMetaData);

                if (param.ProgrammeType.HasValue) rec.SetInt32(0, param.ProgrammeType.Value);
                if (param.StandardCode.HasValue) rec.SetInt64(1, param.StandardCode.Value);
                if (param.FrameworkCode.HasValue) rec.SetInt32(2, param.FrameworkCode.Value);
                if (param.PathwayCode.HasValue) rec.SetInt32(3, param.PathwayCode.Value);
                rec.SetInt64(4, param.Ukprn);
                rec.SetString(5, param.LearnerReferenceNumber);
                if (param.TransferSenderAccountId.HasValue) rec.SetInt64(6, param.TransferSenderAccountId.Value);
                rec.SetInt64(7, param.EmployerAccountId);
                rec.SetInt32(8, param.PaymentStatus);
                rec.SetDecimal(9, param.NegotiatedPrice);
                rec.SetDateTime(10, param.StartDate);
                rec.SetDateTime(11, param.EndDate);
                rec.SetDateTime(12, param.EffectiveFrom);
                if (param.EffectiveTo != null) rec.SetDateTime(13, param.EffectiveTo.Value);
                rec.SetInt64(14, param.Uln);

                items.Add(rec);
            }

            var p = sqlCommand.Parameters.Add("@commitments", SqlDbType.Structured);
            p.Direction = ParameterDirection.Input;
            p.TypeName = "[dbo].[CommitmentType]";
            p.Value = items;            
        }
    }
}
