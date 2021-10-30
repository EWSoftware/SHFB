-- NOTE: Change the file locations if necessary
CREATE DATABASE [Sandcastle]
 ON  PRIMARY
( NAME = N'Sandcastle', FILENAME = N'C:\Databases\Sandcastle.mdf' )
 LOG ON
( NAME = N'Sandcastle_log', FILENAME = N'C:\Databases\Sandcastle_log.ldf' )
GO

USE [Sandcastle]
GO

-- Member ID URLs table
CREATE TABLE [dbo].[MemberIdUrls](
	[TargetKey] [varchar](2048) NOT NULL,
	[MemberUrl] [varchar](2048) NULL,
 CONSTRAINT [PK_MemberIdUrls] PRIMARY KEY CLUSTERED
(
	[TargetKey] ASC
) ON [PRIMARY]
) ON [PRIMARY]

GO

-- Reflection data and XML comments index table
CREATE TABLE [dbo].[IndexData](
	[GroupId] [varchar](50) NOT NULL,
	[IndexKey] [varchar](2048) NOT NULL,
	[IndexValue] [varchar](max) NULL,
 CONSTRAINT [PK_IndexData] PRIMARY KEY CLUSTERED 
(
	[GroupId] ASC,
	[IndexKey] ASC
) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

CREATE NONCLUSTERED INDEX [IndexData_GroupId] ON [dbo].[IndexData]
(
	[GroupId] ASC
) ON [PRIMARY]
GO

-- Reflection targets table
CREATE TABLE [dbo].[Targets](
	[GroupId] [varchar](50) NOT NULL,
	[TargetKey] [varchar](2048) NOT NULL,
	[TargetValue] [varbinary](max) NULL,
 CONSTRAINT [PK_Targets] PRIMARY KEY CLUSTERED 
(
	[GroupId] ASC,
	[TargetKey] ASC
) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

CREATE NONCLUSTERED INDEX [Targets_GroupId] ON [dbo].[Targets]
(
	[GroupId] ASC
) ON [PRIMARY]
GO
