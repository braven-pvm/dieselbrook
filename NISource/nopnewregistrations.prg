DEFINE CLASS ANQ_NewRegistrations AS NOPDATA

	
	NAME="ANQ_NewRegistrations"
	calias="ANQ_NewRegistrations"
	cFileName="ANQ_NewRegistrations"
&&-------------------------------------------------------------------------------------
FUNCTION SAVE()
&&-------------------------------------------------------------------------------------

	THIS.oData.UpdatedonUTC = DATETIME()
	IF THIS.nUpdateMode = 2
		THIS.oData.CreatedonUTC = DATETIME()
		THIS.oData.STATUS="NEW"
	ENDIF
	RETURN DODEFAULT()
	ENDFUNC

&&-------------------------------------------------------------------------------------
FUNCTION VALIDATE()
&&-------------------------------------------------------------------------------------
	loCust = THIS.oData
	lcErrorMsg = ""
	pcEmail=loCust.cEmail
	pcCell=loCust.cPhone1
	&&pcIdno=loCust.cIDno

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

	IF 	THIS.QUERY("select 1 from ANQ_newregistrations where ( cemail=?pcEmail ) "+;
			IIF(ISNULL(loCust.ID),""," AND id<>'"+TRANSFORM(loCust.ID)+"'"))>0
		THIS.AddValidationError("Already registered","cemail")
	ENDIF

	IF 	THIS.QUERY("select 1 from ANQ_newregistrations where ( cphone1=?pcCell) "+;
			IIF(ISNULL(loCust.ID),""," AND id<>'"+TRANSFORM(loCust.ID)+"'"))>0
		THIS.AddValidationError("Cell # in use already","cphone1")
	ENDIF

	lret=oAmSql.EXECUTE("select ccustno,csponsor from arcust where  cemail=?pcEmail","IDCheck")
	IF lret=1 AND oAmsql.nAffectedRecords<>0
		this.AddValidationError("Previously registered","cemail")
	ENDIF

	lret=oAmSql.EXECUTE("select ccustno,csponsor from arcust where  cphone2=?pcCell","IDCheck")
	IF lret=1 AND oAmsql.nAffectedRecords<>0
		this.AddValidationError("Cell # has bee used before","cemail")
	ENDIF

	IF EMPTY(loCust.cLName) OR EMPTY(loCust.cFName)
		THIS.AddValidationError("Name incomplete","cfname")
	ENDIF
	


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

	lSa=.t.
	oNopSql.begintransaction()
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
		oData.CPHONE2=IIF(!ISNULLOREMPTY(ALLTRIM(oData.CPHONE1)),ALLTRIM(oData.CPHONE1),ALLTRIM(oData.CPHONE2))
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
		oData.CREGMETHOD = NVL(oNew.Createdby,'')
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

		loNew.oData.cCustno=oData.cCustno
		loNew.oData.STATUS="ACTIVATED"
		IF !loNew.SAVE()
			llError=.T.
			EXIT
		ENDIF


		EXIT

	ENDDO



	IF llError
		oNopSql.ROLLBACK()
		oAmsql.ROLLBACK()
		RETURN .F.
	ELSE
		oNopSql.Commit()
		oAmsql.Commit()
		
		
		IF !oSync.SyncOne(loArcust.oData.cCustno)
				
		
		ENDIF
		
		IF oSync.SendWelcomeMail(loArcust)<>""
		
		ENDIF


	ENDIF
&&-------------------------------------------------------------------------------------

	ENDFUNC

FUNCTION Destroy
IF VARTYPE(oNopsql)="O"
	oNopSql.ROLLBACK()
ENDIF	
IF VARTYPE(oAMsql)="O"
	oAmsql.ROLLBACK()
ENDIF

ENDDEFINE


