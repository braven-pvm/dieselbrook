#INCLUDE wconnect.h
DO wconnect
DO WWUTILS
SET PROCEDURE TO SyncClass ADDIT
SET PROCEDURE TO BaseData ADDIT
SET PROCEDURE TO WWSQL ADDIT
SET PROCEDURE TO wwJsonSerializer ADDIT
SET PROCEDURE TO wwBusinessObject ADDIT
SET PROCEDURE TO NOPAPI ADDIT
SET PROCEDURE TO NOPData ADDIT
SET PROCEDURE TO AMData ADDIT
SET DATE YMD
CLEAR




	
*/--------------------------------------------------------------------------------
DEFINE CLASS sso AS Custom
*/--------------------------------------------------------------------------------

	url = ""
	api = "/wp-json/"
	username = ""
	password = ""
	admintoken = ""
	token = .F.
	token_expires={}
	oser = .F.
	ohttp = .F.
	oX=.f.
	oSettings=.f.
	applicationpassword=''
	lerror=.f.
	cerrormsg=""
	sitename="Support"
	
*/--------------------------------------------------------------------------------
FUNCTION INIT
*/--------------------------------------------------------------------------------
this.oSer=CREATEOBJECT("wwJsonSerializer")
this.oX=CREATEOBJECT("WWXML")
this.oHttp=CREATEOBJECT("WWhttp")
this.oHttp.cContentType = "application/json"
this.oSettings=CREATEOBJECT("Empty")
loSettings = CREATEOBJECT("NopSettings")
loSettings.SetSqlObject(oSql)
loSettings.LoadSettings(this.oSettings)

ENDFUNC	

*/--------------------------------------------------------------------------------
FUNCTION SetError(lcErrorMsg, lnError)
*/--------------------------------------------------------------------------------
IF PCOUNT() = 0
    THIS.cerrormsg = ""
    THIS.lerror = .F.
    RETURN
ENDIF

THIS.cerrormsg = lcErrorMsg
THIS.lerror = .T.
ENDFUNC
*   SetError

*/--------------------------------------------------------------------------------
FUNCTION UpdateSSO (loCus,loBus,PasswordHash)
*/--------------------------------------------------------------------------------

		** Check if it has a 
		IF ISNULLOREMPTY(loBus.oData.ID)
			loBUS.NEW(.t.)
			loBus.oData.CustomerID=loCus.odata.id
			loBus.oData.Site=this.site
		ENDIF

		lnRet=THIS.updateuser(loBUs.oData.WPID,ALLTRIM(loCus.oData.UserName),;
				ALLTRIM(loCus.oData.FirstName),ALLTRIM(loCus.oData.LastName),;
				ALLTRIM(loCus.oData.Email),ALLTRIM(PasswordHash),this.role)

		IF VARTYPE(lnRet)<>"N" OR lnRet<0
			RETURN .F.
		ENDIF

		loBus.oDAta.WPID=lnRet
		loBus.oDAta.lnew=0
		loBus.oDAta.lUpdated=1
		loBus.oData.dUPdated=DATETIME()
		RETURN loBus.Save()

ENDFUNC


*/--------------------------------------------------------------------------------
FUNCTION Authenticateadmin
*/--------------------------------------------------------------------------------	
		
		LOCAL loBus,loCus
		lError=.f.
		lcUserName=this.username
		lcPassword=IIF(this.site='S',this.oSettings.support.adminhash,this.oSettings.academy.adminhash)
		IF EMPTY(lcUserName) OR EMPTY(lcPassword)
			THIS.seterror("Need username and Password")
			RETURN .F.
		ENDIF
		*lcUrl=this.Url+"/wp-json/jwt-auth/v1/token"

		*"username": "<<lcusername>>","password": "<<lcpassword>>",
TEXT TO lcJson TEXTMERGE NOSHOW
		{ "alg" : "HS256" , "typ" : "JWT" }
ENDTEXT

TEXT TO cUrl TEXTMERGE NOSHOW
<<this.url>>?rest_route=/simple-jwt-login/v1/auth&username=<<lcUserName>>&password=<<lcPassword>>
ENDTEXT
		
	    this.oHTTP.AddPostKey()  && Clear POST buffer
		this.oHTTP.AddPostKey(lcJson)
		lcAuth= this.oHTTP.HTTPGet(cUrl)
		IF !ISNULLOREMPTY(lcAuth)
			TRY 
				lo=THIS.oSer.DeSerializeJson(lcAuth)
*				this.admintoken="Bearer "+lo.Token
				this.admintoken=lo.data.jwt
			CATCH
				lError=.t.
				=LOGSTRING(lcAuth + " " + lcJson,"SSO.log")
			ENDTRY
		ELSE
			lError=.t.
		ENDIF
	

		RETURN !lError
ENDFUNC


*/--------------------------------------------------------------------------------
FUNCTION AutoLogin(lcUserName,lcUrl)
*/--------------------------------------------------------------------------------

pcErrormsg=""
lError=.F.
loCus=CREATEOBJECT("Customer")
loCus.SetSqlObject(oNopSql)

IF !loCus.LoadBase("UserName='"+lcUserName+"'")
	THIS.SetError("Could not find user")
	RETURN .F.
ENDIF
		
TEXT TO lcSql NOSHOW TEXTMERGE
SELECT c.id,c.UserName,(select TOP 1 Password FROM CustomerPassword WHERE 
	CustomerID=c.ID ORDER BY CreatedOnUtc DESC) PasswordHash 
FROM Customer c WHERE UserName='<<lcUserName>>'	

ENDTEXT
IF loCus.Query(lcSql,"TUSER")<>1
	THIS.seterror("Could not find user")
	RETURN .F.
ENDIF

*TRY 
IF !this.Authenticate(loCus,Tuser.PasswordHash)
	=LOGSTRING(this.cErrorMsg,"SSO.log")
	this.setError(this.cErrorMsg)
	RETURN .F.
ELSE
	lcQs=""
	lcUrl=this.url+"/?rest_route=/simple-jwt-login/v1/autologin&JWT=" + this.Token+lcQs
ENDIF
RETURN
ENDFUNC


*/--------------------------------------------------------------------------------
FUNCTION Authenticate (loCus,passwordhash)
*/--------------------------------------------------------------------------------

		LOCAL loBus,loCus
		loBus=CREATE("NOPSSO")
		loBus.SETSQLOBJECT(oSql)
		lnCustomerID=loCus.odata.id

		** Check if it has an SSO 
		IF !loBUS.LoadBase("CustomerID="+TRANSFORM(lnCustomerID)+" AND Site='"+THIS.Site+"'") 
			IF !THIS.updatesso(loCus,loBus,passwordHash)
				RETURN .F.
			ENDIF
			loBUS.LoadBase("CustomerID="+TRANSFORM(lnCustomerID)+" AND Site='"+THIS.Site+"'")
		ENDIF

		IF loBus.oData.Expiry > DATETIME()
			THIS.token=loBus.oData.Token
			RETURN
		ENDIF

		lcUserName=ALLTRIM(loCus.oData.UserName)
		lcPassword=ALLTRIM(PasswordHASH)
		IF EMPTY(lcUserName) OR EMPTY(lcPassword)
			THIS.seterror("Need username and Password")
			RETURN .F.
		ENDIF

		TEXT TO lcJson TEXTMERGE NOSHOW
			{ "alg" : "HS256" , "typ" : "JWT" }
		ENDTEXT


TEXT TO cUrl TEXTMERGE NOSHOW
<<this.url>>?rest_route=/simple-jwt-login/v1/auth&username=<<lcUserName>>&password=<<lcPassword>>
ENDTEXT
		oHttp=CREATEOBJECT("wwhttp")
		oHttp.cContentType="application/json"
		oHTTP.AddPostKey()  && Clear POST buffer
		oHTTP.AddPostKey(lcJson)
		
		lcJ=lcJson
		lcAuth= oHTTP.HTTPGet(cUrl)
		IF EMPTY(lcAuth)
			THIS.seterror("Could not get token")
			RETURN .F.
		ENDIF
		IF "Wrong user credentials"$lcauth OR "Lost your password"$lcAuth OR "Unknown username"$lcAuth OR "password is an invalid"$lcAuth 
			lnRet=THIS.updateuser(loBUs.oData.WPID,ALLTRIM(loCus.oData.UserName),;
				ALLTRIM(loCus.oData.FirstName),ALLTRIM(loCus.oData.LastNAme),;
				ALLTRIM(loCus.oData.Email),ALLTRIM(PasswordHash),this.role)
				IF lnret<1
					=LOGSTRING("Could not reset password of"+ALLTRIM(loCus.oData.UserName),"SSO.log")
					RETURN .F.
				ENDIF

			oHTTP.AddPostKey()  && Clear POST buffer
			oHTTP.AddPostKey(lcJson)
			lcJSon=lcJ
			lcAuth= oHTTP.HTTPGet(cUrl)
			*lcAuth= oHTTP.HTTPGet(this.url+"/wp-json/aam/v1/authenticate")
			
			IF EMPTY(lcAuth)
				THIS.seterror("Could not get token")
				RETURN .F.
			ENDIF
		ENDIF

		lError=.F.
		TRY 
			lo=THIS.oSer.DeSerialize(lcAuth)
			this.token=lo.data.jwt
			this.token_expires=DTOT(DATE()+1)
			&&DATETIME(1970,01,01,0,0,0)+lo.jwt.token_expires

		CATCH
			THIS.seterror("Invalid Token Returned "+lcAuth)
			lError=.t.
		ENDTRY


		IF lError
			*oBUS.DELETE(lnCustomerID)
			RETURN .F.
		ENDIF
		loBus.oData.Expiry=THIS.token_expires
		loBus.oData.Token=This.Token
		RETURN loBus.Save()

ENDFUNC

*/--------------------------------------------------------------------------------
FUNCTION UpdateUser
*/--------------------------------------------------------------------------------

LPARAMETERS lnID,lcUserName,lcFirstName,lcLastName,lcEmail,lcPassword,lcRole

		lcFirstName=this.ox.EncodeXML(lcFirstName)
		lcLastName=this.ox.EncodeXML(lcLastName)

		IF !this.authenticateadmin() 
			RETURN .F.
		ENDIF
		IF ISNULL(lnID)
			lnID=0
		ENDIF
		lcCo=THIS.oSettings.Common.Country
		TEXT TO lcJson TEXTMERGE NOSHOW
		{ "email": "<<ALLTRIM(lcEmail)>>",
		  "name" : "<<ALLTRIM(lcFirstName)+' '+ALLTRIM(lcLastName)+' '+lcco+ALLTRIM(lcUserName)>>",
		  "first_name": "<<ALLTRIM(lcFirstName)>>",
		  "last_name": "<<ALLTRIM(lcLastName)>>",
		  "username": "<<ALLTRIM(lcUserName)>>"
		  <<IIF(!EMPTY(lcPassword),',"password": "'+lcPassword+'"',"")>>  
		  <<IIF(!EMPTY(lcRole),',"roles": ["'+lcRole+'"]',"")>> 
		}
		ENDTEXT
		

		lcUrl=THIS.Url+THIS.api+"wp/v2/users/"+IIF(!EMPTY(lnID),TRANSFORM(lnID),"")
		oHttp=CREATEOBJECT("wwhttp")
		oHttp.cContentType="application/json"
		*oHTTP.cExtraHeaders = 	"Authorization: Bearer "+THIS.AdminToken
		oHTTP.AddPostKey()  && Clear POST buffer
		oHTTP.AddPostKey(lcJson)
		lcResponse = oHttp.HttpGet(lcUrl+"?JWT="+THIS.AdminToken)
		lnResult   = oHTTP.nError
		lcOriginal = lcResponse
		
		IF "existing_user"$lcResponse
			lcUrl=THIS.Url+THIS.api+"wp/v2/users/?slug="+ALLTRIM(lcUserName)+;
			"&JWT="+oSSo.admintoken
			oHTTP.AddPostKey()  && Clear POST buffer
			lcResponse = oHttp.HttpGet(lcUrl)
			lnResult   = oHTTP.nError
			IF lnResult<>0
				RETURN -1
			ENDIF
			EXTERNAL ARRAY loc
			loC = this.oser.Deserialize(lcResponse)
			loMessage=this.oser.Deserialize(lcOriginal)
			
			IF !PEMSTATUS(loC,"count",5) OR loc.count=0
				THIS.SetError("User could not be added to "+this.sitename+" -"+;
				IIF(PEMSTATUS(loMessage,"message",5),loMessage.message,""))
				
				RETURN -1
			ENDIF
			IF !PEMSTATUS(loC[1],"id",5)
				 RETURN -1
			ENDIF
			lcUrl=THIS.Url+THIS.api+"wp/v2/users/"+TRANSFORM(loC[1].ID)
			oHTTP.AddPostKey()  && Clear POST buffer
			TEXT TO lcJson TEXTMERGE NOSHOW
       { "name" : "<<ALLTRIM(lcFirstName)+' '+ALLTRIM(lcLastName)+' '+lcco+ALLTRIM(lcUserName)>>",
		  "first_name": "<<ALLTRIM(lcFirstName)>>",
		  "last_name": "<<ALLTRIM(lcLastName)>>",
		  "username": "<<ALLTRIM(lcUserName)>>"
		  <<IIF(!EMPTY(lcPassword),',"password": "'+lcPassword+'"',"")>>  
		}
		ENDTEXT
			oHTTP.AddPostKey(lcJson)
			lcResponse = oHttp.Put(lcUrl+"?JWT="+THIS.AdminToken)
			lnResult   = oHTTP.nError
			IF lnResult=400
				loU = this.oser.Deserialize(lcResponse)
				THIS.SetError(lou.message)
							
				RETURN -1
			ENDIF
			IF lnResult<>0
				RETURN -1
			ENDIF
			IF ATC("{",lcResponse)=0
				RETURN -1
			ENDIF
			
			loU = this.oser.Deserialize(lcResponse)
			IF PEMSTATUS(loU,"id",5)
				 RETURN loU.ID
			ELSE
				RETURN -1
			ENDIF

		ENDIF


		DO CASE
		CASE lnResult=500
			THIS.Seterror(lcResponse + " " + lcJson)
			RETURN -1


		CASE lnResult=0
			IF ATC("{",lcResponse)=0
				RETURN -1
			ENDIF
			*lcJson=SUBSTR(lcResponse,ATC("{",lcResponse))
			loC = this.oser.DeserializeJson(lcResponse)
			IF PEMSTATUS(loC,"id",5)
				 RETURN loC.ID
			ELSE
				RETURN -1
			ENDIF
		OTHERWISE

			RETURN	-1
		ENDCASE
ENDFUNC




*/--------------------------------------------------------------------------------
PROCEDURE setextraheaders
*/--------------------------------------------------------------------------------
	PARAMETERS oHttp
	cUserName=THIS.username
	cPassword=THIS.applicationpassword
	ckey=STRCONV(cUserName+":"+cPassword,13)
	oHTTP.cExtraHeaders = ;
		oHTTP.cExtraHeaders + ;
		"Authorization: Basic "+cKey 
ENDPROC


ENDDEFINE



*/--------------------------------------------------------------------------------
DEFINE CLASS Support AS sso
*/--------------------------------------------------------------------------------
	Name = "sso"
	Site = "S"
	url =  "https://crm.anniquestore.co.za"
	username="Administrator"
	password=""
	role="consultant"

*/--------------------------------------------------------------------------------
FUNCTION Support(lcUserName,lcUrl)
*/--------------------------------------------------------------------------------

pcErrormsg=""
lError=.F.
loCus=CREATEOBJECT("Customer")
loCus.SetSqlObject(oNopSql)

IF !loCus.LOadBase("UserName='"+lcUserName+"'")
	? "Could not find user"
	RETURN .F.
ENDIF
		
TEXT TO lcSql NOSHOW TEXTMERGE
SELECT c.id,c.UserName,(select TOP 1 Password FROM CustomerPassword WHERE 
	CustomerID=c.ID ORDER BY CreatedOnUtc DESC) PasswordHash 
FROM Customer c WHERE UserName='<<lcUserName>>'	

ENDTEXT
IF loCus.Query(lcSql,"TUSER")<>1
	THIS.seterror("Could not find user")
	RETURN .F.
ENDIF

*TRY 
IF !this.Authenticate(loCus,Tuser.PasswordHash)
	=LOGSTRING(this.cErrorMsg,"SSO.log")
	this.setError("Support unavailable at the moment..")
	RETURN .F.
ELSE

	lcQs=""

	lcUrl="https://crm.anniquestore.co.za/?rest_route=/simple-jwt-login/v1/autologin&JWT=" + this.Token+lcQs
ENDIF
RETURN
ENDFUNC



*/--------------------------------------------------------------------------------
FUNCTION SupportNam(lcUserName,lcUrl)
*/--------------------------------------------------------------------------------

pcErrormsg=""
lError=.F.
loCus=CREATEOBJECT("Customer")
loCus.SetSqlObject(oNamSql)

IF !loCus.LOadBase("UserName='"+lcUserName+"'")
	? "Could not find user"
	RETURN .F.
ENDIF
		
TEXT TO lcSql NOSHOW TEXTMERGE
SELECT c.id,c.UserName, PasswordHash FROM Customer c WHERE UserName='<<lcUserName>>'	
ENDTEXT
IF loCus.Query(lcSql,"TUSER")<>1
	THIS.seterror("Could not find user")
	RETURN .F.
ENDIF

*TRY 
IF !this.AuthenticateNam(loCus,Tuser.PasswordHash)
	=LOGSTRING(this.cErrorMsg,"SSO.log")
	this.setError("Support unavailable at the moment..")
	RETURN .F.
ELSE

	lcQs=""

	lcUrl="https://crm.anniquestore.co.za/?rest_route=/simple-jwt-login/v1/autologin&JWT=" + this.Token+lcQs
ENDIF
RETURN
ENDFUNC

*/--------------------------------------------------------------------------------
FUNCTION AuthenticateNam (loCus,passwordhash)
*/--------------------------------------------------------------------------------

		LOCAL loBus,loCus
		loBus=CREATE("NOPSSO")
		loBus.SETSQLOBJECT(oSql)
		lnCustomerID=loCus.odata.id

		** Check if it has an SSO 
		IF !loBUS.LoadBase("CustomerID="+TRANSFORM(lnCustomerID)+" AND Site='"+THIS.Site+"'") 
			IF !THIS.updatesso(loCus,loBus,passwordHash)
				RETURN .F.
			ENDIF
			loBUS.LoadBase("CustomerID="+TRANSFORM(lnCustomerID)+" AND Site='"+THIS.Site+"'")
		ENDIF

		IF loBus.oData.Expiry > DATETIME()
			THIS.token=loBus.oData.Token
			RETURN
		ENDIF

		lcUserName=ALLTRIM(loCus.oData.UserName)
		lcPassword=ALLTRIM(PasswordHASH)
		IF EMPTY(lcUserName) OR EMPTY(lcPassword)
			THIS.seterror("Need username and Password")
			RETURN .F.
		ENDIF

		TEXT TO lcJson TEXTMERGE NOSHOW
			{ "alg" : "HS256" , "typ" : "JWT" }
		ENDTEXT


TEXT TO cUrl TEXTMERGE NOSHOW
<<this.url>>?rest_route=/simple-jwt-login/v1/auth&username=<<lcUserName>>&password=<<lcPassword>>
ENDTEXT
		oHttp=CREATEOBJECT("wwhttp")
		oHttp.cContentType="application/json"
		oHTTP.AddPostKey()  && Clear POST buffer
		oHTTP.AddPostKey(lcJson)
		
		lcJ=lcJson
		lcAuth= oHTTP.HTTPGet(cUrl)
		IF EMPTY(lcAuth)
			THIS.seterror("Could not get token")
			RETURN .F.
		ENDIF
		IF "Wrong user credentials"$lcauth OR "Lost your password"$lcAuth OR "Unknown username"$lcAuth OR "password is an invalid"$lcAuth 
			lnRet=THIS.updateuser(loBUs.oData.WPID,ALLTRIM(loCus.oData.UserName),;
				ALLTRIM(loCus.oData.FirstName),ALLTRIM(loCus.oData.LastNAme),;
				ALLTRIM(loCus.oData.Email),ALLTRIM(PasswordHash),this.role)
				IF lnret<1
					=LOGSTRING("Could not reset password of"+ALLTRIM(loCus.oData.UserName),"SSO.log")
					RETURN .F.
				ENDIF

			oHTTP.AddPostKey()  && Clear POST buffer
			oHTTP.AddPostKey(lcJson)
			lcJSon=lcJ
			lcAuth= oHTTP.HTTPGet(cUrl)
			*lcAuth= oHTTP.HTTPGet(this.url+"/wp-json/aam/v1/authenticate")
			
			IF EMPTY(lcAuth)
				THIS.seterror("Could not get token")
				RETURN .F.
			ENDIF
		ENDIF

		lError=.F.
		TRY 
			lo=THIS.oSer.DeSerialize(lcAuth)
			this.token=lo.data.jwt
			this.token_expires=DTOT(DATE()+1)
			&&DATETIME(1970,01,01,0,0,0)+lo.jwt.token_expires

		CATCH
			THIS.seterror("Invalid Token Returned "+lcAuth)
			lError=.t.
		ENDTRY


		IF lError
			*oBUS.DELETE(lnCustomerID)
			RETURN .F.
		ENDIF
		loBus.oData.Expiry=THIS.token_expires
		loBus.oData.Token=This.Token
		RETURN loBus.Save()

ENDFUNC


ENDDEFINE


*/--------------------------------------------------------------------------------
DEFINE CLASS academy AS SSO
*/--------------------------------------------------------------------------------
url="https://www.anniqueacademy.com/"
username="apidev"
password="REDACTED"
Site = "A"
sitename="Academy"
role="subscriber"

*/--------------------------------------------------------------------------------
FUNCTION Academy
*/--------------------------------------------------------------------------------
	pcErrorMsg=""
	

	pcToken="" &&SESSION.GetSessionVar("academy-token") 
	IF EMPTY(pcToken)
		lcKey=SESSION.getSessionVar("ap")
		lcPassword=DECRYPT(STRCONV(lcKey,16),"REDACTED",1024)
		THIS.AcademyAuthentication(Process.cAuthenticatedUser, lcPassword)
		pcToken=SESSION.GetSessionVar("academy-token") 
	ENDIF
	
	
	IF !EMPTY(pcToken)

		lcUrl="https://www.anniqueacademy.com/?rest_route=/simple-jwt-login/v1/autologin&JWT="+pcToken
		process.StandardPage(pcErrorMsg, "<p>Redirecting to Academy...", ,2,lcUrl)
		RETURN
	ENDIF
	process.StandardPage(pcErrorMsg, "<p>Redirecting to Selection Page...", ,2,REQUEST.GetReferrer())

ENDFUNC


*/--------------------------------------------------------------------------------
FUNCTION AcademyAuthentication (lcUserName,lcPassword)
*/--------------------------------------------------------------------------------
	pcErrorMsg=""
	pcToken=""
	LOCAL loUser,loBus
	loUser=CREATEOBJECT("cCustomer")
	loUser.SetSQLObject(oSQL)
	IF !loUser.Load(Process.nCustomerid)
		RETURN .F.
	ENDIF	

	loBus.SetSQLObject(oSQL)
	IF !ISNULLOREMPTY(loUser.oData.AcademyUserID)
		IF !loBus.authenticate(lcUserName, lcPassword)
			loBus.updatepassword(loUser.oData.AcademyUserID,lcpassword)
			IF !loBus.authenticate(lcUserName, lcPassword)
				pcToken=""
			ELSE
				pcToken=loBus.token	
			ENDIF
		ELSE
			pcToken=loBus.token
		ENDIF	



	ELSE
		lnUserID=loBus.createuser(lcUserName,loUser.oData.cCompany,loUser.oData.cFName,;
		loUser.oData.cLName,loUser.oData.Email,lcPassword)
		IF lnUserId>0
			loUser.oData.AcademyUserID=lnUserID
				loUser.Save()
				IF !loBus.authenticate(lcUserName, lcPassword)
				pcToken=""
			ELSE
				pcToken=loBus.token
			ENDIF	
		ENDIF
	ENDIF
	SESSION.SetSessionVar("academy-token",pcToken)
	RETURN
ENDFUNC




ENDDEFINE
*
*-- EndDefine: academy
**************************************************

**************************************************
*-- Class:        sso (c:\webconnectionprojects\anniquestore\deploy\supportsso.vcx)
*-- ParentClass:  wprest (c:\webconnectionprojects\shopapi\deploy\shopapi.vcx)
*-- BaseClass:    custom
*-- Time Stamp:   10/11/22 11:45:04 AM
*


*
*-- EndDefine: sso
**************************************************

**************************************************
*-- Class:        wprest (c:\webconnectionprojects\shopapi\deploy\shopapi.vcx)
*-- ParentClass:  busbase (c:\development\wconnect\common\rebusiness.vcx)
*-- BaseClass:    custom
*-- Time Stamp:   04/19/22 05:15:02 PM
*
*
DEFINE CLASS wprest AS busbase


	url = "https://anniqueshop.com"
	username = "admin"
	password = "REDACTED"
	admintoken = .F.
	token = .F.
	oser = .F.
	ohttp = .F.
	ocache = .F.
	ck = .F.
	cs = .F.
	baseurl = .F.
	api = .F.
	applicationpassword = .F.
	token_expires = .F.


	PROCEDURE authenticate
		LPARAMETERS lcUserName,lcPassword
		IF EMPTY(lcUserName) OR EMPTY(lcPassword)
			THIS.seterror("Need username and Password")
			RETURN .F.
		ENDIF

		TEXT TO lcJson TEXTMERGE NOSHOW
		{"username": "<<lcUserName>>","password": "<<lcPassword>>"}
		ENDTEXT

		oHttp = CREATEOBJECT("wwHTTP")
		oHttp.cContentType = "application/json"

		*lcAuth=oHTTP.HTTPGet("https://www.anniqueacademy.com/wp-json/aam/v1/authenticate")
		TEXT TO lcUrl TEXTMERGE NOSHOW
		<<this.url>>/wp-json/aam/v1/authenticate
		ENDTEXT
		oHTTP.AddPostKey()  && Clear POST buffer
		oHTTP.AddPostKey(lcJson)
		lcAuth= oHTTP.HTTPGet(lcUrl)
		IF EMPTY(lcAuth)
			THIS.seterror("Could not get token")
			RETURN .F.
		ENDIF
		lError=.F.
		TRY 
			lo=THIS.oSer.DeSerializeJson(lcAuth)
			this.token=lo.Token
			this.token_expires=DATETIME(1970,01,01,0,0,0)+lo.token_expires

		CATCH
			THIS.seterror("Invalid Totken Returned")
			lError=.t.
		ENDTRY
		RETURN !lError
	ENDPROC


	PROCEDURE authenticateadmin
		lcAuth=THIS.oCache.GetItem("shop-token")
		lerror=.f.
		IF !ISNULLOREMPTY(lcAuth)
			TRY 
				lo=THIS.oSer.DeSerializeJson(lcAuth)
				this.admintoken="Bearer "+lo.Token
			CATCH
				lError=.t.
			ENDTRY
			IF !lError
				RETURN
			ENDIF
		ENDIF


		TEXT TO lcUrl TEXTMERGE NOSHOW
		<<this.url>>/wp-json/jwt-auth/v1/token?username=<<this.username>>&password=<<this.password>>
		ENDTEXT
		oHttp = CREATEOBJECT("wwHTTP")
		oHttp.cContentType = "application/json"
		oHTTP.AddPostKey()  && Clear POST buffer
		TEXT TO lcJson TEXTMERGE NOSHOW
		{"username": "<<this.username>>","password": "<<this.password>>"}
		ENDTEXT

		oHTTP.AddPostKey(lcJson)
		lcAuth= oHTTP.HTTPGet(lcUrl)
		IF EMPTY(lcAuth)
			RETURN .F.
		ENDIF
		TRY 
		lo=THIS.oSer.DeSerializeJson(lcAuth)
			this.admintoken="Bearer "+lo.Token
			this.oCache.AddItem("shop-token",lcAuth)
		CATCH
			lError=.t.
		ENDTRY
		RETURN !lError
	ENDPROC


	PROCEDURE updatepassword
		LPARAMETERS lnUserid,lcPassword
		IF EMPTY(lnUserID)
			THIS.seterror("Need a valid User ID")
			RETURN .F.
		ENDIF
		TEXT TO lcJson TEXTMERGE NOSHOW
		{ "password":"<<lcPassword>>"}
		ENDTEXT
		IF !this.authenticateadmin()
			THIS.seterror("Could not get token")
			RETURN .F.
		ENDIF
		oHTTP=CREATEOBJECT("wwHttp")
		oHTTP.cExtraHeaders = ;
			oHTTP.cExtraHeaders + ;
			"Authorization: "+THIS.admintoken +CRLF 
		lcUrl=this.url+"/wp-json/wp/v2/users/"+TRANSFORM(lnUserID)
		oHTTP.AddPostKey()  && Clear POST buffer
		oHTTP.AddPostKey(lcJson)
		lcret=oHTTP.HTTPGet(lcUrl)
		IF EMPTY(lcRet)
			THIS.seterror("Could not update academy user")
			RETURN .F.
		ENDIF
		lError=.F.
		TRY 
		lo=THIS.oSer.DeSerializeJson(lcRet)
		IF !PEMSTATUS(lo,"id",5)
			lError=.T.
		ENDIF

		CATCH
			lError=.t.
		ENDTRY
		IF !lError
			RETURN
		ENDIF
		THIS.seterror("Could not update academy user")
		RETURN lError
	ENDPROC


	PROCEDURE createuser
		LPARAMETERS lcUserName,lcName,lcFirstName,lcLastName,lcEmail,lcPassword

		IF !this.authenticateadmin()
			THIS.seterror("Could not get token")
			RETURN -1
		ENDIF

		TEXT TO lcJson TEXTMERGE NOSHOW
		{ "email": "<<ALLTRIM(lcEmail)>>",
		  "first_name": "<<ALLTRIM(lcFirstName)>>",
		  "last_name": "<<ALLTRIM(lcLastName)>>",
		  "username": "<<ALLTRIM(lcUserName)>>
		  <<IIF(!EMPTY(lcPassword),',"password": ["'+lcPassword+'"]',"")>>  
		  <<IIF(!EMPTY(lcRole),',"roles": ["'+lcRole+'"]',"")>> 
		}
		ENDTEXT
		oHttp = CREATEOBJECT("wwHTTP")
		oHttp.cContentType = "application/json"
		oHTTP.cExtraHeaders = ;
			oHTTP.cExtraHeaders + ;
			"Authorization: "+THIS.admintoken +CRLF 
		lcUrl=this.url+"/wp-json/wp/v2/users/"
		*---------------check if user exists ------------
		oHTTP.AddPostKey()  && Clear POST buffer
		lcret=oHTTP.HTTPGet(lcUrl+"?slug="+ALLTRIM(lcUserName))
		IF EMPTY(lcRet) 
			THIS.seterror("Could not create user")
			RETURN -1
		ENDIF
		lFound=.F.
		IF lcRet<>"[]"
			TRY 
			loX=THIS.oSer.DeSerializeJson(lcRet)

			FOR EACH lo IN loX
				IF !PEMSTATUS(lo,"id",5)
					lFound=.F.
				ELSE
					IF lo.ID>0
						lFound=.T.
						lnId=lo.ID
					ENDIF
				ENDIF
				EXIT
			NEXT

			CATCH
				lFound=.F.
			ENDTRY
		ENDIF

		IF lFound
			IF !THIS.Updatepassword(lnID,ALLTRIM(lcPassword))
				RETURN -1
			ELSE
				RETURN lnId
			ENDIF
		ENDIF

		*---------------ok to create ------------
		oHTTP.AddPostKey()  && Clear POST buffer
		oHTTP.AddPostKey(lcJson)
		lcret=oHTTP.HTTPGet(lcUrl)
		IF EMPTY(lcRet)
			THIS.seterror("Could not create academy user")
			RETURN -1
		ENDIF
		lError=.F.
		TRY 
		lo=THIS.oSer.DeSerializeJson(lcRet)
		IF !PEMSTATUS(lo,"id",5)
			lError=.T.
		ENDIF

		CATCH
			lError=.t.
		ENDTRY
		IF !lError
			RETURN lo.id
		ENDIF
		THIS.seterror("Could not create user")
		RETURN -1
	ENDPROC


	PROCEDURE updateuser
		LPARAMETERS lnID,lcUserName,lcFirstName,lcLastName,lcEmail,lcPassword,lcRole
		LOCAL oHttp,lo
		oHttp=CREATEOBJECT("wwHttp")
		oHttp.cContentType="application/json"
		THIS.SetExtraHeaders(oHttp)
		ox=CREATEOBJECT("WWXML")
		lcFirstName=ox.EncodeXML(lcFirstName)
		lcLastName=ox.EncodeXML(lcLastName)

		TEXT TO lcJson TEXTMERGE NOSHOW
		{ "email": "<<ALLTRIM(lcEmail)>>",
		  "first_name": "<<ALLTRIM(lcFirstName)>>",
		  "last_name": "<<ALLTRIM(lcLastName)>>",
		  "username": "<<ALLTRIM(lcUserName)>>"
		  <<IIF(!EMPTY(lcPassword),',"password": "'+lcPassword+'"',"")>>  
		  <<IIF(!EMPTY(lcRole),',"roles": ["'+lcRole+'"]',"")>> 
		}
		ENDTEXT

		
		TEXT TO lcJson TEXTMERGE NOSHOW
{ "email": "melinda.kotze123450@annique.com",
		  "name" : "MyName MySurname SA000000",
		  "first_name": "MyName",
		  "last_name": "MySurname",
		  "username": "000123450"
		  ,"password": "33650525CF9281F9FB5A71E34B3B61F479F137CCD01ADC6115E50697B673ADEE163107C9A8F7F58B09879F0128922F2BD5DCE73EF77FB1CB462BC3EE34F93D4B","roles": ["consultant"] 
}
ENDTEXT
		
		
		lcUrl=THIS.Url+THIS.api+"wp/v2/users/"+IIF(!EMPTY(lnID),TRANSFORM(lnID),"")
		oHTTP.AddPostKey()  && Clear POST buffer
		oHTTP.AddPostKey(lcJson)
		lcResponse = oHttp.HttpGet(lcUrl)
		lnResult = oHTTP.nError


		DO CASE
		CASE lnResult=500
			THIS.Seterror(lcResponse)
			RETURN -1


		CASE lnResult=0
			IF ATC("{",lcResponse)=0
				RETURN -1
			ENDIF
			*lcJson=SUBSTR(lcResponse,ATC("{",lcResponse))
			loC = this.oser.DeserializeJson(lcResponse)
			IF PEMSTATUS(loC,"id",5)
				 RETURN loC.ID
			ELSE
				RETURN -1
			ENDIF
		OTHERWISE

		RETURN	-1

		ENDCASE


		





	
	PROCEDURE updateuseremail
		LPARAMETERS lnID,lcEmail
		LOCAL oHttp,lo
		oHttp=CREATEOBJECT("wwHttp")
		oHttp.cContentType="application/json"
		THIS.SetExtraHeaders(oHttp)
		ox=CREATEOBJECT("WWXML")


		TEXT TO lcJson TEXTMERGE NOSHOW
		{ "email": "<<ALLTRIM(lcEmail)>>"}
		ENDTEXT

		lcUrl=THIS.Url+THIS.api+"wp/v2/users/"+IIF(!EMPTY(lnID),TRANSFORM(lnID),"")
		oHTTP.AddPostKey()  && Clear POST buffer
		oHTTP.AddPostKey(lcJson)
		lcResponse = oHttp.HttpGet(lcUrl)
		lnResult = oHTTP.nError


		DO CASE
		CASE lnResult=500
			THIS.Seterror(lcResponse)
			RETURN -1


		CASE lnResult=0
			IF ATC("{",lcResponse)=0
				RETURN -1
			ENDIF
			*lcJson=SUBSTR(lcResponse,ATC("{",lcResponse))
			loC = this.oser.DeserializeJson(lcResponse)
			IF PEMSTATUS(loC,"id",5)
				 RETURN loC.ID
			ELSE
				RETURN -1
			ENDIF
		OTHERWISE

		RETURN	-1

		ENDCASE
	ENDPROC

ENDDEFINE



*
*-- EndDefine: wprest
**************************************************

