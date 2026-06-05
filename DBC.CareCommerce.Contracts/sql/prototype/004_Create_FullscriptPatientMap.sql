CREATE TABLE dbo.FullscriptPatientMap
(
    FullscriptPatientMapID INT IDENTITY(1,1) NOT NULL,
    FullscriptPatientMapGuid UNIQUEIDENTIFIER NOT NULL 
        CONSTRAINT DF_FullscriptPatientMap_Guid DEFAULT NEWID(),

    PatientID INT NOT NULL,

    FullscriptPatientID NVARCHAR(100) NOT NULL,
    FullscriptMetadataID NVARCHAR(100) NULL,

    FullscriptEmail NVARCHAR(255) NULL,
    FullscriptFirstName NVARCHAR(100) NULL,
    FullscriptLastName NVARCHAR(100) NULL,

    Environment NVARCHAR(50) NOT NULL 
        CONSTRAINT DF_FullscriptPatientMap_Environment DEFAULT 'UsSandbox',

    ClinicID NVARCHAR(100) NULL,

    LastSyncedDateTime DATETIME2 NULL,

    Active BIT NOT NULL 
        CONSTRAINT DF_FullscriptPatientMap_Active DEFAULT 1,

    CreatedDateTime DATETIME2 NOT NULL 
        CONSTRAINT DF_FullscriptPatientMap_CreatedDateTime DEFAULT SYSUTCDATETIME(),

    UpdatedDateTime DATETIME2 NULL,

    CONSTRAINT PK_FullscriptPatientMap 
        PRIMARY KEY CLUSTERED (FullscriptPatientMapID)
);
GO

CREATE UNIQUE INDEX UX_FullscriptPatientMap_Guid
ON dbo.FullscriptPatientMap (FullscriptPatientMapGuid);
GO

CREATE INDEX IX_FullscriptPatientMap_PatientID
ON dbo.FullscriptPatientMap (PatientID);
GO

CREATE INDEX IX_FullscriptPatientMap_FullscriptPatientID
ON dbo.FullscriptPatientMap (FullscriptPatientID);
GO

CREATE INDEX IX_FullscriptPatientMap_MetadataID
ON dbo.FullscriptPatientMap (FullscriptMetadataID);
GO

CREATE INDEX IX_FullscriptPatientMap_Environment_Clinic_Patient
ON dbo.FullscriptPatientMap (Environment, ClinicID, PatientID);
GO