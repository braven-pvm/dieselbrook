DEFINE CLASS BusBase AS wwbusinessobject

cDatabaseName=''
cSkipFieldsforUpdates="ID"
cPkField="ID"
laudit = .F.
auditexclude = "lastuser,dlastupdate"
oaudit = .NULL.
linternalerror = .F.
ndatamode = 2
lcompareupdates = .T.
Name = "busbase"


	PROCEDURE getlist
		LPARAMETER lcFilter,lcCursor, lcFieldList,lnResultmode
		LOCAL loRecord, lcPKField, lnResult

		IF EMPTY(lcFieldList)
		  lcFieldList = "*"
		ENDIF
		IF EMPTY(lnResultmode)
			lnResultMode=0
		ENDIF

		THIS.SetError()

		lcOldCursor = THIS.cSQLCursor 
		THIS.osql.cSQLCursor = IIF(!EMPTY(lcCursor),lcCursor,THIS.cSQLCursor)
		lcCursor=THIS.cSQLCursor
		lnResult = this.oSQL.Execute("select "  + lcFieldList + " from " + THIS.cFileName + IIF(!EMPTY(lcFilter)," where ","") + lcFilter,lcCursor)
		IF lnResult < 0
		    THIS.seterror(THIS.osql.cErrorMsg)
		    RETURN 0
		ENDIF
		THIS.cSQLCursor = lcOldCursor
		lnResult = RECCOUNT()
		*** Convert data if necessary
		IF lnResultmode # 0
		   THIS.ConvertData(lnResultmode,,lcCursor)   &&51 is json
		ENDIF
		RETURN lnResult


		 
	ENDPROC


	PROCEDURE save
		lNew=THIS.nupdatemode = 2 
		IF DODEFAULT()
			IF lNew AND PEMSTATUS(this.oData,"ID",5)
				IF THIS.oSql.Execute("SELECT @@identity","cID")=1
					this.oData.ID  = cID.EXP
				ELSE
					THIS.lerror=.T.
					THIS.cErrormsg="Could not get ID"
					RETURN .F.
				ENDIF
			ENDIF

		ELSE
			RETURN.F.
		ENDIF
	ENDPROC


	PROCEDURE new
		LPARAMETERS llNoNewPk
		RETURN DODEFAULT(.T.)
	ENDPROC


ENDDEFINE
*
*-- EndDefine: busbase
