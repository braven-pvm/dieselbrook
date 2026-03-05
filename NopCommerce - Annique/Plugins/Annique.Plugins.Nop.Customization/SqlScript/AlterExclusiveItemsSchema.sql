CREATE TABLE dbo.ANQ_ExclusiveItems_Temp
(
    [Id] [int] NOT NULL,
    [ProductID] [int] NULL,
    [CustomerID] [int] NULL,
    [RegistrationID] [int] NULL,
    [nQtyLimit] [int] NULL,
    [nQtyPurchased] [int] NULL,
    [dFrom] [datetime2](6) NULL,
    [dTo] [datetime2](6) NULL,
    [IActive] [bit] NULL,
    [IForce] [bit] NULL,
    [IStarter] [bit] NULL,
);
INSERT INTO dbo.ANQ_ExclusiveItems_Temp (Id, ProductID, CustomerID, RegistrationID, nQtyLimit, nQtyPurchased, dFrom, dTo, IActive, IForce, IStarter) SELECT Id, ProductID, CustomerID, RegistrationID, nQtyLimit, nQtyPurchased, dFrom, dTo, IActive, IForce, IStarter FROM dbo.ANQ_ExclusiveItems;
DROP TABLE dbo.ANQ_ExclusiveItems;
EXEC sp_rename 'dbo.ANQ_ExclusiveItems_Temp', 'ANQ_ExclusiveItems';
ALTER TABLE dbo.ANQ_ExclusiveItems ADD PRIMARY KEY (Id)