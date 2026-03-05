

DEFINE CLASS accountmate AS busbase

	amserver = ""
	cconnectstring = ""
	ndatamode = 2
	NAME = "accountmate"


	PROCEDURE getnewdocno
	LPARAMETERS lcsystfield, lcautodocno, lcalias, lckeyfield
	cDocno=""
	WITH THIS
		.oSql.AddParameter(lcsystfield,"csystfield","IN")
		.oSql.AddParameter(lcautodocno,"cautodocno","IN")
		.oSql.AddParameter( lcalias,"calias","IN")
		.oSql.AddParameter(lckeyfield,"ckeyfield","IN")
		.oSql.AddParameter("","ckeyvalue")
		.oSql.AddParameter(1,"bfromfront")
		.oSql.AddParameter(10,"nkeylength","IN")
		.oSql.AddParameter("arsyst","csystalias","IN")
		.oSql.AddParameter(0,"bfromio")
		.oSql.AddParameter("","cdocno","OUT")
*? oSQL.Execute("EXEC vsp_am_getnewdocno ?csystfield,?cautodocno,?calias,?ckeyfield,?ckeyvalue,?bfromfront,?nkeylength,?csystalias,?bfromio,?@cdocno",,.t.)
		IF !.oSql.ExecuteStoredProcedure("vsp_am_getnewdocno")
			RETURN .F.
		ENDIF
		lnResultValue = .oSql.oParameters["cdocno"].VALUE
		IF USED("TSQLQUERY")
			=TABLEREVERT(.T.,"TSQLQUERY")
			USE IN TSQLQUERY
		ENDIF
		RETURN lnResultValue

	ENDWITH

*GetNewDocNo("Arsyst.cInvNo", "lAutoInvNo", "Arinvc/Arinvch/Soaord", "cInvNo", .cInvNo)
*GetNewDocNo("Arsyst.cShipNo", , "Soship/Soshiph", "cShipNo")
	ENDPROC


	PROCEDURE sp_assignuid
	THIS.oSql.AddParameter("","cretvalue","OUT")
	IF !THIS.oSql.ExecuteStoredProcedure("vsp_am_assignuidnew")
		RETURN .F.
	ENDIF
	lnResultValue = THIS.oSql.oParameters["cretvalue"].VALUE
	IF USED("TSQLQUERY")
		=TABLEREVERT(.T.,"TSQLQUERY")
		USE IN TSQLQUERY
	ENDIF
	RETURN RIGHT(SYS(2015),5) +  lnResultValue
	ENDPROC


	PROCEDURE new
	LPARAMETERS llNoNewPk
	LOCAL lcPKField, lnPK

*** Not setting lError and cErrorMsg here - let worker methods
*** do it for us. All code here
	THIS.SetError()

** Create a new record object
	IF !THIS.getblankrecord()
		RETURN .F.
	ENDIF
	THIS.nUpdateMode = 2 && New Record

	RETURN .T.
	ENDPROC


	PROCEDURE INIT
	LPARAM loSql,lcAmServer
	IF VARTYPE(loSql)="O"
		THIS.Setsqlobject(loSql)
	ENDIF
	#IF .F.
		IF !EMPTY(lcAmServer)
			THIS.amserver=lcAmServer
		ENDIF
		IF !EMPTY(THIS.amserver)
			THIS.cFileName=ALLTRIM(THIS.amserver)+THIS.cFileName
		ENDIF
		IF VARTYPE(loSql)="O"
			THIS.oSql=loSql
		ELSE
			IF VARTYPE(oSql)="O"
				THIS.oSql=oSql
			ENDIF
		ENDIF
	#ENDIF
	ENDPROC


ENDDEFINE
*
*-- EndDefine: accountmate
**************************************************


**************************************************
*-- Class:        apvend (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- ParentClass:  accountmate (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- BaseClass:    custom
*-- Time Stamp:   07/09/18 08:30:12 AM
*
DEFINE CLASS apvend AS accountmate


	cpkfield = "cvendno"
	cFileName = "apvend"
	calias = "apvend"
	NAME = "apvend"


ENDDEFINE
*
*-- EndDefine: apvend
**************************************************


**************************************************
*-- Class:        arcadr (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- ParentClass:  accountmate (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- BaseClass:    custom
*-- Time Stamp:   04/10/19 03:09:14 PM
*
DEFINE CLASS arcadr AS accountmate


	calias = "ARCADR"
	cFileName = "ARCADR"
	cpkfield = "cCustno+cAddrno"
	nUpdateMode = 2
	NAME = "arcadr"


ENDDEFINE
*
*-- EndDefine: arcadr
**************************************************


**************************************************
*-- Class:        arcapp (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- ParentClass:  accountmate (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- BaseClass:    custom
*-- Time Stamp:   12/05/18 12:22:04 PM
*
DEFINE CLASS arcapp AS accountmate


	calias = "arcapp"
	cFileName = "arcapp"
	cpkfield = "cuid"
	NAME = "arcapp"


ENDDEFINE
*
*-- EndDefine: arcapp
**************************************************


**************************************************
*-- Class:        arcash (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- ParentClass:  accountmate (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- BaseClass:    custom
*-- Time Stamp:   10/09/17 05:40:06 PM
*
DEFINE CLASS arcash AS accountmate


	cFileName = "arcash"
	calias = "arcash"
	cdatapath = ""
	cidtable = ""
	cpkfield = "cuid"
	ndatamode = 2
	NAME = "arcash"


ENDDEFINE
*
*-- EndDefine: arcash
**************************************************


**************************************************
*-- Class:        arcust (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- ParentClass:  accountmate (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- BaseClass:    custom
*-- Time Stamp:   07/15/20 02:52:12 PM
*
DEFINE CLASS arcust AS accountmate


	cFileName = "arcust"
	calias = "arcust"
	cdatapath = ""
	cidtable = ""
	cpkfield = "ccustno"
	cconnectstring = ""
	ndatamode = 2
	NAME = "arcust"


	PROCEDURE VALIDATE

	DODEFAULT()

*** Always clear errors first in case you're reusing
	THIS.oValidationErrors.CLEAR()

	IF THIS.oData.cStatus<>'A'
		THIS.AddValidationError("Consultant is inactive","Account")
	ENDIF


	IF THIS.oValidationErrors.COUNT > 0
		THIS.SetError( THIS.oValidationErrors.ToString() )
		RETURN .F.
	ENDIF

	RETURN .T.
	ENDPROC

&&-------------------------------------------------------------------------------------
	PROCEDURE getlist
&&-------------------------------------------------------------------------------------
	LPARAMETER pcCustno,pcSponsor,pcName,pcSono,lcCursor

	LOCAL loRecord, lcPKField, lnResult

	IF !EMPTY(pcSono) AND (EMPTY(pcCustno) AND EMPTY(pcSponsor) AND EMPTY(pcName))
		lnResult = THIS.oSql.Execute("SELECT ccustno FROM Sosord where cSono LIKE '%"+pcSono+"'","TCUST")
		IF lnResult < 0
			THIS.SetError(THIS.oSql.cErrorMsg)
			RETURN 0
		ENDIF
		pcCustno=TCUST.cCustno
	ENDIF

	lcFIlter=""
	IF !EMPTY(pcCustno)
		lcFIlter=lcFIlter+IIF(!EMPTY(lcFIlter)," AND ","")+;
			"r.ccustno='"+pcCustno+"'"
	ENDIF
	IF !EMPTY(pcSponsor)
		lcFIlter=lcFIlter+IIF(!EMPTY(lcFIlter)," AND ","")+;
			"r.csponsor='"+pcSponsor+"'"
	ENDIF

	IF !EMPTY(pcName)
		lcFIlter=lcFIlter+IIF(!EMPTY(lcFIlter)," AND ","")+;
			"r.cidno='"+pcName+"' or r.cFname like '"+pcName+"%' OR r.cLname like '"+pcName+"%'"+;
			"OR r.cCompany like '"+pcName+"%'"
	ENDIF


	THIS.SetError()

	TEXT TO lcSQL TEXTMERGE NOSHOW
		SELECT R.csponsor,r.ccustno,r.cidno,
		      r.cLname,r.cFname,r.cEmail,r.cPhone2,r.cStatus,
				r.ccompany,ISNULL(c.cCompany,'') SponsorName
		 FROM Arcust R WITH (NOLOCK)
		  LEFT JOIN Arcust c WITH (NOLOCK) ON r.cSponsor=c.ccustno
		<<IIF(!EMPTY(lcFilter)," where ","") + lcFilter>>
		 ORDER BY R.cCompany DESC
	ENDTEXT

	lcOldCursor = THIS.cSQLCursor
	THIS.oSql.cSQLCursor = IIF(!EMPTY(lcCursor),lcCursor,THIS.cSQLCursor)
*lcCursor=THIS.cSQLCursor
	lnResult = THIS.oSql.Execute(lcSql,lcCursor)
	IF lnResult < 0
		THIS.SetError(THIS.oSql.cErrorMsg)
		RETURN 0
	ENDIF
	THIS.cSQLCursor = lcOldCursor
	lnResult = RECCOUNT()
	RETURN lnResult
	ENDPROC


	PROCEDURE am_check_account
	ENDPROC

	PROCEDURE encrypt
		LPARAMETERS lcOriginal 
		IF ISNULLOREMPTY(lcOriginal)
			RETURN ""
		ENDIF
		LOCAL lc
		lcSecret = this.salt
		lc=Encrypt(lcOriginal,this.salt,1024) 
		RETURN STRCONV(lc,13)
	ENDPROC


	PROCEDURE decrypt
		LPARAMETERS lcOriginal 
		IF ISNULLOREMPTY(lcOriginal)
			RETURN ""
		ENDIF
		LOCAL lc
		lc=STRCONV(lcOriginal,14)
		RETURN decrypt(lc,this.salt,1024)
	ENDPROC

ENDDEFINE
*
*-- EndDefine: arcust
**************************************************


**************************************************
*-- Class:        arinvc (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- ParentClass:  accountmate (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- BaseClass:    custom
*-- Time Stamp:   12/05/18 10:58:10 AM
*
DEFINE CLASS arinvc AS accountmate


	calias = "arinvc"
	cFileName = "arinvc"
	cpkfield = "cinvno"
	NAME = "arinvc"


ENDDEFINE
*
*-- EndDefine: arinvc
**************************************************


**************************************************
*-- Class:        aritrs (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- ParentClass:  accountmate (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- BaseClass:    custom
*-- Time Stamp:   12/05/18 11:02:02 AM
*
DEFINE CLASS aritrs AS accountmate


	calias = "aritrs"
	cFileName = "aritrs"
	cpkfield = "cuid"
	NAME = "aritrs"


ENDDEFINE
*
*-- EndDefine: aritrs
**************************************************


**************************************************
*-- Class:        arsyst (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- ParentClass:  accountmate (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- BaseClass:    custom
*-- Time Stamp:   12/06/18 03:00:10 PM
*
DEFINE CLASS arsyst AS accountmate


	cFileName = "arsyst"
	calias = "arsyst"
	cdatapath = ""
	cidtable = ""
	cpkfield = ""
	cconnectstring = ""
	ndatamode = 2
	NAME = "arsyst"


ENDDEFINE
*
*-- EndDefine: arsyst
**************************************************


**************************************************
*-- Class:        cflowrequests (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- ParentClass:  accountmate (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- BaseClass:    custom
*-- Time Stamp:   05/05/22 02:33:11 PM
*
DEFINE CLASS cflowrequests AS accountmate


	calias = "cflowrequests"
	cFileName = "cflowrequests"
	cskipfieldsforupdates = ""
	NAME = "cflowrequests"


ENDDEFINE
*
*-- EndDefine: cflowrequests
**************************************************


**************************************************
*-- Class:        glacct (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- ParentClass:  accountmate (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- BaseClass:    custom
*-- Time Stamp:   06/30/20 02:08:06 PM
*
DEFINE CLASS glacct AS accountmate


	calias = "GLACCT"
	cFileName = "GLACCT"
	cpkfield = "CACCTID"
	NAME = "glacct"


ENDDEFINE
*
*-- EndDefine: glacct
**************************************************


**************************************************
*-- Class:        gltrsn (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- ParentClass:  accountmate (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- BaseClass:    custom
*-- Time Stamp:   07/01/20 11:03:05 AM
*
DEFINE CLASS gltrsn AS accountmate


	calias = "gltrsn"
	cFileName = "gltrsn"
	cpkfield = "cuid"
	NAME = "gltrsn"



ENDDEFINE
*
*-- EndDefine: gltrsn
**************************************************


**************************************************
*-- Class:        icitem (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- ParentClass:  accountmate (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- BaseClass:    custom
*-- Time Stamp:   10/09/17 05:39:08 PM
*
DEFINE CLASS icitem AS accountmate


	cFileName = "icitem"
	calias = "icitem"
	cdatapath = ""
	cidtable = ""
	cpkfield = "citemno"
	ndatamode = 2
	NAME = "icitem"


	PROCEDURE VALIDATE

	DODEFAULT()

*** Always clear errors first in case you're reusing
	THIS.oValidationErrors.CLEAR()


*!*	lcItemno=this.odata.citemno
*!*	IF EMPTY(lcItemno) OR (","$lcItemno OR "'"$lcItemno OR '"'$lcItemno )
*!*		 this.AddValidationError("Invalid Itemno","Item")
*!*	ENDIF

*!*	IF this.oData.cStatus<>'A' OR this.oData.lioitem=0
*!*	   this.AddValidationError("Item is inactive","Item")
*!*	ENDIF

*!*	IF this.oData.lArItem=0
*!*	   this.AddValidationError("Item is not for sale","Item")
*!*	ENDIF

*!*	loWhs=CREATEOBJECT('iciwhs')
*!*	IF !loWhs.loadbase("citemno = '"+lcItemno+"' and  cwarehouse = 'MAIN'")
*!*		lcMBMsg = "WAREHOUSE DOES_NOT_CARRY_ITEM"
*!*		  this.AddValidationError(lcMBMsg ,"Item")
*!*	ENDIF


	IF THIS.oValidationErrors.COUNT > 0
		THIS.SetError( THIS.oValidationErrors.ToString() )
		RETURN .F.
	ENDIF

	RETURN .T.
	ENDPROC


ENDDEFINE
*
*-- EndDefine: icitem
**************************************************


**************************************************
*-- Class:        iciwhs (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- ParentClass:  accountmate (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- BaseClass:    custom
*-- Time Stamp:   10/10/17 10:05:08 AM
*
DEFINE CLASS iciwhs AS accountmate


	cFileName = "iciwhs"
	calias = "iciwhs"
	cdatapath = ""
	cidtable = ""
	cpkfield = "cuid"
	ndatamode = 2
	NAME = "iciwhs"


	PROCEDURE checkstock
	LPARAMETERS nQtyReq
	iciwhs=THIS.oData
	nqtyAvailable = iciwhs.nOnhand - iciwhs.nbook - oOrderlin.qtyreq
	RETURN nqtyAvailable >= 0
	ENDPROC


	PROCEDURE updatebooked
	LPARAMETERS lcItemno,lcWarehouse,lnQty

	lcSqlCmd = "update "+THIS.cFileName+" set nbook = nbook + "+TRANS(lnQty)+ ;
		"  where citemno = '"+lcItemno+"' and cwarehouse = '"+lcWarehouse+"'"

	RETURN THIS.oSql.EXECUTENONQUERY(lcSqlCmd)
	ENDPROC


ENDDEFINE
*
*-- EndDefine: iciwhs
**************************************************



**************************************************
*-- Class:        popord (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- ParentClass:  accountmate (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- BaseClass:    custom
*-- Time Stamp:   07/09/18 11:48:05 AM
*
DEFINE CLASS popord AS accountmate


	calias = "popord"
	cFileName = "popord"
	cpkfield = "cSono"
	NAME = "popord"



ENDDEFINE
*
*-- EndDefine: popord
**************************************************


**************************************************
*-- Class:        poptrs (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- ParentClass:  accountmate (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- BaseClass:    custom
*-- Time Stamp:   07/09/18 11:49:04 AM
*
DEFINE CLASS poptrs AS accountmate


	calias = "poptrs"
	cFileName = "poptrs"
	cpkfield = "cUID"
	NAME = "poptrs"


ENDDEFINE
*
*-- EndDefine: poptrs
**************************************************

**************************************************
*-- Class:       skytrack
*-- ParentClass:  accountmate (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- BaseClass:    custom
*-- Time Stamp:   07/09/18 11:49:04 AM
*
DEFINE CLASS skytrack AS accountmate


	calias = "skytrack"
	cFileName = "skytrack"
	cpkfield = "eventid"
	NAME = "skytrack"


ENDDEFINE
*
*-- EndDefine: poptrs
**************************************************

**************************************************
*-- Class:        soportal (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- ParentClass:  accountmate (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- BaseClass:    custom
*-- Time Stamp:   10/10/17 10:36:07 AM
*
DEFINE CLASS soportal AS accountmate


	cFileName = "soportal"
	calias = "soportal"
	cdatapath = ""
	cidtable = ""
	ndatamode = 2
	NAME = "soportal"


	PROCEDURE new

	LPARAMETERS llNoNewPk
	LOCAL lcPKField, lnPK

*** Not setting lError and cErrorMsg here - let worker methods
*** do it for us. All code here
	THIS.SetError()

** Create a new record object
	IF !THIS.getblankrecord()
		RETURN .F.
	ENDIF

	THIS.nUpdateMode = 2 && New Record

	RETURN .T.
	ENDPROC


ENDDEFINE
*
*-- EndDefine: soportal
**************************************************


**************************************************
*-- Class:        soship (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- ParentClass:  accountmate (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- BaseClass:    custom
*-- Time Stamp:   12/05/18 11:12:10 AM
*
DEFINE CLASS soship AS accountmate


	calias = "soship"
	cFileName = "soship"
	cpkfield = "cShipno"
	NAME = "soship"


ENDDEFINE
*
*-- EndDefine: soship
**************************************************


**************************************************
*-- Class:        sosord (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- ParentClass:  accountmate (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- BaseClass:    custom
*-- Time Stamp:   10/09/17 05:38:05 PM
*
DEFINE CLASS sosord AS accountmate


	cFileName = "sosord"
	calias = "sosord"
	cdatapath = ""
	cidtable = ""
	cpkfield = "csono"
	ndatamode = 2
	NAME = "sosord"


ENDDEFINE
*
*-- EndDefine: sosord
**************************************************


**************************************************
*-- Class:        sosork (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- ParentClass:  accountmate (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- BaseClass:    custom
*-- Time Stamp:   11/05/19 02:56:04 PM
*
DEFINE CLASS sosork AS accountmate


	cFileName = "sosork"
	calias = "sosork"
	cpkfield = "cSono"
	NAME = "sosork"


ENDDEFINE
*
*-- EndDefine: sosork
**************************************************


**************************************************
*-- Class:        sosptr (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- ParentClass:  accountmate (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- BaseClass:    custom
*-- Time Stamp:   12/05/18 11:14:10 AM
*
DEFINE CLASS sosptr AS accountmate


	calias = "sosptr"
	cFileName = "sosptr"
	cpkfield = "cuid"
	NAME = "sosptr"


ENDDEFINE
*
*-- EndDefine: sosptr
**************************************************


**************************************************
*-- Class:        soxitems (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- ParentClass:  accountmate (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- BaseClass:    custom
*-- Time Stamp:   09/07/20 11:31:14 AM
*
DEFINE CLASS soxitems AS accountmate


	calias = "soxitems"
	cFileName = "soxitems"
	cpkfield = "ID"
	cskipfieldsforupdates = "ID"
	NAME = "soxitems"


ENDDEFINE
*
*-- EndDefine: soxitems
*


**************************************************
*-- Class:        icikit (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- ParentClass:  accountmateitemlist (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- BaseClass:    custom
*-- Time Stamp:   06/13/22 09:11:03 AM
*
DEFINE CLASS icikit AS accountmate


	cFileName = "icikit"
	calias = "icikit"
	cdatapath = ""
	cidtable = ""
	cpkfield = "ckititemno"
	ndatamode = 2
	NAME = "icikit"


	PROCEDURE loadcomponents
	LPARAMETERS lnParentpk,lnlookuptype, lcName
	LOCAL x

	IF EMPTY(lnParentpk)
		RETURN 0
	ENDIF


	lcName=IIF(EMPTY(lcName),THIS.cSQLCursor,lcName)
	pcItemno=lnParentpk

*** Run the query to retrieve the lineitems
	THIS.QUERY("select * FROM "+THIS.cFileName+" WHERE cKitItemno=?pcItemno",lcName)

	IF THIS.lError
		RETURN .F.
	ENDIF

	RETURN



	ENDPROC






ENDDEFINE
*
*-- EndDefine: icikit
**************************************************


**************************************************
*-- Class:        soskit (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- ParentClass:  accountmateitemlist (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- BaseClass:    custom
*-- Time Stamp:   02/03/22 11:59:07 AM
*
DEFINE CLASS soskit AS accountmate

	cFileName = "soskit"
	calias = "soskit"
	cdatapath = ""
	cidtable = ""
	cpkfield = "cuid"
	ndatamode = 2
	NAME = "soskit"


ENDDEFINE
*
*-- EndDefine: soskit
**************************************************






**************************************************
*-- Class:        sostrs (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- ParentClass:  accountmateitemlist (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- BaseClass:    custom
*-- Time Stamp:   10/09/17 05:37:08 PM
*
DEFINE CLASS sostrs AS accountmate


	cFileName = "sostrs"
	calias = "sostrs"
	cdatapath = ""
	cidtable = ""
	cpkfield = "cuid"
	ndatamode = 2
	NAME = "sostrs"


ENDDEFINE
*
*-- EndDefine: sostrs
**************************************************

DEFINE CLASS wsSetting AS accountmate


	cpkfield = "ID"
	cFileName = "wsSetting"
	calias = "wsSetting"
	NAME = "wsSetting"

	FUNCTION LoadSettings
	LPARAMETERS lobject
	ln=THIS.QUERY("select * from WSsetting","TSetting")
	IF THIS.lError
		RETURN .F.
	ENDIF
	lcOldDate=SET("Date")
	SET DATE YMD
	SELECT TSetting
	SCAN

		lc1=JUSTSTEM(NAME)
		lc2=JUSTEXT(NAME)
		IF PEMSTATUS(lobject,lc1,5)
			IF !PEMSTATUS(EVALUATE("lobject."+lc1),lc2,5)
				ADDPROPERTY(EVALUATE("lobject."+lc1),lc2,X8convchar(ALLTRIM(TSetting.VALUE),TSetting.TYPE))
			ELSE
				lcv="lobject."+lc1+"."+lc2
				&lcv=X8convchar(ALLTRIM(TSetting.VALUE),TSetting.TYPE)
			ENDIF
		ELSE
			ADDPROPERTY(lobject,lc1,CREATEOBJECT("empty"))
			ADDPROPERTY(EVALUATE("lobject."+lc1),lc2,X8convchar(ALLTRIM(TSetting.VALUE),TSetting.TYPE))
		ENDIF
	ENDSCAN
	SET DATE &lcOldDate
	ENDFUNC

ENDDEFINE


**************************************************
*-- Class:        amsql (c:\webconnectionprojects\anniquestore\deploy\amdata.vcx)
*-- ParentClass:  wwsql (c:\development\wconnect\classes\wwsql.vcx)
*-- BaseClass:    custom
*-- Time Stamp:   06/15/20 09:27:07 AM
*
DEFINE CLASS amsql AS wwsql


	companylocked = .F.
	noconnect = .F.
	spid = 0
	companyexcl = .F.
	NAME = "amsql"


	PROCEDURE checkiflocked
	TEXT TO lcSQL NOSHOW
		SELECT llock FROM Cosyst
	ENDTEXT
	IF THIS.Execute(lcSql,"lockCheck")<1
		THIS.companylocked=.T.
		RETURN .T.
	ENDIF
	LOCAL ll
	ll=(LockCheck.lLock=1)
	USE IN LockCheck
	THIS.companylocked=ll
	RETURN ll

	ENDPROC


	PROCEDURE checkifexcl
	TEXT TO lcSQL NOSHOW
		SELECT lExcl FROM Cosyst
	ENDTEXT
	IF THIS.Execute(lcSql,"lockCheck")<1
		THIS.companyexcl=.T.
		RETURN .T.
	ENDIF
	LOCAL ll
	ll=(LockCheck.lExcl=1)
	USE IN LockCheck
	THIS.companyexcl=ll
	RETURN ll

	ENDPROC


ENDDEFINE
*
*-- EndDefine: amsql
**************************************************


DEFINE CLASS webstore AS busbase

	cconnectstring = ""
	ndatamode = 2
	NAME = "web"

ENDDEFINE

DEFINE CLASS NewRegistration AS webstore

	cskipfieldsforupdates="id,avatar,idbook,idbooktype"
	NAME="newregistration"
	calias="newregistration"
	cFileName="newregistration"

	FUNCTION SAVE()
	THIS.oData.UpdatedonUTC = DATETIME()
	IF THIS.nUpdateMode = 2
		THIS.oData.CreatedonUTC = DATETIME()
		THIS.oData.STATUS="NEW"
	ENDIF

*!*	IF PEMSTATUS(SERVER.goSettings.Common,"cellcountry",5)
*!*		IF SERVER.goReg.TEST(ALLTRIM(this.oData.cPhone2),ALLTRIM(SERVER.goSettings.Common.cellreg))
*!*			THIS.oData.cphone2=ALLTRIM(SERVER.goSettings.Common.cellcountry)+RIGHT(ALLTRIM(this.oData.cPhone2),10)
*!*		ENDIF
*!*	ENDIF

	RETURN DODEFAULT()

	ENDFUNC

	FUNCTION VALIDATE()

	loCust = THIS.oData
	lcErrorMsg = ""


	pcEmail=loCust.cEmail
	pcIdno=loCust.cIDno
&& Added more validation on email
	oReg=CREATEOBJECT("WWREGEX")
	IF ! oReg.TEST(pcEmail,"^[\w!#$%&'*+/=?`{|}~^-]+(?:\.[\w!#$%&'*+/=?`{|}~^-]+)*@?(?:[A-Z0-9-]+\.)+[A-Z]{2,6}$")
		THIS.AddValidationError("Invalid Email Address.","cemail")
		THIS.SetError( THIS.oValidationErrors.ToString() )
		RETURN .F.
	ENDIF

	IF LEN(JUSTEXT(pcEmail))>3
		THIS.AddValidationError("Bad Email","cemail")
		THIS.SetError( THIS.oValidationErrors.ToString() )
		RETURN .F.
	ENDIF

	IF 	THIS.QUERY("select 1 from newregistration where ( cemail=?pcEmail) "+;
			IIF(ISNULL(loCust.ID),""," AND id<>'"+TRANSFORM(loCust.ID)+"'"))>0
		THIS.AddValidationError("Already registered","cemail")
	ENDIF


	lret=oAmSql.EXECUTE("select ccustno,csponsor from arcust where  cemail=?pcEmail","IDCheck")
	IF lret=1 AND oAmsql.nAffectedRecords<>0
		this.AddValidationError("Previously registered","cemail")
	ENDIF


	IF EMPTY(loCust.cLName) OR EMPTY(loCust.cFName)
		THIS.AddValidationError("Name incomplete","cfname")
	ENDIF
	loCust.cState=ALLTRIM(PADR(loCust.cState,15))


	IF ISNULLOREMPTY(loCust.cCompany)
		loCust.cCompany=ALLTRIM(loCust.cLName)+","+ALLTRIM(loCust.cFName)
	ENDIF
	
	IF 	LEN(loCust.cCompany)>40
		THIS.AddValidationError("Names too long","cfname")
	ENDIF
			
	
	IF THIS.oValidationErrors.COUNT > 0
		THIS.SetError( THIS.oValidationErrors.ToString() )
		RETURN .F.
	ENDIF

	IF ISNULLOREMPTY(THIS.oData.ActivateLink)
		THIS.oData.ActivateLink=X8GUID(36)
	ENDIF

	ENDFUNC

&&-------------------------------------------------------------------------------------
	FUNCTION CreateFromReferral
&&-------------------------------------------------------------------------------------
	LPARAMETERS loNew
	SET LIBRARY TO vfpencryption ADDIT
	THIS.SetError()
	oAmsql.EXECUTENONQUERY("SET ANSI_WARNINGS ON")
	THIS.oSql.EXECUTENONQUERY("SET ANSI_WARNINGS ON")
	loArsyst = CREATEOBJECT("arsyst")
	loArsyst.Setsqlobject(oAmsql)
	loArcust = CREATEOBJECT("arcust")
	loArcust.Setsqlobject(oAmsql)
	loArcadr = CREATEOBJECT("arcadr")
	loArcadr.Setsqlobject(oAmsql)

	PRIVATE loCust
	
	#if .f.
	loCust=NEWOBJECT("ccustomer","amdata.prg")
	loCust.Setsqlobject(oWSsql)
*!*	IF goSettings.Common.Country='NAM'
*!*		lSA=.f.
*!*	ELSE
	lSA=.T.
*!*	ENDIF
	
	#endif
	
	#if .t.
	oNop=NULL
	oNopSql.EnableUnicodeToAnsiMapping()

	oCust=CREATEOBJECT("Customer")
	oCust.SetSqlObject(oNopSql)

	oProfile=CREATEOBJECT("ANQ_UserProfileAdditionalInfo")
	oProfile.SetSqlObject(oNopSql)
	SET PROCEDURE TO SyncConsultant ADDIT
	oSync=CREATEOBJECT("SyncConsultants")
	IF !oSync.SETUP("https://annique.com/api-backend/",1)
		THIS.SetError(oSync.cErrormsg)
		RETURN .f.
	ENDIF
   *? oSync.SyncOne("661451")
	lSa=.t.
	
	
	oWSsql.begintransaction()
	oAmsql.begintransaction()
	
	llError=.F.

	DO WHILE .T.



		IF !loNew.LOAD(loNew.oData.ID)
			llError=.T.
			EXIT
		ENDIF
		IF !EMPTY(loNew.oData.cCustno)
			llError=.T.
			EXIT
		ENDIF
		oNew=loNew.oData

		IF !loArcust.new()
			llError=.T.
			EXIT
		ENDIF

		=COPYOBJECTPROPERTIES(oNew,loArcust.oData,2)
		DO WHILE .T.
			IF oAmsql.Execute("execute vsp_ar_getnewcustno", "CurNewCustNo")=-1
				THIS.SetError(oAmsql.cErrorMsg)
				RETURN .F.
			ENDIF
			lcCustno=CurNewCustNo1.cRetValue
			IF ISNULLOREMPTY(lcCustno)
				THIS.SetError("Could not get new account #")
				RETURN .F.
			ENDIF
			IF oAmsql.Execute("select 1 from arcust where ccustno='"+lcCustno+"'", "CurCustCheck")=-1
				THIS.SetError(oAmsql.cErrorMsg)
				RETURN .F.
			ENDIF
			IF RECCOUNT("CurCustCheck")>1
				LOOP
			ENDIF
			EXIT
		ENDDO

		oData=loArcust.oData
		oData.cCustno=lcCustno
		IF ISNULLOREMPTY(oData.cCompany)
			oData.cCompany=ALLTRIM(oData.cLName)+","+ALLTRIM(oData.cFName)
			oData.CCOMPANY2 = ""
		ELSE
			oData.CCOMPANY2 = oData.cCompany
		ENDIF
		oData.CPHONE2=ALLTRIM(oData.CPHONE2)
		*IF !lSA
		*	IF LEFT(oData.CPHONE2,6)<>"(+264)"
*	oData.CPHONE2="(+264)"+SUBSTR(oData.CPHONE2,2)
		*	ENDIF
		*ENDIF
		oData.CPHONE3 = oData.CPHONE2
		oData.CFAX = ''
		oData.CWEBSITE = ''
		oData.CDEAR = ''
		oData.CORDERBY = ''
		oData.CSLPNNO = ''
		oData.cStatus = 'A'
		oData.CCLASS = 'CONSULTANT'
		oData.CINDUSTRY = ''
		oData.CTERR = 'ONLINE'
		oData.CWAREHOUSE = IIF(lSA,'4400','6200')
		oData.CPAYCODE = 'CWO'
		oData.CBILLTONO = 'POST'
		oData.CSHIPTONO = '001'
		oData.CTAXCODE = 'VAT - 15%'
		oData.CREVNCODE = ''
		oData.CTAXFLD1 = ''
		oData.CTAXFLD2 = ''
		oData.CCURRCODE = IIF(lSA,'ZAR','NAM')
		oData.CPRTSTMT = 'O'
		oData.CARACC = '200100-2600'
		oData.CRCALACTN = ''
		oData.CLCALACTN = ''
		oData.CPASSWD = ''
		oData.CPRICECD = ''
		oData.CPCUSTNO = ''
		oData.DCREATE   = DATE()
		oData.DYTDSTART = DATE()
*oData.TMODIFIED = DATETIME()
		oData.TRECALL = {}
		oData.TLCALL = {}
		oData.LPRTSTMT = 1
		oData.LCONSTMT = 0
		oData.LFINCHG = 1
		oData.LIOCUST = 1
		oData.LUSECUSITM = 0
		oData.LUSEITEMNO = 0
		oData.LUSECUSPRC = 0
		oData.LGENINVC = 1
		oData.LUSELPRICE = 0
		oData.LAPPLYTAX = 1
		oData.LPRCINCTAX = 0
		oData.LSAVECARD = 0
		oData.NEXPDAYS = 0
		oData.NAVGDAYS = .NULL.
		oData.NDISCRATE = 20
		oData.NATDSAMT = 0
		oData.NYTDSAMT = 0
		oData.NCRLIMIT = 0
		oData.NSOBOAMT = 0
		oData.NOPENCR = 0
		oData.NUISHPAMT = 0
		oData.NBALANCE = 0
		oData.MIMPTSORD = ''
		oData.MIMPTSTRS = ''
		oData.MIMPTINVC = ''
		oData.MIMPTITRS = ''
		oData.CRATING = ''
		oData.CSHIPVIA = ''
		oData.CDELVDAY = ''
		oData.CALLOCATE = ''
		oData.CTMLDSTAT = ''
		oData.DBBDATE = {}
		oData.DTLDATE = {}
		oData.LALLOWCMPGN = 1
		oData.CBANKNO = "" &&ALLTRIM(lcCustno)+PADR(oData.cLName,4)
		oData.CBANKACC ="" &&oData.CBANKACCT
		oData.CBANKACCT = NULL &&loCust.ENCRYPT(oData.CBANKACCT)
		oData.LEMAIL = 1
		oData.LSMS = 1
		oData.LPOST = 0
		oData.LMINORDDSC = 0
		oData.LMINDEVORD = 0
		oData.LRGSTRNPAID = 0
		oData.LHOLD = 0
		oData.CUSERNAME = 'System'
		oData.CREGMETHOD = oNew.Createdby
		oData.LPOBOX = 0
		oData.LSUSPENDED = 0
		oData.LREBATE = 0
		oData.LUPDATEPORTAL = 1
		oData.LEXADMFEE = 0
		oData.CSTATUSCON = 'A'
		oData.CSTATUSOVR = 'A'
		oData.CSTATUSLST = ''
		oData.DSTATUSCHG = {}
		oData.DLASTLET = {}
		oData.CLASTLET = {}
		oData.CPFIRSTNAME = ''
		oData.CPNAME = ''
		oData.CPCELL = ''
		oData.CPEMAIL = ''
		oData.CFACEBOOK = ''
		oData.CTWITTER = ''
		oData.CESPONSOR = oData.cSponsor
		oData.LSTARTER = 0
		oData.LPATITLE = 0
		oData.LTITLE = 0
		oData.LACCEPT = 0
		oData.DSTARTER = NULL
		oData.DACCEPT = {}
		oData.LSHOWNOTE = 0
		oData.NCRHOLDAMT = 0
		oData.CLINVNO = ''
		oData.CLRCPTNO = ''
		oData.CLINVCURR = ''
		oData.CLRCPTCURR = ''
		oData.DLSALES = {}
		oData.DLRCPT = {}
		oData.NLSALESAMT = 0
		oData.NLRCPTAMT = 0
		oData.NSQAMT = 0
		oData.DTEMPCRVLD = {}
		oData.NTEMPCRINC = 0
		oData.MCRHISTORY = ''
		oData.NADVBILLPMT = 0
		oData.CBNKROUTE = ''
		oData.CPRENOTE = ''
		oData.CSSN = ''
		oData.DPRENOTE = {}
		oData.LEPAYMENT = 0
		oData.DYTDRECALC = {}
		oData.CFOB = ''


		IF !loArcust.SAVE()
			llError=.T.
			EXIT
		ENDIF



		IF !loNew.LOAD(loNew.oData.ID)
			llError=.T.
			EXIT
		ENDIF
		IF !EMPTY(loNew.oData.cCustno)
			llError=.T.
			EXIT
		ENDIF



		#IF .F.
		
		=LOGSTRING('Update to webstore '+loNew.oData.cCustno,"CustSync.log")
		WITH loCust

			IF !.new(.T.)	&& Create a new record
				THIS.cErrorMsg=.oSql.cErrorMsg
				llError=.T.
				EXIT
			ENDIF

			.oData.CustomerGUID 		=  	X8GUID(36)
			.oData.Email				=  	oData.cEmail
			.oData.Username				=  	odata.ccustno		&&cUsername
			.oData.SaltKey			= SYS(2015)
			lcPassword=ALLTRIM(oData.cCustno)+"Anq!"
			.oData.Passwordhash		=STRCONV(Hash(lcPassword+ALLTRIM(.oData.SaltKey),1),15)
			.oData.IsTaxExempt		= 	IIF(oData.LAPPLYTAX=1,0,1)
			.oData.DELETED			=	0
			.oData.CreatedonUTC		= DATETIME()
			.oData.ShippingAddressID = 	0
			.oData.BillingAddressID  = 	0
			.oData.IsSystemAccount  = 0
			.oData.IsAdmin 			= 0
			.oData.ACTIVE 			= IIF(oData.cStatus='A' AND oData.LIOCUST=1,1,0)
			.oData.DELETED 			= 0
			.oData.AvatarID 		= 0
			.oData.UpdatedonUTC=DATETIME()
			.oData.CustomerRoleID=1


			COPYOBJECTPROPERTIES(oData,.oData,2)
			IF oData.CCLASS="CONSULTANT"
				.oData.CustomerRoleID=1 &&THIS.customerrole
			ENDIF
			
			.oData.ACTIVE 		= IIF(oData.cStatus='A' AND oData.LIOCUST=1,.T.,.F.)
			.oData.IsTaxExempt	= IIF(oData.LAPPLYTAX=1,.F.,.T.)
			IF !COMPOBJ(.oData,.oorigdata)
				.oData.UpdatedonUTC=DATETIME()
				IF !.SAVE()
					THIS.lError=.T.
					THIS.cErrorMsg="Could not save webstore record"
					EXIT
				ENDIF
			ENDIF

		ENDWITH
		#ENDIF
		

		loNew.oData.cCustno=oData.cCustno
		loNew.oData.STATUS="ACTIVATED"
		IF !loNew.SAVE()
			llError=.T.
			EXIT
		ENDIF


		EXIT

	ENDDO



	IF llError
		oWSsql.ROLLBACK()
		oAmsql.ROLLBACK()
		RETURN .F.
	ELSE
		oWSsql.Commit()
		oAmsql.Commit()
		
		
		IF !oSync.SyncOne(loArcust.oData.cCustno)
		
		
		
		ENDIF
		IF oSync.SendWelcomeMail(loArcust)<>""
		ENDIF
		
		
		
#IF .F.
&&------------------------------- LOAD STARTER ---------------------------------------
		=LOGSTRING('load starter '+loNew.oData.cCustno,"CustSync.log")
		lok=.T.
		lcSql="sp_ws_loadstarter @date='"+X8convchar(DATE(),"C")+"',@cCustno='"+oData.cCustno+"'"
		=LOGSTRING(lcSql,"CustSync.log")
		IF !oAmsql.EXECUTENONQUERY("sp_ws_loadstarter @date='"+X8convchar(DATE(),"C")+"',@cCustno='"+oData.cCustno+"'")
			=LOGSTRING("Could not load starter AM "+oAmsql.cErrorMsg,"CustSync.log")
			lok=.F.
		ENDIF
		lcSql="exec sp_loadstarter @date='"+X8convchar(DATE(),"C")+"',@cCustno='"+oData.cCustno+"'"
		=LOGSTRING(lcSql,"CustSync.log")
		IF lok AND !oWSsql.EXECUTENONQUERY(lcSql)
			=LOGSTRING("Could not load starter WS "+oSql.cErrorMsg,"CustSync.log")
		ENDIF
#ENDIF

	ENDIF
&&-------------------------------------------------------------------------------------

	ENDFUNC

FUNCTION Destroy
IF VARTYPE(oWSsql)="O"
	oWsSql.ROLLBACK()
ENDIF	
IF VARTYPE(oAMsql)="O"
	oAmsql.ROLLBACK()
ENDIF

ENDDEFINE





DEFINE CLASS ccustomer AS busbase


	salt = "4nnique4admin!"
	reactivate = .F.
	backdoor = .F.
	cidtable = ""
	cfilename = "customer"
	calias = "customer"
	cdatapath = ""
	cpkfield = "id"
	cconnectstring = "driver={sql server};server=(local);database=wwdeveloper;uid=sa;pwd=;"
	ndatamode = 2
	cskipfieldsforupdates = "id,avatar"
	lcompareupdates = .T.
	csqlcursor = "TCustomers"
	Name = "ccustomer"


	*-- Retrieves a user ID.
	PROCEDURE findbyuserid
		LPARAMETER lcUserId

		THIS.Open()

		DO CASE
		CASE THIS.ndatamode = 0
		   LOCATE FOR UserID = lcUserId
		   llFound = FOUND()

		CASE THIS.ndatamode = 2
		   lnResult = THIS.osql.Execute("select * from " + THIS.cFileName + " WHERE Username = '" + lcUserid + "'")
		   IF lnResult # 1
		     	THIS.seterror(THIS.oSQL.cErrorMsg)
		   	RETURN .F.
		   ENDIF

		   llFound = RECCOUNT() > 0
		ENDCASE

		IF llFound
		   SCATTER NAME THIS.oData MEMO
		ELSE
		   THIS.GetBlankRecord()
		   RETURN .F.
		ENDIF

		RETURN .T.
	ENDPROC


	PROCEDURE loadbyname
		LPARAMETERS lcLast, lcFirst

		lnResult = THIS.Query("select id from " + THIS.cAlias + " where UPPER(clname)='" + UPPER(lcLast) + "' AND UPPER(cfname)='"  + UPPER(lcFirst) +"'")

		IF lnResult < 1
		   RETURN 0
		ENDIF

		IF lnResult > 1
		   RETURN 2 
		ENDIF

		THIS.Load(id)

		RETURN lnResult
	ENDPROC


	PROCEDURE authenticateandload
		LPARAMETERS lcUserName, lcPassword

		*'SET LIBRARY TO vfpencryption.fll  addit
		*?STRCONV(Hash('admin'+'TM1yOC8=',1),15)

		*** Authentication against user file
		IF  .NOT. THIS.getuser(lcusername)
		   RETURN .F. 
		ENDIF

		&& :RHE 2021-10-01
		IF  .NOT. THIS.odata.ACTIVE AND (ISNULLOREMPTY(THIS.odata.lastdeactivation) OR THIS.odata.lastdeactivation<DATE()-365)
		   THIS.seterror("This user account is not active or has expired.")
		   RETURN .F.
		ENDIF
		IF  .NOT. THIS.odata.ACTIVE 
			this.reactivate=.t.
		ENDIF

		


		lcPw = STRCONV(Hash(lcPassword+ALLTRIM(this.odata.saltkey),1),15)

		IF !lcPw==ALLTRIM(this.odata.passwordhash)
			THIS.SetError("Invalid Password or Username")
		   SCATTER BLANK MEMO NAME THIS.odata
		   RETURN .F.

		ENDIF
		*!*	THIS.odata.LoginAttempts=0
		*** And save the changes
		*!*	THIS.Save()
	ENDPROC


	PROCEDURE getuser
		************************************************************************
		* wwUserSecurity  :: GetUser
		****************************************
		***  Function: Retrieves a user data record object without affecting the 
		***            currently active user. Pass in a PK or Username and Password. 
		***            GetUser always returns a record unless the record cannot be found. Use Logon to check for Active and Expired status.
		***    Assume:
		***      Pass:
		***    Return:
		************************************************************************
		LPARAMETERS lcPK, lcPassword

		*** lcPK could also be the username

		THIS.lError = .F.

		IF .NOT. THIS.OPEN()
		   THIS.lError = .T.
		   THIS.SetError("Couldn't open "+THIS.cfilename)
		   RETURN .F.
		ENDIF

		*THIS.lNewUser = .F.

		*** Allow retrieving a blank user record
		IF lcPK="BLANK"
		   SCATTER BLANK MEMO NAME THIS.odata
		   RETURN .T.
		ENDIF


		IF !this.loadbase("username='"+lcPk+"'")
		   THIS.SetError("Invalid Password or Username")
		   SCATTER BLANK MEMO NAME THIS.odata
		   RETURN .F.
		ENDIF

		RETURN .t.
	ENDPROC


	PROCEDURE getcustomers
		LPARAMETER lcName, lcFields, lcAdditionalFilter, lcOrder
		LOCAL lnResult, lcFilter

			IF EMPTY(lcName)
			  lcName= ""
			  lcFilter = "1=0"
			ELSE 
		      *** This needs fixing
			  lcName= UPPER(lcName) 
			  lcFilter = [ cCompany like '%]+lcName + [%' OR cCustno=']+lcName+['] 

			ENDIF

			IF !THIS.Open()
			   RETURN 0
			ENDIF   
			THIS.osql.cSqlCursor = THIS.csqlcursor
		    THIS.oSQL.cSQL =  "select * from Customer WHERE isSystemAccount=0 AND "+ lcFilter 
		                     
		    lnResult = THIS.oSQL.Execute()
		    IF lnResult # 1
		       THIS.SetError(THIS.oSQL.cErrorMsg)
		       RETURN 0
		    ENDIF

		lnResult = RECCOUNT()


		RETURN lnResult
	ENDPROC


	PROCEDURE getusers
		LPARAMETER lcName, lcFields, lcAdditionalFilter, lcOrder
		LOCAL lnResult, lcFilter


			IF !THIS.Open()
			   RETURN 0
			ENDIF   
			THIS.osql.cSqlCursor = THIS.csqlcursor
		    THIS.oSQL.cSQL =  "select * from Customer WHERE isSystemAccount=0 and isAdmin=1"
		                     
		    lnResult = THIS.oSQL.Execute()
		    IF lnResult # 1
		       THIS.SetError(THIS.oSQL.cErrorMsg)
		       RETURN 0
		    ENDIF

		lnResult = RECCOUNT()


		RETURN lnResult
	ENDPROC


	PROCEDURE encrypt
		LPARAMETERS lcOriginal 
		IF ISNULLOREMPTY(lcOriginal)
			RETURN ""
		ENDIF
		LOCAL lc
		lcSecret = this.salt
		lc=Encrypt(lcOriginal,this.salt,1024) 
		RETURN STRCONV(lc,13)
	ENDPROC


	PROCEDURE decrypt
		LPARAMETERS lcOriginal 
		IF ISNULLOREMPTY(lcOriginal)
			RETURN ""
		ENDIF
		LOCAL lc
		lc=STRCONV(lcOriginal,14)
		RETURN decrypt(lc,this.salt,1024)
	ENDPROC


	PROCEDURE validateaddress
		&& Added 22/11/2021 to validate profile address

		lnCustomerID=Process.nCustomerID
		loAddress = CREATE('cAddress')
		loAddress.SetSQLObject(osql)
		TEXT TO lcSQL TEXTMERGE NOSHOW
		SELECT id,ccustno,caddrno,ccompany,caddr1,caddr2,ccity,cstate,czip,ccountry,cphone,ccontact,
			IIF(caddrno='POST','POSTAL','SHIPPING') addresstype,
				 IIF(caddr1<>'',RTRIM(caddr1)+', ','')+
			 IIF(caddr2<>'',RTRIM(caddr2)+', ','')+
			 IIF(ccity<>'',RTRIM(ccity)+', ','')+
			 IIF(czip<>'',RTRIM(czip),'') caddress
			 	 from Address
			 where CustomerID=<<TRANSFORM(lnCustomerID)>> 
			 and caddrno in ('001','POST')
		ENDTEXT	 
		lnCount=loAddress.Query(lcSql,"cShip")
		IF lnCount < 2
			this.AddValidationError("You need to have a street and postal address. ","")
		ELSE
			SELECT cShip
			REPLACE ALL cAddr1 WITH CHRTRAN(cAddr1,"'",""), cAddr2 WITH CHRTRAN(cAddr1,"'",""), cCity WITH CHRTRAN(cCity,"'","")
		SCAN
		TEXT TO lcSql TEXTMERGE NOSHOW
		DECLARE	@cErrorMsg nvarchar(max)
		EXEC sp_ws_addressverify 
		@cAddr1='<<caddr1>>' ,@cAddr2='<<caddr2>>',
		@cCity='<<ccity>>',@cState='<<cstate>>',@cZip='<<czip>>',@cCountry='<<ccountry>>',@cErrorMsg = @cErrorMsg OUTPUT

		SELECT	@cErrorMsg as N'@cErrorMsg'
		ENDTEXT
			lnret=oSql.Execute(lcSql,'TVA')
			IF lnret<>1
				this.AddValidationError("Could not validate addresses","addresstab")
				EXIT
			ELSE
				IF !ISNULLOREMPTY(tva._cErrormsg)
				this.AddValidationError(STRTRAN(TVA._cErrormsg,CHR(13)+CHR(10),"<br/>"),"")
				ENDIF
			ENDIF


		ENDSCAN
		ENDIF
		IF THIS.oValidationErrors.Count > 0
			this.AddValidationError("Click on ADDRESSES button above to rectify","addresstab")
			this.SetError( this.oValidationErrors.ToString() )
			RETURN .F.
		ENDIF
	ENDPROC


	PROCEDURE validate
		LOCAL loCust
		   
		loCust = THIS.oData   
		lcErrorMsg = ""
	

		lSameID=.F.
		IF goSettings.Common.country='SA'   && Must remove the .f.
			pcIdno=loCust.cIdno
			pcCustno=loCust.cCustno
			lSameID=.F.
			IF this.query("select ccustno from customer where cCustno<>?pcCustno and cidno=?pcIdno","IDCheck" )>0
				lSameID=.T.

			ENDIF
			&&EMPTY(this.cregistered) AND 
			IF (PEMSTATUS(oAmsql,"noconnect",5) AND (!oAmsql.noConnect))
				lret=oAmSql.EXECUTE("select ccustno,csponsor from arcust where cCustno<>?pcCustno and cidno=?pcIdno","IDCheck")
				IF lret=1 AND oAmsql.nAffectedRecords<>0
					lSameID=.T.
				ENDIF
			ENDIF

			IF lSameID


		TEXT TO lcError TEXTMERGE NOSHOW
		Dear <<loCust.cFName>>,<br/> this ID number already exist on our Annique system showing this ID number applicant is already an Annique Consultant.<br/>
		Please contact our Annique Customer Care Department at email info@annique.com/ telephone number 012 345 9800
		ENDTEXT

			this.AddValidationError(lcError,"cidno")

			ENDIF

		ENDIF


		&& Added more validation on email
		oReg=CREATEOBJECT("WWREGEX")
		IF ! oReg.TEST(loCust.cEmail,"^[\w!#$%&'*+/=?`{|}~^-]+(?:\.[\w!#$%&'*+/=?`{|}~^-]+)*@?(?:[A-Z0-9-]+\.)+[A-Z]{2,6}$")
			this.AddValidationError("Invalid Email Address.","cemail")
		ENDIF
		IF loCust.lupdate=0 AND 	!lSameID 
			loCust.lupdate=1
		
		ENDIF

		IF THIS.oValidationErrors.Count > 0
			this.SetError( this.oValidationErrors.ToString() )
			RETURN .F.
		ENDIF

		RETURN .T.
	ENDPROC


	PROCEDURE save
		THIS.oData.UpdatedonUTC = DATETIME()
		luActive=THIS.oData.Active
		IF VARTYPE(luActive)<>"L"
			THIS.oData.Active=IIF(THIS.oData.Active=1,.t.,.f.)
		ENDIF
		DoDefault()
	ENDPROC


	*-- Returns countries and country codes as a cursor. Code and Name are the fields returned.
	PROCEDURE getcountries
	ENDPROC


	PROCEDURE updatesource
	ENDPROC


ENDDEFINE
*
*-- EndDefine: ccustomer
**************************************************


DEFINE CLASS nopIntegration AS busbase

	ndatamode = 2
	NAME = "nopIntegration"

ENDDEFINE

DEFINE CLASS NopSSO AS nopIntegration

	cpkfield = "ID"
	calias = "NopSSO"
	cfilename = "NopSSO"
	ckeyfield = "ID"
	nDataMode = 2
	lCompareUpdates = .t.
	Name = "NopSSO"
	
	
	
ENDDEFINE	

DEFINE CLASS Nopsettings AS nopIntegration


	cidtable = ""
	cfilename = "Nopsettings"
	calias = "Nopsettings"
	cdatapath = ""
	cpkfield = "ID"
	ndatamode = 2
	cskipfieldsforupdates = "ID"
	Name = "Nopsettings"


	PROCEDURE loadsettings
		LPARAMETERS lobject
		ln=this.query("select * from Nopsettings WHERE StoreID=1","TSetting")
		IF THIS.lError
			RETURN .f.
		ENDIF
		lcOldDate=SET("Date")
		SET DATE YMD
		SELECT TSetting
		SCAN

			lc1=JUSTSTEM(Name)
			lc2=JUSTEXT(Name)
			IF PEMSTATUS(lobject,lc1,5)
				IF !PEMSTATUS(EVALUATE("lobject."+lc1),lc2,5)
					ADDPROPERTY(EVALUATE("lobject."+lc1),lc2,X8convchar(ALLTRIM(TSetting.Value),TSetting.type))
				ELSE
					lcv="lobject."+lc1+"."+lc2
					&lcv=X8convchar(ALLTRIM(TSetting.Value),TSetting.type)
				ENDIF
			ELSE
				ADDPROPERTY(lobject,lc1,CREATEOBJECT("empty"))
				ADDPROPERTY(EVALUATE("lobject."+lc1),lc2,X8convchar(ALLTRIM(TSetting.Value),TSetting.type))
			ENDIF
		ENDSCAN
		SET DATE &lcOldDate
	ENDPROC


ENDDEFINE


*

DEFINE CLASS MailMessage AS busBase


	calias = "mailmessage"
	cfilename = "compsys.dbo.mailmessage"
	nupdatemode = 2
	cpkfield = "ID"
	cskipfieldsforupdates = "ID"
	Name = "mailmessage"


	PROCEDURE storemessage
		LPARAMETERS lcTo, lcSubject, lcMessage, ;
			lcFromName, lcFromEmail, m.lcCc, m.lcBcc, ;
			lcAttachment, lcContentType,lcAlternateText

		lcCc = IIF( VARTYPE( m.lcCc) ="C", m.lcCc, "" )
		lcBcc = IIF( VARTYPE( m.lcBcc) ="C", m.lcBcc, "" )
		lcTo = IIF( NOT EMPTY( m.lcTo), m.lcTo, ;
			IIF( NOT EMPTY( m.lcCc), m.lcCc, m.lcBcc) )
		lcFromName = IIF( EMPTY( m.lcFromName), m.lcFromEmail, m.lcFromName )
		lcContentType = IIF( EMPTY( m.lcContentType), "text/plain", m.lcContentType )
		lcAttachment = IIF( TYPE( "lcAttachment") = "C", m.lcAttachment, "" )
		lcSubject = IIF( EMPTY( m.lcSubject), "Message From Rem Application", m.lcSubject )
		lcMessage = IIF( EMPTY( m.lcMessage), m.lcSubject, m.lcMessage )

		LOCAL lnSelect, ltTime, lcMailServer, lcTable
		lnSelect = SELECT()
		ltTime = DATETIME()

		WITH this

			IF !.New()
				RETURN .F.
			ENDIF

			WITH .ODATA
				.T_Posted=m.ltTime
				.FromName=m.lcFromName
				.FromEmail=m.lcFromEmail
				.ToEmail=m.lcTo
				.CcEmail=m.lcCc
				.BccEmail=m.lcBcc
				.Subject=m.lcSubject
				.Details=m.lcMessage
				.Attachment=m.lcAttachment
				.Content=m.lcContentType
				.AlternateContentType = "text/plain"
				.AlternateText=lcAlternateText
			endwith

			IF !.save()
				RETURN .F.
			ENDIF

		endwith
	ENDPROC


	PROCEDURE getunsentmail
		TEXT TO lcSQL NOSHOW TEXTMERGE
		SELECT * FROM <<this.cfilename>> 
		WHERE (T_Sent IS NULL OR T_Sent='1900-01-01 00:00:00.000') AND Inactive = 0 AND 
			( ToEmail<>'' OR  CcEmail<>'' ) and Attempts<20

		ENDTEXT
		RETURN THIS.Query(lcSql,"TMessage")
	ENDPROC


	


	PROCEDURE new
		IF DODEFAULT(.T.)
			WITH THIS.odata
				.T_Posted=.NULL.
				.T_Posted=DATETIME()
				.FromName="Annique Homeoffice"
				.FromEmail="anniquealerts@annique.com"
				.ToEmail=""
				.CcEmail=""
				.BccEmail=""
				.Subject="New Annique Consulant Application"
				.Details=""
				.Attachment=""
				.Content="text/html"
				.AlternateContentType = "text/plain"
				.AlternateText=""
			ENDWITH
		ELSE 
			RETURN	.f.
		ENDIF
	ENDPROC


ENDDEFINE
*
*-- EndDefine: cmailmessage
**************************************************

*
	