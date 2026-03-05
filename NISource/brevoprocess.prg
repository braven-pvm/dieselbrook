#DEFINE ERROR_LOG     "Brevolog"+DTOS(DATE())+".log"
************************************************************************
*PROCEDURE BrevoProcess
****************************
***  Function: Processes incoming Web Requests for BrevoProcess
***            requests. This function is called from the wwServer
***            process.
***      Pass: loServer -   wwServer object reference
*************************************************************************
LPARAMETER loServer
LOCAL loProcess
PRIVATE REQUEST, Response, SERVER, SESSION, PROCESS
STORE NULL TO REQUEST, Response, SERVER, SESSION, PROCESS


#INCLUDE WCONNECT.H

loProcess = CREATEOBJECT("BrevoProcess", loServer)
loProcess.lShowRequestData = loServer.lShowRequestData

IF VARTYPE(loProcess)#"O"
*** All we can do is return...
	RETURN .F.
ENDIF

*** Call the Process Method that handles the request
loProcess.PROCESS()

*** Explicitly force process class to release
loProcess.Dispose()

RETURN

*************************************************************
DEFINE CLASS BrevoProcess AS WWC_RESTPROCESS
*************************************************************

*** Response class used - override as needed
	cResponseClass = [WWC_PAGERESPONSE]

*** Default for page script processing if no method exists
*** 1 - MVC Template (ExpandTemplate())
*** 2 - Web Control Framework Pages
*** 3 - MVC Script (ExpandScript())
	nPageScriptMode = 3
	oSettings=NULL
	oAmSql =NULL
	oNopSql=NULL
	oNop=NULL
	oSer=NULL
	oSql=NULL
	oSync=NULL
	oXml=NULL
*!* cAuthenticationMode = "UserSecurity"  && `Basic` is default




	#IF .F.
* Intellisense for THIS
		LOCAL THIS AS BrevoProcess OF BrevoProcess.prg
	#ENDIF

*********************************************************************
* Function BrevoProcess :: OnProcessInit
************************************
*** If you need to hook up generic functionality that occurs on
*** every hit against this process class , implement this method.
*********************************************************************
	FUNCTION OnProcessInit


	Response.Encoding = "UTF8"
	REQUEST.lUtf8Encoding = .T.


	Response.AppendHeader("Access-Control-Allow-Origin","*")
	Response.AppendHeader("Access-Control-Allow-Origin",REQUEST.ServerVariables("HTTP_ORIGIN"))
	Response.AppendHeader("Access-Control-Allow-Methods","POST, GET, DELETE, PUT, OPTIONS")
	Response.AppendHeader("Access-Control-Allow-Headers","Content-Type, *")
*!* *** Allow cookies and auth headers
	Response.AppendHeader("Access-Control-Allow-Credentials","true")


	lcVerb = REQUEST.GetHttpVerb()
	IF (lcVerb == "OPTIONS")
*
		RETURN .F.
	ENDIF




	=X8SETPRC("BrevoData.PRG")
	=X8SETPRC("BrevoApi.PRG")

	loSettings = CREATEOBJECT("NopSettings")
	loSettings.SetSqlObject(SERVER.oSql)
	THIS.oSettings=CREATEOBJECT("EMPTY")
	loSettings.LoadSettings(THIS.oSettings)
	WITH THIS
		.oSer=CREATEOBJECT("wwJsonSerializer")
		.oXml=CREATEOBJECT("WWXML")

		.oAmSql=CREATEOBJECT("wwSQL")
		IF !THIS.DBConnect(THIS.oConfig.cAMSqlconnectstring,.oAmSql)
			=LOGSTRING(THIS.cerrormsg,ERROR_LOG)
			RETURN .F.
		ENDIF

		.oNopSql=CREATEOBJECT("wwSQL")
		IF !THIS.DBConnect(THIS.oConfig.cNopSqlconnectstring,.oNopSql)
			=LOGSTRING(THIS.cerrormsg,ERROR_LOG)
			RETURN .F.
		ENDIF
		.oNopSql.EnableUnicodeToAnsiMapping()

		.oSql=CREATEOBJECT("wwSQL")
		IF !THIS.DBConnect(THIS.oConfig.cSqlconnectstring,.oSql)
			=LOGSTRING(THIS.cerrormsg,ERROR_LOG)
			RETURN .F.
		ENDIF


		.oSync=CREATEOBJECT("Brevo","https://api.brevo.com/v3/",;
			"REDACTED")


	ENDWITH


	RETURN .T.
	ENDFUNC


*********************************************************************
	FUNCTION TestPage
***********************
	LPARAMETERS lvParm
*** Any posted JSON string is automatically deserialized
*** into a FoxPro object or value

	#IF .F.
* Intellisense for intrinsic objects
		LOCAL REQUEST AS wwRequest, Response AS wwPageResponse, SERVER AS wwServer, ;
			PROCESS AS wwProcess, SESSION AS wwSession
	#ENDIF

*** Simply create objects, collections, values and return them
*** they are automatically serialized to JSON
	loObject = CREATEOBJECT("EMPTY")
	ADDPROPERTY(loObject,"name","TestPage")
	ADDPROPERTY(loObject,"description",;
		"This is a JSON API method that returns an object.")
	ADDPROPERTY(loObject,"entered",DATETIME())

*** To get proper case you have to override property names
*** otherwise all properties are serialized as lower case in JSON
	Serializer.PropertyNameOverrides = "Name,Description,Entered"


	RETURN loObject

*** To return a cursor use this string result:
*!* RETURN "cursor:TCustomers"


*** To return a raw Response result (non JSON) use:
*!*	JsonService.IsRawResponse = .T.   && use Response output
*!*	Response.ExpandScript()
*!*	RETURN                            && ignored

	ENDFUNC

*********************************************************************
	FUNCTION HelloScript()
***********************

	SELECT TOP 10 TIME, script, querystr, VERB, remoteaddr ;
		FROM wwRequestLog  ;
		INTO CURSOR TRequests ;
		ORDER BY TIME DESC

	loObj = CREATEOBJECT("EMPTY")

*** Simple Properties
	ADDPROPERTY(loObj,"message","Surprise!!! This is not a script response! Instead we'll return you a cursor as a JSON result.")
	ADDPROPERTY(loObj,"requestName","Recent Requests")
	ADDPROPERTY(loObj,"recordCount",_TALLY)

*** Nested Cursor Result as an Array
	ADDPROPERTY(loObj,"recentRequests","cursor:TRequests")

*** Normalize property names for case sensitivity
	Serializer.PropertyNameOverrides = "requestName,recentRequests,recordCount"

	RETURN loObj
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
	=LOGSTRING(THIS.cerrormsg,ERROR_LOG)
	ENDFUNC

*************************************************************
*** PUT YOUR OWN CUSTOM METHODS HERE
***
*** Any method added to this class becomes accessible
*** as an HTTP endpoint with MethodName.Extension where
*** .Extension is your scriptmap. If your scriptmap is .rs
*** and you have a function called Helloworld your
*** endpoint handler becomes HelloWorld.rs
*************************************************************
&&-----------------------------------------------------------------------------
	FUNCTION DBConnect(lcSqlConnect,loSql)
&&-----------------------------------------------------------------------------
	IF !loSql.CONNECT(lcSqlConnect)
&&	THIS.SetError(loSql.cErrorMsg)
		=LOGSTRING(loSql.cerrormsg,ERROR_LOG)
		RETURN
	ENDIF
	ENDFUNC


&&-----------------------------------------------------------------------------
	FUNCTION SyncConsultantsToBrevo
&&-----------------------------------------------------------------------------


	=LOGSTRING("Sync Consultants to Brevo ",ERROR_LOG)
	lcDays=REQUEST.params("days")
	IF EMPTY(lcDays)
		lcDays="5"
	ENDIF

	lcNew="1"
	lcCustno=REQUEST.params("ccustno")
	IF !EMPTY(lcCustno)
		lcNew="0"
	ENDIF

	lcStore=REQUEST.params("StoreID")
	IF EMPTY(lcStore)
		lcStore="1"
	ENDIF

	DO CASE
	CASE lcStore="1"
		cList="4"
		IF THIS.oNopSql.EXECUTE("EXEC ANQ_Brevo_AllCOnsultants @new="+lcNew+",@Days="+lcDays+;
				IIF(!EMPTY(lcCustno),",@cCustno='"+lcCustno+"'",""),"TIMP")<>1
			Response.STATUS = "400 "+THIS.oNopSql.cerrormsg
			Response.WRITE("false")
			Response.END()
			RETURN
		ENDIF
	CASE lcStore="2"
		cList="13"
		oWSSql=CREATEOBJECT("wwSQL")
		IF !oWSSql.CONNECT("DRIVER={SQL Server};SERVER=stage.AnniqueStore.co.za,61023;UID=sa;PWD=REDACTED;database=AnniqueStoreNam")
			=LOGSTRING(THIS.cerrormsg,ERROR_LOG)
			RETURN .F.
		ENDIF

		IF THIS.oSql.EXECUTE("EXEC sp_Brevo_AllConsultantsNam @new=1,@Days="+lcDays+;
				IIF(!EMPTY(lcCustno),",@cCustno='"+lcCustno+"'",""),"TIMP")<>1
			Response.STATUS = "400 "+THIS.oSql.cerrormsg
			Response.WRITE("false")
			Response.END()
			RETURN
		ENDIF

	CASE lcStore="3"	&& Clients
		cList="74"
		IF THIS.oNopSql.EXECUTE("EXEC ANQ_Brevo_AllCustomers @Role='Client', @new="+lcNew+",@Days="+lcDays+;
				IIF(!EMPTY(lcCustno),",@cCustno='"+lcCustno+"'",""),"TIMP")<>1
			Response.STATUS = "400 "+THIS.oNopSql.cerrormsg
			Response.WRITE("false")
			Response.END()
			RETURN
		ENDIF

	CASE lcStore="4"	&& Customers
		cList="75"
		IF THIS.oNopSql.EXECUTE("EXEC ANQ_Brevo_AllCustomers @Role='Customer', @new="+lcNew+",@Days="+lcDays+;
				IIF(!EMPTY(lcCustno),",@cCustno='"+lcCustno+"'",""),"TIMP")<>1
			Response.STATUS = "400 "+THIS.oNopSql.cerrormsg
			Response.WRITE("false")
			Response.END()
			RETURN
		ENDIF
	OTHERWISE
		Response.STATUS = "400 Invalid Store Code"
		Response.WRITE("false")
		Response.END()
		RETURN
	ENDCASE



	oXml=THIS.oXml
	=LOGSTRING("Sync "+TRANSFORM(RECCOUNT("TIMP"))+" records to Brevo",ERROR_LOG)

	SET STEP ON
	SELECT TIMP
	SCAN &&FOR BrevoID=25120 && NEXT 1
		SCATTER MEMVAR MEMO
		m.email=LOWER(ALLTRIM(m.email))
		=LOGSTRING("Sync "+m.UserName+" to Brevo",ERROR_LOG)

		lRetry=.T.
		DO WHILE lRetry

			TEXT TO lcJson TEXTMERGE NOSHOW
{
    "email": "<<LOWER(ALLTRIM(m.Email))>>",
    "emailBlacklisted": false,
    "smsBlacklisted": <<smsBlacklisted>>,
    "listIds": [<<cList>>]
    ,
    "listUnsubscribed": null,
    "attributes": {
	    "email": "<<LOWER(ALLTRIM(m.Email))>>",
        "LANGUAGE": "<<m.language>>",
        "FIRSTNAME": "<<oxml.EncodeXML(ALLTRIM(STRTRAN(m.FirstName,["],[\"])))>>",
        "LASTNAME": "<<oxml.EncodeXML(ALLTRIM(m.LastName))>>",
        "USERNAME": "<<ALLTRIM(m.UserName)>>",
        "PHONE": "<<CHRTRAN(m.cPhone,'+','')>>",
        "SMS": "<<CHRTRAN(m.cWhatsAppNumber,'+','')>>",
        "FAX": "<<CHRTRAN(m.cFax,'+','')>>",
        "STORE_ID": "<<lcStore>>",
        "GENDER": "<<m.Gender>>",
        "DATE_OF_BIRTH": "<<m.dateofBirth>>",
        "ETHICITY": "<<m.Ethnicity>>",
        "COUNTRY": "<<m.country>>",
        "COMPANY": "<<oxml.EncodeXML(ALLTRIM(m.Company))>>",
        "ADDRESS_1" : "<<oxml.EncodeXML(ALLTRIM(m.Address_1))>>",
        "ADDRESS_2": "<<oxml.EncodeXML(ALLTRIM(m.Address_2))>>",
        "ZIP_CODE": "<<m.ZipPostalCode>>",
        "CITY" : "<<m.City>>",
        "STATE" : "<<m.State>>",
        "STATUS" : "<<m.Status>>",
        << IIF(!EMPTY(m.cWhatsAppNumber),["WHATSAPP" :"]+CHRTRAN(m.cWhatsAppNumber,'+','')+[",],[]) >>
        "NOPCOMMERCE_CA_USER": 0,
        "NOPCOMMERCE_LAST_30_DAYS_CA": 0,
        "NOPCOMMERCE_ORDER_TOTAL": 0,
        "LASTORDER" : "<<NVL(TTOD(m.lastorder),'')>>",
        "FIRSTORDER" : "<<NVL(TTOD(m.firstorder),'')>>",
        "SALES_MONTH" : <<NVL(m.SalesMonth,0)>>,
        "SALES_TOTAL" : <<NVL(m.SalesTotal,0)>>,
        "ROLE" : "<<m.Role>>"
    }
}
			ENDTEXT


			IF !ISNULLOREMPTY(m.BrevoID) AND m.BrevoID<>-1 AND m.BrevoID<>0
				luret=THIS.oSync.UpdateContact( TRANSFORM(m.BrevoID),lcJson)

			ELSE

				luret=THIS.oSync.AddContact(lcJson)

				IF VARTYPE(luret)="O" AND PEMSTATUS(luret,"ID",5)
					IF lcStore="1"
						lOk=THIS.oNopSql.EXECUTENONQUERY(;
							"UPDATE ANQ_UserProfileAdditionalInfo SET BrevoID="+;
							TRANSFORM(luret.ID)+" WHERE CustomerID="+TRANSFORM(m.CustomerID))
					ENDIF
					IF lcStore="2"
						lOk=oWSSql.EXECUTENONQUERY(;
							"UPDATE Customer SET BrevoID="+;
							TRANSFORM(luret.ID)+" WHERE ID="+TRANSFORM(m.CustomerID))
					ENDIF
					IF lcStore ="3" OR lcStore="4"
						TEXT TO lcSQL TEXTMERGE NOSHOW

    IF NOT EXISTS(select 1 from ANQ_UserProfileAdditionalInfo WHERE Customerid=<<TRANSFORM(m.CustomerID)>>)
	BEGIN
		INSERT ANQ_UserProfileAdditionalInfo ( [CustomerId],[WhatsappNumber] )
		VALUES ( <<TRANSFORM(m.CustomerID)>>,  '<<m.cWhatsAppNumber>>'   )

	END
    UPDATE ANQ_UserProfileAdditionalInfo SET BrevoID=<<TRANSFORM(luRet.ID)>>
     WHERE CustomerID=<<TRANSFORM(m.CustomerID))>>
						ENDTEXT


						lOk=THIS.oNopSql.EXECUTENONQUERY(lcSql)
					ENDIF


					EXIT

				ENDIF

			ENDIF

			IF VARTYPE(luret)="O" AND PEMSTATUS(luret,"Message",5)
				DO CASE
				CASE luret.MESSAGE="Unable to create contact, WHATSAPP is already associated with another Contact"
					=LOGSTRING("Sync "+m.UserName+" "+m.cWhatsAppNumber+" "+luret.MESSAGE,ERROR_LOG)
					m.cWhatsAppNumber=""
					LOOP

				CASE luret.MESSAGE= "Unable to create contact, SMS is already associated with another Contact"
					=LOGSTRING("Sync "+m.UserName+" "+m.cWhatsAppNumber+" "+luret.MESSAGE,ERROR_LOG)
					m.cWhatsAppNumber=""
					LOOP

				CASE luret.MESSAGE= "Invalid phone number" OR luret.MESSAGE= "Unable to update contact, SMS or WHATSAPP are already associated with another Contact"
					=LOGSTRING("Sync "+m.UserName+" "+m.cPhone+" "+luret.MESSAGE,ERROR_LOG)
					m.cWhatsAppNumber=""
					m.cPhone=""
					LOOP

				CASE luret.MESSAGE="Contact already exist"
					luret=THIS.oSync.GetContactDetails(m.email)

					IF VARTYPE(luret)="O" AND PEMSTATUS(luret,"ID",5)
						IF lcStore="2"
							lOk=oWSSql.EXECUTENONQUERY(;
								"UPDATE Customer SET BrevoID="+;
								TRANSFORM(luret.ID)+" WHERE ID="+TRANSFORM(m.CustomerID))
						ELSE
							lOk=THIS.oNopSql.EXECUTENONQUERY(;
								"UPDATE ANQ_UserProfileAdditionalInfo SET BrevoID="+;
								TRANSFORM(luret.ID)+" WHERE CustomerID="+TRANSFORM(m.CustomerID))
						ENDIF
						luret=THIS.oSync.UpdateContact( TRANSFORM(luret.ID),lcJson)
						EXIT

					ENDIF
					EXIT

				CASE luret.MESSAGE="Unable to create contact, email is already associated with another Contact"
					luret=THIS.oSync.GetContactDetails(m.email)
						IF lcStore ="3" OR lcStore="4"
						TEXT TO lcSQL TEXTMERGE NOSHOW

    IF NOT EXISTS(select 1 from ANQ_UserProfileAdditionalInfo WHERE Customerid=<<TRANSFORM(m.CustomerID)>>)
	BEGIN
		INSERT ANQ_UserProfileAdditionalInfo ( [CustomerId],[WhatsappNumber] )
		VALUES ( <<TRANSFORM(m.CustomerID)>>,  '<<m.cWhatsAppNumber>>'   )

	END
    UPDATE ANQ_UserProfileAdditionalInfo SET BrevoID=<<TRANSFORM(luRet.ID)>>
     WHERE CustomerID=<<TRANSFORM(m.CustomerID))>>
						ENDTEXT


						lOk=THIS.oNopSql.EXECUTENONQUERY(lcSql)
					ENDIF

					luret=THIS.oSync.UpdateContact( TRANSFORM(luret.ID),lcJson)
					EXIT

				OTHERWISE

					=LOGSTRING("Sync "+m.UserName+" "+luret.MESSAGE,ERROR_LOG)
					IF lcStore="2"
						lOk=oWSSql.EXECUTENONQUERY(;
							"UPDATE Customer SET BrevoID=-1"+;
							" WHERE ID="+TRANSFORM(m.CustomerID))
					ELSE
						lOk=THIS.oNopSql.EXECUTENONQUERY(;
							"UPDATE ANQ_UserProfileAdditionalInfo SET BrevoID=-1"+;
							" WHERE CustomerID="+TRANSFORM(m.CustomerID))
					ENDIF

					EXIT
&& LOGSTRING
				ENDCASE


			ENDIF
			EXIT
		ENDDO



	ENDSCAN

	ENDFUNC

&&-----------------------------------------------------------------------------
	FUNCTION SyncStatusBrevo
&&-----------------------------------------------------------------------------


	IF THIS.oSql.EXECUTE("sp_Brevo_GetDataStatus","TUpdates")<>2
		Response.STATUS = "400 "+THIS.oNopSql.cerrormsg
		Response.WRITE("false")
		Response.END()
		RETURN
	ENDIF

	=LOGSTRING("Sync "+TRANSFORM(RECCOUNT("TUPDATES"))+" records to Brevo",ERROR_LOG)

	SELECT TUpdates
	SCAN
		SCATTER MEMVAR MEMO


		TEXT TO lcSql TEXTMERGE NOSHOW
DECLARE
    @json nvarchar (max);

;WITH src (n) AS
(
	<<Tupdates1.cSql>>
	WHERE c.ID=<<m.id>>
	 FOR JSON PATH, Root('contacts')
	)
SELECT @json = src.n
FROM src

SELECT @json JSON, LEN(@json);

		ENDTEXT
		IF THIS.oNopSql.EXECUTE(lcSql,"TTT")=1

			SELECT ttt
			=LOGSTRING("Sync  Status"+TRANSFORM(m.ID)+" to Brevo",ERROR_LOG)

			luret=THIS.oSync.UpdateContactBatch(ttt.Json)

			IF VARTYPE(luret)="O" AND THIS.oSync.ohTTP.CRESULTCODE='204'
*lOk=this.oSql.EXECUTENONQUERY(;
"UPDATE BrevoData SET lUpdate=0 WHERE ID="+TRANSFORM(m.ID))
			ELSE
				=LOGSTRING(	TRANSFORM(m.ID)+ " " +THIS.oSync.ohTTP.CRESULTCODE+" "+ttt.Json,ERROR_LOG))
			ENDIF


		ENDIF


	ENDSCAN
	ENDFUNC


&&-----------------------------------------------------------------------------
	FUNCTION SyncDataToBrevo
&&-----------------------------------------------------------------------------


	lcStore=REQUEST.params("StoreID")
	IF EMPTY(lcStore)
		lcStore="1"
	ENDIF
	IF lcstore="3" OR lcStore="4"
		sp="sp_Brevo_GetDataClientsCustomers"
	ELSE	
		sp="sp_Brevo_GetData"
	ENDIF	
	
	IF THIS.oSql.EXECUTE(sp,"TUpdates")<>2
		Response.STATUS = "400 "+THIS.oNopSql.cerrormsg
		Response.WRITE("false")
		Response.END()
		RETURN
	ENDIF
	
	
	

	=LOGSTRING("Sync "+TRANSFORM(RECCOUNT("TUPDATES"))+" records to Brevo",ERROR_LOG)
	SET STEP ON
	SELECT TUpdates
	SCAN
		SCATTER MEMVAR MEMO

		TEXT TO lcSql TEXTMERGE NOSHOW
DECLARE
    @json nvarchar (max);

;WITH src (n) AS
(
	<<Tupdates1.cSql>>
	WHERE b.ID=<<m.id>>
	AND lUpdate=1 FOR JSON PATH, Root('contacts')
	)
SELECT @json = src.n
FROM src

SELECT @json JSON, LEN(@json);
		ENDTEXT



*!*	TEXT TO lcSql TEXTMERGE NOSHOW
*!*	DECLARE
*!*	    @json nvarchar (max);

*!*	;WITH src (n) AS
*!*	(
*!*		<<Tupdates1.cSql>>
*!*		WHERE c.ID=<<m.id>>
*!*		 FOR JSON PATH, Root('contacts')
*!*		)
*!*	SELECT @json = src.n
*!*	FROM src

*!*	SELECT @json JSON, LEN(@json);
*!*	ENDTEXT
		luret= THIS.oNopSql.EXECUTE(lcSql,"TTT")
		IF luRet=1

			SELECT ttt
			=LOGSTRING("Sync "+TRANSFORM(m.ID)+" to Brevo",ERROR_LOG)

			luret=THIS.oSync.UpdateContactBatch(ttt.Json)

			IF VARTYPE(luret)="O" AND THIS.oSync.ohTTP.CRESULTCODE='204'
				lOk=THIS.oSql.EXECUTENONQUERY(;
					"UPDATE BrevoData SET lUpdate=0 WHERE ID="+TRANSFORM(m.ID))
			ELSE
				=LOGSTRING(	TRANSFORM(m.ID)+ " " +THIS.oSync.ohTTP.CRESULTCODE+" "+ttt.Json,ERROR_LOG))
			ENDIF


		ENDIF


	ENDSCAN
	ENDFUNC

&&-----------------------------------------------------------------------------
	FUNCTION BrevoUpdate
&&-----------------------------------------------------------------------------


	IF USED("TBrevo")
		USE IN TBrevo
	ENDIF
	oBrevo=CREATEOBJECT("BrevoLog")
	oBrevo.SetSqlObject(THIS.oSql)
	TEXT TO lcSql NOSHOW
SELECT l.*,c.ListID,c.type,c.Sender,c.SenderName,c.tags from BrevoLog l
JOIN BrevoCampaign c ON l.CampaignID=c.CampaignID where dUpdated IS NULL
 AND GETDATE() BETWEEN ActiveFrom and ActiveTO
	ENDTEXT

	IF oBrevo.QUERY(lcSql,"TBrevo")=0
		=LOGSTRING("Nothig to Process "+oBrevo.cerrormsg,ERROR_LOG)
*	RETURN
	ENDIF
	LOCAL lBrevo
	THIS.oSync.SetError("")
	SELECT TBrevo
	SCAN
		SCATTER MEMVAR MEMO FIELDS EXCEPT Response
		lBrevo=oBrevo.LOAD(m.ID)
		DO CASE
*!*			CASE UPPER(m.mcAction)="TAG"
*!*
*!*			luRet= this.oSync.AddTag(ALLTRIM(m.cEmail),TRANSFORM(m.ListID)

		CASE UPPER(m.Action)="LIST"

			TEXT TO lcJson TEXTMERGE NOSHOW
{
  "emails": [
    "<<ALLTRIM(m.cEmail)>>"
     ]
}
			ENDTEXT
			luret= THIS.oSync.ADDLIST(TRANSFORM(m.ListID),lcJson)
			IF PEMSTATUS(luret,"contacts",5) AND PEMSTATUS(luret.contacts.success,"count",5)
				IF luret.contacts.success.COUNT>0
					oBrevo.oData.Response="OK"
					oBrevo.oData.dUpdated=DATETIME()

				ENDIF
			ELSE

				IF PEMSTATUS(luret,"message",5)
					oBrevo.oData.Response=luret.MESSAGE
					IF luret.MESSAGE="Contact already"
						oBrevo.oData.Response="OK"
						oBrevo.oData.dUpdated=DATETIME()
					ENDIF
				ELSE
					oBrevo.oData.Response=THIS.oSync.cerrormsg
				ENDIF
			ENDIF

		CASE UPPER(m.Action)="TRANS"

			luret=THIS.oSync.SendTransactionEmail(m.Sender,m.SenderName,;
				m.cEmail,m.cCompany,m.ListID,m.params,m.tags)
			IF PEMSTATUS(luret,"messageID",5)
				oBrevo.oData.Response=luret.messageID
*oBrevo.oData.Response="OK"
				oBrevo.oData.dUpdated=DATETIME()
			ELSE

				oBrevo.oData.dUpdated=DATETIME()
				oBrevo.oData.Response=THIS.oSync.cerrormsg
			ENDIF

		CASE UPPER(m.Action)="WHATSAPP"

			luret=THIS.oSync.SendWhatsApp(m.Sender,m.SenderName,;
				m.cEmail,m.cCompany,m.ListID,m.params,m.tags)
			IF PEMSTATUS(luret,"messageID",5)
				oBrevo.oData.Response=luret.messageID
*oBrevo.oData.Response="OK"
				oBrevo.oData.dUpdated=DATETIME()
			ELSE

				oBrevo.oData.dUpdated=DATETIME()
				oBrevo.oData.Response=THIS.oSync.cerrormsg
			ENDIF

		ENDCASE

		oBrevo.SAVE()




	ENDSCAN

	ENDFUNC

ENDDEFINE
