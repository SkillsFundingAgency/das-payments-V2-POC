CREATE TYPE [dbo].[CommitmentType] AS TABLE
(
	[ProgrammeType] [int] NULL,
	[StandardCode] [bigint] NULL,
	[FrameworkCode] [int] NULL,
	[PathwayCode] [int] NULL,
	[Ukprn] [bigint] NULL,
	[LearnerReferenceNumber] [varchar](50) NULL,
	[TransferSenderAccountId] [bigint] NULL,
	[EmployerAccountId] [bigint] NULL,
	[PaymentStatus] [int] NULL,
	[NegotiatedPrice] [bigint] NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[EffectiveFrom] [datetime] NULL,
	[EffectiveTo] [datetime] NULL,
	[Uln] [bigint] NULL
)
