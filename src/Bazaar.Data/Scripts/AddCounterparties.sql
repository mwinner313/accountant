-- Run against Bazaar database when not using EF migrations.
IF OBJECT_ID(N'dbo.Counterparties', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Counterparties (
        CounterpartyId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Id UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        CreateDate DATETIME2(0) NOT NULL DEFAULT GETDATE(),
        Deleted BIT NOT NULL DEFAULT 0,
        OwnerId UNIQUEIDENTIFIER NOT NULL,
        FullName NVARCHAR(300) NOT NULL
    );
    CREATE UNIQUE INDEX IX_Counterparties_Id ON dbo.Counterparties(Id);
    CREATE INDEX IX_Counterparties_OwnerId ON dbo.Counterparties(OwnerId);
    CREATE INDEX IX_Counterparties_OwnerId_Deleted ON dbo.Counterparties(OwnerId, Deleted);
    CREATE INDEX IX_Counterparties_FullName ON dbo.Counterparties(FullName);
END

IF OBJECT_ID(N'dbo.CounterpartyPhones', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CounterpartyPhones (
        CounterpartyPhoneId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CounterpartyId UNIQUEIDENTIFIER NOT NULL,
        Number NVARCHAR(40) NOT NULL,
        CONSTRAINT FK_CounterpartyPhones_Counterparties FOREIGN KEY (CounterpartyId)
            REFERENCES dbo.Counterparties(Id) ON DELETE CASCADE
    );
END

IF OBJECT_ID(N'dbo.CounterpartyBankAccounts', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.CounterpartyBankAccounts (
        CounterpartyBankAccountId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CounterpartyId UNIQUEIDENTIFIER NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        AccountNumber NVARCHAR(50) NOT NULL,
        ShebaNumber NVARCHAR(34) NOT NULL,
        CardNumber NVARCHAR(20) NOT NULL,
        CONSTRAINT FK_CounterpartyBankAccounts_Counterparties FOREIGN KEY (CounterpartyId)
            REFERENCES dbo.Counterparties(Id) ON DELETE CASCADE
    );
END

IF COL_LENGTH('dbo.Factors', 'CounterpartyId') IS NULL
BEGIN
    ALTER TABLE dbo.Factors ADD CounterpartyId UNIQUEIDENTIFIER NULL;
    ALTER TABLE dbo.Factors ADD CONSTRAINT FK_Factors_Counterparties FOREIGN KEY (CounterpartyId)
        REFERENCES dbo.Counterparties(Id);
    CREATE INDEX IX_Factors_CounterpartyId ON dbo.Factors(CounterpartyId);
END
