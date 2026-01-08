IF DB_ID('CADDevicesDb') IS NULL
BEGIN
    CREATE DATABASE CADDevicesDb;
END
GO

USE CADDevicesDb;
GO

IF OBJECT_ID('dbo.Devices','U') IS NULL
BEGIN
    CREATE TABLE dbo.Devices (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL,
        Code NVARCHAR(50) NULL,
        Description NVARCHAR(1000) NULL,
        NxModelPath NVARCHAR(500) NULL,
        DrawingImage VARBINARY(MAX) NULL
    );
END
GO

IF COL_LENGTH('dbo.Devices','NxModelPath') IS NULL
    ALTER TABLE dbo.Devices ADD NxModelPath NVARCHAR(500) NULL;
GO

IF COL_LENGTH('dbo.Devices','DrawingImage') IS NULL
    ALTER TABLE dbo.Devices ADD DrawingImage VARBINARY(MAX) NULL;
GO

IF OBJECT_ID('dbo.Parts','U') IS NULL
BEGIN
    CREATE TABLE dbo.Parts (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        DeviceId INT NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        PartType NVARCHAR(100) NOT NULL,
        NxModelPath NVARCHAR(500) NULL,
        DrawingImage VARBINARY(MAX) NULL,
        Notes NVARCHAR(1000) NULL,
        CONSTRAINT FK_Parts_Devices FOREIGN KEY (DeviceId) REFERENCES dbo.Devices(Id) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID('dbo.Spec_07_02_Bracket','U') IS NULL
BEGIN
    CREATE TABLE dbo.Spec_07_02_Bracket (
        PartId INT PRIMARY KEY,
        Length DECIMAL(10,2) NULL,
        Width DECIMAL(10,2) NULL,
        Thickness DECIMAL(10,2) NULL,
        Material NVARCHAR(100) NULL,
        CONSTRAINT FK_Spec_07_02_Bracket_Parts FOREIGN KEY (PartId) REFERENCES dbo.Parts(Id) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID('dbo.Spec_07_05_Roller','U') IS NULL
BEGIN
    CREATE TABLE dbo.Spec_07_05_Roller (
        PartId INT PRIMARY KEY,
        Diameter DECIMAL(10,2) NULL,
        Width DECIMAL(10,2) NULL,
        Material NVARCHAR(100) NULL,
        CONSTRAINT FK_Spec_07_05_Roller_Parts FOREIGN KEY (PartId) REFERENCES dbo.Parts(Id) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID('dbo.Spec_07_06_Frame','U') IS NULL
BEGIN
    CREATE TABLE dbo.Spec_07_06_Frame (
        PartId INT PRIMARY KEY,
        Length DECIMAL(10,2) NULL,
        Width DECIMAL(10,2) NULL,
        Height DECIMAL(10,2) NULL,
        Material NVARCHAR(100) NULL,
        CONSTRAINT FK_Spec_07_06_Frame_Parts FOREIGN KEY (PartId) REFERENCES dbo.Parts(Id) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID('dbo.Spec_07_07_Roller','U') IS NULL
BEGIN
    CREATE TABLE dbo.Spec_07_07_Roller (
        PartId INT PRIMARY KEY,
        Diameter DECIMAL(10,2) NULL,
        Width DECIMAL(10,2) NULL,
        Material NVARCHAR(100) NULL,
        CONSTRAINT FK_Spec_07_07_Roller_Parts FOREIGN KEY (PartId) REFERENCES dbo.Parts(Id) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID('dbo.Spec_07_08_Adapter','U') IS NULL
BEGIN
    CREATE TABLE dbo.Spec_07_08_Adapter (
        PartId INT PRIMARY KEY,
        Length DECIMAL(10,2) NULL,
        OuterDiameter DECIMAL(10,2) NULL,
        InnerDiameter DECIMAL(10,2) NULL,
        Material NVARCHAR(100) NULL,
        CONSTRAINT FK_Spec_07_08_Adapter_Parts FOREIGN KEY (PartId) REFERENCES dbo.Parts(Id) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID('dbo.Spec_07_10_Spring','U') IS NULL
BEGIN
    CREATE TABLE dbo.Spec_07_10_Spring (
        PartId INT PRIMARY KEY,
        WireDiameter DECIMAL(10,2) NULL,
        OuterDiameter DECIMAL(10,2) NULL,
        FreeLength DECIMAL(10,2) NULL,
        CoilsCount INT NULL,
        Material NVARCHAR(100) NULL,
        CONSTRAINT FK_Spec_07_10_Spring_Parts FOREIGN KEY (PartId) REFERENCES dbo.Parts(Id) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID('dbo.Spec_07_11_Stop','U') IS NULL
BEGIN
    CREATE TABLE dbo.Spec_07_11_Stop (
        PartId INT PRIMARY KEY,
        Length DECIMAL(10,2) NULL,
        Width DECIMAL(10,2) NULL,
        Height DECIMAL(10,2) NULL,
        Material NVARCHAR(100) NULL,
        CONSTRAINT FK_Spec_07_11_Stop_Parts FOREIGN KEY (PartId) REFERENCES dbo.Parts(Id) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID('dbo.Spec_07_14_Ring','U') IS NULL
BEGIN
    CREATE TABLE dbo.Spec_07_14_Ring (
        PartId INT PRIMARY KEY,
        OuterDiameter DECIMAL(10,2) NULL,
        InnerDiameter DECIMAL(10,2) NULL,
        Thickness DECIMAL(10,2) NULL,
        Material NVARCHAR(100) NULL,
        CONSTRAINT FK_Spec_07_14_Ring_Parts FOREIGN KEY (PartId) REFERENCES dbo.Parts(Id) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID('dbo.Spec_07_15_Screw','U') IS NULL
BEGIN
    CREATE TABLE dbo.Spec_07_15_Screw (
        PartId INT PRIMARY KEY,
        Diameter DECIMAL(10,2) NULL,
        Length DECIMAL(10,2) NULL,
        ThreadPitch DECIMAL(10,2) NULL,
        HeadType NVARCHAR(100) NULL,
        Material NVARCHAR(100) NULL,
        CONSTRAINT FK_Spec_07_15_Screw_Parts FOREIGN KEY (PartId) REFERENCES dbo.Parts(Id) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID('dbo.CustomPartTypes','U') IS NULL
BEGIN
    CREATE TABLE dbo.CustomPartTypes (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(200) NOT NULL
    );
END
GO

IF OBJECT_ID('dbo.CustomPartParams','U') IS NULL
BEGIN
    CREATE TABLE dbo.CustomPartParams (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        TypeId INT NOT NULL,
        ParamName NVARCHAR(200) NOT NULL,
        ParamKind NVARCHAR(20) NOT NULL,
        ListValues NVARCHAR(1000) NULL,
        SortOrder INT NULL,
        CONSTRAINT FK_CustomPartParams_Types FOREIGN KEY (TypeId) REFERENCES dbo.CustomPartTypes(Id) ON DELETE CASCADE
    );
END
GO

IF OBJECT_ID('dbo.CustomPartValues','U') IS NULL
BEGIN
    CREATE TABLE dbo.CustomPartValues (
        PartId INT NOT NULL,
        ParamId INT NOT NULL,
        ValueText NVARCHAR(1000) NULL,
        CONSTRAINT PK_CustomPartValues PRIMARY KEY (PartId, ParamId),
        CONSTRAINT FK_CustomPartValues_Parts FOREIGN KEY (PartId) REFERENCES dbo.Parts(Id) ON DELETE CASCADE,
        CONSTRAINT FK_CustomPartValues_Params FOREIGN KEY (ParamId) REFERENCES dbo.CustomPartParams(Id) ON DELETE CASCADE
    );
END
GO
