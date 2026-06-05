SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE dbo.PendingCharge
(
    PendingChargeID INT IDENTITY(1,1) NOT NULL,
    PendingChargeGuid UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_PendingCharge_Guid DEFAULT NEWID(),

    CareItemID INT NULL,

    PatientID INT NOT NULL,
    PatientCaseID INT NOT NULL,
    ProviderID INT NULL,

    CatalogItemID INT NULL,
    FeeID INT NULL,
    ProductID INT NULL,

    Description NVARCHAR(255) NOT NULL,

    Quantity DECIMAL(18,4) NOT NULL CONSTRAINT DF_PendingCharge_Quantity DEFAULT 1,
    UnitAmount DECIMAL(18,2) NULL,
    TotalAmount DECIMAL(18,2) NULL,

    BillingAction NVARCHAR(50) NOT NULL,
    InventoryAction NVARCHAR(50) NOT NULL,
    FulfillmentSource NVARCHAR(50) NOT NULL,

    Status NVARCHAR(50) NOT NULL CONSTRAINT DF_PendingCharge_Status DEFAULT 'Pending',

    ApprovedByUserID INT NULL,
    ApprovedDateTime DATETIME2 NULL,

    RejectedByUserID INT NULL,
    RejectedDateTime DATETIME2 NULL,
    RejectionReason NVARCHAR(500) NULL,

    PostedDateTime DATETIME2 NULL,
    PostingID INT NULL,

    ErrorMessage NVARCHAR(MAX) NULL,

    CreatedByUserID INT NULL,
    CreatedDateTime DATETIME2 NOT NULL CONSTRAINT DF_PendingCharge_CreatedDateTime DEFAULT SYSUTCDATETIME(),
    UpdatedDateTime DATETIME2 NULL,

    Active BIT NOT NULL CONSTRAINT DF_PendingCharge_Active DEFAULT 1,

    CONSTRAINT PK_PendingCharge PRIMARY KEY CLUSTERED (PendingChargeID),

    CONSTRAINT FK_PendingCharge_CareItem
        FOREIGN KEY (CareItemID)
        REFERENCES dbo.CareItem (CareItemID),

    CONSTRAINT FK_PendingCharge_CatalogItem
        FOREIGN KEY (CatalogItemID)
        REFERENCES dbo.CatalogItem (CatalogItemID)
);
GO

CREATE UNIQUE INDEX UX_PendingCharge_Guid
ON dbo.PendingCharge (PendingChargeGuid);
GO

CREATE INDEX IX_PendingCharge_PatientCase_Status
ON dbo.PendingCharge (PatientID, PatientCaseID, Status);
GO

CREATE INDEX IX_PendingCharge_CareItemID
ON dbo.PendingCharge (CareItemID);
GO

CREATE INDEX IX_PendingCharge_CatalogItemID
ON dbo.PendingCharge (CatalogItemID);
GO

CREATE INDEX IX_PendingCharge_PostingID
ON dbo.PendingCharge (PostingID);
GO

CREATE INDEX IX_PendingCharge_FeeID
ON dbo.PendingCharge (FeeID);
GO

CREATE INDEX IX_PendingCharge_ProductID
ON dbo.PendingCharge (ProductID);
GO

CREATE INDEX IX_PendingCharge_Status_Active
ON dbo.PendingCharge (Status, Active);
GO