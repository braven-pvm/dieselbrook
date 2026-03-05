ALTER TABLE dbo.ANQ_UserProfileAdditionalInfo ALTER COLUMN Title NVARCHAR(10) NOT NULL;
ALTER TABLE dbo.ANQ_UserProfileAdditionalInfo ALTER COLUMN Nationality NVARCHAR(50) NOT NULL;
ALTER TABLE dbo.ANQ_UserProfileAdditionalInfo ALTER COLUMN IdNumber NVARCHAR(20) NOT NULL;
ALTER TABLE dbo.ANQ_UserProfileAdditionalInfo ALTER COLUMN Language NVARCHAR(20) NOT NULL;
ALTER TABLE dbo.ANQ_UserProfileAdditionalInfo ALTER COLUMN Ethnicity NVARCHAR(20) NOT NULL;
ALTER TABLE dbo.ANQ_UserProfileAdditionalInfo ALTER COLUMN BankName NVARCHAR(30) NULL;
ALTER TABLE dbo.ANQ_UserProfileAdditionalInfo ALTER COLUMN AccountHolder NVARCHAR(50) NULL;
ALTER TABLE dbo.ANQ_UserProfileAdditionalInfo ALTER COLUMN AccountNumber NVARCHAR(50) NULL;
ALTER TABLE dbo.ANQ_UserProfileAdditionalInfo ALTER COLUMN AccountType NVARCHAR(1) NULL;
ALTER TABLE dbo.ANQ_UserProfileAdditionalInfo DROP COLUMN ActivationDate;
ALTER TABLE dbo.ANQ_UserProfileAdditionalInfo ADD ActivationDate Datetime null