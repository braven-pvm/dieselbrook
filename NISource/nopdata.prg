
DEFINE CLASS NOPDATA AS busBase 
cDatabaseName=''
cSkipFieldsforUpdates="ID"
cPkField="ID"
laudit = .F.
auditexclude = "lastuser,dlastupdate"
oaudit = .NULL.
linternalerror = .F.
*lvalidateonsave = .T.



ENDDEFINE

&&---------------------------------------------------------------
&& Address
&&---------------------------------------------------------------
DEFINE CLASS Address AS NOPData
	cpkfield = "ID"
	calias = "Address"
	cfilename = "Address"
	ckeyfield = "ID"
	nDataMode = 2
	lCompareUpdates = .t.
	Name = "Address"
	
FUNCTION GetFullAddress (lnID)
LOCAL ln,lcSql

TEXT TO lcSql TEXTMERGE NOSHOW
SELECT a.*,c.Name Country,s.Abbreviation,S.Name State FROM ADDRESS a
 LEFT OUTER JOIN Country c ON a.CountryID=c.ID
 LEFT OUTER JOIN StateProvince s on s.ID = a.StateProvinceID
 WHERE a.ID=<<lnID>>
ENDTEXT

ln=THIS.Query(lcSql)
IF ln=0
	SCATTER NAME this.oData MEMO	BLANK
	RETURN .F.
ENDIF
SCATTER NAME this.oData MEMO	

ENDFUNC	
	
ENDDEFINE

&&---------------------------------------------------------------
&& CustomerAddresses
&&---------------------------------------------------------------
DEFINE CLASS CustomerAddresses AS NOPData
	cpkfield = ""
	calias = "CustomerAddresses"
	cfilename = "CustomerAddresses"
	ckeyfield = ""
	nDataMode = 2
	lCompareUpdates = .f.
	Name = "CustomerAddresses"

	
ENDDEFINE

&&---------------------------------------------------------------
&& Affiliate
&&---------------------------------------------------------------
DEFINE CLASS Affiliate AS NOPData
	cpkfield = "ID"
	calias = "Affiliate"
	cfilename = "Affiliate"
	ckeyfield = "ID"
	nDataMode = 2
	lCompareUpdates = .t.
	Name = "Affiliate"
	
ENDDEFINE

&&---------------------------------------------------------------
&& Orders
&&---------------------------------------------------------------
DEFINE CLASS Orders AS NOPData
	cpkfield = "ID"
	calias = "Orders"
	cfilename = "[Order]"
	ckeyfield = "ID"
	nDataMode = 2
	lCompareUpdates = .t.
	Name = "Orders"
	
ENDDEFINE

&&---------------------------------------------------------------
&& OrderItem
&&---------------------------------------------------------------
DEFINE CLASS OrderItem AS NOPData
	cpkfield = "ID"
	calias = "OrderItem"
	cfilename = "OrderItem"
	ckeyfield = "ID"
	nDataMode = 2
	lCompareUpdates = .t.
	Name = "OrderItem"
	
ENDDEFINE

&&---------------------------------------------------------------
&& OrderNote
&&---------------------------------------------------------------
DEFINE CLASS OrderNote AS NOPData
	cpkfield = "ID"
	calias = "OrderNote"
	cfilename = "OrderNote"
	ckeyfield = "ID"
	nDataMode = 2
	lCompareUpdates = .t.
	Name = "OrderNote"
	
ENDDEFINE
&&---------------------------------------------------------------
&& Shipment
&&---------------------------------------------------------------
DEFINE CLASS Shipment AS NOPData
	cpkfield = "ID"
	calias = "Shipment"
	cfilename = "Shipment"
	ckeyfield = "ID"
	nDataMode = 2
	lCompareUpdates = .t.
	Name = "Shipment"
	
ENDDEFINE

&&---------------------------------------------------------------
&& OrderDiscount
&&---------------------------------------------------------------
DEFINE CLASS OrderDiscount AS NOPData
	cpkfield = "ID"
	calias = "OrderDiscount"
	cfilename = "ANQ_Discount_Usage"
	ckeyfield = "ID"
	nDataMode = 2
	Name = "OrderDiscount"
	
FUNCTION Get_Discounts (lnOrderID)
	IF USED("tDiscount")
		USE IN tDiscount
	ENDIF
	RETURN THIS.EXECUTE("EXEC ANQ_GetOrderDiscounts "+CAST(lnOrderID AS VARCHAR(10)),"TDiscount")

ENDFUNC	
	
ENDDEFINE


&&---------------------------------------------------------------
&& Product
&&---------------------------------------------------------------
DEFINE CLASS Product AS NOPData
	cpkfield = "ID"
	calias = "Product"
	cfilename = "Product"
	ckeyfield = "ID"
	nDataMode = 2
	lCompareUpdates = .t.
	Name = "Product"
	
FUNCTION Load (lnpk, lnLookupType)
RETURN DODEFAULT(lnpk, lnLookupType)
ENDFUNC

FUNCTION LoadbySku(lcSku)
	RETURN This.LoadBase("sku='"+lcSku+"'")
ENDFUNC
	
ENDDEFINE

&&---------------------------------------------------------------
&& BackInStockSubscription
&&---------------------------------------------------------------
DEFINE CLASS BackInStockSubscription AS NOPData
	cpkfield = "ID"
	calias = "BackInStockSubscription"
	cfilename = "BackInStockSubscription"
	ckeyfield = "ID"
	nDataMode = 2
	lCompareUpdates = .f.
	Name = "BackInStockSubscription"
	

ENDDEFINE

&&---------------------------------------------------------------
&& Category
&&---------------------------------------------------------------
DEFINE CLASS Category AS NOPData
	cpkfield = "ID"
	calias = "Category"
	cfilename = "Category"
	ckeyfield = "ID"
	nDataMode = 2
	lCompareUpdates = .t.
	Name = "Category"
	

ENDDEFINE

&&---------------------------------------------------------------
&& Customer
&&---------------------------------------------------------------
DEFINE CLASS Customer AS NOPData
	cpkfield = "ID"
	calias = "Customer"
	cfilename = "Customer"
	ckeyfield = "ID"
	nDataMode = 2
	lCompareUpdates = .t.
	Name = "Customer"
	
FUNCTION VALIDATE
   
loCust = THIS.oData   
lcErrorMsg = ""


pcEmail=loCust.Email

&& Added more validation on email
oReg=CREATEOBJECT("WWREGEX")
IF ! oReg.TEST(pcEmail,"^[\w!#$%&'*+/=?`{|}~^-]+(?:\.[\w!#$%&'*+/=?`{|}~^-]+)*@?(?:[A-Z0-9-]+\.)+[A-Z]{2,6}$")
	this.AddValidationError("Invalid Email Address.","email")
	this.SetError( this.oValidationErrors.ToString() )
	RETURN .F.
ENDIF

IF LEN(JUSTEXT(pcEmail))>3
	this.AddValidationError("Bad Email","email")
	this.SetError( this.oValidationErrors.ToString() )
	RETURN .F.
ENDIF
lcSql="select 1 from customer where ( email=?pcEmail) AND id<>"+TRANSFORM(NVL(loCust.ID,0))
IF this.query(lcSql)>0
	this.AddValidationError("Already registered","email")
ENDIF

IF EMPTY(loCust.LastName) OR EMPTY(loCust.FirstName) 
   this.AddValidationError("Name incomplete","FirstName")
ENDIF

IF THIS.oValidationErrors.Count > 0
	this.SetError( this.oValidationErrors.ToString() )
	RETURN .F.
ENDIF

ENDFUNC	
	
ENDDEFINE

&&---------------------------------------------------------------
&& Customer
&&---------------------------------------------------------------
DEFINE CLASS NopCustomer AS NOPData
	cpkfield = "ID"
	calias = "Customer"
	cfilename = "Customer"
	ckeyfield = "ID"
	nDataMode = 2
	lCompareUpdates = .t.
	Name = "Customer"
	
&&---------------------------------------------------------------	
FUNCTION Load_Username(lcCustno)
&&---------------------------------------------------------------
	IF !THIS.LoadBase("UserName='"+lcCustno+"'")
		THIS.SetError("Customer Code not Found")
		RETURN .f.
	ELSE
		RETURN 
	ENDIF	

ENDFUNC		
&&---------------------------------------------------------------	
FUNCTION Get_CUSTOMERID(lcCustno)
&&---------------------------------------------------------------
	IF !THIS.LoadBase(lcCustno)
		THIS.SetError("Customer Code not Found")
		RETURN 0
	ELSE
		RETURN this.oData.CustomerID
	ENDIF	

ENDFUNC	
&&---------------------------------------------------------------
FUNCTION Get_CCUSTNO(ID)
&&---------------------------------------------------------------
	IF !THIS.Load(ID)
		THIS.SetError("Customer ID not Found")
		RETURN 0
	ELSE
		RETURN this.oData.UserName
	ENDIF	

ENDFUNC	

	
ENDDEFINE
&&---------------------------------------------------------------
&& Product_Category_Mapping
&&---------------------------------------------------------------
DEFINE CLASS Product_Category_Mapping AS NOPData
	cpkfield = "ID"
	calias = "Product_Category_Mapping"
	cfilename = "Product_Category_Mapping"
	ckeyfield = "ID"
	nDataMode = 2
	lCompareUpdates = .t.
	Name = "Product_Category_Mapping"
	
	
FUNCTION REMOVEDeleted
LPARAMETERS lID,lcIds
TEXT TO LCSQL TEXTMERGE NOSHOW
DELETE FROM Product_Category_Mapping WHERE ProductID=<<TRANSFORM(lID)>> 
	AND CategoryID NOT IN (<<lcIds>>)
ENDTEXT
luret=THIS.oSql.EXECUTENONQUERY(lcSql)
RETURN luRet
ENDFUNC		

ENDDEFINE

&&---------------------------------------------------------------
&& Product_Manufacturer_Mapping
&&---------------------------------------------------------------
DEFINE CLASS Product_Manufacturer_Mapping AS NOPData
	cpkfield = "ID"
	calias = "Product_Manufacturer_Mapping"
	cfilename = "Product_Manufacturer_Mapping"
	ckeyfield = "ID"
	nDataMode = 2
	lCompareUpdates = .t.
	Name = "Product_Manufacturer_Mapping"

	
ENDDEFINE

&&---------------------------------------------------------------
&& ANQ_CategoryIntegration
&&---------------------------------------------------------------
DEFINE CLASS ANQ_CategoryIntegration AS NOPData
	cpkfield = "ID"
	calias = "ANQ_CategoryIntegration"
	cfilename = "ANQ_CategoryIntegration"
	ckeyfield = "ID"
	nDataMode = 2
	lCompareUpdates = .t.
	Name = "ANQ_CategoryIntegration"
	

ENDDEFINE


&&---------------------------------------------------------------
&& ANQ_Lookups
&&---------------------------------------------------------------
DEFINE CLASS ANQ_Lookups AS NOPData
	cpkfield = "ID"
	calias = "ANQ_Lookups"
	cfilename = "ANQ_Lookups"
	ckeyfield = "ID"
	nDataMode = 2
	lCompareUpdates = .f.
	Name = "ANQ_Lookups"
	

ENDDEFINE

&&---------------------------------------------------------------
&& ANQ_ManufacturerIntegration
&&---------------------------------------------------------------
DEFINE CLASS ANQ_ManufacturerIntegration AS NOPData
	cpkfield = "ID"
	calias = "ANQ_ManufacturerIntegration"
	cfilename = "ANQ_ManufacturerIntegration"
	ckeyfield = "ID"
	nDataMode = 2
	lCompareUpdates = .t.
	Name = "ANQ_ManufacturerIntegration"
	

ENDDEFINE

&&---------------------------------------------------------------
&& ANQ_UserProfileAdditionalInfo
&&---------------------------------------------------------------
DEFINE CLASS ANQ_UserProfileAdditionalInfo AS NOPData
	cpkfield = "ID"
	calias = "ANQ_UserProfileAdditionalInfo"
	cfilename = "ANQ_UserProfileAdditionalInfo"
	ckeyfield = "ID"
	nDataMode = 2
	lCompareUpdates = .t.
	Name = "ANQ_UserProfileAdditionalInfo"
	

ENDDEFINE


&&---------------------------------------------------------------
&& ANQ_Events
&&---------------------------------------------------------------
DEFINE CLASS ANQ_Events AS NOPData
	cpkfield = "ID"
	calias = "ANQ_Events"
	cfilename = "ANQ_Events"
	ckeyfield = "ID"
	nDataMode = 2
	lCompareUpdates = .t.
	Name = "ANQ_Events"
	cSqlCursor="TEVENTS"

ENDDEFINE


&&---------------------------------------------------------------
&& ANQ_EventItems
&&---------------------------------------------------------------
DEFINE CLASS ANQ_EventItems AS NOPData
	cpkfield = "ID"
	calias = "ANQ_EventItems"
	cfilename = "ANQ_EventItems"
	ckeyfield = "ID"
	nDataMode = 2
	lCompareUpdates = .t.
	Name = "ANQ_EventItems"


ENDDEFINE

&&---------------------------------------------------------------
&& ANQ_Offers
&&---------------------------------------------------------------
DEFINE CLASS ANQ_Offers AS NOPData
	cpkfield = "ID"
	calias = "ANQ_Offers"
	cfilename = "ANQ_Offers"
	ckeyfield = "ID"
	nDataMode = 2
	lCompareUpdates = .t.
	Name = "ANQ_Offers"
	cSqlCursor="TOffers"




ENDDEFINE


&&---------------------------------------------------------------
&& ANQ_OfferList 
&&---------------------------------------------------------------
DEFINE CLASS ANQ_OfferList AS NOPData
	cpkfield = "ID"
	calias = "ANQ_OfferList "
	cfilename = "ANQ_OfferList "
	ckeyfield = "ID"
	nDataMode = 2
	lCompareUpdates = .t.
	Name = "ANQ_OfferList"


ENDDEFINE

&&---------------------------------------------------------------
&& ANQ_Discount_AppliedToCustomers
&&---------------------------------------------------------------
DEFINE CLASS ANQ_Discount_AppliedToCustomers AS NOPData
	cpkfield = "ID"
	calias = "ANQ_Discount_AppliedToCustomers"
	cfilename = "ANQ_Discount_AppliedToCustomers"
	ckeyfield = "ID"
	nDataMode = 2
	lCompareUpdates = .t.
	Name = "ANQ_Discount_AppliedToCustomers"

FUNCTION GetList
LPARAMETER lcFilter,lcCursor, lcFieldList,lnResultmode
LOCAL loRecord, lcPKField, lnResult

IF EMPTY(lcFieldList)
  lcFieldList = "*"
ENDIF
IF EMPTY(lnResultmode)
	lnResultMode=0
ENDIF	

THIS.SetError()


TEXT TO lcSQL TEXTMERGE NOSHOW
 SELECT R.id,ISNULL(c.username,'') cCustno, c.FirstName+' '+c.lastname Name,
	r.DiscountID,e.Name Voucher,r.isActive,r.StartDateUtc,r.EndDateUtc,r.NoTimesUsed,
	r.LimitationTimes
	FROM ANQ_Discount_AppliedToCustomers R 
	JOIN Discount e on r.DiscountID=e.id
	JOIN Customer c ON r.CustomerID=c.ID 
<<IIF(!EMPTY(lcFilter)," where ","") + lcFilter>>
ENDTEXT      

lcOldCursor = THIS.cSQLCursor 
THIS.cSQLCursor = IIF(!EMPTY(lcCursor),lcCursor,THIS.cSQLCursor)
lcCursor=THIS.cSQLCursor
lnResult = this.oSQL.Execute(lcSql,lcCursor)
IF lnResult < 0
    THIS.seterror(THIS.osql.cErrorMsg)
    RETURN .f.
ENDIF
THIS.cSQLCursor = lcOldCursor
lnResult = RECCOUNT()
*** Convert data if necessary
IF lnResultmode # 0
   THIS.ConvertData(lnResultmode,,lcCursor)   &&51 is json
ENDIF
RETURN 

ENDFUNC

ENDDEFINE


&&---------------------------------------------------------------
&& ANQ_Booking
&&---------------------------------------------------------------
DEFINE CLASS ANQ_Booking AS NOPData
	cpkfield = "ID"
	calias = "ANQ_Booking"
	cfilename = "ANQ_Booking"
	ckeyfield = "ID"
	nDataMode = 2
	lCompareUpdates = .t.
	Name = "ANQ_Booking"
	cSqlCursor="TBookings"

FUNCTION GetList
LPARAMETER lcFilter,lcCursor, lcFieldList,lnResultmode
LOCAL loRecord, lcPKField, lnResult

IF EMPTY(lcFieldList)
  lcFieldList = "*"
ENDIF
IF EMPTY(lnResultmode)
	lnResultMode=0
ENDIF	

THIS.SetError()


TEXT TO lcSQL TEXTMERGE NOSHOW
  SELECT R.id,ISNULL(f.username,'') cCustno,ISNULL(f.Company,r.Name) cCompany,CAST(R.DateBooked AS DATETIME) DateRegistered,R.OrderID,
  	ISNULL(R.cSono,'') cSono,ISNULL(R.cInvno,'') cInvno,
	R.IsPrimaryRegistrant,c.Username cSponsor,c.Company SponsorName,r.EventID,e.Name
	FROM ANQ_Booking R 
	JOIN ANQ_Events e on r.EventID=e.id
	JOIN Customer c ON r.CustomerID=c.ID 
	LEFT JOIN Customer f ON r.ConsultantCustomerID=f.id
<<IIF(!EMPTY(lcFilter)," where ","") + lcFilter>>
ENDTEXT      

lcOldCursor = THIS.cSQLCursor 
THIS.osql.cSQLCursor = IIF(!EMPTY(lcCursor),lcCursor,THIS.cSQLCursor)
lcCursor=THIS.cSQLCursor
lnResult = this.oSQL.Execute(lcSql,lcCursor)
IF lnResult < 0
    THIS.seterror(THIS.osql.cErrorMsg)
    RETURN 0
ENDIF
THIS.cSQLCursor = lcOldCursor
lnResult = RECCOUNT()
*** Convert data if necessary
IF lnResultmode # 0
   THIS.ConvertData(lnResultmode,,lcCursor)   &&51 is json
ENDIF
RETURN lnResult

ENDFUNC

FUNCTION Validate

this.SetError()
this.oValidationerrors.Clear()
loData = THIS.oData   
lcErrorMsg = ""

IF !loCust.LoadBase("UserName='"+loData.cSponsor+"'")
	this.AddValidationError("Sponsor not found","csponsor")
ELSE
	loData.Customerid=loCust.oData.id
ENDIF

IF !ISNULLOREMPTY(loData.cCustno)
	IF !loCust.LoadBase("UserName='"+loData.cCustno+"'")
		this.AddValidationError("Consultant not found","ccustno")
	ELSE
		loData.ConsultantCustomerid=loCust.oData.id
	ENDIF


	IF this.Query("select * from ANQ_Booking where eventid="+TRANSFORM(loData.EventID)+;
		" and ConsultantCustomerID="+TRANSFORM(loData.ConsultantCustomerid)+;
	  			 	"and name ='"+lodata.Name+"' and id<>"+TRANSFORM(loData.ID))>0
				   this.AddValidationError("Already booked.","ccustno")
	ENDIF
ENDIF
	

IF THIS.oValidationErrors.Count > 0
	this.SetError( this.oValidationErrors.ToString() )
	RETURN .F.
ENDIF


ENDFUNC



ENDDEFINE

&&---------------------------------------------------------------
&& ANQ_Booking
&&---------------------------------------------------------------
DEFINE CLASS NOP_DISCOUNT AS NOPData
	cpkfield = "ID"
	calias = "Discount"
	cfilename = "Discount"
	ckeyfield = "ID"
	nDataMode = 2
	lCompareUpdates = .t.
	Name = "NOP_Discount"
	cSqlCursor="TDiscount"

FUNCTION GetList
LPARAMETER lcFilter,lcCursor, lcFieldList,lnResultmode
LOCAL loRecord, lcPKField, lnResult

IF EMPTY(lcFieldList)
  lcFieldList = "*"
ENDIF
IF EMPTY(lnResultmode)
	lnResultMode=0
ENDIF	

THIS.SetError()


TEXT TO lcSQL TEXTMERGE NOSHOW
SELECT *	FROM Discount WHERE isActive=1
		and CouponCode is NOT NULL and  CouponCode<>''
ENDTEXT      

lcOldCursor = THIS.cSQLCursor 
THIS.osql.cSQLCursor = IIF(!EMPTY(lcCursor),lcCursor,THIS.cSQLCursor)
lcCursor=THIS.cSQLCursor
lnResult = this.oSQL.Execute(lcSql,lcCursor)
IF lnResult < 0
    THIS.seterror(THIS.osql.cErrorMsg)
    RETURN .f.
ENDIF
THIS.cSQLCursor = lcOldCursor
lnResult = RECCOUNT()
*** Convert data if necessary
IF lnResultmode # 0
   THIS.ConvertData(lnResultmode,,lcCursor)   &&51 is json
ENDIF
RETURN 

ENDFUNC



ENDDEFINE

