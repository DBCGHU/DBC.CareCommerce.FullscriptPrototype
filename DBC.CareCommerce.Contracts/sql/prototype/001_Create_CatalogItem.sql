SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE dbo.CatalogItem
(
    CatalogItemID INT IDENTITY(1,1) NOT NULL,
    CatalogItemGuid UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_CatalogItem_Guid DEFAULT NEWID(),

    CatalogItemType NVARCHAR(50) NOT NULL,
    ClinicalCategory NVARCHAR(50) NULL,
    BillingCategory NVARCHAR(50) NULL,
    FulfillmentCategory NVARCHAR(50) NULL,

    DisplayName NVARCHAR(255) NOT NULL,
    ShortName NVARCHAR(100) NULL,
    Description NVARCHAR(MAX) NULL,
    SearchKeywords NVARCHAR(MAX) NULL,

    BrandName NVARCHAR(255) NULL,
    ManufacturerName NVARCHAR(255) NULL,
    SKU NVARCHAR(100) NULL,
    UPC NVARCHAR(100) NULL,

    FeeID INT NULL,
    ProductID INT NULL,
    SupplementID INT NULL,

    DefaultChargeAmount DECIMAL(18,2) NULL,
    DefaultUnits DECIMAL(18,4) NULL,
    Taxable BIT NULL,
    RevenueCategory NVARCHAR(100) NULL,
    LedgerCode NVARCHAR(100) NULL,

    InventoryEnabled BIT NOT NULL CONSTRAINT DF_CatalogItem_InventoryEnabled DEFAULT 0,
    TrackQuantity BIT NOT NULL CONSTRAINT DF_CatalogItem_TrackQuantity DEFAULT 0,
    DefaultInventoryLocationID INT NULL,
    UnitOfMeasure NVARCHAR(50) NULL,
    PackageSize NVARCHAR(100) NULL,
    ReorderPoint DECIMAL(18,4) NULL,
    ReorderQuantity DECIMAL(18,4) NULL,

    FullscriptEnabled BIT NOT NULL CONSTRAINT DF_CatalogItem_FullscriptEnabled DEFAULT 0,
    FullscriptProductID NVARCHAR(100) NULL,
    FullscriptVariantID NVARCHAR(100) NULL,
    FullscriptSKU NVARCHAR(100) NULL,
    FullscriptUPC NVARCHAR(100) NULL,
    FullscriptBrandID NVARCHAR(100) NULL,
    FullscriptBrandName NVARCHAR(255) NULL,
    FullscriptProductName NVARCHAR(255) NULL,
    FullscriptVariantStatus NVARCHAR(100) NULL,
    FullscriptAvailability NVARCHAR(100) NULL,
    FullscriptMSRP DECIMAL(18,2) NULL,
    FullscriptLastSyncedDateTime DATETIME2 NULL,

    DefaultFulfillmentSource NVARCHAR(50) NOT NULL CONSTRAINT DF_CatalogItem_DefaultFulfillmentSource DEFAULT 'None',
    DefaultBillingAction NVARCHAR(50) NOT NULL CONSTRAINT DF_CatalogItem_DefaultBillingAction DEFAULT 'NoBilling',
    DefaultInventoryAction NVARCHAR(50) NOT NULL CONSTRAINT DF_CatalogItem_DefaultInventoryAction DEFAULT 'None',

    RequiresPatient BIT NOT NULL CONSTRAINT DF_CatalogItem_RequiresPatient DEFAULT 1,
    RequiresPatientCase BIT NOT NULL CONSTRAINT DF_CatalogItem_RequiresPatientCase DEFAULT 1,
    RequiresProvider BIT NOT NULL CONSTRAINT DF_CatalogItem_RequiresProvider DEFAULT 0,
    RequiresDosage BIT NOT NULL CONSTRAINT DF_CatalogItem_RequiresDosage DEFAULT 0,
    RequiresInstructions BIT NOT NULL CONSTRAINT DF_CatalogItem_RequiresInstructions DEFAULT 0,

    DefaultDosageAmount NVARCHAR(100) NULL,
    DefaultDosageFrequency NVARCHAR(100) NULL,
    DefaultDosageDuration NVARCHAR(100) NULL,
    DefaultDosageFormat NVARCHAR(100) NULL,
    DefaultTakeWith NVARCHAR(100) NULL,
    DefaultInstructions NVARCHAR(MAX) NULL,

    CatalogStatus NVARCHAR(50) NOT NULL CONSTRAINT DF_CatalogItem_CatalogStatus DEFAULT 'Active',
    MappingConfidence NVARCHAR(50) NULL,
    NeedsReview BIT NOT NULL CONSTRAINT DF_CatalogItem_NeedsReview DEFAULT 0,

    Active BIT NOT NULL CONSTRAINT DF_CatalogItem_Active DEFAULT 1,
    CreatedDateTime DATETIME2 NOT NULL CONSTRAINT DF_CatalogItem_CreatedDateTime DEFAULT SYSUTCDATETIME(),
    UpdatedDateTime DATETIME2 NULL,

    CONSTRAINT PK_CatalogItem PRIMARY KEY CLUSTERED (CatalogItemID)
);
GO

CREATE UNIQUE INDEX UX_CatalogItem_Guid
ON dbo.CatalogItem (CatalogItemGuid);
GO

CREATE INDEX IX_CatalogItem_DisplayName
ON dbo.CatalogItem (DisplayName);
GO

CREATE INDEX IX_CatalogItem_FeeID
ON dbo.CatalogItem (FeeID);
GO

CREATE INDEX IX_CatalogItem_ProductID
ON dbo.CatalogItem (ProductID);
GO

CREATE INDEX IX_CatalogItem_SupplementID
ON dbo.CatalogItem (SupplementID);
GO

CREATE INDEX IX_CatalogItem_FullscriptVariantID
ON dbo.CatalogItem (FullscriptVariantID);
GO

CREATE INDEX IX_CatalogItem_Type_Status_Active
ON dbo.CatalogItem (CatalogItemType, CatalogStatus, Active);
GO