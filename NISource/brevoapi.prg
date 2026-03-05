#DEFINE ERROR_LOG     "Synchlog"+DTOS(DATE())+".log"
#DEFINE INI_FILE	  "nop.ini"
#DEFINE LOGTIME		  STRTRAN(LEFT(TIME(),5),":","")
#INCLUDE wconnect.h
DO wconnect
DO WWUTILS
SET PROCEDURE TO WWSQL ADDIT
SET PROCEDURE TO wwJsonSerializer ADDIT
SET PROCEDURE TO wwBusinessObject ADDIT
SET PROCEDURE TO NOPAPI ADDIT
SET DATE YMD



DEFINE CLASS Brevo as Custom

ClientID=0
Email=""
UserName=""
Password=""
Token="REDACTED"
BaseUrl="https://api.brevo.com/v3/"
PageNumber=0
PageSize=100
EndPoint=NULL
oSer=NULL
oHttp=NULL
oApi=NULL
lError=.f.
cErrormsg=""
oConfig=NULL
cMasterList=9 


FUNCTION Init(lcUrl,lcToken)
WITH THIS
.oApi=CREATEOBJECT("wwApi")
.oSer=CREATEOBJECT("wwJsonSerializer")
.oHttp=CREATEOBJECT("wwHttp")
.oHttp.cContentType = "application/json"
.oHttp.AppendHeader("Accept","application/json")
.oHttp.AppendHeader("api-key",IIF(EMPTY(lcToken),.Token,lctoken))
.BaseUrl=IIF(EMPTY(lcUrl),.BaseUrl,lcUrl)

RETURN 
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
FUNCTION CallEndPoint (lcEndpoint,lcParams,lcJson,lcVerb,isPath)
&&------------------------------------------------------------------------------

LOCAL lo
lcParams=IIF(EMPTY(lcParams),"",IIF(!isPath,"?","/")+lcParams)
loObj=CREATEOBJECT("EMPTY")

WITH THIS

.oHttp.nConnectTimeout = 1000
lcUrl=ALLTRIM(this.BaseUrl)+lcEndPoint+lcParams
.oHTTP.AddPostKey()
IF !EMPTY(lcJson)
	.oHttp.AddPostKey(lcJSON)
ENDIF	
IF EMPTY(lcVerb) OR lcVerb<>"PUT"
lcResponse = .oHttp.HttpGet(lcUrl)
ELSE
lcResponse = .oHttp.PUT(lcUrl)
ENDIF
*=logstring(" CALLED :"+lcUrl+" "+lcJson+" "+lcResponse+.oHttp.cResultCode+" "+TRANSFORM(.oHttp.nError),"ym.log")

*** Check for hard HTTP protocol/connection errors first


IF .oHttp.cResultCode $ "404" 
	THIS.SetError("Not Found")
	TRY 
		lo=THIS.oSer.DeSerializeJson(lcResponse)
	CATCH
		lo=lcResponse
	ENDTRY
	RETURN lo
ENDIF

IF .oHttp.cResultCode $ "400,401,402,403" 
	THIS.SetError(TRANSFORM(.oHttp.nError)+"-"+.oHttp.cErrormsg)
	TRY 
		lo=THIS.oSer.DeSerializeJson(lcResponse)
	CATCH
		lo=lcResponse
	ENDTRY
	RETURN lo
ENDIF



IF .oHttp.nError # 0
	.lError=.t.
	=logstring(lcUrl+" "+lcJson+" "+lcResponse,"_BREVOPAPI.log")
	THIS.SetError(TRANSFORM(.oHttp.nError)+"-"+.oHttp.cErrormsg)
	RETURN .f.
ENDIF



lcValidResults="200,201,202,203,204"

*** Then check the result code
IF  !.oHttp.cResultCode $ lcValidResults
   =logstring(lcUrl+" "+lcJson+" "+lcResponse,"_BREVOAPI.log")
	THIS.SetError(.oHttp.cResultCode+"-"+.oHttp.cResultCodeMessage)  && Echo message from server
	RETURN .f.
ENDIF


IF EMPTY(lcResponse) AND .oHttp.cResultCode = "200" 
	THIS.SetError("No Response")
	RETURN .f.
ENDIF

IF (EMPTY(lcResponse) OR lcResponse="{}") AND (.oHttp.cResultCode $  "201,202,203,204" )
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
FUNCTION GetContactDetails(lcID)
&&------------------------------------------------------------------------------

luret=THIS.CallEndPoint("contacts",URLENCODE(TRANSFORM(lcID)),"","",.t.)
IF VARTYPE(luret)<>"O"
	RETURN CREATEOBJECT("EMPTY")
ELSE
	RETURN luRet
ENDIF	
ENDFUNC


&&------------------------------------------------------------------------------
FUNCTION AddContact(lcJson)
&&------------------------------------------------------------------------------

luret=THIS.CallEndPoint("contacts","",lcJson,"",.t.)
IF VARTYPE(luret)<>"O"
	RETURN CREATEOBJECT("EMPTY")
ELSE
	RETURN luRet
ENDIF	
ENDFUNC



&&------------------------------------------------------------------------------
FUNCTION UpdateContact(lcID,lcJson)
&&------------------------------------------------------------------------------

luret=THIS.CallEndPoint("contacts",lcID,lcJson,"PUT",.t.)
IF VARTYPE(luret)<>"O"
	RETURN CREATEOBJECT("EMPTY")
ELSE
	RETURN luRet
ENDIF	
ENDFUNC
&&--{{baseUrl}}/contacts/batch

&&------------------------------------------------------------------------------
FUNCTION UpdateContactBatch(lcJson)
&&------------------------------------------------------------------------------

luret=THIS.CallEndPoint("contacts","batch",lcJson,"",.t.)
IF VARTYPE(luret)<>"O"
	RETURN CREATEOBJECT("EMPTY")
ELSE
	RETURN luRet
ENDIF	

&&------------------------------------------------------------------------------
FUNCTION AddList(lcID,lcJson)
&&------------------------------------------------------------------------------
&&{{baseUrl}}/contacts/lists/:listId/contacts/add
luret=THIS.CallEndPoint("contacts/lists",lcID+"/contacts/add",lcJson,"",.t.)
IF VARTYPE(luret)<>"O"
	RETURN CREATEOBJECT("EMPTY")
ELSE
	RETURN luRet
ENDIF	
ENDFUNC

&&------------------------------------------------------------------------------



&&------------------------------------------------------------------------------
FUNCTION AddAttribute(lcAttrib,lcType)
&&------------------------------------------------------------------------------

&&https://api.brevo.com/v3/contacts/attributes/normal/Personal_Sales_Month' \

? lcAttrib
luret=THIS.CallEndPoint("contacts/attributes/normal",lcAttrib,[{"type": "]+lcType+["}],"",.t.)
lo=CREATEOBJECT("EMPTY")
ADDPROPERTY(lo,"Status","")
DO CASE 
	CASE VARTYPE(luret)="O" 
		ADDPROPERTY(lo,"Error",luRet)
		lo.Status="Fail"
	CASE VARTYPE(luret)="L" AND !luret
		ADDPROPERTY(lo,"Error",this.cerrorMsg)
		lo.Status="Fail"
	CASE VARTYPE(luret)="L"	AND luret
		lo.Status="Success"
		ADDPROPERTY(lo,"Success",.t.)
	OTHERWISE
		ADDPROPERTY(lo,"Error",this.cerrorMsg)
		lo.Status="Fail"
ENDCASE	
&&RETURN lo

? lo.status
IF PEMSTATUS(lo,"Error",5)
	? lo.Error.Message
ENDIF	



ENDFUNC


&&------------------------------------------------------------------------------
FUNCTION SendTransactionEmail(lcSender,lcSenderName,lcTo,lcToName,lctemplateId,lcParams,lcTags)
&& {{baseUrl}}/smtp/email
&&------------------------------------------------------------------------------


IF !ISNULLOREMPTY(lcTags)
	DIMENSION aTags[1]
	=APARSESTRING(@aTags,lcTags,",")
	lcTags=This.oSer.Serialize(@aTags)
ENDIF

TEXT TO lcJSON NOSHOW TEXTMERGE
{
    "sender": {
        "name": "<<lcSenderName>>",
        "email": "<<lcSender>>"
    },
    "to": [
        {
            "email": "<<lcTo>>",
            "name": "<<lcToName>>"
        }
    ],
  
    "templateId": <<lcTemplateID>>
    << IIF(!ISNULLOREMPTY(lcTags),[,"tags": ]+lcTags+[],"") >>
    << IIF(!ISNULLOREMPTY(lcParams),[,"params": {]+lcParams+[}],"") >>
}

ENDTEXT


luret=THIS.CallEndPoint("smtp/email","",lcJson,"POST")
IF VARTYPE(luret)<>"O"
	RETURN CREATEOBJECT("EMPTY")
ELSE
	RETURN luRet
ENDIF	

ENDFUNC

&&------------------------------------------------------------------------------
FUNCTION SendWhatsApp(lcSender,lcSenderName,lcTo,lcToName,lctemplateId,lcParams,lcTags)
&&------------------------------------------------------------------------------


luret=this.GetContactDetails(ALLTRIM(lcto))
IF !PEMSTATUS(luRet,"attributes",5)
	IF VARTYPE(luret)<>"O"
	RETURN CREATEOBJECT("EMPTY")
	ELSE
		RETURN luRet
	ENDIF	
ENDIF	
IF !PEMSTATUS(luRet.attributes,"WHATSAPP",5) OR  !PEMSTATUS(luRet.attributes,"WHATSAPP_CONSENT",5)
 THIS.cErrormsg="Cannot receive Whatsapp"
 RETURN CREATEOBJECT("EMPTY")
ENDIF
IF !luRet.attributes.WHATSAPP_CONSENT
	 THIS.cErrormsg="No Whatsapp Consent"
	 RETURN CREATEOBJECT("EMPTY")
ENDIF

TEXT TO lcJson TEXTMERGE NOSHOW
{  "templateId": <<lctemplateId>>, 
 "senderNumber": "<<lcSender>>",  "contactNumbers": ["<<ALLTRIM(luRet.attributes.WHATSAPP)>>"]
    << IIF(!ISNULLOREMPTY(lcParams),[,"params": {]+lcParams+[}],"") >>
 }
ENDTEXT



luret=THIS.CallEndPoint("whatsapp/sendMessage","",lcJson,"POST")
IF VARTYPE(luret)<>"O"
	RETURN CREATEOBJECT("EMPTY")
ELSE
	RETURN luRet
ENDIF	


ENDDEFINE







