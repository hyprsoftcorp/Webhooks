IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Subscriptions]') AND type in (N'U'))
DROP TABLE [dbo].[Subscriptions]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[AggregatedCounter]') AND type in (N'U'))
DROP TABLE [HangFire].[AggregatedCounter]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[Counter]') AND type in (N'U'))
DROP TABLE [HangFire].[Counter]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[Hash]') AND type in (N'U'))
DROP TABLE [HangFire].[Hash]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[Job]') AND type in (N'U'))
DROP TABLE [HangFire].[Job]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[JobParameter]') AND type in (N'U'))
DROP TABLE [HangFire].[JobParameter]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[JobQueue]') AND type in (N'U'))
DROP TABLE [HangFire].[JobQueue]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[List]') AND type in (N'U'))
DROP TABLE [HangFire].[List]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[Schema]') AND type in (N'U'))
DROP TABLE [HangFire].[Schema]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[Server]') AND type in (N'U'))
DROP TABLE [HangFire].[Server]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[Set]') AND type in (N'U'))
DROP TABLE [HangFire].[Set]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[HangFire].[State]') AND type in (N'U'))
DROP TABLE [HangFire].[State]
GO

CREATE TABLE [dbo].[Subscriptions] (
    [SubscriptionId]   INT            IDENTITY (1, 1) NOT NULL,
    [TypeName]         NVARCHAR (450) NOT NULL,
    [CreatedUtc]       DATETIME2 (7)  NOT NULL,
    [WebhookUri]       NVARCHAR (450) NOT NULL,
    [FilterExpression] NVARCHAR (MAX) NULL,
    [Filter]           NVARCHAR (MAX) NULL,
    [IsActive]         BIT            NOT NULL
);
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_Subscriptions_TypeName_WebhookUri]
    ON [dbo].[Subscriptions]([TypeName] ASC, [WebhookUri] ASC);
GO

ALTER TABLE [dbo].[Subscriptions]
    ADD CONSTRAINT [PK_Subscriptions] PRIMARY KEY CLUSTERED ([SubscriptionId] ASC);
GO

ALTER TABLE [dbo].[Subscriptions] ADD  DEFAULT (GETUTCDATE()) FOR [CreatedUtc]
GO