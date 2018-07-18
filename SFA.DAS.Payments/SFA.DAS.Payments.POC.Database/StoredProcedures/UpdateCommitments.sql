CREATE PROCEDURE [dbo].[UpdateCommitments]
	@commitments [dbo].[CommitmentType] READONLY
AS
BEGIN
	UPDATE [Commitment]
	SET [NegotiatedPrice] = p.[NegotiatedPrice],
		[PaymentStatus] = p.[PaymentStatus],
		[EffectiveFrom] = p.[EffectiveFrom],
		[EffectiveTo] = p.[EffectiveTo],
		[EmployerAccountId] = p.[EmployerAccountId],
		[EndDate] = p.EndDate,
		[StartDate] = p.[StartDate]
	FROM [Commitment]
	JOIN @commitments p ON p.Ukprn = Commitment.Ukprn
		AND p.LearnerReferenceNumber = Commitment.LearnerReferenceNumber
		AND p.FrameworkCode = Commitment.FrameworkCode
END

