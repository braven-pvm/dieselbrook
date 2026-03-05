ALTER TABLE dbo.ANQ_UserProfileAdditionalInfo ALTER COLUMN WhatsappNumber VARCHAR(20);
ALTER TABLE dbo.Customer ALTER COLUMN Phone VARCHAR(20);

CREATE NONCLUSTERED INDEX IX_Customer_Phone
ON dbo.Customer(Phone)
INCLUDE (Id)
WHERE Phone IS NOT NULL;

CREATE NONCLUSTERED INDEX IX_ANQ_UserProfileAdditionalInfo_WhatsappNumber 
ON dbo.ANQ_UserProfileAdditionalInfo(WhatsappNumber)
INCLUDE (CustomerId)
WHERE WhatsappNumber IS NOT NULL;
