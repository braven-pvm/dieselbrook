
DEFINE CLASS SyncClass as Custom

oSer=NULL
oHttp=NULL
oXml=NULL
oSettings=NULL
lError=.f.
cErrormsg=""
oConfig=NULL
oNop=NULL
lAuthenticated=.f.
VatRate=15


FUNCTION INIT
this.oSer=CREATEOBJECT("wwJsonSerializer")
this.oXml=CREATEOBJECT("WWXML")
this.oSettings=CREATEOBJECT("Empty")
loSettings = CREATEOBJECT("NopSettings")
loSettings.SetSqlObject(oSql)
loSettings.LoadSettings(this.oSettings)
this.vatrate=this.oSettings.common.nvatrate

ENDFUNC


FUNCTION SetUp(lcUrl)

this.oNop=CREATEOBJECT("NOP","IntegrationUser","REDACTED","IntegrationUser@annique.com",lcUrl)
oNop=This.oNop
IF THIS.oNop.Authenticate()
	THIS.lAuthenticated=.t.
ELSE	
	THIS.SetError("Could not Authenticate to NopCommerce")
	RETURN .F.
ENDIF
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

ENDDEFINE