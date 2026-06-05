SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE dbo.CareItem
(
    CareItemID INT IDENTITY(1,1) NOT NULL,
    CareItemGuid UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_CareItem_Guid DEFAULT NEWID(),

    PatientID INT NOT NULL,
    PatientCaseID INT NULL,
    VisitID INT NULL,
    ProviderID INT NULL,

    CatalogItemID INT NULL,

    SourceSystem NVARCHAR(50) NOT NULL,
    SourceEntityType NVARCHAR(50) NULL,
    SourceEntityID INT NULL,

    TreatmentID INT NULL,
    MedicationID INT NULL,
    SupplementRecordID INT NULL,
    PostingID INT NULL,

    CareItemType NVARCHAR(50) NOT NULL,
    ClinicalStatus NVARCHAR(50) NOT NULL CONSTRAINT DF_CareItem_ClinicalStatus DEFAULT 'Draft',

    FulfillmentSource NVARCHAR(50) NOT NULL CONSTRAINT DF_CareItem_FulfillmentSource DEFAULT 'None',
    BillingIntent NVARCHAR(50) NOT NULL CONSTRAINT DF_CareItem_BillingIntent DEFAULT 'NoBilling',
    InventoryIntent NVARCHAR(50) NOT NULL CONSTRAINT DF_CareItem_InventoryIntent DEFAULT 'None',

    QuantityRecommended DECIMAL(18,4) NULL,
    QuantityDispensed DECIMAL(18,4) NULL,

    DosageAmount NVARCHAR(100) NULL,
    DosageFrequency NVARCHAR(100) NULL,
    DosageDuration NVARCHAR(100) NULL,
    DosageFormat NVARCHAR(100) NULL,
    TakeWith NVARCHAR(100) NULL,
    Instructions NVARCHAR(MAX) NULL,
    NarrativeText NVARCHAR(MAX) NULL,

    ProductID INT NULL,
    FeeID INT NULL,
    FullscriptVariantID NVARCHAR(100) NULL,

    RequiresPatientCase BIT NOT NULL CONSTRAINT DF_CareItem_RequiresPatientCase DEFAULT 1,

    CreatedByUserID INT NULL,
    CreatedDateTime DATETIME2 NOT NULL CONSTRAINT DF_CareItem_CreatedDateTime DEFAULT SYSUTCDATETIME(),
    UpdatedDateTime DATETIME2 NULL,

    Active BIT NOT NULL CONSTRAINT DF_CareItem_Active DEFAULT 1,

    CONSTRAINT PK_CareItem PRIMARY KEY CLUSTERED (CareItemID),

    CONSTRAINT FK_CareItem_CatalogItem
        FOREIGN KEY (CatalogItemID)
        REFERENCES dbo.CatalogItem (CatalogItemID)
);
GO

CREATE UNIQUE INDEX UX_CareItem_Guid
ON dbo.CareItem (CareItemGuid);
GO

CREATE INDEX IX_CareItem_PatientID
ON dbo.CareItem (PatientID);
GO

CREATE INDEX IX_CareItem_PatientCaseID
ON dbo.CareItem (PatientCaseID);
GO

CREATE INDEX IX_CareItem_VisitID
ON dbo.CareItem (VisitID);
GO

CREATE INDEX IX_CareItem_ProviderID
ON dbo.CareItem (ProviderID);
GO

CREATE INDEX IX_CareItem_CatalogItemID
ON dbo.CareItem (CatalogItemID);
GO

CREATE INDEX IX_CareItem_Source
ON dbo.CareItem (SourceSystem, SourceEntityType, SourceEntityID);
GO

CREATE INDEX IX_CareItem_FulfillmentSource
ON dbo.CareItem (FulfillmentSource);
GO

CREATE INDEX IX_CareItem_BillingIntent
ON dbo.CareItem (BillingIntent);
GO