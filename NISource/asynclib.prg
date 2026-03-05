DO WWAsyncWebRequest




DEFINE CLASS ASYNCRESPONSE AS CUSTOM

	cErrorMsg=""
	lError=.f.
	oAsync=NULL
	cId=""
	cAction=""
	cSql=""
	cEventName=""
	cClass=""
	cMethod=""
	

FUNCTION INIT (lcConnectString)
	IF VARTYPE(loAsync)<>'O'
		loAsync = CREATEOBJECT("AsyncWebRequest")
		loAsync.CONNECT(lcConnectString)
	ENDIF
	THIS.oAsync=loAsync
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

FUNCTION Execute(lcAction,lcID,lcEventName,lcClass,lcMethod,lcData)


WITH THIS
	pcProgress=""
	pcErrorMsg=""

	lcAction = LOWER(lcAction)
	IF EMPTY(lcAction)
		THIS.setError("No Action Requested")
		RETURN .f.
	ENDIF


	DO CASE
	*** Place the event
		CASE lcAction = "submit"
*** Create new event, but don't save yet (.T. parm)
			lcId = .oAsync.SubmitEvent(lcEventName,lcEventName,.T.)
			.cId=lcId
			.oAsync.oEvent.completed={^1900-01-01}
			.oAsync.oEvent.started={^1900-01-01}
			.oAsync.SetProperty("CLASS",lcClass)
			.oAsync.SetProperty("METHOD",lcMethod)
			.oAsync.oEvent.InputData=TRANSFORM(lcData)
			lcOldDate=SET("Date")
			SET DATE YMD
			.oAsync.SaveEvent()
			SET DATE &lcOldDate
			lcExe = FULLPATH("runasyncrequest.exe") + ;
					" " + lcId + " CLASS"
			&&DO 	runasyncrequest.prg	
			RUN /n4 &lcExe
	
*** Check for completion
		CASE lcAction = "check"
*** Check for completion

			plComplete=.f.
			lnResult = .oAsync.CheckForCompletion(lcId)
			DO CASE
			
			CASE lnResult = 0
				RETURN .f.
			
			CASE lnResult = 1
				RETURN .t.

			CASE lnResult = -2  && No Event found
				THIS.seterror("Couldn't find a matching event.")
				RETURN .f.

			CASE lnResult = -1  && Cancelled
				THIS.seterror("Cancelled")
				RETURN .f.
			ENDCASE

*** Cancel the Event by user
		CASE lcAction = "cancel"
			.oAsync.CancelEvent(lcId)
			RETURN

		
	ENDCASE



	ENDWITH

ENDDEFINE



DEFINE CLASS AsyncWebRequest AS wwSQLAsyncWebRequest

****************************************
***  Function: Saves the currently open Event object
***      Pass: nothing
***    Return: .T. or .F.
************************************************************************
FUNCTION SaveEvent
LOCAL lcID

lcID = THIS.oEvent.id

THIS.Open()

THIS.oSQL.cSQL = "select * from " + THIS.cAlias + " where id='" + lcID + "'"
THIS.oSQL.cSQLCursor = THIS.cAlias
* _cliptext = THIS.oSQL.cSQL
lnResult = THIS.osql.Execute()
IF lnResult # 1
     THIS.seterror(THIS.oSQL.cErrorMsg)
   RETURN .F.
ENDIF
lcOldDate=SET("Date")
IF RECCOUNT() < 1
   SET DATE YMD
   THIS.oSQL.cSQL = THIS.BuildSQLInsertStatement(THIS.oEvent)
   SET DATE &lcoldDate
   * _cliptext = THIS.oSQL.cSQL
   lnResult = THIS.osql.Execute()
   IF lnResult # 1
        THIS.seterror(THIS.oSQL.cErrorMsg)
      RETURN .F.
   ENDIF
ELSE
   SET DATE YMD
   THIS.oSQL.cSQL = THIS.BuildSQLUpdateStatement(THIS.oEvent)
   SET DATE &lcoldDate
   lnResult = THIS.osql.Execute()
   IF lnResult # 1
        THIS.seterror(THIS.oSQL.cErrorMsg)
      RETURN .F.
   ENDIF
ENDIF

RETURN .T.
ENDFUNC
*  wwAsyncWebRequest :: SaveEvent

ENDDEFINE


