CREATE TABLE [dbo].[Metric](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[BatchId] [varchar](50) NOT NULL,
	[Number] [varchar](50) NULL,
	[InsideRead] [bigint] NULL,
	[InsideCalc] [bigint] NULL,
	[InsideWrite] [bigint] NULL,
	[OutsideProxy] [bigint] NULL,
	[OutsideCall] [bigint] NULL,
	[Actor] [varchar](50) NULL,
	[Timestamp] [datetime2](7) NOT NULL,
	[Progress] [decimal](18, 2) NULL,
	[WriteMetrics] [bigint] NULL,
 CONSTRAINT [PK_Metric] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Metric] ADD  CONSTRAINT [DF_Metric_Timestamp]  DEFAULT (getutcdate()) FOR [Timestamp]
GO

