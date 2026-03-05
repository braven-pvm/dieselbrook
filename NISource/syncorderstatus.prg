#DEFINE ERROR_LOG     "SynchOrderStatuslog"+DTOS(DATE())+".log"
#DEFINE INI_FILE	  "nopIntegration.ini"
#DEFINE LOGTIME		  STRTRAN(LEFT(TIME(),5),":","")
#INCLUDE wconnect.h
DO wconnect
DO WWUTILS
SET PROCEDURE TO WWSQL ADDIT
SET PROCEDURE TO wwJsonSerializer ADDIT
SET PROCEDURE TO wwBusinessObject ADDIT
SET PROCEDURE TO SyncClass ADDIT
SET PROCEDURE TO NOPAPI ADDIT
SET PROCEDURE TO BaseData ADDIT
SET PROCEDURE TO AMData ADDIT
SET PROCEDURE TO NOPData ADDIT
SET PROCEDURE TO WsCourier ADDIT
SET DATE YMD
CLEAR



&&----------------------------------------------------------------------------
DEFINE CLASS SyncOrderStatus  AS SyncClass
&&----------------------------------------------------------------------------
&&----------------------------------------------------------------------------
FUNCTION SyncAll
&&----------------------------------------------------------------------------
loOrder = CREATE("Orders")
loOrder.SetSQLObject(oNopsql)
TEXT TO lcSQL TEXTMERGE NOSHOW
SELECT o.ID,o.shippingstatusid,o.CustomOrderNumber,s.trackingnumber,s.ID ShipMentID,s.ShippedDateUtc,
s.AdminComment,s.DeliveryDateUtc,datediff(dd,o.createdonUtc,getdate()) age FROM [Order] o
 LEFT JOIN Shipment s on o.id=s.orderid
   where 
   OrderStatusID=20 and shippingstatusid in (20,25,30   ) 
  	AND (PaymentStatusId=30 OR OrderTotal=0) 
   and datediff(dd,o.createdonUtc,getdate())<30
	AND s.ID IS NOT NULL  
	--and o.id=942373 --929748
ENDTEXT   

ln=loOrder.QUERY(lcSql,"TOrders")
IF ln=0
	LogString("None to update",ERROR_LOG)
	RETURN
ENDIF
LogString(TRANSFORM(ln)+" to update",ERROR_LOG)
lnUPdated=0
SELECT TOrders
SCAN 
	SCATTER NAME oTOrder MEMO
	LogString(TRANSFORM(oTOrder.id)+" Updating",ERROR_LOG)
	this.Sync(oTOrder)
ENDSCAN
LogString(TRANSFORM(lnUpdated)+" Updates",ERROR_LOG)
ENDFUNC


&&----------------------------------------------------------------------------
FUNCTION SyncOne (lnOrderID)
&&----------------------------------------------------------------------------
SET STEP ON 
loOrder = CREATE("Orders")
loOrder.SetSQLObject(oNopsql)
IF USED("TORDERS")
USE IN TORDERS
ENDIF

TEXT TO lcSQL TEXTMERGE NOSHOW
SELECT o.ID,o.shippingstatusid,o.CustomOrderNumber,s.trackingnumber,s.ID ShipMentID,s.ShippedDateUtc,
s.AdminComment,s.DeliveryDateUtc,datediff(dd,o.createdonUtc,getdate()) age FROM [Order] o
 LEFT JOIN Shipment s on o.id=s.orderid
   where o.id=<<lnOrderID>> AND 
   OrderStatusID=20 and shippingstatusid in (20,25,30   ) 
  	AND (PaymentStatusId=30 OR OrderTotal=0) 
   and datediff(dd,o.createdonUtc,getdate())<60
	AND s.ID IS NOT NULL  
ENDTEXT   

ln=loOrder.QUERY(lcSql,"TOrders")
IF ln=0
	LogString("None to update",ERROR_LOG)
	RETURN
ENDIF
LogString(TRANSFORM(ln)+" to update",ERROR_LOG)
lnUPdated=0
SELECT TOrders
SCAN 
	SCATTER NAME oTOrder MEMO
	LogString(TRANSFORM(oTOrder.id)+" Updating",ERROR_LOG)
	this.Sync(oTOrder)
ENDSCAN
LogString(TRANSFORM(lnUpdated)+" Updates",ERROR_LOG)
ENDFUNC


&&----------------------------------------------------------------------------
FUNCTION Sync(loTOrder)
&&----------------------------------------------------------------------------
	
#if .f.
	IF 	loTOrder.Age>=30 

		*? loTorder.CustomOrderNumber
TEXT TO lcSql  TEXTMERGE NOSHOW
SELECT ShippedDateUtc from Shipment WHERE ID=<<TRANSFORM(loTorder.ShipMentID)>> and 
 ShippedDateUtc IS NOT NULL
ENDTEXT	
luret=loOrder.query(lcSql,"TS1")
IF luRet=0 AND loOrder.lError<>1

		luret=this.oNop.ShipMent_Send(loTorder.ShipMentID)
		IF !luRet
			TEXT TO lcSql  TEXTMERGE NOSHOW
UPDATE [order] SET shippingstatusid=40 WHERE ID=<<TRANSFORM(loTorder.ID)>>
			ENDTEXT	
			luret=loOrder.osql.Executenonquery(lcSql)
			RETURN
		ENDIF
		luret=this.oNop.ShipMent_Deliver(loTorder.ShipMentID,.t.)

		RETURN
	ENDIF	

ENDIF	
	#endif
	
	loSosord = CREATEOBJECT("sosord",oAMSQL)
	
	
	IF !loSosord.load(PADL(loTorder.CustomOrderNumber,10))
		LogString("Order not available :"+loTOrder.CustomOrderNumber,ERROR_LOG)
		RETURN
	ENDIF
	oData=loSosord.oData
	lcSql=""
	
	

	
TEXT TO lcSQL TEXTMERGE NOSHOW
EXEC sp_ws_getorderstatus  <<loTorder.id>>
ENDTEXT		


IF USED("TShip")
USE IN TShip
ENDIF

IF loSosord.oSql.Execute(lcSql,"TShip")=0 OR loSosord.oSql.lError
	LogString("SQL ERROR "+loSosord.oSql.cErrorMsg,ERROR_LOG)
	RETURN
ENDIF


lctracking="" 
lcObj=""
	DO CASE 	
		CASE oData.cShipVia="BERCO"
			lcObj="Aramex"
			lcTracking='https://www.aramex.com/ar/en/track/track-results-new?ShipmentNumber='+ TShip.cWaybillno
		CASE oData.cShipVia="FASTWAY"	
			lcObj="Fastway"
			lcTracking='https://www.fastway.co.za/our-services/track-your-parcel?l=' + TShip.cWaybillno
	
		CASE oData.cShipVia="POSTNET"
			lcObj="Aramex"
			lcTracking='https://www.aramex.com/ar/en/track/track-results-new?ShipmentNumber='+ TShip.cWaybillno
		
		CASE oData.cShipVia="SKYNET"
			*lcTracking= 'https://www.paxi.co.za/track?drop_label=' +TShip.cWaybillno
			lcTracking= 'https://parcel-tracking.paxiplatform.com/?id=' +TShip.cWaybillno
			lcObj="Skynet"
			
		CASE oData.cShipVia="COLLECT"
			lcObj="Collect"
			REPLACE cWaybillno WITH TSHIP.cInvno IN TSHIP
	ENDCASE	
	
	
	
lcSql=""
THIS.lError=.F.
DO WHILE .T.
	lcAdminComment=loTorder.AdminComment
	IF (ISNULLOREMPTY(TShip.cInvno) OR ISNULLOREMPTY(TShip.cWaybillno)) 
		
		DO CASE
			CASE (ISNULLOREMPTY(TShip.cInvno) AND ISNULLOREMPTY(TShip.cWaybillno))
				loTorder.AdminComment='Picking' 
		
			CASE ISNULLOREMPTY(TShip.cWaybillno)
				loTorder.AdminComment='Waiting for Courier Collection ' +lcObj
		ENDCASE	
		
		IF lcAdminComment<>loTorder.AdminComment
	
				
		TEXT TO lcSql TEXTMERGE NOSHOW
		UPDATE shipment SET AdminComment='<<loTorder.AdminComment>>'
         	 WHERE id=<<TRANSFORM(loTorder.ShipMentID)>> and AdminComment<>'<<loTorder.AdminComment>>'
         	
        ENDTEXT 
        TEXT TO lcJson NOSHOW TEXTMERGE
        {"order_id": <<loTorder.ID>>,"note": "<<loTorder.AdminComment>>",
         "download_id": 0,"display_to_customer": true,"id": 0,
         "created_on_utc": '<<TOISODATESTRING(DATETIME(),.t.,.t.)>>'
			}
        ENDTEXT
        luret=this.oNop.OrderNote_Create(lcJson)
      
		ENDIF
      
      
    ENDIF
    
	lcWaybillno=""
	lcInvno=""
	IF (INLIST(loTorder.shippingStatusID,20,25)) OR ;
		(ISNULL(loTorder.trackingnumber)  OR loTorder.trackingnumber<>TShip.cWaybillno)  && Picking Check if invoiced
		lnShippingMethod=TShip.ShippingMethod
		lcWaybillno=TShip.cWaybillno
		lcInvno=TShip.cInvno
		&&newstatus=IIF(ISNULL(m.trackingnumber),m.ShippingStatusID,IIF(lnShippingMethod="3",40,30))

		IF 	(ISNULL(loTorder.trackingnumber) OR loTorder.trackingnumber<>TShip.cWaybillno)	;
			AND !ISNULLOREMPTY(lcWayBillno)
			TEXT TO lcSql ADDITIVE TEXTMERGE NOSHOW
			 UPDATE shipment SET trackingnumber='<<lcWayBillNo>>' 
         	 WHERE id=<<TRANSFORM(loTorder.ShipMentID)>>
         	 
         	ENDTEXT 
		ENDIF		
	
	IF EMPTY(lcObj)
		EXIT
	ENDIF
	
	IF EMPTY(lcObj) OR lcObj='Collect'
*!*			TEXT TO lcSql ADDITIVE TEXTMERGE NOSHOW
*!*	         UPDATE [order] SET shippingstatusid=40,OrderStatusID=30 WHERE ID=<<TRANSFORM(M.ID)>>
*!*	         UPDATE shipment SET DeliveryDateUtc=GETDATE(),AdminComment='No Tracking for this Shipping Method ' 
*!*	         	 WHERE id=<<TRANSFORM(M.ShipMentID)>>
*!*	        ENDTEXT

TEXT TO lcSql1  TEXTMERGE NOSHOW
SELECT ShippedDateUtc from Shipment WHERE ID=<<TRANSFORM(loTorder.ShipMentID)>> and 
 ShippedDateUtc IS NOT NULL
ENDTEXT	
luret=loOrder.query(lcSql1,"TS1")
IF luRet=0 AND !loOrder.lError
		luret=this.oNop.ShipMent_Send(loTorder.ShipMentID)
		luret=this.oNop.ShipMent_Deliver(loTorder.ShipMentID)
ENDIF

        EXIT
		ENDIF

	ENDIF
	IF EMPTY(lcObj) OR lcObj='Collect'
		RETURN
	ENDIF
	
	IF ISNULLOREMPTY(TShip.cWaybillno)
		EXIT
	ENDIF
	
	loTrack=CREATEOBJECT(lcObj)
	IF USED("crecs")
		USE IN cRecs
	ENDIF
	luret=loTrack.trackandtrace(TShip.cWaybillno)
	IF VARTYPE(luRet)="C" OR !luret
	SET STEP ON 
		THIS.lerror=.T.
		THIS.cerrormsg="Shipment data not available :"+TShip.cWaybillno
		EXIT
	ENDIF

	IF !USED("cRecs") OR RECCOUNT("cRecs")=0
		loTorder.AdminComment='Waiting for Courier Collection ' +lcObj
		IF lcAdminComment=loTorder.AdminComment
			EXIT
		ENDIF
		
		TEXT TO lcSql TEXTMERGE NOSHOW ADDITIVE 
		
		UPDATE shipment SET AdminComment='<<loTorder.AdminComment>>'
         	 WHERE id=<<TRANSFORM(loTorder.ShipMentID)>> and AdminComment<>'<<loTorder.AdminComment>>'
  	
        ENDTEXT 
        EXIT
	ENDIF	
	
	SELECT cRecs
	SET DATE BRITISH
	lcd=CTOT(cRecs.Date)
	SET DATE YMD
	LOCATE FOR cRecs.STATUS="YES" OR cRecs.STATUS="SH005" OR cRecs.STATUS="CLS" ;
	 OR LOWER(cRecs.StatusDescription)='delivered'     OR  INLIST(cRecs.STATUS,"ATL","PHO","PCY","R35","R34","LAI","R36")              
	IF FOUND()
*!*			TEXT TO lcSql ADDITIVE TEXTMERGE NOSHOW
*!*	         UPDATE [order] SET orderstatusID=30,shippingstatusid=40 WHERE ID=<<TRANSFORM(M.ID)>>
*!*	         UPDATE shipment SET DeliveryDateUtc='<<TTOC(lcd)>>',AdminComment='Delivered' 
*!*	          WHERE Orderid=<<TRANSFORM(M.ID)>>
*!*	        ENDTEXT
		IF ISNULL(loTorder.ShippedDateUtc)
        luret=this.oNop.ShipMent_Send(loTorder.ShipMentID)
        ENDIF
        IF ISNULL(loTorder.DeliveryDateUtc) OR loTOrder.shippingstatusid<>40
		luret=this.oNop.ShipMent_Deliver(loTorder.ShipMentID)
		IF !luRet
			TEXT TO lcSql ADDITIVE TEXTMERGE NOSHOW
              UPDATE [order] SET orderstatusID=30,shippingstatusid=40 WHERE ID=<<TRANSFORM(loTorder.ID)>>
		    ENDTEXT
		ENDIF 
		 TEXT TO lcJson NOSHOW TEXTMERGE
        {"order_id": <<loTorder.ID>>,"note": "Delivered",
         "download_id": 0,"display_to_customer": true,"id": 0,
         "created_on_utc": '<<TOISODATESTRING(DATETIME(),.t.,.t.)>>'
			}
        ENDTEXT
        luret=this.oNop.OrderNote_Create(lcJson)
        
        
        TEXT TO lcSql ADDITIVE TEXTMERGE NOSHOW
	         UPDATE shipment SET DeliveryDateUtc='<<TTOC(lcd)>>',AdminComment='Delivered' 
	           WHERE id=<<TRANSFORM(loTorder.ShipMentID)>> and AdminComment<>'Delivered' 
	    ENDTEXT
	    ENDIF
	    
	    
		EXIT
	ELSE

	
	SELECT cRecs
	LOCATE FOR cRecs.STATUS="PPP" OR INLIST(ALLTRIM(cRecs.STATUS) ,"SH01","SH001","SH014","523","517")
	IF FOUND() AND lcAdminComment<>'With Courier '+lcObj
		TEXT TO lcSql ADDITIVE TEXTMERGE NOSHOW
			--UPDATE [order] SET shippingstatusid=40 WHERE ID=<<TRANSFORM(loTorder.ID)>>,ShipMentDateUtc='<<TTOC(lcd)>>'
			UPDATE shipment SET AdminComment='With Courier '+'<<lcObj>>' 
         	 WHERE id=<<TRANSFORM(loTorder.ShipMentID)>>
		ENDTEXT	
		luret=this.oNop.ShipMent_Send(loTorder.ShipMentID)
		 TEXT TO lcJson NOSHOW TEXTMERGE
        {"order_id": <<loTorder.ID>>,"note": "You can Track your order at <<lcTracking>>",
         "download_id": 0,"display_to_customer": true,"id": 0,
         "created_on_utc": '<<TOISODATESTRING(DATETIME(),.t.,.t.)>>'
			}
        ENDTEXT
        luret=this.oNop.OrderNote_Create(lcJson)
		
		EXIT
	ENDIF	
	
	EXIT
	ENDIF
ENDDO	
	
	IF this.lerror
		=LOGSTRING("Error on Update "+this.cErrorMsg,ERROR_LOG ) 
		RETURN .F.
	ENDIF	
	IF EMPTY(lcSql)
		RETURN
	ENDIF
	lcSql=ALLTRIM(lcSql)
	IF !EMPTY(lcSql)
		IF !loOrder.osql.Executenonquery(lcSql)
			=LOGSTRING("Error on Update "+loOrder.cErrorMsg,ERROR_LOG ) 
		ELSE
		lnUPdated=lnUpdated+1
		ENDIF
	ENDIF
	RETURN

ENDFUNC

ENDDEFINE
&&----------------------------------------------------------------------------

&&----------------------------------------------------------------------------
DEFINE CLASS CustomConfig  as wwConfig
cFileName = "NopIntegration.ini"
cMode = "INI"
*** Persist properties
cDataPath = ".\data"
Htmlpagepath=""
cAMSqlconnectstring=""
cNopSqlconnectstring=""
cWSSqlconnectstring=""
ENDDEFINE

