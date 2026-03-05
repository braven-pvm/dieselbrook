#DEFINE ERROR_LOG     "Synchlog"+DTOS(DATE())+".log"

************************************************************************
*PROCEDURE apiProcess
****************************
***  Function: Processes incoming Web Requests for apiProcess
***            requests. This function is called from the wwServer 
***            process.
***      Pass: loServer -   wwServer object reference
*************************************************************************
LPARAMETER loServer
LOCAL loProcess
PRIVATE Request, Response, Server, Session, Process
STORE NULL TO Request, Response, Server, Session, Process



#INCLUDE WCONNECT.H

loProcess = CREATEOBJECT("apiProcess", loServer)
loProcess.lShowRequestData = loServer.lShowRequestData

IF VARTYPE(loProcess)#"O"
   *** All we can do is return...
   RETURN .F.
ENDIF

*** Call the Process Method that handles the request
loProcess.Process()

*** Explicitly force process class to release
loProcess.Dispose()

RETURN

*************************************************************
DEFINE CLASS apiProcess AS WWC_RESTPROCESS
*************************************************************

*** Response class used - override as needed
cResponseClass = [WWC_PAGERESPONSE]

*** Default for page script processing if no method exists
*** 1 - MVC Template (ExpandTemplate()) 
*** 2 - Web Control Framework Pages
*** 3 - MVC Script (ExpandScript())
nPageScriptMode = 3
cerrormsg=""
lerror=1
*!* cAuthenticationMode = "UserSecurity"  && `Basic` is default


*** ADD PROCESS CLASS EXTENSIONS ABOVE - DO NOT MOVE THIS LINE ***


#IF .F.
* Intellisense for THIS
LOCAL THIS as apiProcess OF apiProcess.prg
#ENDIF
 
*********************************************************************
* Function apiProcess :: OnProcessInit
************************************
*** If you need to hook up generic functionality that occurs on
*** every hit against this process class , implement this method.
*********************************************************************
FUNCTION OnProcessInit

Response.Encoding = "UTF8"
Request.lUtf8Encoding = .T.


*** Add CORS header to allow cross-site access from other domains/mobile devices on Ajax calls
*Response.AppendHeader("Access-Control-Allow-Origin","https://stage.annique.com")
Response.AppendHeader("Access-Control-Allow-Origin","*")
*wResponse.AppendHeader("Access-Control-Allow-Origin",Request.ServerVariables("HTTP_ORIGIN"))
Response.AppendHeader("Access-Control-Allow-Methods","POST, GET, DELETE, PUT, OPTIONS")
Response.AppendHeader("Access-Control-Allow-Headers","Content-Type, *")
*!* *** Allow cookies and auth headers
Response.AppendHeader("Access-Control-Allow-Credentials","true")
 
 *** CORS headers are requested with OPTION by XHR clients. OPTIONS returns no content
lcVerb = Request.GetHttpVerb()
IF (lcVerb == "OPTIONS")
   *** Just exit with CORS headers set
   *** Required to make CORS work from Mobile devices
   RETURN .F.
ENDIF   


RETURN .T.
ENDFUNC


FUNCTION OnBeforeCallMethod
return

	lcVerb = Request.GetHttpVerb()
  	lcParms = Request.GetRawFormData()
	lcContentType =  LOWER(Request.ServerVariables("CONTENT_TYPE"))    && application/json; charset=utf-8
	lcAcceptType = LOWER(Request.ServerVariables("HTTP_ACCEPT"))	   && application/json

	=LOGSTRING(lcVerb+" "+REquest.GetBrowser())
	=LOGSTRING(lcContentType,"SKYNETTEST.LOG")
	=LOGSTRING( lcAcceptType,"SKYNETTEST.LOG")
	=LOGSTRING("form data:"+lcParms,"SKYNETTEST.LOG")

ENDFUNC

************************************************************************
*  SetError
****************************************
***  Function: Sets the error message on the object
***    Assume:
***      Pass:
***    Return:
************************************************************************
FUNCTION SetError(lcErrorMsg, lnError)

IF PCOUNT() = 0
    THIS.cerrormsg = ""
    THIS.lerror = .F.
    RETURN
ENDIF

THIS.cerrormsg = lcErrorMsg
THIS.lerror = .T.
=LOGSTRING(this.cerrormsg,ERROR_LOG)
ENDFUNC
*   SetError
*********************************************************************
FUNCTION TestPage
***********************
LPARAMETERS lvParm
*** Any posted JSON string is automatically deserialized
*** into a FoxPro object or value

#IF .F. 
* Intellisense for intrinsic objects
LOCAL Request as wwRequest, Response as wwPageResponse, Server as wwServer, ;
      Process as wwProcess, Session as wwSession
#ENDIF

*** Simply create objects, collections, values and return them
*** they are automatically serialized to JSON
loObject = CREATEOBJECT("EMPTY")
ADDPROPERTY(loObject,"name","TestPage")
ADDPROPERTY(loObject,"description",;
            "This is a JSON API method that returns an object nnn.")
ADDPROPERTY(loObject,"entered",DATETIME())

*** To get proper case you have to override property names
*** otherwise all properties are serialized as lower case in JSON
Serializer.PropertyNameOverrides = "Name,Description,Entered"


RETURN loObject



ENDFUNC

*********************************************************************
FUNCTION HelloScript()
***********************

SELECT TOP 10 time, script, querystr, verb, remoteaddr ;
  FROM wwRequestLog  ;
  INTO CURSOR TRequests ;
  ORDER BY Time Desc

loObj = CREATEOBJECT("EMPTY")

*** Simple Properties
ADDPROPERTY(loObj,"message","Surprise!!! This is not a script response! Instead we'll return you a cursor as a JSON result.")
ADDPROPERTY(loObj,"requestName","Recent Requests")
ADDPROPERTY(loObj,"recordCount",_Tally)

*** Nested Cursor Result as an Array
ADDPROPERTY(loObj,"recentRequests","cursor:TRequests")

*** Normalize property names for case sensitivity
Serializer.PropertyNameOverrides = "requestName,recentRequests,recordCount"

RETURN loObj
ENDFUNC




&&-----------------------------------------------------------------------------
FUNCTION DBConnect(lcSqlConnect,loSql)
&&-----------------------------------------------------------------------------
IF !loSql.CONNECT(lcSqlConnect)
	&&	THIS.SetError(loSql.cErrorMsg)
	=LOGSTRING(loSql.cErrorMsg,ERROR_LOG)
	RETURN
ENDIF
ENDFUNC

&&-----------------------------------------------------------------------------
FUNCTION SyncOrders
&&-----------------------------------------------------------------------------
PRIVATE oAmSql,oNopSql,oWsSql,oNop


=LOGSTRING("Sync Orders Start",ERROR_LOG)

oAMSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cAMSqlconnectstring,oAmSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	
oNopSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cNopSqlconnectstring,oNopSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	
oNopSql.EnableUnicodeToAnsiMapping()
oWSSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cWSSqlconnectstring,oWSSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	


=X8SETPRC("SyncOrders.PRG")

oSql=CREATEOBJECT("wwSQL")
	IF !this.DBConnect(this.oConfig.cSqlconnectstring,oSql)
		=LOGSTRING(this.cerrormsg,ERROR_LOG)
		RETURN .F.
	ENDIF	

oSync=CREATEOBJECT("SyncOrder")
IF !oSync.SetUp(this.oConfig.cAPIUrl)
	=LOGSTRING("Orders "+oSync.cErrormsg,ERROR_LOG)
	RETURN .f.
ENDIF	

IF  !oSync.SyncAll()
	=LOGSTRING("Orders "+oSync.cErrormsg,ERROR_LOG)
	Response.Status = "400 "+oSync.cErrorMsg
	Response.Write("false")
	Response.End()
	RETURN
ELSE
	=LOGSTRING("Sync Orders Success",ERROR_LOG)
ENDIF	

ENDFUNC


&&-----------------------------------------------------------------------------
FUNCTION SyncCancelOrders
&&-----------------------------------------------------------------------------
PRIVATE oAmSql,oNopSql,oWsSql,oNop
=LOGSTRING("Sync Caancel Orders Start",ERROR_LOG)

oAMSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cAMSqlconnectstring,oAmSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF
oNopSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cNopSqlconnectstring,oNopSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	
oNopSql.EnableUnicodeToAnsiMapping()

=X8SETPRC("SyncOrders.PRG")
oSql=CREATEOBJECT("wwSQL")
	IF !this.DBConnect(this.oConfig.cSqlconnectstring,oSql)
		=LOGSTRING(this.cerrormsg,ERROR_LOG)
		RETURN .F.
	ENDIF	


oSync=CREATEOBJECT("SyncOrder")
IF !oSync.SetUp(this.oConfig.cAPIUrl)
	=LOGSTRING("Orders "+oSync.cErrormsg,ERROR_LOG)
	RETURN .f.
ENDIF	

IF  !oSync.CancelOrders()
	=LOGSTRING("Orders Cancelled "+oSync.cErrormsg,ERROR_LOG)
	Response.Status = "400 "+oSync.cErrorMsg
	Response.Write("false")
	Response.End()
	RETURN
ELSE
	=LOGSTRING("Sync Cancel Orders Success",ERROR_LOG)
ENDIF	

ENDFUNC

&&-----------------------------------------------------------------------------
FUNCTION SyncOrderStatus (lnOrderID)
&&-----------------------------------------------------------------------------

PRIVATE oAmSql,oNopSql,oNop

=LOGSTRING("Sync Order Status Start",ERROR_LOG)

oAMSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cAMSqlconnectstring,oAmSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	
oNopSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cNopSqlconnectstring,oNopSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	
oNopSql.EnableUnicodeToAnsiMapping()
=X8SETPRC("SyncOrderStatus.PRG")
oSql=CREATEOBJECT("wwSQL")
	IF !this.DBConnect(this.oConfig.cSqlconnectstring,oSql)
		=LOGSTRING(this.cerrormsg,ERROR_LOG)
		RETURN .F.
	ENDIF	
oSync=CREATEOBJECT("SyncOrderStatus")
IF !oSync.SetUp(this.oConfig.cAPIUrl)
	=LOGSTRING(oSync.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	

IF !ISNULLOREMPTY(lnOrderID)
	luret=oSync.SyncOne(lnOrderID)
ELSE
	luret=oSync.SyncAll()
ENDIF

IF !luRet	
	=LOGSTRING("Orders Status "+oSync.cErrormsg,ERROR_LOG)
	Response.Status = "400 "+oSync.cErrorMsg
	Response.Write("false")
	Response.End()
	RETURN
	

ELSE
	=LOGSTRING("Sync Orders Status Success",ERROR_LOG)
	
	
	IF !ISNULLOREMPTY(lnOrderID)
		lnRet=oSql.Execute("EXEC sp_NOP_rep_OrderStatusBO @OrderID="+TRANSFORM(lnOrderID),"TRET")
		IF lnRet=3
			RETURN "cursor:Tret1"
		ENDI
	ENDIF
	
	
	
ENDIF	

ENDFUNC

&&-----------------------------------------------------------------------------
FUNCTION SyncProducts
&&-----------------------------------------------------------------------------
PRIVATE oAmSql,oNopSql,oNop
oNop=.f.

oSer=CREATEOBJECT("wwJsonSerializer")

oAMSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cAMSqlconnectstring,oAmSql)

	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	
oNopSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cNopSqlconnectstring,oNopSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	
oNopSql.EnableUnicodeToAnsiMapping()
=X8SETPRC("SyncProducts.PRG")
oSql=CREATEOBJECT("wwSQL")
	IF !this.DBConnect(this.oConfig.cSqlconnectstring,oSql)
		=LOGSTRING(this.cerrormsg,ERROR_LOG)
		RETURN .F.
	ENDIF	
oSync=CREATEOBJECT("SyncProducts")
IF !oSync.SetUp(this.oConfig.cAPIUrl)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	

lcSku=Request.params("sku")
lcType=Request.params("type")
=LOGSTRING("Sync Products Start "+lcType+" "+lcSku,ERROR_LOG)
llerror=.f.
DO CASE 
	CASE !EMPTY(lcSku)
		lcType="SKU"
		IF !oSync.SyncOne(lcSku)
			llError=.t.
		ENDIF
	CASE LOWER(lcType)=="changes"
		IF !oSync.SyncChanges()
			llError=.t.
		ENDIF
	CASE LOWER(lcType)=="all"
		IF !oSync.SyncAll()
			llError=.t.
		ENDIF
	CASE LOWER(lcType)=="availability"
		IF !oSync.SyncAvailability()
			llError=.t.
		ENDIF	

ENDCASE
=LOGSTRING("Sync Products END",ERROR_LOG)
IF llError
	=LOGSTRING(lcType+" "+oSync.cErrormsg,ERROR_LOG)
	Response.Status = "400 "+oSync.cErrorMsg
	Response.Write("false")
	Response.End()
	RETURN
ENDIF

	Response.Write("true")
	Response.End()
	RETURN
	
ENDFUNC


&&-----------------------------------------------------------------------------
FUNCTION referrals (oParms)
&&-----------------------------------------------------------------------------
IF VARTYPE(oParms)<>"O"
	RETURN
ENDIF	
PRIVATE oNopSql
poError = CREATEOBJECT("HtmlErrorDisplayConfig")
IF VARTYPE(oParms)<>"O"
	THIS.ErrorResponse("Invalid Parameter Object", "400 Bad Request")
	RETURN
ENDIF

oSettings=CREATE("EMPTY")
loSettings = CREATEOBJECT("NopSettings")
loSettings.SetSqlObject(Server.oSql)
loSettings.LoadSettings(oSettings)
lNewReg=oSettings.common.newReg

WITH oParms.params
TEXT TO lcSQL TEXTMERGE NOSHOW
EXEC sp_NOP_rep_referralsPaged @cCustno     = '<<NVL(.sponsor,'')>>'
,                              @StoreID     = 1

,

-- NEW: paging
                               @PageNumber  = <<.PageNumber>>
,                              @PageSize    = <<.PageSize>>
,

-- NEW: filters (optional)
                               @CreatedFrom = << IIF(ISNULL(.CreatedFrom),NULL,DTOC(.CreatedFrom ))>>
,                              @CreatedTo   = << IIF(ISNULL(.Createdto),NULL,DTOC(.Createdto) )>>
,                              @Status      = <<NVL(.Status,NULL) >>
,                              @Search      = <<.Search >>

ENDTEXT
ENDWITH
lcSql=STRTRAN(lcSql, '.NULL.','NULL')
IF Server.oSql.Execute(lcSql,"Treferral")<>3
	THIS.ErrorResponse("Invalid Parameter Report", "400 Bad Request")
	RETURN 
ENDIF
oData=CREATEOBJECT("EMPTY")
ADDPROPERTY(oData,"totalRows",tReferral2.TotalRows)
ADDPROPERTY(oData,"items",cursortocollection("Treferral1"))
RETURN oData

ENDFUNC

&&-----------------------------------------------------------------------------
FUNCTION Updatereferral (oParms)
&&-----------------------------------------------------------------------------
PRIVATE oAmSql,oNopSql,oNop,oSer,oWsSql
poError = CREATEOBJECT("HtmlErrorDisplayConfig")
IF VARTYPE(oParms)<>"O"
	THIS.ErrorResponse("Invalid Parameter Object", "400 Bad Request")
	RETURN
ENDIF

oSettings=CREATE("EMPTY")
loSettings = CREATEOBJECT("NopSettings")
loSettings.SetSqlObject(Server.oSql)
loSettings.LoadSettings(oSettings)
lNewReg=oSettings.common.newReg


oSer=CREATEOBJECT("wwJsonSerializer")
oXml=CREATEOBJECT("WWXML")
=LOGSTRING(oSer.Serialize(oParms),"NopReport.log")
oAMSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cAMSqlconnectstring,oAmSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	

oNopSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cNopSqlconnectstring,oNopSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	


lnStoreID=VAL(oParms.params.StoreID)
IF EMPTY(lnStoreID)
	lnStoreID=1
ENDIF

	oWSSql=CREATEOBJECT("wwSQL")
	IF !this.DBConnect(this.oConfig.cWSSqlconnectstring,oWSSql)
		=LOGSTRING(this.cerrormsg,ERROR_LOG)
		RETURN .F.
	ENDIF
IF !lNewReg
	
	oBus=CREATEOBJECT("NewRegistration")
	oBus.SetSqlObject(oWsSql)

ELSE 
	oBus=NEWOBJECT("ANQ_NewRegistrations","NopNewRegistrations.prg")
	oBus.SetSqlObject(oNopSql)
ENDIF

IF ISNULLOREMPTY(oParms.Data.id)
	oBus.New()
	ADDPROPERTY(oParms.Data,"createdby","SPONSOR")
ELSE
	IF !oBus.Load(oParms.Data.id)
		THIS.ErrorResponse("Could not find Record", "400 Bad Request")
		RETURN
	ENDIF
ENDIF	
=COPYOBJECTPROPERTIES(oParms.Data,oBus.oData,2,,"ID")
IF ISNULLOREMPTY(oBus.oData.csponsor)
	oBus.oData.csponsor=oParms.params.UserName
ENDIF	
	IF !oBus.Validate()
		poError.Errors.AddErrors(oBus.oValidationErrors )
	ENDIF
	IF (poError.Errors.COUNT > 0)
		poError.MESSAGE = poError.Errors.ToHtml()
		poError.HEADER = "Please fix the following form entry errors"
		RETURN poError
	ENDIF

	IF !oBus.SAVE()
		poError.Errors.AddError("Could not save Referral")
		RETURN poError
	ENDIF
	
	&& Check if Status is 
	IF oBus.oData.Status="READY"
		oAMSql=CREATEOBJECT("wwSQL")
		IF !this.DBConnect(this.oConfig.cAMSqlconnectstring,oAmSql)
			=LOGSTRING(this.cerrormsg,ERROR_LOG)
			RETURN .F.
		ENDIF	
		IF !oBus.CreateFromReferral(oBus)
			oBus.oData.Status="PENDING"
		ENDIF	
		IF !oBus.SAVE() OR oBus.oData.Status="PENDING"
			poError.Errors.AddError("Could not activate Referral")
			RETURN poError
		ENDIF
		
	ENDIF
	
	
	IF oBus.oData.Status='NOTINTERESTED'
		oBus.Delete(oBus.oData.Id)
	ENDIF	

IF !lNewReg
	
	TEXT TO lcSql TEXTMERGE NOSHOW
EXEC NopIntegration..sp_NOP_rep_referrals @cCustno='<<oParms.params.UserName>>',@StoreID=<<lnStoreID>>,
	@ID=<<oBus.oData.ID>>
ENDTEXT

ELSE

	TEXT TO lcSql TEXTMERGE NOSHOW
EXEC NopIntegration..sp_NOP_rep_referralsNew @cCustno='<<oParms.params.UserName>>',@StoreID=<<lnStoreID>>,
	@ID=<<oBus.oData.ID>>

ENDTEXT
ENDIF

IF oAMsql.Execute(lcSql,"Treferral")#3
	THIS.ErrorResponse("Record Not Found",  "400 Record Not Found")
	RETURN .F.
ENDIF
SELECT Treferral1
SCATTER NAME  oData	MEMO
RETURN oData

ENDFUNC

&&-----------------------------------------------------------------------------
FUNCTION Updatereferral_CheckSponsor (oParms)
&&-----------------------------------------------------------------------------
PRIVATE oAmSql,oNopSql,oNop,oSer,oWsSql
poError = CREATEOBJECT("HtmlErrorDisplayConfig")
IF VARTYPE(oParms)<>"O"
	THIS.ErrorResponse("Invalid Parameter Object", "400 Bad Request")
	RETURN
ENDIF

oAMSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cAMSqlconnectstring,oAmSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	

TEXT TO lcSql NOSHOW TEXTMERGE
SELECT a.cCustno,a.cStatus [STATUS],RTRIM(a.cCompany)+IIF(d.ccustno is not null,'',' NOT IN YOUR DOWNLINE') Name,
CAST(IIF(d.ccustno is not null,1,0) as bit) inDownLine,CAST(1 as bit) isValid
FROM Arcust a
LEFT JOIN 
CompPlanLive.dbo.fn_Get_DownlineHist('<<oParms.params.username>>',<<YEAR(DATE())>>,<<MONTH(DATE())>>) d
ON a.ccustno=d.ccustno
WHERE a.ccustno='<<oParms.data.csponsor>>' AND a.cStatus='A'
ENDTEXT
IF USED('cDownline')
	USE IN cDownline
ENDIF	
luret=oAmsql.EXECUTE(lcSql,"cDownline")
IF luRet<>1
		poError.Errors.AddError("Could not verify sponsor")
		RETURN poError
ENDIF
SELECT cDownline
SCATTER NAME loSpon MEMO
IF RECCOUNT()=0
	loSpon.Status="Consultant "+oParms.data.csponsor+" does not exist"
	loSpon.isValid=.f.
ENDIF	
RETURN loSpon

ENDFUNC


&&-----------------------------------------------------------------------------
FUNCTION UpdateClient (oParms)
&&-----------------------------------------------------------------------------
PRIVATE oAmSql,oNopSql,oNop,oSer,oWsSql

poError = CREATEOBJECT("HtmlErrorDisplayConfig")
IF VARTYPE(oParms)<>"O"
	THIS.ErrorResponse("Invalid Parameter Object", "400 Bad Request")
	RETURN
ENDIF
oSer=CREATEOBJECT("wwJsonSerializer")
oXml=CREATEOBJECT("WWXML")
=LOGSTRING(oSer.Serialize(oParms),"NopReport.log")

oAMSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cAMSqlconnectstring,oAmSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	

oNopSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cNopSqlconnectstring,oNopSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	
oSql=CREATEOBJECT("wwSQL")
	IF !this.DBConnect(this.oConfig.cSqlconnectstring,oSql)
		=LOGSTRING(this.cerrormsg,ERROR_LOG)
		RETURN .F.
	ENDIF	
oNop=CREATEOBJECT("SyncClass")
IF !oNop.SetUp(this.oConfig.cAPIUrl)  && Store ID
	=LOGSTRING(oNop.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	

lnStoreID=VAL(oParms.params.StoreID)
IF EMPTY(lnStoreID)
	lnStoreID=1
ENDIF

oAff=CREATEOBJECT("Affiliate")
oAff.SetSqlObject(oNopSql)

IF oAff.Query("select id from Affiliate where FriendlyUrlName='"+oParms.params.username+"'","TAFF")=0 AND !oAff.lError
	oAff.ExecuteNonQuery("EXEC ANQ_SyncAffiliateCustomer @cCustno='"+oParms.params.username+"'")
ENDIF
IF oAff.lError
	poError.Errors.AddError("Could not locate your affiliate Account")
	RETURN poError
ENDIF	
IF !(USED("TAFF") OR RECCOUNT("TAFF")=0) ;
	AND oAff.Query("select id from Affiliate where FriendlyUrlName='"+oParms.params.username+"'","TAFF")=0  OR oAff.lError
	poError.Errors.AddError("Could not locate your affiliate Account")
	RETURN poError
ENDIF	

oBus=CREATEOBJECT("Customer")
oBus.SetSqlObject(oNopSql)

oBus.New()
=COPYOBJECTPROPERTIES(oParms.Data,oBus.oData,2,,"ID")
oBus.oData.AffiliateID=TAff.ID

IF !oBus.Validate()
	poError.Errors.AddErrors(oBus.oValidationErrors )
ENDIF
IF (poError.Errors.COUNT > 0)
	poError.MESSAGE = poError.Errors.ToHtml()
	poError.HEADER = "Please fix the following form entry errors"
	RETURN poError
ENDIF

WITH oBus.oData

			TEXT TO lcJson TEXTMERGE NOSHOW
{
  "customer_guid": "<<x8guid(36)>>",
  "username": "<<ALLTRIM(.Email)>>",
  "email": "<<ALLTRIM(.Email)>>",
  "active": true,
  "deleted": false,
  "first_name": "<<oxml.EncodeXML(ALLTRIM(.Firstname))>>",
  "last_name": "<<oxml.EncodeXML(ALLTRIM(.LastName))>>",
  "phone": "<<.Phone>>",
  "fax": "<<oParms.params.username>>",
   "affiliate_id": <<.affiliateID>>,
	"registered_in_store_id": 1,	"created_on_utc" : '<<TOISODATESTRING(DATETIME(),.t.,.t.)>>',
     "id": 0
}
ENDTEXT

luret=oNop.Customer_Create(lcJson)
IF VARTYPE(luret)<>"O"
	=LOGSTRING("Failed  Add Client :"+oNop.cErrormsg,"Consultants.Log")
	poError.Errors.AddError("Could not add client")
	RETURN poError
ENDIF	
lnCustomerID=luret.ID
lcPassword=ALLTRIM(.Email)
luret=oNop.Customer_SetPassword(lnCustomerID,lcPassword)
luret=oNop.Customer_AddRole(lnCustomerID,"Registered")

ENDWITH

&& Send the email here
&& 
&& 
	
TEXT TO lcSql TEXTMERGE NOSHOW
EXEC NopIntegration..sp_NOP_rep_shopclients @cCustno='<<oParms.params.UserName>>',@ActiveOnly=0
ENDTEXT

IF oAMsql.Execute(lcSql,"Treferral")#3
	THIS.ErrorResponse("Record Not Found",  "400 Record Not Found")
	RETURN .F.
ENDIF
SELECT Treferral1
SCATTER NAME  oData	MEMO
RETURN oData

ENDFUNC


&&-----------------------------------------------------------------------------
FUNCTION UpateOrderStatusDelivered (oParms)
&&-----------------------------------------------------------------------------
PRIVATE oAmSql,oNopSql,oNop,oSer,oWsSql
poError = CREATEOBJECT("HtmlErrorDisplayConfig")
IF VARTYPE(oParms)<>"O"
	THIS.ErrorResponse("Invalid Parameter Object", "400 Bad Request")
	RETURN
ENDIF

oNop=CREATEOBJECT("SyncClass")
IF !oNop.SetUp(this.oConfig.cAPIUrl)  && Store ID
	=LOGSTRING(oNop.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	


oNopSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cNopSqlconnectstring,oNopSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF

oOrd=CREATEOBJECT("Orders")
oOrd.SetSqlObject(oNopSql)
IF !oOrd.Load(oParms.id)
	poError.Errors.AddError("Order not found")
	RETURN poError
ENDIF



oShip=CREATEOBJECT("Shipment")
oShip.SetSqlObject(oNopSql)
IF !oShip.Load(oParms.ShipMentID)
	poError.Errors.AddError("Shipment not found")
	RETURN poError
ENDIF
IF oShip.oData.OrderID<>	oParms.id
	poError.Errors.AddError("Shipment mismatch")
	RETURN poError
ENDIF	

DO CASE
	CASE	oOrd.oData.ShippingStatusID=40
	
	
	CASE	oOrd.oData.ShippingStatusID=30
*!*			TEXT TO lcSql  TEXTMERGE NOSHOW
*!*			SELECT ShippedDateUtc from Shipment WHERE ID=<<TRANSFORM(loTorder.ShipMentID)>> and 
*!*			 ShippedDateUtc IS NOT NULL
*!*			ENDTEXT	
		luret=oNop.ShipMent_Deliver(oShip.oData.id)

	CASE oOrd.oData.ShippingStatusID=20
		luret=oNop.ShipMent_Send(oShip.oData.id)
		luret=oNop.ShipMent_Deliver(oShip.oData.id)

ENDCASE


RETURN This.SyncOrderStatus (oParms.id)
ENDFUNC



&&-----------------------------------------------------------------------------
FUNCTION UpateOrderStatusTracking (oParms)
&&-----------------------------------------------------------------------------
PRIVATE oAmSql,oNopSql,oNop,oSer,oWsSql
poError = CREATEOBJECT("HtmlErrorDisplayConfig")
IF VARTYPE(oParms)<>"O"
	THIS.ErrorResponse("Invalid Parameter Object", "400 Bad Request")
	RETURN
ENDIF

IF ISNULLOREMPTY(oParms.trackingnumber)
	poError.Errors.AddError("Tracking number cannot be blank")
	RETURN poError
	
ENDIF
oSer=CREATEOBJECT("wwJsonSerializer")
oXml=CREATEOBJECT("WWXML")

oAMSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cAMSqlconnectstring,oAmSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	

oWSSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cWSSqlconnectstring,oWSSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	




oNopSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cNopSqlconnectstring,oNopSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	


oShip=CREATEOBJECT("Shipment")
oShip.SetSqlObject(oNopSql)
IF !oShip.Load(oParms.ShipMentID)
	poError.Errors.AddError("Shipment not found")
	RETURN poError
ENDIF
IF oShip.oData.OrderID<>	oParms.id
	poError.Errors.AddError("Shipment mismatch")
	RETURN poError
ENDIF	

oShip.oData.trackingnumber=oParms.trackingnumber
IF !oShip.Save()
	THIS.ErrorResponse(oShip.cErrorMsg,"400 Could not save" )
	RETURN
ENDIF
IF oParms.cshipvia='FASTWAY'

TEXT TO lcSql TEXTMERGE NOSHOW
EXEC sp_Fastway_AddManualConsignment @cConsignmentID=<<oParms.invoiceno>>,@cLabelNo='<<oParms.trackingnumber>>'
ENDTEXT

IF !oAmSql.EXECUTENONQUERY(lcSql)
	THIS.ErrorResponse(oShip.cErrorMsg,"400 Could not Update Fastway Consignment" )
	RETURN
ENDIF

ENDIF


RETURN This.SyncOrderStatus (oParms.id)
ENDFUNC

&&-----------------------------------------------------------------------------
FUNCTION SyncConsultant
&&-----------------------------------------------------------------------------
PRIVATE oAmSql,oNopSql,oNop,oSer,oWsSql


lcCustno=Request.params("custno")
lcStoreID=Request.params("storeID")
Welcome=UPPER(Request.params("welcome"))

IF EMPTY(lcCustno)
	RETURN .F.
ENDIF
lnStoreID=VAL(lcStoreID)
IF EMPTY(lnStoreID)
	lnStoreID=1
ENDIF
oSer=CREATEOBJECT("wwJsonSerializer")
oXml=CREATEOBJECT("WWXML")
oNop=.f.
=LOGSTRING("Sync Consultant "+lcCustno,ERROR_LOG)

oAMSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cAMSqlconnectstring,oAmSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	

oWSSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cWSSqlconnectstring,oWSSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	

oNopSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cNopSqlconnectstring,oNopSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	
oNopSql.EnableUnicodeToAnsiMapping()
=X8SETPRC("SyncConSultant.PRG")
oSql=CREATEOBJECT("wwSQL")
	IF !this.DBConnect(this.oConfig.cSqlconnectstring,oSql)
		=LOGSTRING(this.cerrormsg,ERROR_LOG)
		RETURN .F.
	ENDIF	
oSync=CREATEOBJECT("SyncConsultants")
IF !oSync.SetUp(this.oConfig.cAPIUrl,1)  && Store ID
	=LOGSTRING(oSync.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	

IF Welcome='YES'
	loArcust = CREATEOBJECT("arcust")
	loArcust.Setsqlobject(oAmsql)
	IF loArCust.Load(lcCustno)
		oSync.SendWelcomeMail(loArcust)
	ENDIF
	RETURN
ENDIF	

IF  !oSync.SyncOne( lcCustno)
	=LOGSTRING("Sync Consultant "+oSync.cErrormsg,ERROR_LOG)
	Response.Status = "400 "+oSync.cErrorMsg
	Response.Write("false")
	Response.End()
	RETURN
ELSE
	=LOGSTRING("Sync Consultant Done",ERROR_LOG)
ENDIF	

ENDFUNC


&&-----------------------------------------------------------------------------
FUNCTION SyncStaff
&&-----------------------------------------------------------------------------
PRIVATE oAmSql,oNopSql,oNop,oSer

oSer=CREATEOBJECT("wwJsonSerializer")
oNop=.f.
=LOGSTRING("Sync Staff Start",ERROR_LOG)

oAMSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cAMSqlconnectstring,oAmSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	
oNopSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cNopSqlconnectstring,oNopSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	
oNopSql.EnableUnicodeToAnsiMapping()
=X8SETPRC("SyncStaff.PRG")
oSql=CREATEOBJECT("wwSQL")
	IF !this.DBConnect(this.oConfig.cSqlconnectstring,oSql)
		=LOGSTRING(this.cerrormsg,ERROR_LOG)
		RETURN .F.
	ENDIF	
oSync=CREATEOBJECT("SyncStaff")
IF !oSync.SetUp(this.oConfig.cAPIUrl)
	=LOGSTRING(oSync.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	
IF  !oSync.SyncAll()
	=LOGSTRING("Sync Staff "+oSync.cErrormsg,ERROR_LOG)
	Response.Status = "400 "+oSync.cErrorMsg
	Response.Write("false")
	Response.End()
	RETURN
ELSE
	=LOGSTRING("Sync Staff",ERROR_LOG)
ENDIF	

ENDFUNC


&&-----------------------------------------------------------------------------
FUNCTION SyncPickupPoints
&&-----------------------------------------------------------------------------
LOCAL oSql
=LOGSTRING("Sync Pick Points",ERROR_LOG)
oSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cSqlconnectstring,oSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	
oSql.csKIPFIELDSFORUPDATES="ID"

oNopSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cNopSqlconnectstring,oNopSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	

oSer=CREATEOBJECT("wwJsonSerializer")
oHttp=CREATEOBJECT("wwHttp")
oHttp.cContentType = "application/json"
oHttp.AppendHeader("Accept","application/json")

=LOGSTRING("Sync PEP",ERROR_LOG)
&&--------------------  Pep ---------------------
oHttp.nConnectTimeout=200

#IF .T.
lcresponse=oHttp.HttpGet("https://api.skynet.co.za:3227/api/Counter/GetSkynetCounters")
oRet=oSer.Deserialize(lcresponse)


TEXT TO lcSql NOSHOW
TRUNCATE TABLE PickUpPoints
SELECT * FROM PickUpPoints WHERE 0=1
ENDTEXT

oSql.Execute(lcSql,"cTemp")

SELECT cTemp
SCATTER NAME oTemp
FOR EACH loData IN oRet.Results
	lcSql=oSql.SQLBuildInsertStatementFromObject(loData,"PickUpPoints",oTemp)
	IF !oSql.ExecuteNonQuery(lcSql)
		SET STEP ON 
	ENDIF
NEXT

#endif
oNopSql.ExecuteNonQuery("EXEC ANQ_SyncPep")
=LOGSTRING("Sync POSTNET",ERROR_LOG)
&&-------------------- Postnet ---------------------
lcJson='{"api_key": "REDACTED", "account_number": "P50362" } '
lcUrl="https://onlineapp.postnet.co.za/JsonAPI/GetActiveClickCollectStores"
oHttp.AddPostKey(lcJSON)
lcResponse = oHttp.HttpGet(lcUrl)
lo=oSer.deserialize(lcResponse)	

TEXT TO lcSql NOSHOW
SET ANSI_WARNINGS OFF
TRUNCATE TABLE PickUpPoints
SELECT * FROM PickUpPoints WHERE 0=1
ENDTEXT

oSql.Execute(lcSql,"cTemp")
SELECT cTemp
SCATTER NAME oPep MEMO

SCATTER NAME oTemp

FOR EACH oPN IN lo.postnet_stores
	oPep.NodeCode="PN"+PADL(TRANSFORM(oPn.ID),4,'0')
	oPep.NodeShortName=oPn.company_name
	oPep.NodeShortName=oPn.company_name
	oPep.addressline1=ALLTRIM(oPn.street_Address)
	oPep.addressline2=ALLTRIM(LEFT(PADR(oPn.Business_Park,40),40))
	oPep.addressline3=ALLTRIM(IIF(LEN(oPn.Business_Park)>40,SUBSTR(oPn.Business_Park,41),""))
	oPep.suburbName=oPn.Suburb
	oPep.cityName=oPn.City
	oPep.Province=oPn.Province
	oPep.PostalCode=oPn.postal_code
	oPep.longitude=VAL(TRANSFORM(oPn.geo_long))
	oPep.latitude=VAL(TRANSFORM(oPn.geo_lat))
	
	lcSql=oSql.SQLBuildInsertStatementFromObject(oPep,"PickUpPoints",oTemp)
	IF !oSql.ExecuteNonQuery(lcSql)
		SET STEP ON 
	ENDIF

NEXT
oNopSql.ExecuteNonQuery("EXEC ANQ_SyncPostnet")
=LOGSTRING("Sync Pickup Points Done",ERROR_LOG)



ENDFUNC





&&-----------------------------------------------------------------------------
	FUNCTION SkyWebHook
&&-----------------------------------------------------------------------------
	LPARAMETERS lo
	
    lcParms = Request.GetRawFormData()
	IF EMPTY(lcParms)
		RETURN
	ENDIF	
	
	lcToken = REQUEST.ServerVariables("HTTP_TOKEN")
	IF EMPTY(lcToken)
		lcToken=REQUEST.params("APIKEY")
	ENDIF
		
	IF EMPTY(lcToken)
		RETURN THIS.ErrorResponse("Could not Authenticate", "403 Could not Authenticate")
	ENDIF
	
	TEXT TO lc NOSHOW
	REDACTED
	ENDTEXT	
	IF lcToken<>lc
		RETURN THIS.ErrorResponse("Could not Authenticate", "403 Could not Authenticate")
	ENDIF
	

	IF VARTYPE(lo)<>"O" OR !PEMSTATUS(lo,"eventid",5)
		RETURN THIS.ErrorResponse("Invalid post data", "400 Invalid post data")
	ENDIF	
	oSer=CREATEOBJECT("wwJsonSerializer")
	lerror=.f.
	TRY
	lcJson=oSer.Serialize(lo)
	CATCH
		lError=.t.
		
	ENDTRY


	IF lError
		RETURN THIS.ErrorResponse("Invalid json data", "400 Invalid json data")
	ENDIF

	
oAMSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cAMSqlconnectstring,oAmSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF		

	oBus=CREATEOBJECT("SkyTrack",oAMSql)
=LOGSTRING(lcJson,ERROR_LOG)

	IF oBus.Load(lo.eventid)
		return
	ENDIF	
	oBus.New()
	COPYOBJECTPROPERTIES(lo,oBus.odata,2)
	oBus.odata.logdate=DATETIME()
	luret=oBus.Save()
	=LOGSTRING(oBus.cerrormsg,ERROR_LOG)


RETURN

ENDFUNC

	
&&-----------------------------------------------------------------------------
	FUNCTION BrevoWebHook
&&-----------------------------------------------------------------------------
	LPARAMETERS lo
	
    lcParms = Request.GetRawFormData()
	IF EMPTY(lcParms)
		RETURN
	ENDIF	
	
	
	IF !BETWEEN(REquest.GetIpAddress(),"1.179.112.0","1.179.127.255") AND  !"brevo"$LOWER(REquest.GetBrowser())
	
	
	lcToken = REQUEST.ServerVariables("HTTP_TOKEN")
	IF EMPTY(lcToken)
		lcToken=REQUEST.params("APIKEY")
	ENDIF
		
	IF EMPTY(lcToken)
		RETURN THIS.ErrorResponse("Could not Authenticate", "403 Could not Authenticate")
	ENDIF
	
	TEXT TO lc NOSHOW
		REDACTED
	ENDTEXT	
	IF lcToken<>lc
		RETURN THIS.ErrorResponse("Could not Authenticate", "403 Could not Authenticate")
	ENDIF
	
	ENDIF

	


	IF VARTYPE(lo)<>"O"
		=LOGSTRING("Invalid Data "+TRANSFORM(lo),"BREVOWEBHOOK.LOG")
		RETURN THIS.ErrorResponse("Invalid post data", "400 Invalid post data")
	ENDIF	
	oSer=CREATEOBJECT("wwJsonSerializer")
	lerror=.f.
	TRY
	lcJson=oSer.Serialize(lo)
	CATCH
		lError=.t.
		
	ENDTRY

	IF lError
		RETURN THIS.ErrorResponse("Invalid json data", "400 Invalid json data")
	ENDIF
	=LOGSTRING(lcJson,"BREVOWEBHOOK.LOG")

		
	RETURN

	ENDFUNC


FUNCTION BackInStockSubscription(oParms)

IF VARTYPE(oParms)<>"O"
	RETURN
ENDIF	

oNopSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(   this.oConfig.cNopSqlconnectstring,oNopSql)


	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN THIS.ErrorResponse("Could not create subscription", "400")
ENDIF	



IF !oNopSql.EXECUTENONQUERY("ANQ_Upsert_BackInStockSubscription @ProductID="+TRANSFORM(oParms.params.productID)+;
		",@UserName='"+ALLTRIM(oParms.params.UserName)+"'")
		
	RETURN THIS.ErrorResponse("Could not create subscription", "400")
ENDIF	

ENDFUNC




&&--------------------------------------------------------------------
FUNCTION NopReport(oParms)
&&--------------------------------------------------------------------

lError=.f.
lReturn=.f.
DO WHILE .T.

TRY

poError = CREATEOBJECT("HtmlErrorDisplayConfig")
IF VARTYPE(oParms)<>"O"
	THIS.ErrorResponse("Invalid Parameter Object", "400 Bad Request")
	EXIT
ENDIF
oSer=CREATEOBJECT("wwJsonSerializer")
=LOGSTRING(oSer.Serialize(oParms),"NopReport.log")

oAMSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cAMSqlconnectstring,oAmSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	THIS.ErrorResponse("Communication Failure", "400 Bad Request")
	lReturn=.t.
	EXIT
ENDIF	

lDrill=PEMSTATUS(oParms,"drilldown",5)
IF lDrill 
TEXT TO lcSql TEXTMERGE NOSHOW
Select * from NopIntegration..NopReports where systemname='<<oParms.data.drill>>'
ENDTEXT
ELSE
IF !PEMSTATUS(oParms,"Report",5) 
	THIS.ErrorResponse("Invalid Parameter Report", "400 Bad Request")
	lReturn=.t.
	EXIT
** Select * from NopReports 
ENDIF
TEXT TO lcSql TEXTMERGE NOSHOW
Select * from NopIntegration..NopReports where ReportName='<<oParms.Report>>'
ENDTEXT
ENDIF
IF oAMsql.Execute(lcSql,"Treport")#1
	THIS.ErrorResponse("No Report",  "400 No Report Available")
	lReturn=.t.
	EXIT
ENDIF


CATCH TO oErr 

      =LOGSTRING([  Message: ] + oErr.Message + CHR(13)+CHR(10)+;
      [  Procedure: ] + oErr.Procedure  + CHR(13)+CHR(10)+;
      [  Details: ] + oErr.Details  + CHR(13)+CHR(10)+;
      [  LineContents: ] + oErr.LineContents ,ERROR_LOG)

	lError=.t.

ENDTRY
EXIT

ENDDO

IF lReturn
	RETURN
ENDIF	
IF lError
	THIS.ErrorResponse("Internal Error", "400 Bad Request")
	RETURN
ENDIF

=LOGSTRING("Check Parameters","NopReport.log")

lError=.f.
*TRY
lcCustno=oParms.userName
IF PEMSTATUS(oParms,"data",5) AND PEMSTATUS(oParms.data,"filter",5)
oParms.data.filter=strtran(oParms.data.filter,"'","''")

ENDIF


db=ALLTRIM(tReport.databasename)
lcSql=TEXTMERGE(Treport.SqlCmd)




IF lError
		Set Alternate To ("NopReport.log") ADDITIVE 
		Set Alternate On
		
		*DISPLAY OBJECTS LIKE oParms NOCONSOLE 
		
		
		DISPLAY OBJECTS LIKE oParms.data NOCONSOLE 
		
		
		Set Alternate To
		Set Alternate Off
	THIS.ErrorResponse("Missing Required Parameters", "400 Bad Request")
	RETURN
ENDIF

IF PEMSTATUS(oParms,"data",5) AND PEMSTATUS(oParms.data,"async",5) AND PEMSTATUS(oParms.data,"excel",5)
		lcID= THIS.StartAsyncExcelReport(oParms,lcSql)
		IF !EMPTY(lcID)
			RETURN lcID
		ENDIF
		=LOGSTRING("Could not start Job","TFS.log")
		THIS.ErrorResponse("Could not start", "400 Could not Start")
		RETURN
ENDIF


luret=oAMSQL.Execute(lcSql,"TFS" )
=LOGSTRING(lcSql,"TFS.log")
LOCAL lo
lo=CREATEOBJECT("EMPTY")


lnCursors=3
IF UPPER(LEFT(lcSql,6))="SELECT"
	lnCursors=1
ENDIF


IF luret<>lnCursors
	=LOGSTRING(oAMSQL.cErrorMsg,"TFS.log")
	THIS.ErrorResponse("Could not get data", "400 No Data Available")
	RETURN
ENDIF	



IF lnCursors=1
loD = CursorToCollection("TFS") 
ADDPROPERTY(lo,"Detail",loD)
ENDIF

IF lnCursors>1
SELECT TFS
SCATTER NAME loH MEMO
ADDPROPERTY(lo,"Header",loH)
ENDIF

IF lnCursors>1
loD = CursorToCollection("TFS1") 
ADDPROPERTY(lo,"Detail",loD)
ENDIF

IF lnCursors>2
	SELECT TFS2
	IF RECCOUNT()>1
	loF= CursorToCollection("TFS2") 
	ELSE
	SCATTER NAME loF MEMO
	ENDIF
	ADDPROPERTY(lo,"Footer",loF)
ENDIF




 IF USED("TFS1")
 DIMENSION aFld[1]
 =AFIELDS(aFld,"TFS1")
 =ReportFields(lo,@aFld,TReport.ID,oAMSql)
 ENDIF	
 
IF  PEMSTATUS(oParms,"data",5) AND PEMSTATUS(oParms.data,"excel",5)
	oConFig=PROCESS.oConfig
	RETURN THIS.ReportTOExcel()
ENDIF
	
=LOGSTRING("Run Report","NopReport.log")

lError=.f.
lReturn=.f.
DO WHILE .T.

 
TRY

	oReport=CREATEOBJECT("Reports")
	IF !lDrill
		IF PEMSTATUS(oReport,CHRTRAN(ALLTRIM(oParms.Report)," ","_"),5)
			mFunc="oReport."+CHRTRAN(ALLTRIM(oParms.Report)," ","_")+"(lo)"
			luret=&mFunc
			IF !luRet
				THIS.ErrorResponse(IIF(PEMSTATUS(lo.header,"cerrormsg",5),lo.header.cerrormsg,"Could not get detail data"),;
					 "400 No Detail Data Available")
				lReturn=.t.
				EXIT
			ENDIF
		ENDIF
	ELSE
		IF PEMSTATUS(oReport,JUSTEXT(oParms.data.drill),5)
		mFunc="oReport."+JUSTEXT(oParms.data.drill)+"(lo)"
		&mFunc
		ENDIF

	ENDIF
	=LOGSTRING("Done","NopReport.log")
	serializer.PropertyNameOverrides=" headerTitle,headerAbbr,sortKey,sortDirection,sortByFormatted,filterByFormatted,tdClass,thClass,thStyle,variant,tdAttr,thAttr,isRowHeader,stickyColumn"
	EXIT

CATCH TO oErr 

      =LOGSTRING([  Message: ] + oErr.Message + CHR(13)+CHR(10)+;
      [  Procedure: ] + oErr.Procedure  + CHR(13)+CHR(10)+;
      [  Details: ] + oErr.Details  + CHR(13)+CHR(10)+;
      [  LineContents: ] + oErr.LineContents ,ERROR_LOG)

	llError=.t.


ENDTRY
EXIT

ENDDO

IF lReturn
	RETURN
ENDIF	

RETURN lo

ENDFUNC

&&------------------------------------------------------------------------------------------------------
	FUNCTION ReportTOExcel (noDownload,osync)
&&------------------------------------------------------------------------------------------------------	
	PRIVATE oAsync
	oAsync=osync

	SET PROCEDURE TO Excelx ADDIT
	SET PROCEDURE TO Reports ADDIT
	lnRecs=RECCOUNT("TFS1")
	IF VARTYPE(osync)="O"
		oAsync.oEvent.ReturnData=TRANSFORM(lnRecs)
		oAsync.oEvent.STATUS="5"
		oAsync.SaveEvent()
	ENDIF

	SELECT TFS1
	GO TOP IN TFS1
	IF EOF()
		THIS.ErrorResponse("Could not get data", "400 No Data Available")
		RETURN
	ENDIF



	SCATTER MEMVAR MEMO
	lError=.F.

	TRY
		oxlConfig=CREATEOBJECT("exceldatagridconfig")
		DIMENSION aLFld[1]
		SELECT TLineFields
		=AFIELDS(aLFld,"TLINEFIELDS")
		SCAN FOR Displayorder>0 AND (ISNULL(_class) OR _class<>"noexcel")
			SCATTER MEMVAR MEMO
			STORE "" TO lcKey,lcLabel
			lcKey=ALLTRIM(m._Key)
			lcLabel=ALLTRIM(m._Label)+"      "
			lcFld="m."+lcKey
			lcType=VARTYPE(&lcFld)
			lcformat=""
			IF _formatter="format_decimal"
				lcformat="#,##0.00;[Red]-#,##0.00" &&;-;-@" &&"_-* #,##0.00_-;-* #,##0.00_-;_-* "-"??_-;_-@_-" &&"#,##0.00"
			ENDIF
			oxlConfig.ADDCOLUMN(lcKey,lcLabel,lcType,lcformat)

		ENDSCAN
		oxlConfig.cHeader=TFS.TITLE
		oxl=CREATEOBJECT(STRTRAN(TFS.NAME," ","")) &&"SaleXls")
		oxl.FileName=oConfig.cHTMLPagePath + "\temp\"+SYS(2015)+".xlsx"
		oxl.GO(oxlConfig,"TFS1")
		lcFileName=oxl.FileName
		oxl=NULL


	CATCH TO oErr

			=LOGSTRING([  Message: ] + oErr.MESSAGE + CHR(13)+CHR(10)+;
				[  Procedure: ] + oErr.PROCEDURE  + CHR(13)+CHR(10)+;
				[  Details: ] + oErr.DETAILS  + CHR(13)+CHR(10)+;
				[  LineContents: ] + oErr.LINECONTENTS ,"TFS.LOG")


			lError=.T.
	ENDTRY

	oxl=NULL
	IF lError AND !noDownload

		oxl=NULL
		THIS.ErrorResponse("Could not get data", "400 No Excel Available")
		RETURN
	ENDIF
	IF lError
		RETURN ""
	ENDIF
	IF !noDownload
		Response.DownloadFile(lcFileName,"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",TFS.Title)
	ELSE
		RETURN lcFileName
	ENDIF




	ENDFUNC
	
&&------------------------------------------------------------------------------	
	FUNCTION StartAsyncExcelReport(loReport,lcSql)
&&------------------------------------------------------------------------------	
	LOCAL o,oSql
	oSql=CREATEOBJECT("wwSQL")
	IF !this.DBConnect(this.oConfig.cSqlconnectstring,oSql)
		=LOGSTRING(this.cerrormsg,ERROR_LOG)
		RETURN .F.
	ENDIF	
	lcReportName=loReport.Report
	loReport=oSer.Serialize(loReport)
	PRIVATE loAsync
	loAsync = CREATEOBJECT("AsyncWebRequest")
	loAsync.CONNECT(osql)
	o=CREATEOBJECT("ASYNCRESPONSE")
*** SubmitEvent saves the request
	IF !o.execute("submit","",lcReportName,"oApp","asyncexcelreport()",[{ sql: "]+lcSql+[", report: ']+loReport+['}])
		RETURN ""
	ENDIF
	RETURN loAsync.oEvent.ID
	ENDFUNC

&&------------------------------------------------------------------------------
	FUNCTION AsyncExcelReport()
&&------------------------------------------------------------------------------	
*ReportName=loAsync.GetProperty("Report")
	LOCAL oSql
	oSql=CREATEOBJECT("wwSQL")
	IF !this.DBConnect(oConfig.cAMSqlconnectstring,oSql)
		=LOGSTRING(this.cerrormsg,ERROR_LOG)
		RETURN .F.
	ENDIF	
	loParams=oSer.DeserializeJson(lcJobData)
	lcSql=loParams.SQL
	oParms=oSer.DeserializeJson(loParams.report)
	
	lDrill=PEMSTATUS(oParms,"drilldown",5)
IF lDrill 
TEXT TO lcSql TEXTMERGE NOSHOW
Select * from NopReports where systemname='<<oParms.data.drill>>'
ENDTEXT
ELSE
IF !PEMSTATUS(oParms,"Report",5) 
	THIS.ErrorResponse("Invalid Parameter Report", "400 Bad Request")
	RETURN	
** Select * from NopReports 
ENDIF
TEXT TO lcSql TEXTMERGE NOSHOW
Select * from NopReports where ReportName='<<oParms.Report>>'
ENDTEXT
ENDIF
IF osql.Execute(lcSql,"Treport")#1
	THIS.ErrorResponse("No Report",  "400 No Report Available")
	RETURN .F.
ENDIF
IF osql.execute("Select * from NopReportDetail where REPORTID="+TRANSFORM(Treport.ID)+" order by displayorder","TLineFields")#1
	THIS.SetError("No Report Available")
	RETURN .F.
ENDIF

lError=.f.
TRY
lcCustno=oParms.userName
IF PEMSTATUS(oParms,"data",5) AND PEMSTATUS(oParms.data,"filter",5)
oParms.data.filter=strtran(oParms.data.filter,"'","''")

ENDIF


db=ALLTRIM(tReport.databasename)
lcSql=TEXTMERGE(Treport.SqlCmd)
CATCH
	
lError=.t.
ENDTRY

IF lError
	THIS.ErrorResponse("Missing Required Parameters", "400 Bad Request")
	RETURN
ENDIF
	
	
	luret=osql.execute(lcSql,"TFS" )
	=LOGSTRING(lcSql,"TFS.log")


	IF luret<>3
		=LOGSTRING(osql.cErrorMsg,"TFS.log")
		THIS.SetError("No Data Available")
		RETURN
	ENDIF

	lcFile=THIS.ReportTOExcel(.T.,loAsync)
	IF VARTYPE(lc)<>"C"
		RETURN .F.
	ENDIF
	loAsync.oEvent.COMPLETED = DATETIME()
	loAsync.oEvent.ReturnData=lcFile
	loAsync.oEvent.STATUS="Done"
	loAsync.SaveEvent()
	RETURN

ENDFUNC
&&------------------------------------------------------------------------------
	FUNCTION AsyncDownloadStatus(lcID)
&&------------------------------------------------------------------------------	
	LOCAL o,oSql
	PRIVATE losync
	oSql=CREATEOBJECT("wwSQL")
	IF !this.DBConnect(this.oConfig.cSqlconnectstring,oSql)
		=LOGSTRING(this.cerrormsg,ERROR_LOG)
		RETURN .F.
	ENDIF	

	loAsync = CREATEOBJECT("AsyncWebRequest")
	loAsync.CONNECT(osql)
	o=CREATEOBJECT("ASYNCRESPONSE")
*** SubmitEvent saves the request
	lobj=CREATEOBJECT("Empty")

	IF !o.execute("check",lcID)
		ADDPROPERTY(lobj,"Status",loAsync.oEvent.STATUS)
		ADDPROPERTY(lobj,"Records",loAsync.oEvent.ReturnData)
		ADDPROPERTY(lobj,"ID",loAsync.oEvent.ID)
		RETURN lobj

	ENDIF

	ADDPROPERTY(lobj,"Status","Done")
	ADDPROPERTY(lobj,"ID",loAsync.oEvent.ID)
	RETURN lobj


	ENDFUNC

&&------------------------------------------------------------------------------
	FUNCTION AsyncGetDownload(lo)
&&------------------------------------------------------------------------------
	LOCAL o,oSql
	PRIVATE losync
	oSql=CREATEOBJECT("wwSQL")
	IF !this.DBConnect(this.oConfig.cSqlconnectstring,oSql)
		=LOGSTRING(this.cerrormsg,ERROR_LOG)
		RETURN .F.
	ENDIF	
	loAsync = CREATEOBJECT("AsyncWebRequest")
	loAsync.CONNECT(osql)
	IF !loAsync.LoadEvent(lo.ID)
		THIS.ErrorResponse(loAsync.cErrorMsg, "400 No Job Available")
		RETURN
	ENDIF
	IF loAsync.oEvent.STATUS<>"Done"
		THIS.ErrorResponse(loAsync.cErrorMsg, "400 Not Completed")
		RETURN
	ENDIF
	lcFileName=loAsync.oEvent.ReturnData
	Response.DownloadFile(lcFileName,"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",loAsync.oEvent.TITLE)

	ENDFUNC

&&------------------------------------------------------------------------------
FUNCTION DownloadInvoice(oParms)
&&------------------------------------------------------------------------------
** SET CURRENCY DEPENDING ON PARAMETERS
SET CURRENCY TO "R"
	*response.DownloadFile("E:\Development\NopIntegration\web\test\Files\test.pdf","application/pdf","test.pdf")
poError = CREATEOBJECT("HtmlErrorDisplayConfig")
IF VARTYPE(oParms)<>"O"
	THIS.ErrorResponse("Invalid Parameter Object", "400 Bad Request")
	RETURN
ENDIF
oSer=CREATEOBJECT("wwJsonSerializer")
lcInvno=oParms.cInvno

oAMSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cAMSqlconnectstring,oAmSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	
lo=CREATEOBJECT("EMPTY")
IF EMPTY(lcInvno)
	ADDPROPERTY(lo,"Error",1)
	ADDPROPERTY(lo,"Errormsg","No Invoice #")
	RETURN lo
ENDIF	


lcSql="exec [AMSERVER-v9].amanniquelive.dbo.sp_WS_rpt_invoice '"+lcInvno+"'"
IF  oAmSql.execute(lcSql,"curInvoice")<1
	=LOGSTRING(lcSql,"TFS.log")
	THIS.ErrorResponse("Could not get data", "400 No Data Available")
	RETURN
ENDIF
	=LOGSTRING("Start Invoice","TFS.log")
lcRet=this.reporttopdf("invoice.frx",.t.,.t.)

*	=LOGSTRING("After "+TRANSFORM(lcRet),"TFS.log")
response.DownloadFile(lcRet,"application/pdf","invoice"+lcInvno+".pdf")
RETURN 



ENDFUNC


FUNCTION reporttopdf (cReportName,lnoredirect,lfile)

Local loSession, loViwer,lnRetval, lcPageCaption

loSession=Evaluate([xfrx("XFRX#LISTENER")])
losession.targetType = "PDF"
losession.donotopenviewer=.t.
lcFileName=SYS(2015)+".pdf"
lcPath=ADDBS(process.oConfig.cHTMLPagePath)+'temp'+"\"
losession.targetFileName = lcPath+lcFileName
=LOGSTRING(losession.targetFileName,"TFS.log")
lnRetval = losession.SetParams()
Report Form (creportname) Object loSession 
oSer=CREATEOBJECT("wwJsonSerializer")
=LOGSTRING(oSer.Serialize(loSession),"TFS.log")
loSession=null


		RETURN lcPath+lcFileName



RETURN 

ENDFUNC



&&-----------------------------------------------------------------------------
FUNCTION OTPGenerate
&&-----------------------------------------------------------------------------
PRIVATE oSql
oSql=.f.
oSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cSQLConnectString,oSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	

lnCustomerID=Request.params("id")
lcvia=Request.params("sendvia")
IF EMPTY(lnCustomerID) OR EMPTY(lcVia)
	THIS.ErrorResponse("Invalid Parameters", "400 Inavlid Parameters")
	RETURN
ENDIF
liLive=IIF(LOWER(Request.params("staging"))="true",0,1)
oOtp=CREATEOBJECT("OTP")
IF !oOtp.GenerateOTP(VAL(TRANSFORM(lnCustomerID)),LOWER(lcVia),liLive)
	IF !"You have an invalid"$oOtp.cErrorMsg
		&&	oOtp.cErrorMsg="We are unable to send your OTP at present please try again later or contact Annique Support"
	ENDIF
	THIS.ErrorResponse(oOtp.cErrorMsg, "400 "+oOtp.cErrorMsg)
	RETURN
ENDIF
RETURN



ENDFUNC



&&-----------------------------------------------------------------------------
FUNCTION SENDSMS(lo)
&&-----------------------------------------------------------------------------

poError = CREATEOBJECT("HtmlErrorDisplayConfig")
IF VARTYPE(lo)<>"O"
	THIS.ErrorResponse("Invalid Parameter Object", "400 Bad Request")
	RETURN
ENDIF

IF !PEMSTATUS(lo,"username",5) AND !PEMSTATUS(lo,"customerid",5)
	THIS.ErrorResponse("No User Specified", "400 Bad Request")
	RETURN
ENDIF


IF PEMSTATUS(lo,"customerid",5) AND VAL(TRANSFORM(lo.CustomerID))=0
	THIS.ErrorResponse("No Customer ID Specified", "400 Bad Request")
	RETURN
ENDIF



IF !PEMSTATUS(lo,"message",5)
	THIS.ErrorResponse("No Message", "400 Bad Request")
	RETURN
ENDIF

isStaging=.f.
IF PEMSTATUS(lo,"staging",5) AND !EMPTY(lo.staging)
	isStaging=.t.
ENDIF

oNopSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cNopSqlconnectstring,oNopSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	
oNopSql.EnableUnicodeToAnsiMapping()

oBus=CREATEOBJECT("Customer")
oBus.SetSqlObject(oNopSql)

DO CASE
	 CASE PEMSTATUS(lo,"username",5) 
		 IF !oBus.LoadBase("Username='"+lo.UserName+"'")
				THIS.ErrorResponse("Invalid User Specified", "400 Bad Request")
				RETURN
		 ENDIF
				 
	  CASE PEMSTATUS(lo,"customerid",5) 

		IF !oBus.Load(lo.CustomerID)
			THIS.ErrorResponse("Invalid Customer Specified", "400 Bad Request")
			RETURN
		ENDIF

ENDCASE
IF ISNULLOREMPTY(oBus.odata.phone)
		THIS.ErrorResponse("Invalid Cell Specified", "400 Bad Request")
		RETURN
ENDIF

IF isStaging
	oBus.odata.Phone="0825532407"
	lo.Message="**STAGING**"+lo.Message
ENDIF

oSms=CREATEOBJECT("SendMail")
IF !oSms.SendSMS(oBus.odata.Phone,lo.Message)
	THIS.ErrorResponse("Could not Send "+oSms.cErrorMsg, "400 Bad Request")
	RETURN
ENDIF



ENDFUNC



&&-----------------------------------------------------------------------------
FUNCTION ValidateAddress
&&-----------------------------------------------------------------------------
PRIVATE oSql
oSql=.f.
oSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cSQLConnectString,oSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	

lnCountryID=Request.params("countryid")

lcSuburb=Request.params("query")
lcProvince=Request.params("province")
IF EMPTY(lnCountryID) OR EMPTY(lcSuburb) && OR EMPTY(lcProvince)
	THIS.ErrorResponse("Invalid Parameters", "400 Invalid Parameters")
	RETURN
ENDIF
lcSuburb=lcSuburb+"%"

TEXT TO lcSql TEXTMERGE NOSHOW
SELECT RTRIM(p.suburb)+' ('+RTRIM(p.Province)+' - '+RTRIM(p.postalcode_street)+')' [text],
 RTRIM(p.suburb)+' ('+RTRIM(p.Province)+' - '+RTRIM(p.postalcode_street)+')' [value]
 FROM ad_PostalCOdes p WHERE suburb like ?lcSuburb
 << IIF(!EMPTY(lcProvince),"and province=?lcProvince "," ") >> and CountryID=?lnCountryID
ENDTEXT
ln=osql.execute(lcSql,"TPost")
IF ln=0 and this.lerror
   THIS.ErrorResponse("Invalid Response", "400 Invalid Response")
   RETURN .F.
ENDIF
IF RECCOUNT("TPost")=0
	 THIS.ErrorResponse("Suburb no Found", "400 Suburb no Found")
	 RETURN
ENDIF
RETURN "cursor:TPost"
ENDFUNC

&&-----------------------------------------------------------------------------
FUNCTION MessagetoInbox(loMessage)
&&-----------------------------------------------------------------------------
PRIVATE oNop
oNop=.f.

oSer=CREATEOBJECT("wwJsonSerializer")
oNop=CREATEOBJECT("SyncClass")
IF !oNop.SetUp(this.oConfig.cAPIUrl)  && Store ID
	=LOGSTRING(oNop.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	

TEXT TO lcJson NOSHOW TEXTMERGE
{
  "store_id": 1,
  "from_customer_id": <<loMessage.fromcustomerid>>,
  "to_customer_id": <<loMessage.tocustomerid>>,
  "subject": "<<loMessage.subject>>",
  "text": "<<loMessage.text>>",
  "is_read": false,
  "is_deleted_by_author": false,
  "is_deleted_by_recipient": false,
  "created_on_utc": "<<TOISODATESTRING(DATETIME(),.t.,.f.)>>",
  "id": 0
}
ENDTEXT
luret=oNop.PrivateMessage_Create(lcJson)
IF VARTYPE(luret)<>"O"
	=LOGSTRING("Failed Send Message :"+oNop.cErrormsg,ERROR_LOG)
	Response.Status = "400 "+oNop.cErrorMsg
	Response.Write("false")
	Response.End()
	RETURN 
ENDIF	
RETURN luRet.ID
ENDFUNC





&&-----------------------------------------------------------------------------
FUNCTION VoucherProcess
&&-----------------------------------------------------------------------------


LOCAL ii,lcUrl, lcFile,oExcel
CREATE CURSOR cTemp (cCustno varchar(100),DiscountID I,dFrom DATE,dTo DATE,cComment varChar(100))
TRY
oExcel = CreateObject("Excel.Application")
lHasExcel=.t.
CATCH TO oErr 

      =LOGSTRING([  Message: ] + oErr.Message + CHR(13)+CHR(10)+;
      [  Procedure: ] + oErr.Procedure  + CHR(13)+CHR(10)+;
      [  Details: ] + oErr.Details  + CHR(13)+CHR(10)+;
      [  LineContents: ] + oErr.LineContents ,"upload.log")

	llError=.t.
	lHasExcel=.f.

ENDTRY


IF !lHasExcel
	Response.Status = "400 Excel Not Found"
	Response.Write("false")
ENDIF


ln=ADIR(aZ,this.oConfig.cVoucherUploadPath+"_???_*.xlsx")
IF ln=0
	Response.Status = "201 Nothing to Process"
	Response.Write("false")
	RETURN
ENDIF


FOR ii=1 TO ALEN(az,1)
	lcFile= this.oConfig.cVoucherUploadPath+aZ[ii,1]
	=LOGSTRING("Processing "+lcFile,"upload.log")
	
	
	IF FILE(FORCEEXT(lcFile,"done"))
		=LOGSTRING("Done"+lcFile,"upload.log")
		LOOP
	ENDIF	
	
	
	
	
	lnDiscount=VAL(SUBSTR(JUSTFNAME(lcFile),2,3))
	IF lnDiscount=0
		=LOGSTRING("Invalid Discount Format in "+lcFile,"upload.log")
		LOOP
	ENDIF
	
	
=LOGSTRING("Opening "+lcFile,"upload.log")
	llerror=.f.
	TRY
		oWorkbook = oExcel.Workbooks.Open(lcFile,1,.t.)
	CATCH TO oErr 
	      =LOGSTRING([  Message: ] + oErr.Message + CHR(13)+CHR(10)+;
	      [  Procedure: ] + oErr.Procedure  + CHR(13)+CHR(10)+;
	      [  Details: ] + oErr.Details  + CHR(13)+CHR(10)+;
	      [  LineContents: ] + oErr.LineContents ,"upload.log")

			llError=.t.
	ENDTRY
	IF llerror
		Loop
	ENDIF
		
=LOGSTRING("Start "+lcFile,"upload.log")
	
	oWorksheet1 = oWorkbook.Worksheets[1]

	WITH oWorksheet1

		IF UPPER(.range("A1").Value)<>UPPER("From Date")
			=LOGSTRING("Invalid Excel Format in "+lcFile+" "+.range("A1").Value,"upload.log")
			LOOP
		ENDIF
		=LOGSTRING("Check Format 2","upload.log")
		ldFrom=.range("B1").Value
		ldTo=.range("d1").Value
		IF !VARTYPE(ldFrom)$"DT" OR !VARTYPE(ldTo)$"DT"
			=LOGSTRING("Invalid Date Format in "+lcFile+" "+.range("A1").Value,"upload.log")
			LOOP
		ENDIF

		lcComment=ALLTRIM(TRANSFORM(.range("f1").Value))
		=LOGSTRING("Check Rows","upload.log")
		lnLastRow=oWorksheet1.RANGE("A1").End(-4121).Row
		IF lnLastRow<2
			=LOGSTRING("No Rows in "+lcFile+" "+.range("A1").Value,"upload.log")
			LOOP
		ENDIF
			
		FOR xx=2 TO lnLastRow
		lcCustno=ALLTRIM(TRANSFORM(.Range(this.xlrange(xx,1)).value))	
		INSERT INTO cTemp VALUES(lcCustno,lnDiscount,ldFrom,ldTo,lcComment)	
		NEXT
		RENAME (lcFile) TO FORCEEXT(lcFile,"done")
	ENDWITH	
NEXT
oExcel.Quit()	

oNopSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cNopSqlconnectstring,oNopSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	
oNopSql.EnableUnicodeToAnsiMapping()	
oSox=CREATEOBJECT("ANQ_Discount_AppliedToCustomers")
oSox.SETSQLOBJECT(oNopSql)
	
SELECT cTemp
SCAN
	SCATTER MEMVAR MEMO	

	IF oSox.Query("select id from Customer where username='"+m.cCustno+"'","TCust")=0
		=LOGSTRING("No Customer "+lcCustno,"upload.log")
		LOOP
	ENDIF	



	 TEXT TO lcSql TEXTMERGE NOSHOW
CustomerID=<<TRANSFORM(TCust.ID)>> and '<<TOISODATESTRING(m.dFrom,,.t.)>>' between startdateutc and enddateutc
	ENDTEXT	 
	 IF oSox.LOADBASE(lcSql)
		LOOP  && Already Exists
	ENDIF
		
		
	IF !oSox.New()
		=LOGSTRING("Could not add voucher "+oSox.cErrorMsg,"upload.log")
		RETURN .F.
	ENDIF
		
	
	ldTo=DTOT(m.dTo+1)-1

		WITH oSox.oData
			.CustomerId=TCust.ID
		    .DiscountId=m.DiscountID
            .LimitationTimes=1
            .DiscountLimitationId=25
		   	.NoTimesUsed=0
		    .StartDateUtc=m.dFrom
		    .EndDateUtc=ldTo
      		.isActive=1
     		.notified=0
			.Comment=m.cComment
		ENDWITH
		IF !oSox.Save()
			=LOGSTRING("Could not add voucher "+oSox.cErrorMsg,"upload.log")
			LOOP
		ENDIF
		=LOGSTRING("Loaded "+m.cCustno,"upload.log")
ENDSCAN

=LOGSTRING("Completed ","upload.log")

RETURN
ENDFUNC


&&-----------------------------------------------------------------------------
FUNCTION NotifyVouchers
&&-----------------------------------------------------------------------------
oNopSql=CREATEOBJECT("wwSQL")
IF !this.DBConnect(this.oConfig.cNopSqlconnectstring,oNopSql)
	=LOGSTRING(this.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	
oNopSql.EnableUnicodeToAnsiMapping()
PRIVATE oNop
oNop=.f.

oSer=CREATEOBJECT("wwJsonSerializer")
oNop=CREATEOBJECT("SyncClass")
IF !oNop.SetUp(this.oConfig.cAPIUrl)  && Store ID
	=LOGSTRING(oNop.cerrormsg,ERROR_LOG)
	RETURN .F.
ENDIF	

TEXT TO lcSql TEXTMERGE NOSHOW
select c.id,CustomerID,LEFT(C.EndDateUtc,10) ExpiresOn,
	  D.NAME,
	  CAST(value as INT) SenderID from ANQ_Discount_AppliedToCustomers c
  join discount d on c.DiscountId=d.id
  join Setting s ON  storeid=0 and s.name = 'anniquecustomizationsettings.admincustomerid'
  WHERE (notified IS NULL or notified=0) and c.isActive=1 --d.id=8 
ENDTEXT
oNopSql.Execute(lcSql,"TMessage")
IF RECCOUNT("TMessage")<1
	RETURN .f.
ENDIF	
SELECT TMessage 
SCAN
TEXT TO lcJson NOSHOW TEXTMERGE
{
  "store_id": 1,
  "from_customer_id": <<tMEssage.SenderID>>,
  "to_customer_id": <<tmessage.customerID>>,
  "subject": "VOUCHER [<<tmessage.id">>]",
  "text": "<<tmessage.name>> Expires on <<tmessage.ExpiresOn>>",
  "is_read": false,
  "is_deleted_by_author": false,
  "is_deleted_by_recipient": false,
  "created_on_utc": "<<ToIsoDateString(DATETIME(),.t.,.f.)>>",
  "id": 0
}
ENDTEXT
luret=oNop.PrivateMessage_Create(lcJson)

oNopSql.EXECUTENONQUERY("UPDATE ANQ_Discount_AppliedToCustomers SET Notified=1 where id="+;
TRANSFORM(tmessage.id))

ENDSCAN


oNopSql.ExecuteNonQuery("EXEC ANQ_CleanNotifications")
ENDFUNC



	PROCEDURE row_col2string
		lparameters nROW, nCOL
		local Letter1, Letter2

		m.Letter2 =  m.nCOL % 26
		m.Letter1 =  (m.nCOL - m.nCOL % 27) / 27

		m.Letter2 = iif(empty(m.Letter2),'Z',chr(64+m.Letter2))
		m.Letter1 = iif(empty(m.Letter1),space(0),chr(64+m.Letter1))

		return m.Letter1 + m.Letter2 + alltrim(trans(m.nROW))
	ENDPROC


	PROCEDURE xlrange

		lparameters ROW1, COL1, ROWn, COLn
			return this.row_col2string(m.ROW1,m.COL1);
				+ iif(empty(m.ROWn),space(0),":" + this.row_col2string(m.ROWn,m.COLn))
	ENDPROC


ENDDEFINE



