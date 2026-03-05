&&----------------------------------------------------------
&& CommunicationsApi
&&----------------------------------------------------------
#DEFINE ERROR_LOG     "Commslog"+DTOS(DATE())+".log"
#DEFINE INI_FILE	  "Nopintegration.ini"
#DEFINE LOGTIME		  STRTRAN(LEFT(TIME(),5),":","")
#INCLUDE wconnect.h
DO wconnect
DO WWUTILS
SET PROCEDURE TO wwJsonSerializer ADDIT
SET PROCEDURE TO wwBusinessObject ADDIT
SET DATE YMD
CLEAR



&&----------------------------------------------------------
DEFINE CLASS smsbase AS wwBusinessObject
&&----------------------------------------------------------

	oser = .F.
	url = .F.
	user = .F.
	password = .F.
	token = .F.
	ohttp = .F.
	Name = "smsbase"


	PROCEDURE Init
		THIS.oHttp=CREATEOBJECT("wwHTTP")
		THIS.oHttp.cContentType = "application/json"
		this.oser=CREATEOBJECT("WWJSONSERIALIZER")
	ENDPROC


	PROCEDURE authenticate
	ENDPROC


	PROCEDURE sendsms
	ENDPROC


ENDDEFINE

&&----------------------------------------------------------
DEFINE CLASS yomo AS smsbase
&&----------------------------------------------------------
	url = "https://rest.mymobileapi.com/v1"
	user = "a32e917b-bbbc-4451-b420-bee740c26c02"
	password = "REDACTED"
	Name = "yomo"


	PROCEDURE sendsms
		LPARAMETERS lcMobile,lcMessage

		TEXT TO lcJson NOSHOW TEXTMERGE
		{
		  "Messages": [
		    {
		      "Content": "<<lcMessage>>",
		      "Destination": "<<ALLTRIM(lcMobile)>>"
		    }
		  ]
		}
		ENDTEXT

		THIS.oHttp.AppendHeader("Authorization","Bearer "+this.token)
		THIS.oHttp.AddPostKey(lcJson)
		lcResponse = THIS.oHttp.HttpGet(THIS.Url+"/bulkmessages")
		lerror=.f.

		IF THIS.oHTTP.nError # 0
		    THIS.SetError("Unable to send SMS. ("+THIS.oHttp.cErrorMSg+")"+lcResponse)
			RETURN  .f.
		ELSE
		   IF THIS.oHTTP.cResultCode # "200" AND THIS.oHTTP.cResultCode # "304"
		   		THIS.SetError("Unable to send SMS. ("+THIS.oHttp.cErrorMSg+")")
				RETURN  .f.
		   ENDIF
		   
		ENDIF
	ENDPROC

&&----------------------------------------------------------
	PROCEDURE authenticate
&&----------------------------------------------------------	
		lAuth=.f.
			IF VARTYPE(oYomo)="O" 
				IF oYomo.Expiry<DATETIME()-120
					lAuth=.t.
				ENDIF

			ELSE 
				RELEASE oYomo
				PUBLIC oYomo
				oYomo=CREATEOBJECT("EMPTY")
				ADDPROPERTY(oYomo,"Token","")
				ADDPROPERTY(oYomo,"Expiry",DATETIME()-999999)
			ENDIF
			IF lAuth
				THIS.Token=oYomo.Token
				RETURN
			ENDIF
			THIS.oHttp.AppendHeader("Authorization","Basic "+STRCONV(this.User+":"+THIS.Password,13))
			*THIS.oHttp.AppendHeader("Authorization","Basic "YTMyZTkxN2ItYmJiYy00NDUxLWI0MjAtYmVlNzQwYzI2YzAyOkEvVGNBL0E0ZFhrNU8wS29LbXd1UFZnb3kyNUcrVmF1")
			THIS.oHTTP.AddPostKey()  && Clear POST buffer
			lcAuth= THIS.oHTTP.HTTPGet(THIS.Url+"/Authentication")
			IF EMPTY(lcAuth)
				THIS.seterror("Could not get token")
				RETURN .F.
			ENDIF
			lError=.F.
			TRY 
				lo=THIS.oSer.DeSerializeJson(lcAuth)
				oYomo.Expiry=DATETIME()+86400
				oYomo.Token=lo.Token
				this.token=lo.Token
			CATCH
				THIS.seterror("Invalid Token Returned")
				lError=.t.
			ENDTRY
			RETURN !lError
	ENDPROC


ENDDEFINE

&&----------------------------------------------------------
DEFINE CLASS yomonam AS yomo
&&----------------------------------------------------------

	user = "eb66fb0b-6178-4eb7-9030-22d11664a657"
	password = "REDACTED"
	Name = "yomonam"


ENDDEFINE
&&----------------------------------------------------------
DEFINE CLASS yomouk AS yomo
&&----------------------------------------------------------


	user = "8184bf13-3166-448f-8dfe-f044ace7ccb2"
	password = "REDACTED"
	Name = "yomouk"


ENDDEFINE


&&----------------------------------------------------------
DEFINE CLASS OTP AS wwBusinessObject
&&----------------------------------------------------------

	oser = .F.
	url = .F.
	user = .F.
	password = .F.
	token = .F.
	ohttp = .F.
	Name = "OTP"

&&----------------------------------------------------------
FUNCTION GenerateOTP(lnCustomerID,SendVia,liLive)
&&----------------------------------------------------------

oMail=CREATEOBJECT("SendMail")
loObject=CREATEOBJECT("EMPTY")
LOCAL liOtp
liOTP=INT(RAND(0)*10000)
oSQL.AddParameter("CLEAR")
oSQL.AddParameter(lnCustomerID,"CustomerID")
oSQL.AddParameter(liOtp,"OTP")
oSQL.AddParameter(liLive,"Stage")
oSQL.AddParameter("","Cell","OUT")
oSQL.AddParameter("","Email","OUT")

IF !oSQL.ExecuteStoredprocedure("sp_NOP_OTPGenerate")
  THIS.SetError("Could not generate OTP")
  RETURN .f.
ENDIF
***Grab the return value
lcMobile=oSQL.oParameters["Cell"].Value
*lcMobile="0724361109"
lcRecipient=oSQL.oParameters["Email"].Value
*lcRecipient="rodney@annique.com"
lcMessage="Your OTP for the Annique.com is "+TRANSFORM(liOtp)
IF SendVia="email"
	
	IF ISNULLOREMPTY(lcRecipient)
		THIS.SetError("Invalid Email")
	    RETURN .f.
	ENDIF

	IF !oMail.SendEmail("Annique Store OTP", lcMessage, lcRecipient)
	    THIS.SetError("Could not send Email")
	    RETURN .f.
	ENDIF
	RETURN
ENDIF
IF ISNULLOREMPTY(lcMobile)
 	 THIS.SetError("Invalid Cell #")
	 RETURN .f.
ENDIF

IF !oMail.SendSMS(lcMobile,lcMessage)
 	THIS.SetError("Could not send SMS")
	 RETURN .f.
ENDIF
ENDDEFINE

&&------------------------------------------------------------------------------
DEFINE CLASS SendMail AS wwBusinessObject
&&------------------------------------------------------------------------------
*******************************************************************
FUNCTION SendEmail(lcSubject, lcMessage, ;
                        lcRecipient, lcSender,lcAttachment, llForce)
*********************************************************************
LOCAL loIP


   lcMessage=IIF(EMPTY(lcMessage),"",lcMessage + CRLF + CRLF)
   lcRecipient=IIF(EMPTY(lcRecipient),Server.oConfig.cAdminEmail,lcRecipient)
   lcSender=IIF(EMPTY(lcSender),Server.oConfig.cAdminEmail,lcSender)



LOCAL loSMTP as wwSmtp
   loSmtp=CREATEOBJECT("wwSmtp")
   loSmtp.nMailMode = 0  && wwIPStuff mode (Win32 - default)   0 - .NET wwSmtp
   loSmtp.cMailServer =Server.oConfig.cAdminMailServer
   loSmtp.cUsername =Server.oConfig.cAdminMailUsername
   loSmtp.cPassword =Server.oConfig.cAdminMailPassword
   loSmtp.cSenderName="Annique Store Admin"
   loSmtp.cSenderEmail="noreply@annique.com"
*** Optional SSL Messages (only in .NET mode (nMailMode = 0))
 loSmtp.lUseSsl = .T.

loSmtp.cRecipient=lcRecipient 
loSmtp.cCCList=""
loSmtp.cBCCList="ITsupport@annique.com"
loSmtp.cSubject=lcSubject

*** Optionally specify content type - text/plain is default
loSmtp.cContentType = "text/plain"  
loSmtp.cMessage=lcmeSSAGE


*** Optionally send file attachments
llResult =loSmtp.SendmailAsync()
*llResult = loSmtp.SendMail()    
=logstring(lcRecipient +" "+lcSubject+"  "+TRANSFORM(llresult),"email.log")
RETURN

ENDFUNC


* ==================================================== *
* SEND SMS
* ==================================================== *
FUNCTION SendSms(lcMobile,lcMessage)
LOCAL loSms
IF "+264"$lcMobile
loSms=CREATE("YOMONAM")
ELSE
loSms=CREATE("YOMO")
ENDIF
lcMessage=STRIPHTML(lcMessage)
=logstring(lcMobile+" "+lcMessage,"email.log")
IF ! loSms.Authenticate()
	THIS.SetError(loSms.cErrorMsg)
	RETURN .f.
ENDIF
IF !loSms.SendSMS(lcMobile,lcMessage)
	=logstring(lcMobile+" "+loSms.cErrorMsg,"email.log")
	THIS.SetError(loSms.cErrorMsg)
	RETURN .f.
ENDIF	
ENDFUNC

ENDDEFINE


DEFINE CLASS NopintegrationConfig AS wwServerConfig
ENDDEFINE


&&--------------------------------------------------------------------
FUNCTION ValidateEmail(lcEmail)
&&--------------------------------------------------------------------
LOCAL oHttp,oSer

oHttp=CREATEOBJECT("wwHTTP")
oHttp.cContentType = "application/json"
oser=CREATEOBJECT("WWJSONSERIALIZER")
lerror=.f.
lcUrl="https://api.emailable.com/v1/verify?api_key=REDACTED&email="+ALLTRIM(TRANSFORM(lcEmail))
lcResponse = oHttp.HttpGet(lcUrl)
lerror=.f.
IF oHTTP.nError # 0
    cErrorMsg=oHttp.cErrorMSg
	lerror=.t.
	RETURN  "Unable to verify email address serice unavailable"
ELSE
   IF oHTTP.cResultCode # "200" 
   		RETURN "Unable to verify email address"
   ENDIF
   
TRY 
	luret=oSER.deserialize(lcResponse)
CATCH	
		lerror=.t.
	
ENDTRY	
IF lError
RETURN "Unable to verify email address"
ENDIF	
	
	IF INLIST(luRet.state,'unknown','undeliverable')
		RETURN "Invalid email address"
	ELSE
		RETURN ""
	ENDIF	


ENDIF
ENDFUNC