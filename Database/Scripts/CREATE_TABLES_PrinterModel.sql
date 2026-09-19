USE [BarCodeSannerV3]
GO

/****** Object:  Table [dbo].[PrinterModel]    Script Date: 8/14/2026 10:33:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PrinterModel](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedOn] [datetime] NULL,
 CONSTRAINT [PK_PrinterModel] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[PrinterModelValue]    Script Date: 8/14/2026 10:33:00 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PrinterModelValue](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ModelId] [int] NOT NULL,
	[Name] [nvarchar](255) NOT NULL,
	[Value] [nvarchar](max) NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[ModifiedOn] [datetime] NULL,
 CONSTRAINT [PK_PrinterModelValue] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  ForeignKey [FK_PrinterModelValue_PrinterModel]    Script Date: 8/14/2026 10:33:00 PM ******/
ALTER TABLE [dbo].[PrinterModelValue] ADD CONSTRAINT [FK_PrinterModelValue_PrinterModel] FOREIGN KEY([ModelId])
REFERENCES [dbo].[PrinterModel] ([Id])
ON DELETE CASCADE
GO