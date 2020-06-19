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