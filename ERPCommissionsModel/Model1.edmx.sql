
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, 2012 and Azure
-- --------------------------------------------------
-- Date Created: 11/17/2014 12:31:40
-- Generated from EDMX file: C:\Users\Total Mobile P\documents\visual studio 2013\Projects\ERPComisions\ERPCommissionsModel\Model1.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [ERPCommissions];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_CommissionCommissionPayType]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Commissions] DROP CONSTRAINT [FK_CommissionCommissionPayType];
GO
IF OBJECT_ID(N'[dbo].[FK_CommissionDeaeler]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Commissions] DROP CONSTRAINT [FK_CommissionDeaeler];
GO
IF OBJECT_ID(N'[dbo].[FK_Spiff_inherits_Commission]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Commissions_Spiff] DROP CONSTRAINT [FK_Spiff_inherits_Commission];
GO
IF OBJECT_ID(N'[dbo].[FK_Residual_inherits_Commission]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Commissions_Residual] DROP CONSTRAINT [FK_Residual_inherits_Commission];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Deaelers1]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Deaelers1];
GO
IF OBJECT_ID(N'[dbo].[Operators]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Operators];
GO
IF OBJECT_ID(N'[dbo].[Commissions]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Commissions];
GO
IF OBJECT_ID(N'[dbo].[Plans]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Plans];
GO
IF OBJECT_ID(N'[dbo].[CommissionTypes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CommissionTypes];
GO
IF OBJECT_ID(N'[dbo].[CommissionPayTypes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[CommissionPayTypes];
GO
IF OBJECT_ID(N'[dbo].[Commissions_Spiff]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Commissions_Spiff];
GO
IF OBJECT_ID(N'[dbo].[Commissions_Residual]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Commissions_Residual];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Deaelers1'
CREATE TABLE [dbo].[Deaelers1] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'Operators'
CREATE TABLE [dbo].[Operators] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'Commissions'
CREATE TABLE [dbo].[Commissions] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Value] nvarchar(max)  NOT NULL,
    [StartDate] datetime  NOT NULL,
    [EndDate] datetime  NOT NULL,
    [PlanId] int  NOT NULL,
    [CommissionTypeId] int  NOT NULL,
    [CommissionPayType_Id] int  NOT NULL,
    [Deaeler_Id] int  NOT NULL
);
GO

-- Creating table 'Plans'
CREATE TABLE [dbo].[Plans] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL,
    [Value] decimal(18,0)  NOT NULL,
    [Description] nvarchar(max)  NULL,
    [OperatorId] int  NOT NULL
);
GO

-- Creating table 'CommissionTypes'
CREATE TABLE [dbo].[CommissionTypes] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'CommissionPayTypes'
CREATE TABLE [dbo].[CommissionPayTypes] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [TypeName] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'CommissionRules'
CREATE TABLE [dbo].[CommissionRules] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Name] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'Commissions_Spiff'
CREATE TABLE [dbo].[Commissions_Spiff] (
    [Id] int  NOT NULL
);
GO

-- Creating table 'Commissions_Residual'
CREATE TABLE [dbo].[Commissions_Residual] (
    [Id] int  NOT NULL
);
GO

-- Creating table 'CommissionRules_ActivationNumberRule'
CREATE TABLE [dbo].[CommissionRules_ActivationNumberRule] (
    [NumberOfActivations] nvarchar(max)  NOT NULL,
    [Id] int  NOT NULL
);
GO

-- Creating table 'CommissionCommissionRule'
CREATE TABLE [dbo].[CommissionCommissionRule] (
    [Commissions_Id] int  NOT NULL,
    [CommissionRules_Id] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [Id] in table 'Deaelers1'
ALTER TABLE [dbo].[Deaelers1]
ADD CONSTRAINT [PK_Deaelers1]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Operators'
ALTER TABLE [dbo].[Operators]
ADD CONSTRAINT [PK_Operators]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Commissions'
ALTER TABLE [dbo].[Commissions]
ADD CONSTRAINT [PK_Commissions]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Plans'
ALTER TABLE [dbo].[Plans]
ADD CONSTRAINT [PK_Plans]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'CommissionTypes'
ALTER TABLE [dbo].[CommissionTypes]
ADD CONSTRAINT [PK_CommissionTypes]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'CommissionPayTypes'
ALTER TABLE [dbo].[CommissionPayTypes]
ADD CONSTRAINT [PK_CommissionPayTypes]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'CommissionRules'
ALTER TABLE [dbo].[CommissionRules]
ADD CONSTRAINT [PK_CommissionRules]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Commissions_Spiff'
ALTER TABLE [dbo].[Commissions_Spiff]
ADD CONSTRAINT [PK_Commissions_Spiff]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Commissions_Residual'
ALTER TABLE [dbo].[Commissions_Residual]
ADD CONSTRAINT [PK_Commissions_Residual]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'CommissionRules_ActivationNumberRule'
ALTER TABLE [dbo].[CommissionRules_ActivationNumberRule]
ADD CONSTRAINT [PK_CommissionRules_ActivationNumberRule]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Commissions_Id], [CommissionRules_Id] in table 'CommissionCommissionRule'
ALTER TABLE [dbo].[CommissionCommissionRule]
ADD CONSTRAINT [PK_CommissionCommissionRule]
    PRIMARY KEY CLUSTERED ([Commissions_Id], [CommissionRules_Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [CommissionPayType_Id] in table 'Commissions'
ALTER TABLE [dbo].[Commissions]
ADD CONSTRAINT [FK_CommissionCommissionPayType]
    FOREIGN KEY ([CommissionPayType_Id])
    REFERENCES [dbo].[CommissionPayTypes]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CommissionCommissionPayType'
CREATE INDEX [IX_FK_CommissionCommissionPayType]
ON [dbo].[Commissions]
    ([CommissionPayType_Id]);
GO

-- Creating foreign key on [Deaeler_Id] in table 'Commissions'
ALTER TABLE [dbo].[Commissions]
ADD CONSTRAINT [FK_CommissionDeaeler]
    FOREIGN KEY ([Deaeler_Id])
    REFERENCES [dbo].[Deaelers1]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CommissionDeaeler'
CREATE INDEX [IX_FK_CommissionDeaeler]
ON [dbo].[Commissions]
    ([Deaeler_Id]);
GO

-- Creating foreign key on [PlanId] in table 'Commissions'
ALTER TABLE [dbo].[Commissions]
ADD CONSTRAINT [FK_CommissionPlan]
    FOREIGN KEY ([PlanId])
    REFERENCES [dbo].[Plans]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CommissionPlan'
CREATE INDEX [IX_FK_CommissionPlan]
ON [dbo].[Commissions]
    ([PlanId]);
GO

-- Creating foreign key on [OperatorId] in table 'Plans'
ALTER TABLE [dbo].[Plans]
ADD CONSTRAINT [FK_OperatorPlan]
    FOREIGN KEY ([OperatorId])
    REFERENCES [dbo].[Operators]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_OperatorPlan'
CREATE INDEX [IX_FK_OperatorPlan]
ON [dbo].[Plans]
    ([OperatorId]);
GO

-- Creating foreign key on [CommissionTypeId] in table 'Commissions'
ALTER TABLE [dbo].[Commissions]
ADD CONSTRAINT [FK_CommissionCommissionType]
    FOREIGN KEY ([CommissionTypeId])
    REFERENCES [dbo].[CommissionTypes]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CommissionCommissionType'
CREATE INDEX [IX_FK_CommissionCommissionType]
ON [dbo].[Commissions]
    ([CommissionTypeId]);
GO

-- Creating foreign key on [Commissions_Id] in table 'CommissionCommissionRule'
ALTER TABLE [dbo].[CommissionCommissionRule]
ADD CONSTRAINT [FK_CommissionCommissionRule_Commission]
    FOREIGN KEY ([Commissions_Id])
    REFERENCES [dbo].[Commissions]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating foreign key on [CommissionRules_Id] in table 'CommissionCommissionRule'
ALTER TABLE [dbo].[CommissionCommissionRule]
ADD CONSTRAINT [FK_CommissionCommissionRule_CommissionRule]
    FOREIGN KEY ([CommissionRules_Id])
    REFERENCES [dbo].[CommissionRules]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;
GO

-- Creating non-clustered index for FOREIGN KEY 'FK_CommissionCommissionRule_CommissionRule'
CREATE INDEX [IX_FK_CommissionCommissionRule_CommissionRule]
ON [dbo].[CommissionCommissionRule]
    ([CommissionRules_Id]);
GO

-- Creating foreign key on [Id] in table 'Commissions_Spiff'
ALTER TABLE [dbo].[Commissions_Spiff]
ADD CONSTRAINT [FK_Spiff_inherits_Commission]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[Commissions]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Id] in table 'Commissions_Residual'
ALTER TABLE [dbo].[Commissions_Residual]
ADD CONSTRAINT [FK_Residual_inherits_Commission]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[Commissions]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- Creating foreign key on [Id] in table 'CommissionRules_ActivationNumberRule'
ALTER TABLE [dbo].[CommissionRules_ActivationNumberRule]
ADD CONSTRAINT [FK_ActivationNumberRule_inherits_CommissionRule]
    FOREIGN KEY ([Id])
    REFERENCES [dbo].[CommissionRules]
        ([Id])
    ON DELETE CASCADE ON UPDATE NO ACTION;
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------