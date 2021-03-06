﻿using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Payments.Domain;

namespace SFA.DAS.Payments.Application.Interfaces
{
    public class CommitmentProvider : ICommitmentProvider
    {
        private static IList<Commitment> _commitments = null;

        public IReadOnlyList<Commitment> GetCommitments(long ukprn)
        {
            Initialise();
            return _commitments.Where(c => c.Ukprn == ukprn).ToList();
        }

        public List<Commitment> GetCommitments(long ukprn, IReadOnlyList<string> learnerReferenceNumbers)
        {
            Initialise();
            return _commitments.Where(c => c.Ukprn == ukprn && learnerReferenceNumbers.Contains(c.LearnerReferenceNumber)).ToList();
        }

        public List<Commitment> GetCommitments(long ukprn, IList<Earning> earnings)
        {
            if (_commitments == null)
            {
                _commitments = TestDataGenerator.TestDataGenerator.CreateCommitmentsFromEarnings(earnings);
            }

            return _commitments.ToList();
        }

        private void Initialise()
        {
            if (_commitments == null)
                _commitments = TestDataGenerator.TestDataGenerator.CreateCommitmentsFromEarnings();
        }
    }
}