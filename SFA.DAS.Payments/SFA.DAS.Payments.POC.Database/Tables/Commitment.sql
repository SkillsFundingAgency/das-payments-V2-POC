CREATE TABLE [dbo].[Commitment](
	[Id] [bigint] NOT NULL,
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
	[Uln] [bigint] NULL,
 CONSTRAINT [PK_Commitment] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO



CREATE INDEX [IX_Commitment_Ukprn_LearnerReferenceNumber] ON [dbo].[Commitment] ([Ukprn], [LearnerReferenceNumber])
