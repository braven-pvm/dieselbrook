Create proc sp_GetPickUpStoreById(@id int)  
as 
	select * from dbo.StorePickupPoint
	where Id=@id