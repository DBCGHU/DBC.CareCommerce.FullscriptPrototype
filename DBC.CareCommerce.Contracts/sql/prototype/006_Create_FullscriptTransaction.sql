IF OBJECT_ID('dbo.FullscriptTransaction', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.FullscriptTransaction
    (
        FullscriptTransactionID INT IDENTITY(1,1) NOT NULL,
        FullscriptTransactionGuid UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_FullscriptTransaction_Guid DEFAULT NEWID(),

        CareItemID INT NULL,
        CatalogItemID INT NULL,

        PatientID INT NOT NULL,
        PatientCaseID INT NULL,
        ProviderID INT NULL,

        FullscriptPatientID NVARCHAR(200) NULL,
        FullscriptPractitionerID NVARCHAR(200) NULL,
        FullscriptProductID NVARCHAR(200) NULL,
        FullscriptVariantID NVARCHAR(200) NULL,

        FullscriptTreatmentPlanID NVARCHAR(200) NULL,
        FullscriptOrderID NVARCHAR(200) NULL,
        FullscriptOrderNumber NVARCHAR(200) NULL,

        TreatmentPlanState NVARCHAR(100) NULL,
        OrderStatus NVARCHAR(100) NULL,

        InvitationUrl NVARCHAR(1000) NULL,

        CompletedAt DATETIME2 NULL,
        ItemTotal DECIMAL(18,2) NULL,
        MsrpTotal DECIMAL(18,2) NULL,
        PaymentTotal DECIMAL(18,2) NULL,

        LastSyncedDateTime DATETIME2 NULL,

        Status NVARCHAR(50) NOT NULL CONSTRAINT DF_FullscriptTransaction_Status DEFAULT N'Pending',
        ErrorMessage NVARCHAR(MAX) NULL,

        CreatedDateTime DATETIME2 NOT NULL CONSTRAINT DF_FullscriptTransaction_CreatedDateTime DEFAULT SYSUTCDATETIME(),
        UpdatedDateTime DATETIME2 NULL,

        Active BIT NOT NULL CONSTRAINT DF_FullscriptTransaction_Active DEFAULT 1,

        CONSTRAINT PK_FullscriptTransaction PRIMARY KEY CLUSTERED (FullscriptTransactionID),

        CONSTRAINT FK_FullscriptTransaction_CareItem
            FOREIGN KEY (CareItemID)
            REFERENCES dbo.CareItem (CareItemID),

        CONSTRAINT FK_FullscriptTransaction_CatalogItem
            FOREIGN KEY (CatalogItemID)
            REFERENCES dbo.CatalogItem (CatalogItemID)
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE
        object_id = OBJECT_ID('dbo.FullscriptTransaction')
        AND name = 'IX_FullscriptTransaction_CareItemID'
)
BEGIN
    CREATE INDEX IX_FullscriptTransaction_CareItemID
    ON dbo.FullscriptTransaction (CareItemID);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE
        object_id = OBJECT_ID('dbo.FullscriptTransaction')
        AND name = 'IX_FullscriptTransaction_Status'
)
BEGIN
    CREATE INDEX IX_FullscriptTransaction_Status
    ON dbo.FullscriptTransaction (Status, Active);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE
        object_id = OBJECT_ID('dbo.FullscriptTransaction')
        AND name = 'IX_FullscriptTransaction_PatientCase'
)
BEGIN
    CREATE INDEX IX_FullscriptTransaction_PatientCase
    ON dbo.FullscriptTransaction (PatientID, PatientCaseID);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE
        object_id = OBJECT_ID('dbo.FullscriptTransaction')
        AND name = 'IX_FullscriptTransaction_FullscriptTreatmentPlanID'
)
BEGIN
    CREATE INDEX IX_FullscriptTransaction_FullscriptTreatmentPlanID
    ON dbo.FullscriptTransaction (FullscriptTreatmentPlanID);
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE
        object_id = OBJECT_ID('dbo.FullscriptTransaction')
        AND name = 'IX_FullscriptTransaction_FullscriptOrderID'
)
BEGIN
    CREATE INDEX IX_FullscriptTransaction_FullscriptOrderID
    ON dbo.FullscriptTransaction (FullscriptOrderID);
END;
GO