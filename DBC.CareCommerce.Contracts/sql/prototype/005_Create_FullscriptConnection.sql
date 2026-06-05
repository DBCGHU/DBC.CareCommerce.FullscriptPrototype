SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE dbo.FullscriptConnection
(
    FullscriptConnectionID INT IDENTITY(1,1) NOT NULL,
    FullscriptConnectionGuid UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT DF_FullscriptConnection_Guid DEFAULT NEWID(),

    Environment NVARCHAR(50) NOT NULL,
    ClinicID NVARCHAR(100) NULL,
    ClinicName NVARCHAR(255) NULL,

    PractitionerID NVARCHAR(100) NULL,
    PractitionerType NVARCHAR(50) NULL,

    ClientID NVARCHAR(255) NULL,

    AccessTokenEncrypted NVARCHAR(MAX) NULL,
    RefreshTokenEncrypted NVARCHAR(MAX) NULL,

    TokenType NVARCHAR(50) NULL,
    Scope NVARCHAR(MAX) NULL,

    TokenReceivedDateTime DATETIME2 NULL,
    TokenExpiresAtDateTime DATETIME2 NULL,
    LastRefreshDateTime DATETIME2 NULL,

    DispensaryUrl NVARCHAR(500) NULL,
    IntegrationID NVARCHAR(100) NULL,
    IntegrationActivatedAt DATETIME2 NULL,

    Status NVARCHAR(50) NOT NULL
        CONSTRAINT DF_FullscriptConnection_Status DEFAULT 'Active',

    ErrorMessage NVARCHAR(MAX) NULL,

    Active BIT NOT NULL
        CONSTRAINT DF_FullscriptConnection_Active DEFAULT 1,

    CreatedDateTime DATETIME2 NOT NULL
        CONSTRAINT DF_FullscriptConnection_CreatedDateTime DEFAULT SYSUTCDATETIME(),

    UpdatedDateTime DATETIME2 NULL,

    CONSTRAINT PK_FullscriptConnection
        PRIMARY KEY CLUSTERED (FullscriptConnectionID)
);
GO

CREATE UNIQUE INDEX UX_FullscriptConnection_Guid
ON dbo.FullscriptConnection (FullscriptConnectionGuid);
GO

CREATE INDEX IX_FullscriptConnection_Environment_ClinicID
ON dbo.FullscriptConnection (Environment, ClinicID);
GO

CREATE INDEX IX_FullscriptConnection_Status_Active
ON dbo.FullscriptConnection (Status, Active);
GO