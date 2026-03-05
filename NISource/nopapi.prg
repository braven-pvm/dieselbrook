#DEFINE ERROR_LOG     "Synchlog"+DTOS(DATE())+".log"
#DEFINE INI_FILE	  "nop.ini"
#DEFINE LOGTIME		  STRTRAN(LEFT(TIME(),5),":","")
#INCLUDE wconnect.h
DO wconnect
DO WWUTILS
#IF .F.
SET PROCEDURE TO WWSQL ADDIT
SET PROCEDURE TO wwJsonSerializer ADDIT

SET PROCEDURE TO wwBusinessObject ADDIT
#ENDIF
SET PROCEDURE TO NOPAPI ADDIT
SET DATE YMD
oSer=CREATEOBJECT("wwJsonSerializer")
oXml=CREATEOBJECT("WWXML")


DEFINE CLASS NOP as Custom

ClientID=0
Email=""
UserName=""
Password=""
Token=""
BaseUrl="https://stage.annique.com/api-backend/"
PageNumber=0
PageSize=100
EndPoint=NULL
oSer=NULL
oHttp=NULL
lError=.f.
cErrormsg=""
oConfig=NULL


FUNCTION Init(lcUserName,lcPassword,lcEmail,lcUrl)
WITH THIS
.oSer=CREATEOBJECT("wwJsonSerializer")
.oHttp=CREATEOBJECT("wwHttp")
.oHttp.cContentType = "application/json; charset=utf-8"
.oHttp.AppendHeader("Accept","application/json")
.email=lcEmail
.Username=lcUserName
.Password=lcPassword
.BaseUrl=IIF(EMPTY(lcUrl),.BaseUrl,lcUrl)
ENDWITH
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
ENDFUNC
*   SetError

&&------------------------------------------------------------------------------
FUNCTION CallEndPoint (lcEndpoint,lcParams,lcJson,lcVerb)
&&------------------------------------------------------------------------------

IF !lcendpoint='authenticate' AND EMPTY(this.Token)
	IF !this.authenticate()
		THIS.Seterror("Could not connect to NOP Site "+this.cErrorMsg)
		RETURN .F.
	ENDIF
ENDIF


lcParams=IIF(EMPTY(lcParams),"","?"+lcParams)
loObj=CREATEOBJECT("EMPTY")

WITH THIS

.oHttp.nConnectTimeout = 1000
lcUrl=ALLTRIM(this.BaseUrl)+lcEndPoint+lcParams
.oHTTP.AddPostKey()
IF !EMPTY(lcJson)
	.oHttp.AddPostKey( lcJson) &&STRCONV(lcJson, 9))
ENDIF	
IF EMPTY(lcVerb) OR lcVerb<>"PUT"
lcResponse = .oHttp.HttpGet(lcUrl)
ELSE
lcResponse = .oHttp.PUT(lcUrl)
ENDIF
*=logstring(" CALLED :"+lcUrl+" "+lcJson+" "+lcResponse+.oHttp.cResultCode+" "+TRANSFORM(.oHttp.nError),"ym.log")

*** Check for hard HTTP protocol/connection errors first
IF .oHttp.nError # 0
	.lError=.t.
	=logstring(lcUrl+" "+lcJson+" "+lcResponse,"_NOPAPI.log")
	THIS.SetError(TRANSFORM(.oHttp.nError)+"-"+.oHttp.cErrormsg)
	RETURN .f.
ENDIF

*** Then check the result code
IF .oHttp.cResultCode # "200" AND .oHttp.cResultCode # "304"
   =logstring(lcUrl+" "+lcJson+" "+lcResponse,"_NOPAPI.log")
	THIS.SetError(oHttp.cResultCode+"-"+.oHttp.cResultCodeMessage)  && Echo message from server
	RETURN .f.
ENDIF


IF EMPTY(lcResponse) AND .oHttp.cResultCode # "200" 
	THIS.SetError("No Response")
	RETURN .f.
ENDIF
IF EMPTY(lcResponse) AND .oHttp.cResultCode = "200" 
	RETURN 
ENDIF
ENDWITH

*logstring(lcResponse,"_NOPAPI.log")
lError=.F.
TRY 
	lo=THIS.oSer.DeSerializeJson(lcResponse)
	
CATCH
	lo=lcResponse
ENDTRY
RETURN lo



ENDFUNC



&&------------------------------------------------------------------------------
FUNCTION Authenticate
&&------------------------------------------------------------------------------

this.Token=""
TEXT TO lcJson TEXTMERGE NOSHOW
{"email"    :  "<<this.email>>",
"Username" : "<<this.username>>",
"Password" : "<<this.password>>"
}
ENDTEXT

luRet=THIS.CallendPoint("authenticate/GetToken","",lcJson)
IF VARTYPE(luRet)<>"O"
	RETURN .F.
ENDIF
IF !PEMSTATUS(luRet,"Token",5)
	THIS.SetError("No Token returned")
	RETURN .F.
ENDIF
this.Token=luRet.Token
THIS.oHTTP.cExtraHeaders = ;
	this.oHTTP.cExtraHeaders + ;
	" Authorization: " +this.Token
ENDFUNC

&&------------------------------------------------------------------------------
FUNCTION Category_Getall
&&------------------------------------------------------------------------------
lcParams=""
luRet=THIS.CallendPoint("Category/Getall",lcParams,"")
IF VARTYPE(luRet)<>"O"
	THIS.setError(TRANSFORM(luRet))
	RETURN .F.
ENDIF
RETURN luret
ENDFUNC



&&------------------------------------------------------------------------------
FUNCTION Customer_GetbyUserName(lcUserName)
&&------------------------------------------------------------------------------
lcParams="username="+lcUserName
luRet=THIS.CallendPoint("Customer/GetCustomerbyUserName",lcParams,"")
IF VARTYPE(luRet)<>"O"
	THIS.setError(TRANSFORM(luRet))
	RETURN .F.
ENDIF
RETURN luret
ENDFUNC

&&------------------------------------------------------------------------------
FUNCTION Address_Create(lcJson,lnCustomerID)
&&------------------------------------------------------------------------------
luRet=THIS.CallendPoint("Address/Create","",lcJson)
IF VARTYPE(luRet)<>"O"
	THIS.setError(TRANSFORM(luRet))
	RETURN .F.
ENDIF

lu=THIS.CallendPoint("Customer/InsertCustomerAddress/"+TRANSFORM(lnCustomerID)+;
	"/"+TRANS(luret.id))

RETURN luret
ENDFUNC


&&------------------------------------------------------------------------------
FUNCTION Address_Update(lcJson,lnCustomerID)
&&------------------------------------------------------------------------------
luRet=THIS.CallendPoint("Address/Update","",lcJson,"PUT")
IF VARTYPE(luRet)<>"L"
	THIS.setError(TRANSFORM(luRet))
	RETURN .F.
ENDIF


RETURN luret
ENDFUNC

&&------------------------------------------------------------------------------
FUNCTION Customer_Create(lcJson)
&&------------------------------------------------------------------------------
luRet=THIS.CallendPoint("Customer/Create","",lcJson)
IF VARTYPE(luRet)<>"O"
	THIS.setError(TRANSFORM(luRet))
	RETURN .F.
ENDIF
RETURN luret
ENDFUNC

&&------------------------------------------------------------------------------
FUNCTION Customer_Get(lnCustomerID)
&&------------------------------------------------------------------------------
luRet=THIS.CallendPoint("Customer/GetbyID/"+TRANSFORM(lnCustomerID),"")
IF VARTYPE(luRet)<>"O"
	THIS.setError(TRANSFORM(luRet))
	RETURN .F.
ENDIF
RETURN luret
ENDFUNC


&&------------------------------------------------------------------------------
FUNCTION Customer_CheckRole(lnCustomerID,lcRole)
&&------------------------------------------------------------------------------

luRet=THIS.CallendPoint("CustomerRole/IsInCustomerRole/"+TRANSFORM(lnCustomerID),;
	"customerRoleSystemName="+lcRole+"&onlyActiveCustomerRoles=true")

RETURN luRet

ENDFUNC

&&------------------------------------------------------------------------------
FUNCTION Customer_AddRole(lnCustomerID,lcRole)
&&------------------------------------------------------------------------------

luRet=THIS.CallendPoint("CustomerRole/IsInCustomerRole/"+TRANSFORM(lnCustomerID),;
	"customerRoleSystemName="+lcRole+"&onlyActiveCustomerRoles=false")
IF luRet
	RETURN
ENDIF

luRet=THIS.CallendPoint("CustomerRole/GetCustomerRoleBySystemName",;
"systemName="+lcRole)
	
IF VARTYPE(luRet)<>"O"
	THIS.setError(TRANSFORM(luRet))
	RETURN .F.
ENDIF

lObj=CREATEOBJECT("EMPTY")
ADDPROPERTY(lObj,"customer_id",lnCustomerID)
ADDPROPERTY(lObj,"customer_role_id",luRet.id)
ADDPROPERTY(lObj,"id",0)
lcJson=this.oSer.Serialize(lObj)
luRet=THIS.CallendPoint("CustomerRole/AddCustomerRoleMapping","",lcJson)
IF THIS.cerrormsg="No Response" OR THIS.cerrormsg=".T."  OR EMPTY(THIS.cerrormsg)
	RETURN
ENDIF	
IF VARTYPE(luRet)<>"O"
	THIS.setError(TRANSFORM(luRet))
	RETURN .F.
ENDIF


ENDFUNC

&&------------------------------------------------------------------------------
FUNCTION Customer_Update(lcJson)
&&------------------------------------------------------------------------------
luRet=THIS.CallendPoint("Customer/Update","",lcJson,"PUT")
IF luRet 
	RETURN
ENDIF	
	
IF THIS.cerrormsg="No Response" OR EMPTY(THIS.cerrormsg) OR THIS.cerrormsg=".T."
	RETURN
ENDIF	
RETURN .F.

ENDFUNC

&&------------------------------------------------------------------------------
FUNCTION Customer_SetPassword(lnCustomerID,lcPassword)
&&------------------------------------------------------------------------------

lObj=CREATEOBJECT("EMPTY")
ADDPROPERTY(lObj,"customer_id",lnCustomerID)
ADDPROPERTY(lObj,"password",lcPassword)
ADDPROPERTY(lObj,"password_format_id",0)
ADDPROPERTY(lObj,"id",0)
lcJson=this.oSer.Serialize(lObj)
luRet=THIS.CallendPoint("CustomerPassword/Create","",lcJson)
IF VARTYPE(luRet)<>"O"
	THIS.setError(TRANSFORM(luRet))
	RETURN .F.
ENDIF
RETURN luret
ENDFUNC


&&------------------------------------------------------------------------------
FUNCTION StateProvince_GetbyCountry(lnCountryID)
&&------------------------------------------------------------------------------
lcParams="languageId=0&showHidden=false"
luRet=THIS.CallendPoint("StateProvince/GetStateProvincesByCountryId/"+TRANSFORM(lnCountryID),lcParams,"")
IF VARTYPE(luRet)<>"O"
	THIS.setError(TRANSFORM(luRet))
	RETURN .F.
ENDIF
RETURN luret
ENDFUNC


&&------------------------------------------------------------------------------
FUNCTION Order_Cancel(lnID)
&&------------------------------------------------------------------------------
luRet=THIS.CallendPoint("OrderProcessing/CancelOrder/"+TRANSFORM(lnID),"notifyCustomer=true","")
IF luRet 
	RETURN
ENDIF	
	
IF THIS.cerrormsg="No Response" OR EMPTY(THIS.cerrormsg) OR THIS.cerrormsg=".T."
	RETURN
ENDIF	
RETURN .F.

ENDFUNC

&&------------------------------------------------------------------------------
FUNCTION Product_Get(lnID)
&&------------------------------------------------------------------------------
luRet=THIS.CallendPoint("Product/GetById/"+TRANSFORM(lnID))
IF VARTYPE(luRet)<>"O"
	THIS.setError(TRANSFORM(luRet))
	RETURN .F.
ENDIF
RETURN luret
ENDFUNC


&&------------------------------------------------------------------------------
FUNCTION Product_Create(lcJson)
&&------------------------------------------------------------------------------
luRet=THIS.CallendPoint("Product/Create","",lcJson)
IF VARTYPE(luRet)<>"O"
	THIS.setError(TRANSFORM(luRet))
	RETURN .F.
ENDIF
RETURN luret
ENDFUNC

&&------------------------------------------------------------------------------
FUNCTION Product_Update(lcJson)
&&------------------------------------------------------------------------------
luRet=THIS.CallendPoint("Product/Update","",lcJson,"PUT")
IF VARTYPE(luRet)<>"L"
	THIS.setError(TRANSFORM(luRet))
	RETURN .F.
ENDIF
RETURN luret
ENDFUNC

&&------------------------------------------------------------------------------
FUNCTION Product_Adjust(lnProductID,lnQty,lcMessage)
&&------------------------------------------------------------------------------
lcParam="quantityToChange="+TRANSFORM(lnQty)+"&message="+URLENCODE(TRANSFORM(lcMessage))
luRet=THIS.CallendPoint("Product/AdjustInventory/"+TRANSFORM(lnProductID),lcParam,[""])
IF VARTYPE(luRet)<>"L"
	THIS.setError(TRANSFORM(luRet))
	RETURN .F.
ENDIF
RETURN luret
ENDFUNC

&&------------------------------------------------------------------------------
FUNCTION Product_CREATEWH(lnProductID,lnQty,lnSToreID)
&&------------------------------------------------------------------------------
TEXT TO lcJSON TEXTMERGE NOSHOW
{
  "product_id": <<lnProductID>>,
  "warehouse_id": <<lnStoreID>>,
  "stock_quantity": <<lnQty>>,
  "reserved_quantity": 0,
  "id": 0
}
ENDTEXT
luRet=THIS.CallendPoint("Productwarehouse/create","",lcJson)
IF VARTYPE(luRet)<>"O"
	THIS.setError(TRANSFORM(luRet))
	RETURN .F.
ENDIF
RETURN luret
ENDFUNC

&&------------------------------------------------------------------------------
FUNCTION Product_AdjustWH(lnProductID,lnQty,lnSToreID,lnWHID)
&&------------------------------------------------------------------------------
TEXT TO lcJSON TEXTMERGE NOSHOW
{
  "product_id": <<lnProductID>>,
  "warehouse_id": <<lnStoreID>>,
  "stock_quantity": <<lnQty>>,
  "id": <<lnWHid>>
}
ENDTEXT

luRet=THIS.CallendPoint("Productwarehouse/update","",lcJson,"PUT")
IF VARTYPE(luRet)<>"L"
	THIS.setError(TRANSFORM(luRet))
	RETURN .F.
ENDIF
RETURN luret
ENDFUNC

&&------------------------------------------------------------------------------
FUNCTION ProductCategory_Create(lcJson)
&&------------------------------------------------------------------------------
luRet=THIS.CallendPoint("ProductCategory/Create","",lcJson)
IF VARTYPE(luRet)<>"O"
	THIS.setError(TRANSFORM(luRet))
	RETURN .F.
ENDIF
RETURN luret
ENDFUNC

&&------------------------------------------------------------------------------
FUNCTION ProductManufacturer_Create(lcJson)
&&------------------------------------------------------------------------------
luRet=THIS.CallendPoint("ProductManufacturer/Create","",lcJson)
IF VARTYPE(luRet)<>"O"
	THIS.setError(TRANSFORM(luRet))
	RETURN .F.
ENDIF
RETURN luret
ENDFUNC

&&------------------------------------------------------------------------------
FUNCTION Picture_Insert(lcType,lcSeo,img)
&&------------------------------------------------------------------------------
lcParams="mimeType=image/"+lcType+"&seoFilename="+lcSeo+"&validateBinary=true"
lcUrl=ALLTRIM(this.BaseUrl)+"Picture/InsertPicture?"+lcParams
WITH THIS
this.oHTTP.AddPostKey()

*lcResponse = this.oHttp.HttpGet(lcUrl)
this.oHttp.cContentType="application/json"
this.oHttp.cHttpVerb="PUT"


this.oHttp.AddPostKey(["]+STRCONV(img,13)+["])

lcResponse = .oHttp.HttpGet(lcUrl)
IF .oHttp.nError # 0
	.lError=.t.
	=logstring(lcUrl+" "+lcJson+" "+lcResponse,"_NOPAPI.log")
	THIS.SetError(TRANSFORM(.oHttp.nError)+"-"+.oHttp.cErrormsg)
	RETURN .f.
ENDIF

*** Then check the result code
IF .oHttp.cResultCode # "200" AND .oHttp.cResultCode # "304"
   =logstring(lcUrl+" "+lcJson+" "+lcResponse,"_NOPAPI.log")
	THIS.SetError(oHttp.cResultCode+"-"+.oHttp.cResultCodeMessage)  && Echo message from server
	RETURN .f.
ENDIF
ENDWITH

lError=.F.
TRY 
	lo=THIS.oSer.DeSerializeJson(lcResponse)
	
CATCH
	lo=lcResponse
ENDTRY
RETURN lo

ENDFUNC

&&------------------------------------------------------------------------------
FUNCTION OrderNote_Create(lcJson)
&&------------------------------------------------------------------------------
luRet=THIS.CallendPoint("OrderNote/Create","",lcJson)
IF VARTYPE(luRet)<>"O"
	THIS.setError(TRANSFORM(luRet))
	RETURN .F.
ENDIF
RETURN luret
ENDFUNC


&&------------------------------------------------------------------------------
FUNCTION PrivateMessage_Create(lcJson)
&&------------------------------------------------------------------------------
luRet=THIS.CallendPoint("PrivateMessage/Create","",lcJson)
IF VARTYPE(luRet)<>"O"
	THIS.setError(TRANSFORM(luRet))
	RETURN .F.
ENDIF
RETURN luret
ENDFUNC
*!*	{
*!*	  "store_id": 0,
*!*	  "from_customer_id": 0,
*!*	  "to_customer_id": 0,
*!*	  "subject": "string",
*!*	  "text": "string",
*!*	  "is_read": true,
*!*	  "is_deleted_by_author": true,
*!*	  "is_deleted_by_recipient": true,
*!*	  "created_on_utc": "2023-03-24T15:34:17.486Z",
*!*	  "id": 0
*!*	}

&&------------------------------------------------------------------------------
FUNCTION ShipMent_Create(lcJson)
&&------------------------------------------------------------------------------
luRet=THIS.CallendPoint("Shipment/Create","",lcJson)
IF VARTYPE(luRet)<>"O"
	THIS.setError(TRANSFORM(luRet))
	RETURN .F.
ENDIF
RETURN luret
ENDFUNC

&&------------------------------------------------------------------------------
FUNCTION ShipMentItem_Create(lcJson)
&&------------------------------------------------------------------------------
luRet=THIS.CallendPoint("ShipmentItem/Create","",lcJson)
IF VARTYPE(luRet)<>"O"
	THIS.setError(TRANSFORM(luRet))
	RETURN .F.
ENDIF
RETURN luret
ENDFUNC
*!*	{
*!*	  "shipment_id": 0,
*!*	  "order_item_id": 0,
*!*	  "quantity": 0,
*!*	  "warehouse_id": 0,
*!*	  "id": 0
*!*	}

&&------------------------------------------------------------------------------
FUNCTION ShipMent_Send(lnShipID,lDontnotify)
&&------------------------------------------------------------------------------
&&lcparam=IIF(EMPTY(lDontnotify),"notifyCustomer=false","")
luRet=THIS.CallendPoint("OrderProcessing/Ship/"+TRANSFORM(lnShipID),"notifyCustomer=false","")
IF VARTYPE(luRet)<>"L"
	THIS.setError(TRANSFORM(luRet))
	RETURN .F.
ENDIF
RETURN luret
ENDFUNC


&&------------------------------------------------------------------------------
FUNCTION ShipMent_Deliver(lnShipID,lDontnotify)
&&------------------------------------------------------------------------------
lcparam=IIF(EMPTY(lDontnotify),"notifyCustomer=true","notifyCustomer=false")
luRet=THIS.CallendPoint("OrderProcessing/Deliver/"+TRANSFORM(lnShipID),lcParam,"")
IF VARTYPE(luRet)<>"L"
	THIS.setError(TRANSFORM(luRet))
	RETURN .F.
ENDIF
RETURN luret
ENDFUNC


&&------------------------------------------------------------------------------
FUNCTION LoadJSonObjectTemplate(lcEntity)
&&------------------------------------------------------------------------------
IF !FILE("OBJECTS\"+lcEntity+".json")
RETURN .f.
ENDIF
RETURN FILETOSTR("OBJECTS\"+lcEntity+".json")


&&------------------------------------------------------------------------------
FUNCTION LoadJSonObject(lcEntity)
&&------------------------------------------------------------------------------
IF !FILE("OBJECTS\"+lcEntity+".json")
RETURN .f.
ENDIF
RETURN THIS.oser.Deserialize(FILETOSTR("OBJECTS\"+lcEntity+".json"))

ENDDEFINE