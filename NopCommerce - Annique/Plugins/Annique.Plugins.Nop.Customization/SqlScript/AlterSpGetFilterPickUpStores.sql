ALTER Procedure sp_GetFilterPickUpStoresPC
  @radius INT=10,@longitude Float,@latitude Float,@PostCode varChar(50)=''
AS
	select TOP 30 
	Kms=CASE WHEN a.ZipPostalCode=@PostCode then 0
	ELSE
	ROUND ( 3959 * acos( cos( radians(@latitude) ) * cos( radians( latitude ) ) * cos( radians( Longitude)
	 - radians(@longitude) ) + sin( radians(@latitude) ) * sin( radians( latitude ) ) ) ,1)*8/5 End ,
	 p.* from dbo.StorePickupPoint p JOIN Address a ON p.AddressId=a.id
	 where a.ZipPostalCode=@PostCode OR (
	 (latitude!=0.00 and Longitude!=0.00)
	and ( 3959 * acos( cos( radians(@latitude) ) * cos( radians( latitude ) ) * cos( radians( Longitude)
	 - radians(@longitude) ) + sin( radians(@latitude) ) * sin( radians( latitude ) ) ) )
	 < @radius  )
	 OR (p.name like '%'+@PostCode+'%') OR (a.city like '%'+@PostCode+'%')
	 order by Kms