---Altered Index: The IX_Order_CustomerId index was updated to include the columns OrderStatusId and Deleted. 
---These columns are included but not part of the key, meaning they are stored within the index to allow for covering queries, speeding up read performance for tak 626 pending order task 

-- Drop the existing index (if necessary, done automatically in SSMS when altering)
DROP INDEX [IX_Order_CustomerId] ON [dbo].[Order];

-- Recreate the index with additional columns included
CREATE NONCLUSTERED INDEX [IX_Order_CustomerId] ON [dbo].[Order]
(
    [CustomerId] ASC
)
INCLUDE([OrderStatusId], [Deleted])
WITH (
    PAD_INDEX = OFF, 
    STATISTICS_NORECOMPUTE = OFF, 
    SORT_IN_TEMPDB = OFF, 
    DROP_EXISTING = OFF, 
    ONLINE = OFF, 
    ALLOW_ROW_LOCKS = ON, 
    ALLOW_PAGE_LOCKS = ON, 
    OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF
) 
ON [PRIMARY];
