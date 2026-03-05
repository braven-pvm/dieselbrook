#DEFINE ERROR_LOG     "Synchlog"+DTOS(DATE())+".log"
#DEFINE INI_FILE	  "nop.ini"
#DEFINE LOGTIME		  STRTRAN(LEFT(TIME(),5),":","")
#INCLUDE wconnect.h
DO wconnect
DO WWUTILS
EXTERNAL ARRAY op
SET ESCAPE ON
SET PROCEDURE TO WWSQL ADDIT
SET PROCEDURE TO wwJsonSerializer ADDIT
SET PROCEDURE TO wwBusinessObject ADDIT
SET PROCEDURE TO SyncClass ADDIT
SET PROCEDURE TO NOPAPI ADDIT
SET PROCEDURE TO BaseData ADDIT
SET PROCEDURE TO AMData ADDIT
SET PROCEDURE TO NOPData ADDIT
SET DATE YMD
SET TALK OFF
CLEAR


	

DEFINE CLASS SyncOrder AS SyncClass
VatRate=15

PROCEDURE SyncAll
*!*	TEXT TO lcSql NOSHOW
*!*	select o.* from [Order] o
*!*	LEFT OUTER JOIN Shipment s ON s.orderid=o.id
*!*	WHERE OrderStatusID=20 and ShippingStatusId=20 
*!*		AND (PaymentStatusId=30 OR OrderTotal=0) 
*!*		AND s.ID IS NULL
*!*		AND o.ID>30
*!*	ENDTEXT
IF oNopSql.EXECUTE("EXEC ANQ_UnprocessedOrders","TProcess")>0
SELECT TProcess
SCAN
IF !THIS.Sync(Tprocess.ID)
SET STEP ON 
	=LOGSTRING(THIS.cErrormsg,ERROR_LOG)
ENDIF	
ENDSCAN
ENDIF

ENDPROC

PROCEDURE Sync
&&---------------------------------------------------------------------------------------------------------
LPARAMETERS lnOrderID
THIS.SetError()

pnOrderID=lnOrderID
&& oOrder has line items and ocustomer record loaded and shipping address
=LOGSTRING(TRANSFORM(lnOrderID)+ "Start",ERROR_LOG)
oNopSql.EnableUnicodeToAnsiMapping()

loCust=CREATEOBJECT("Customer")
loCust.SetSqlObject(oNopsql)

loShip=CREATEOBJECT("Address")
loShip.SetSqlObject(oNopsql)
loBill=CREATEOBJECT("Address")
loBill.SetSqlObject(oNopsql)
loOrder=CREATEOBJECT("Orders")
loOrder.SetSqlObject(oNopsql)
loOrderItem=CREATEOBJECT("OrderItem")
loOrderItem.SetSqlObject(oNopsql)
loOrderNote=CREATEOBJECT("OrderNote")
loOrderNote.SetSqlObject(oNopsql)
loOrderDiscount=CREATEOBJECT("OrderDiscount")
loOrderDiscount.SetSqlObject(oNopsql)
loAffiliate=CREATEOBJECT("Affiliate")
loAffiliate.SetSqlObject(oNopsql)
loItem= CREATEOBJECT('Product')
loItem.SetSQLObject(oNopsql)


IF USED("Sosord")
USE IN Sosord
ENDIF
IF USED("Sostrs")
	USE IN Sostrs
ENDIF
IF USED("Soskit")
	USE IN Soskit
ENDIF
IF USED("TOrderLines")
	USE IN TOrderLines
ENDIF
IF USED("TOrderNotes")
	USE IN TOrderNotes
ENDIF	

	

IF !loOrder.load(lnOrderID)
	THIS.Seterror("Order Not Found:"+TRANSFORM(lnOrderID))
	RETURN .F.
ENDIF
oOrder=loOrder.oData

&&loOrder.oData.OrderStatusID<>20
DO CASE
CASE loOrder.oData.OrderStatusID=20 AND ShippingStatusId=20 ;
	AND (PaymentStatusId=30 OR OrderTotal=0) 

CASE loOrder.oData.OrderStatusID=30 AND ShippingStatusId=10 ;
	AND (PaymentStatusId=30 OR OrderTotal=0) 

OTHERWISE
THIS.Seterror("Order Not Ready for Processing "+TRANSFORM(lnOrderID))
	RETURN .F.
ENDCASE
	
*!*	IF (loOrder.oData.OrderStatusID<>20) OR loOrder.oData.ShippingStatusId<>20 OR ;
*!*	(loOrder.oData.PaymentStatusId<>30 AND loOrder.oData.OrderTotal<>0)
*!*		
*!*	ENDIF

IF EMPTY(oOrder.CustomOrderNumber)
	THIS.lerror=.T.
	THIS.cerrormsg="No Order Number " +TRANSFORM(oOrder.ID)
	RETURN .F.
ENDIF

lnShip=loOrder.QUERY("select * from Shipment where orderid="+TRANSFORM(oOrder.ID),"tShip")
IF lnShip>0
	THIS.Seterror("Already Processed "+TRANSFORM(lnOrderID))
	RETURN .F.
ENDIF


IF !loCust.Load(oOrder.CustomerID)
	THIS.Seterror("Customer Not Found on Order "+TRANSFORM(lnOrderID))
	RETURN .F.
ENDIF


IF !loShip.GetFullAddress(NVL(oOrder.ShippingAddressID,oOrder.BillingAddressID))
	THIS.Seterror("Customer Ship Address Not Found on Order "+TRANSFORM(lnOrderID))
	RETURN .F.
ENDIF
IF !loBill.GetFullAddress(oOrder.BillingAddressID)
	THIS.Seterror("Customer Bill Address Not Found on Order "+TRANSFORM(lnOrderID))
	RETURN .F.
ENDIF

lnOrderNotes=loOrderNote.QUERY("select * from OrderNote where orderid="+TRANSFORM(oOrder.ID),"tOrderNotes")
lnOrderLines=loOrderItem.QUERY("select * from OrderItem where orderid="+TRANSFORM(oOrder.ID),"tOrderLines")
lnDiscounts = loOrderDiscount.Get_Discounts(oOrder.ID)

IF lnOrderLines=0
	THIS.lerror=.T.
	THIS.cerrormsg="No Order Lines :"+TRANSFORM(lnOrderID)
	RETURN .F.
ENDIF
lInterCo=.f.
DO CASE
	CASE this.oNop.Customer_CheckRole(oOrder.CustomerID,"Annique Consultant")
		lcCustno=ALLTRIM(STRTRAN(loCust.oData.UserName,CHR(0),""))
	CASE this.oNop.Customer_CheckRole(oOrder.CustomerID,"AnniqueStaff")
		lcCustno="STAFF1"
	CASE this.oNop.Customer_CheckRole(oOrder.CustomerID,"Bounty")
		lcCustno="ASHOP1"	
		lInterCo=.t.
	CASE this.oNop.Customer_CheckRole(oOrder.CustomerID,"AnniqueExco")
		lcCustno="STAFF1"
	CASE this.oNop.Customer_CheckRole(oOrder.CustomerID,"Client")
			lcCustno="ASHOP2"
	OTHERWISE
			lcCustno="ASHOP1"
ENDCASE



&& Check Payment Method
op=CREATEOBJECT("COLLECTION")
lnGiftTotal=0
lnGift=loOrder.QUERY("select * from GiftCardUsageHistory where usedWithorderid="+TRANSFORM(oOrder.ID),"tGift")
IF lnGift>0
	SELECT tGift
	SCAN
		lObj=CREATEOBJECT("EMPTY")
		ADDPROPERTY(lObj,"PaymentMethod","giftcard")
		ADDPROPERTY(lObj,"PaymentAmount",tgift.usedvalue)
		op.Add(lObj)
		lnGiftTotal=lnGiftTotal+tgift.usedvalue
	ENDSCAN
ENDIF
lnAmount=0
DO CASE
	CASE oOrder.PaymentMethodSystemName='Atluz.PayUSouthAfrica'
	
		SELECT tOrderNotes
		SCAN FOR  ["successful": true]$Note  &&="<PaymentNotification"

*!*				IF !FOUND()
*!*					THIS.Seterror("No Payment Notification on Order "+TRANSFORM(lnOrderID))
*!*					RETURN .F.
*!*				ENDIF
			oPay=THIS.oSer.Deserialize(tOrderNotes.Note)	
			lnAmount=0
			lcRef=""
			IF oPay.ResultCode<>"00"
				LOOP
			ENDIF
				
			IF !PEMSTATUS(oPay,"PaymentMethodsUsed",5)
			LOOP
			ENDIF
				
			FOR EACH lop IN oPay.PaymentMethodsUsed
				ldup=.f.
				FOR EACH opcheck IN op
					IF PEMSTATUS(opcheck,"reference",5) AND ;
						PEMSTATUS(lop,"reference",5) AND ;
						  lop.reference=opcheck.reference
						  lDup=.t.
						
					ENDIF		
				NEXT
				IF lDup
					LOOP
				ENDIF
						
			
				lObj=CREATEOBJECT("EMPTY")	
				DO CASE 
					CASE PEMSTATUS(lop,"cardnumber",5)
						ADDPROPERTY(lObj,"PaymentMethod","creditcard")
						lnAmount=VAL(lop.AmountinCents)
						lcRef=TRANSFORM(IIF(PEMSTATUS(lop,"gatewayReference",5),lop.gatewayReference,""))
						ADDPROPERTY(lObj,"PaymentAmount",lnAmount/100)
						ADDPROPERTY(lObj,"Reference",lcRef)
						op.Add(lObj)
					CASE PEMSTATUS(lop,"bankname",5)
						ADDPROPERTY(lObj,"PaymentMethod","eft")
						lnAmount=VAL(lop.AmountinCents)
						lcRef=TRANSFORM(lop.Reference)
						ADDPROPERTY(lObj,"PaymentAmount",lnAmount/100)
						ADDPROPERTY(lObj,"Reference",lcRef)
						op.Add(lObj)
						
					OTHERWISE
						IF PEMSTATUS(oPay,"payUReference",5)
							lcPayu=oPay.payUReference
							 oPayu=NEWOBJECT("Payu","PAYU.prg")
							 lcret= oPayu.GetTransaction(lcPayu)
							 IF !EMPTY(lcRet) AND "payflex"$lcRet
							 	ADDPROPERTY(lObj,"PaymentMethod","payflex")
								lnAmount=VAL(lop.AmountinCents)
								lcRef=TRANSFORM(lcpayu)
								ADDPROPERTY(lObj,"PaymentAmount",lnAmount/100)
								ADDPROPERTY(lObj,"Reference",lcRef)
								op.Add(lObj)
							 ENDIF
						ENDIF   
				ENDCASE		
				
				
			NEXT	
			IF op.count>0
			EXIT
			ENDIF
		ENDSCAN	
		IF lnAmount=0
			SELECT tOrderNotes
			SCAN FOR  [Order has been marked as paid]$Note  
				lObj=CREATEOBJECT("EMPTY")	
				ADDPROPERTY(lObj,"PaymentMethod","eft")
				lcRef="Manual Release"
				ADDPROPERTY(lObj,"PaymentAmount",oOrder.OrderTotal-lnGiftTotal)
				ADDPROPERTY(lObj,"Reference",lcRef)
				op.Add(lObj)
				EXIT
				
			ENDSCAN
		ENDIF
		
		*CATCH
		*	THIS.Seterror("Bad Payment Notification on Order "+TRANSFORM(lnOrderID))
		*	ENDTRY
		
		IF THIS.lERROR
			RETURN .F.
		ENDIF	
		
		
	
	CASE oOrder.PaymentMethodSystemName='Payments.CashOnDelivery'
				lObj=CREATEOBJECT("EMPTY")
				ADDPROPERTY(lObj,"PaymentMethod","account")
				ADDPROPERTY(lObj,"PaymentAmount",oOrder.OrderTotal-lnGiftTotal)
				op.Add(lObj)
				
			
	CASE oOrder.PaymentMethodSystemName='Payments.Manual'
				lObj=CREATEOBJECT("EMPTY")	
				ADDPROPERTY(lObj,"PaymentMethod","eft")
				lcRef="Manual Test"
				ADDPROPERTY(lObj,"PaymentAmount",oOrder.OrderTotal-lnGiftTotal)
				ADDPROPERTY(lObj,"Reference",lcRef)
				op.Add(lObj)
				
				
ENDCASE	

IF loOrder.oData.OrderTotal=0
	lObj=CREATEOBJECT("EMPTY")	
	ADDPROPERTY(lObj,"PaymentMethod","eft")
	lcRef="Free"
	ADDPROPERTY(lObj,"PaymentAmount",0)
	ADDPROPERTY(lObj,"Reference",lcRef)
	op.Add(lObj)
endif				



IF op.count=0
	THIS.Seterror("No Valid Payment Notification on Order "+TRANSFORM(lnOrderID))
	RETURN .F.
ENDIF

&& ----- Need to Connect to the correct AM database year
&& Look at settings on NopIntegration
&& 
oAMSQL.EXECUTENONQUERY("SET ANSI_WARNINGS OFF")
loArsyst = CREATEOBJECT("arsyst",oAMSQL) &&,THIS.amserver
loArcust = CREATEOBJECT("arcust",oAMSQL)
loArcash = CREATEOBJECT("arcash",oAMSQL)
loSosord = CREATEOBJECT("sosord",oAMSQL)
loSostrs = CREATEOBJECT("sostrs",oAMSQL)
loSoskit = CREATEOBJECT("soskit",oAMSQL)
loicitem = CREATEOBJECT("icitem",oAMSQL)
loIciwhs = CREATEOBJECT("iciwhs",oAMSQL)
loIcikit = CREATEOBJECT("icikit",oAMSQL)
losoportal =CREATEOBJECT("soportal",oAMSQL)
loSosork = CREATEOBJECT("sosork",oAMSQL)
lHasTicket=.f.

loSostrs.QUERY("select * from sostrs where 0=1","sostrs")
loSostrs.New(.T.)
IF loSostrs.lerror
	THIS.lerror=.T.
	THIS.cerrormsg="Not Ready for Processing :"+TRANSFORM(oOrder.ID)
	RETURN .F.
ENDIF
loSoskit.QUERY("select * from soskit where 0=1","soskit")
IF loSoskit.lerror
	THIS.lerror=.T.
	THIS.cerrormsg="Not Ready for Processing :"+TRANSFORM(oOrder.ID)
	RETURN .F.
ENDIF


&&------------------ Check if not processed before --------------------
IF loSosord.loadbase("cPono='"+TRANSFORM(oOrder.ID)+"' AND ccustno='"+lcCustno+"'")
	THIS.lerror=.T.
	THIS.cerrormsg="Already Processed :"+TRANSFORM(oOrder.ID)
	IF !THIS.Update_nOP()
		RETURN .F.
	ENDIF	
	return
ENDIF

IF !loArcust.LOAD(lcCustno)
	THIS.lerror=.T.
	THIS.cerrormsg="Customer not found :"+lcCustno
	RETURN .F.
ENDIF
oArcust=loArcust.oData
IF oArcust.cStatus <> "A"
	&&THIS.lerror=.T.
	&&THIS.cerrormsg="Account Inactive :"+lccustno
	&&RETURN .F.
	&& Automatic Reactivation 
	IF !oAmSql.ExecuteNonQuery("EXEC sp_ws_reactivate  @ccustno='"+lcCustno+"'")
		THIS.lerror=.T.
		THIS.cerrormsg="Account could not be reactivated :"+lccustno
		return
	ENDIF
ENDIF

IF !loSosord.New(.T.)
	THIS.lerror=.T.
	THIS.cerrormsg="Could not get new order records :"+loSosord.cerrormsg
	RETURN .F.
ENDIF


lCSLPNNO  = oArcust.CSLPNNO  
IF lcCustno="ASHOP1" AND !lInterCo
	lCSLPNNO=THIS.Set_Affiliate()
ENDIF
IF EMPTY(oOrder.CustomOrderNumber)
	THIS.lerror=.T.
	THIS.cerrormsg="No Order Number " +TRANSFORM(oOrder.ID)
	RETURN .F.
ENDIF

oSosOrd=loSosord.oData
oSosOrd.cSono=PADL(oOrder.CustomOrderNumber,10)
oSosOrd.cCustno=lcCustno
oSosOrd.CSHIPVIA=oOrder.ShippingMethod    
oSosOrd.CFRGTCODE=oOrder.ShippingMethod
oSosOrd.CREVISION	="0"        &&C(1) not validated
oSosOrd.CORDERBY=	TRANSFORM(oOrder.CustomerID)        &&C(30)
oSosOrd.CSLPNNO=	lCSLPNNO     &&C(10) Salesperson ??
oSosOrd.CENTERBY=	"Shop Order"        &&C(30)
IF !INLIST(lcCustno,"ASHOP1" ,"ASHOP2")
oSosOrd.CENTERBY=	"Web Order"        &&C(30)
oSosOrd.CBADDRNO= 	"MAIN"       &&C(10)
oSosOrd.CBCOMPANY= 	oArcust.cCompany        &&C(40)
oSosOrd.CBADDR1= 	oArcust.cAddr1         &&C(40)
oSosOrd.CBADDR2= 	oArcust.cAddr2         &&C(40)
oSosOrd.CBCITY=  	oArcust.cCity        &&C(20)
oSosOrd.CBSTATE= 	oArcust.cState       &&C(15)
oSosOrd.CBZIP=   	oArcust.cZip      &&C(10)
oSosOrd.CBCOUNTRY=	oArcust.cCountry        &&C(25)
oSosOrd.CBPHONE=  	oArcust.cPhone1      &&C(20)
oSosOrd.CBCONTACT= ALLTRIM(TRIM(oArcust.cFname) + " " + oArcust.cLname)       &&C(30)
ELSE
oSosOrd.CBADDRNO= 	"B"+TRANSFORM(loBill.oData.ID)
oSosOrd.CBCOMPANY= 	IIF(!ISNULLOREMPTY(loBill.oData.Company),ALLTRIM(loBill.oData.Company), ;
	ALLTRIM(loBill.oData.LastName)+","+ ALLTRIM(loBill.oData.FirstName)    )
oSosOrd.CBADDR1= 	NVL(loBill.oData.Address1,"")         &&C(40)
oSosOrd.CBADDR2= 	NVL(loBill.oData.Address2,"")         &&C(40)
oSosOrd.CBCITY=  	NVL(loBill.oData.City ,"")        &&C(20)
oSosOrd.CBSTATE= 	NVL(loBill.oData.State ,"")       &&C(15)
oSosOrd.CBZIP=   	NVL(loBill.oData.ZipPostalCode ,"")      &&C(10)
oSosOrd.CBCOUNTRY=	loBill.oData.Country        &&C(25)
oSosOrd.CBPHONE=  	NVL(loBill.oData.PhoneNumber,"")     &&C(20)
oSosOrd.CBCONTACT= ALLTRIM(TRIM(loBill.oData.Firstname) + " " + loBill.oData.LastName)       &&C(30)
oSosOrd.CBEMAIL=   ALLTRIM(NVL(loBill.oData.Email,""))
ENDIF




oSosOrd.CSADDRNO= 	"S"+TRANSFORM(loShip.oData.ID)
oSosOrd.CSCOMPANY= 	IIF(!ISNULLOREMPTY(loShip.oData.Company),ALLTRIM(loShip.oData.Company), ;
	ALLTRIM(loShip.oData.LastName)+","+ ALLTRIM(loShip.oData.FirstName)    )
oSosOrd.CSADDR1= 	NVL(loShip.oData.Address1,"")         &&C(40)
oSosOrd.CSADDR2= 	NVL(loShip.oData.Address2,"")         &&C(40)
oSosOrd.CSCITY=  	loShip.oData.City        &&C(20)
oSosOrd.CSSTATE= 	loShip.oData.State       &&C(15)
oSosOrd.CSZIP=   	loShip.oData.ZipPostalCode      &&C(10)
oSosOrd.CSCOUNTRY=	loShip.oData.Country        &&C(25)
oSosOrd.CSPHONE=  	loShip.oData.PhoneNumber     &&C(20)
oSosOrd.CSCONTACT= ALLTRIM(TRIM(loShip.oData.Firstname) + " " + loShip.oData.LastName)       &&C(30)
oSosOrd.CSEMAIL=   ALLTRIM(NVL(loShip.oData.Email,""))
oSosOrd.CIONO	=""        &&C(10)
lCollect=.f.
*!*	IF PEMSTATUS(SERVER.goSettings.Common,"collectid",5) AND SERVER.goSettings.Common.collectid=oShip.id
*!*		lCollect=.t.
*!*	ENDIF
DO CASE
CASE lCollect OR lcCustno="STAFF1"
	oSosOrd.CSHIPVIA="COLLECT"
	oSosOrd.CFRGTCODE="COLLECT"

CASE lcCustno="TEST01"
	oSosOrd.CSHIPVIA="BERCO"
	oSosOrd.CFRGTCODE="BERCO"	
	
CASE !ISNULLOREMPTY(loShip.oData.CustomAttributes) 
	oSosOrd.CSHIPVIA="COURIER"  &&"BERCO"
	oSosOrd.CFRGTCODE="COURIER"
	lnPPID=VAL(STREXTRACT(loShip.oData.CustomAttributes,;
	[<AddressAttribute ID="1"><AddressAttributeValue><Value>],;
	[</Value></AddressAttributeValue></AddressAttribute></Attributes>]))
	IF lnPPID<>0
		lcSql="select a.company,FirstName cAddrno from Address a join StorePickupPoint p on a.id=p.AddressId where p.id="+TRANSFORM(lnPPID)
		IF loOrder.QUERY(lcSql,"tPep")>0
		
			oSosOrd.CSCOMPANY=tPep.Company
		
			DO CASE
				CASE tPep.cAddrno='PN'
					oSosOrd.CSHIPVIA="POSTNET"
					oSosOrd.CFRGTCODE="POSTNET"
					oSosOrd.CIONO=tPep.cAddrno
					oSosOrd.CSADDRNO=tPep.cAddrno
					
					
					
				CASE  !tPep.cAddrno='PN'
					oSosOrd.CSHIPVIA="SKYNET"
					oSosOrd.CFRGTCODE="SKYNET"
					oSosOrd.CIONO=tPep.cAddrno	
					oSosOrd.CSADDRNO=tPep.cAddrno
			ENDCASE
		ENDIF
	ENDIF				
					
OTHERWISE
	oSosOrd.CSHIPVIA="COURIER"  &&"BERCO"
	oSosOrd.CFRGTCODE="COURIER" &&"BERCO"
ENDCASE

oSosOrd.CFOB		=""        &&C(10)
oSosOrd.CPONO		=TRANSFORM(oOrder.ID)
oSosOrd.CFRTAXCODE	=oArcust.CTAXCODE &&C(10)				??
oSosOrd.CTAXCODE	=oArcust.CTAXCODE   	&&C(10)			??
oSosOrd.CPAYCODE	="CWO"        &&C(10)					??
oSosOrd.CBANKNO		="ABS423"        &&C(10)
oSosOrd.CCHKNO		=""        &&C(20)
oSosOrd.CCARDNO		=""        &&C(20)
oSosOrd.CEXPDATE	=""        &&C(5)
oSosOrd.CCARDNAME	=""        &&C(30)
oSosOrd.CPAYREF		=""        &&C(20)
oSosOrd.cCurrCode	=oOrder.CustomerCurrencyCode       &&C(3)

oSosOrd.CCOMMISS	=""        &&C(10)
oSosOrd.lSOURCE		=4       &&C(10)
oSosOrd.CBSONO		=""        &&C(10)
oSosOrd.dCreate		=DATE()        && T
oSosOrd.DORDER 		=x8CONVCHAR(oOrder.CreatedOnUtc,"D")    
oSosOrd.LQUOTE		=0        && I
oSosOrd.LHOLD		=IIF(LEFT(ALLTRIM(oSosOrd.cSono),1)='Z' OR oSosOrd.ccustno='TEST01',1,0)        && I
oSosOrd.LVOID		=0        && I
oSosOrd.LNOBO		=1       && I
oSosOrd.LUSECUSITM	=0        && I
oSosOrd.LFRTTAX1	=0        && I
oSosOrd.LFRTTAX2	=0        && I
oSosOrd.lApplyTax	=1        && I
oSosOrd.LPRCINCTAX	=0        && I
oSosOrd.LPRTSORD	=1        && I
oSosOrd.LPRTLIST	=0        && I
oSosOrd.LPRTCOD		=0        && I
oSosOrd.LPRTLBL		=0        && I
oSosOrd.NDISCDAY	=0        && I
oSosOrd.NDUEDAY		=0        && I					??
oSosOrd.NTERMDISC	=0        && N(8, 2)
oSosOrd.NDISCRATE	=oArcust.NDISCRATE        && N(8, 2)
oSosOrd.NTAXVER		=1       && N(7, 0)
oSosOrd.NFRTAXVER	=1        && N(7, 0)
oSosOrd.NTAXABLE1	=0        && N(20, 4)
oSosOrd.NTAXABLE2	=0        && N(20, 4)
oSosOrd.NSALESAMT	=0        && N(20, 4)
oSosOrd.NDISCAMT	=0        && N(20, 4)
oSosOrd.NFRTAMT		=0        && N(20, 4)
oSosOrd.NTAXAMT1	=0        && N(20, 4)
oSosOrd.NTAXAMT2	=0        && N(20, 4)
oSosOrd.NTAXAMT3	=0        && N(20, 4)
oSosOrd.NFRTTAX1	=0        && N(20, 4)
oSosOrd.NFRTTAX2	=0        && N(20, 4)
oSosOrd.NFRTTAX3	=0        && N(20, 4)
oSosOrd.NADJAMT		=0 &&-oOrder.Rounding         && N(20, 4)
oSosOrd.NADJUSTED	=0        && N(20, 4)
oSosOrd.NFRTAMTCHG	=0        && N(20, 4)
oSosOrd.NFTAXABLE1	=0        && N(20, 4)
oSosOrd.NFTAXABLE2	=0        && N(20, 4)
oSosOrd.NFSALESAMT	=0        && N(20, 4)
oSosOrd.NFDISCAMT	=0       && N(20, 4)
oSosOrd.NFFRTAMT	=ROUND(oOrder.OrderShippingExclTax,2)  && N(20, 4)
oSosOrd.NFTAXAMT1	=0        && N(20, 4)
oSosOrd.NFTAXAMT2	=0        && N(20, 4)
oSosOrd.NFTAXAMT3	=0        && N(20, 4)
oSosOrd.NFFRTTAX1	=0        && N(20, 4)
oSosOrd.NFFRTTAX2	=0        && N(20, 4)
oSosOrd.NFFRTTAX3	=0        && N(20, 4)
oSosOrd.NFADJAMT	=0 &&-oOrder.Rounding       && N(20, 4)
oSosOrd.NFADJUSTED	=0        && N(20, 4)
oSosOrd.NFFRTAMTCH	=0        && N(20, 4)
oSosOrd.NWEIGHT		=0        && N(18, 2)
oSosOrd.NXCHGRATE	=oOrder.CurrencyRate

oSosOrd.NSALESAMT=ROUND(oOrder.ORDERSUBTOTALEXCLTAX ,2)
lnCouponTotal=0
lnCouponTax=0
*!*	IF !EMPTY(oOrder.CheckoutAttributes) AND oOrder.CheckoutAttributes<>"reward"
*!*		loSer=server.goJsonSer	
*!*		loCoupon=loSer.DeserializeJson(oOrder.CheckoutAttributes)
*!*		FOR EACH oC IN loCoupon
*!*			IF oc.lSelect=0
*!*				LOOP
*!*			ENDIF	
*!*			lnCouponTotal=lnCouponTotal-ROUND(oc.nValue/1.15,2)
*!*			lnCouponTax=lnCouponTax+ROUND(oc.nValue-ROUND(oc.nValue/1.15,2),2)
*!*		NEXT
*!*	ENDIF

oSosOrd.NSALESAMT=oSosOrd.NSALESAMT+lnCouponTotal
oSosOrd.NFSALESAMT=oSosOrd.NSALESAMT * oOrder.CurrencyRate
oSosOrd.NTAXAMT1 =ROUND(oOrder.OrderTax,2)
oSosOrd.NFTAXAMT1=oSosOrd.NTAXAMT1 &&oOrder.OrderTax * oOrder.CurrencyRate
oSosOrd.NDISCAMT=oOrder.ORDERDISCOUNT
oSosOrd.NFDISCAMT=oOrder.ORDERDISCOUNT* oOrder.CurrencyRate
oSosOrd.NFRTAMT=ROUND(oOrder.OrderShippingExclTax,2)
oSosOrd.NFFRTAMT=ROUND(oOrder.OrderShippingExclTax * oOrder.CurrencyRate,2)

***********TESTING ONLY***************************
*oOrder.amountpaid=round(oSosOrd.NSALESAMT-oSosOrd.NDISCAMT+oSosOrd.NFRTAMT+oSosOrd.NTAXAMT1+oSosOrd.NFRTTAX1,2)
**************************************

oSosOrd.NFRTTAX1=oOrder.OrderShippingInclTax -oOrder.OrderShippingExclTax
oSosOrd.NFFRTTAX1=oSosOrd.NFRTTAX1* oOrder.CurrencyRate
oSosOrd.NADJAMT=0
*!*	;
*!*		oOrder.amountpaid-;
*!*		(oSosOrd.NSALESAMT-oSosOrd.NDISCAMT+oSosOrd.NFRTAMT+oSosOrd.NTAXAMT1) &&+oSosOrd.NFRTTAX1)
oSosOrd.NFADJAMT=oSosOrd.NADJAMT

lhasticket=.F.
lnT=0


SELECT tOrderLines
SCAN
	IF !ISNULLOREMPTY(AttributeDescription) AND LOWER(AttributeDescription)	='ticket'
		lnT=lnT+1
	ENDIF
ENDSCAN
IF lnT=RECCOUNT()  && Only tickets
	oSosOrd.CFOB="tickets"
	oSosOrd.CSHIPVIA=""
	oSosOrd.CFRGTCODE=""
ENDIF



lcFastStart=""
lnSeq=0
SELECT tOrderLines
SCAN
	SCATTER NAME oOrderLin MEMO
	lnSeq=lnSeq+10
	
	IF !loItem.LOAD(oOrderLin.ProductID)
		THIS.lerror=.T.
		THIS.cerrormsg="Could not get Product records :"+TRANSFORM(oOrderLin.ProductID)
		RETURN .F.
	ENDIF
	
	product=loItem.oData
	ADDPROPERTY(oOrderLin,"sku",CHRTRAN(Product.Sku,CHR(0),''))
	ADDPROPERTY(oOrderLin,"cuid",CHRTRAN(Product.ManuFacturerPartNumber,CHR(0),''))
	ADDPROPERTY(oOrderLin,"Descript",CHRTRAN(Product.ShortDescription,CHR(0),''))
	ADDPROPERTY(oOrderLin,"PriceIncl",(oOrderLin.PriceInclTax/oOrderLin.Quantity))
	ADDPROPERTY(oOrderLin,"PriceExcl",(oOrderLin.PriceExclTax/oOrderLin.Quantity))
	ADDPROPERTY(oOrderLin,"DiscRate",0)
	ADDPROPERTY(oOrderLin,"VatRate",15)
	IF oOrderLin.unitPriceInclTax=oOrderLin.unitPriceExclTax
		oOrderLin.VatRate=0
	ENDIF	
	
	&& Get the discounts here ------ >
	SET STEP ON 
	&& Adjust the discount so that it is just the standard discount
	SELECT Tdiscount
	LOCATE FOR  TRANSFORM(DiscountID)+","$this.osettings.discount.standard AND OrderItemID=oOrderLin.ID
	IF FOUND()
			lnDiscRate=Tdiscount.DiscountPercentage
			ADDPROPERTY(oOrderLin,"DiscRate",lnDiscRate)
			oOrderLin.DiscountAmountInclTax=ROUND(ROUND(oOrderLin.PriceInclTax/(1-(lnDiscRate/100)),2)*(lnDiscRate/100),2)
			
			*IF oOrderLin.PriceInclTax=0
				*oOrderLin.DiscRate=0
				*oOrderLin.DiscountAmountInclTax=0
			*ENDIF
			
			SELECT Tdiscount
			LOCATE FOR !TRANSFORM(DiscountID)+","$this.osettings.discount.standard AND OrderItemID=oOrderLin.ID
			IF FOUND()
				oOrderLin.PriceInclTax=oOrderLin.Quantity*oOrderLin.UnitPriceIncltax
				oOrderLin.PriceExclTax=oOrderLin.Quantity*oOrderLin.UnitPriceExcltax
				oOrderLin.DiscountAmountInclTax=ROUND(ROUND(oOrderLin.PriceInclTax/(1-(lnDiscRate/100)),2)*(lnDiscRate/100),2)
			
					
			ENDIF
			
			
			
			lnPrice=ROUND((oOrderLin.PriceInclTax+oOrderLin.DiscountAmountInclTax)/oOrderLin.Quantity,2)  &&
			lnPriceExcl=ROUND(lnPrice / 1.15,2) && 
			&&ROUND((oOrderLin.PriceExclTax+Tdiscount.DiscountAmount/oOrderLin.DiscountAmountExclTax)/oOrderLin.Quantity,2)
			oOrderLin.PriceIncl=lnPrice
			oOrderLin.PriceExcl=lnPriceExcl
			oOrderLin.DiscountAmountInclTax=ROUND(lnPrice * (lnDiscRate/100),2) * oOrderLin.Quantity
			oOrderLin.DiscountAmountExclTax=ROUND(lnPriceExcl * (lnDiscRate/100),2) * oOrderLin.Quantity
	ELSE  && This will only work if no other discounst
		LOCATE FOR !TRANSFORM(DiscountID)+","$this.osettings.discount.standard AND OrderItemID=oOrderLin.ID
		IF !FOUND()
*!*				ADDPROPERTY(oOrderLin,"PriceIncl",ROUND( (oOrderLin.PriceInclTax+oOrderLin.DiscountAmountInclTax)/oOrderLin.Quantity,2))
*!*				ADDPROPERTY(oOrderLin,"PriceExcl", ROUND( (oOrderLin.PriceExclTax+oOrderLin.DiscountAmountExclTax)/oOrderLin.Quantity,2))
*!*				ADDPROPERTY(oOrderLin,"DiscRate",  ;
*!*				ROUND(IIF((oOrderLin.PriceInclTax+oOrderLin.DiscountAmountInclTax)<>0,;
*!*				oOrderLin.DiscountAmountInclTax/(oOrderLin.PriceInclTax+oOrderLin.DiscountAmountInclTax)*100,0),2) )
		ELSE
		
*!*				lnPrice =TDiscount.DiscountAmount
*!*				ADDPROPERTY(oOrderLin,"PriceIncl",ROUND( (oOrderLin.PriceInclTax+oOrderLin.DiscountAmountInclTax)/oOrderLin.Quantity,2))
*!*				ADDPROPERTY(oOrderLin,"PriceExcl", ROUND( (oOrderLin.PriceExclTax+oOrderLin.DiscountAmountExclTax)/oOrderLin.Quantity,2))
			REPLACE CouponCode WITH "" IN TDISCOUNT && Prevent it from creating a coupon line
		
		ENDIF
	ENDIF
	

	SELECT tOrderLines	
	IF !loicitem.LOAD(oOrderLin.sku)
		THIS.lerror=.T.
		THIS.cerrormsg="Could not get Item records :"+oOrderLin.sku
		RETURN .F.
	ENDIF
	icitem=loicitem.oData
	
	IF !loIciwhs.loadbase("citemno='"+oOrderLin.sku+"' and cwarehouse = '"+this.osettings.ws.warehouse+"'")  
		THIS.cerrormsg="WAREHOUSE DOES_NOT_CARRY_ITEM :"+oOrderLin.sku
		RETURN .F.
	ENDIF
	iciwhs=loIciwhs.oData
	lcMBMsg = ""
	DO CASE
	CASE icitem.lArItem=0
		lcMBMsg = "IS_NOT_FOR_SALE :"
	CASE icitem.cStatus <> "A"
		lcMBMsg = "RECORD_IS_INACTIVE :"
	ENDCASE

	IF !EMPTY(lcMBMsg)
		THIS.lerror=.T.
		THIS.cerrormsg=lcMBMsg+oOrderLin.sku
		RETURN .F.
	ENDIF


	SELECT sostrs
	SCATTER NAME loSostrs.oData BLANK
	

	oSostrs=loSostrs.oData
&&------------------------------------------------------------------------------
	oSostrs.cuid			=""              		&&C(15)	GUID
	oSostrs.cSono			=oSosOrd.cSono
	oSostrs.cCustno			=lccustno
	oSostrs.CLINEITEM		=PADL(TRANSFORM(oOrderLin.id),10,'0') &&'a' + RIGHT(SYS(2015), 9)
	oSostrs.cItemno			=oOrderLin.sku
	oSostrs.CSPECCODE1		= ""
	oSostrs.CSPECCODE2		= ""
	oSostrs.cDescript		=IIF(oOrderLin.sku="COUPON",oOrderLin.Descript,icitem.cDescript)
	oSostrs.cWarehouse		=this.osettings.ws.warehouse &&"4400"   &&gosettings.common.warehouse
	oSostrs.CMEASURE		=icitem.csmeasure
	oSostrs.CCOMMISS		=""
	&& Code in here to handle award
	oSostrs.cRevnCode		=IIF(lcCustno<>"STAFF1",iciwhs.cRevnCode,"STAFF-01")
	oSostrs.DREQUEST		=DATE()
	oSostrs.LKITITEM		=icitem.LKITITEM
	oSostrs.LSTOCK			=IIF((icitem.LKITITEM=0 AND icitem.lUpdonhand=0),0,1)
	oSostrs.NWEIGHT			=icitem.NWEIGHT
	oSostrs.NCOST			=iciwhs.NCOST
	oSostrs.NSEQ			=lnSeq
	oSostrs.NDISCRATE		=0
	oSostrs.nordqty			=oOrderLin.quantity
	IF	lcCustno="STAFF1"
		oSostrs.nPrice=iciwhs.NCOST
		oSostrs.nfprice=iciwhs.NCOST
		oSostrs.nprcinctx=ROUND(oSostrs.nPrice*(1+(oOrderLin.VatRate/100)),2)
        oSostrs.nfprcinctx=ROUND(oSostrs.nPrice*(1+(oOrderLin.VatRate/100)),2)
		oSostrs.nsalesamt=oSostrs.nPrice*oSostrs.nordqty
		oSostrs.nfsalesamt=oSostrs.nPrice*oSostrs.nordqty
		oSostrs.ntaxamt1=ROUND(oSostrs.nsalesamt*(oOrderLin.VatRate/100),2)
		oSostrs.nftaxamt1=oSostrs.ntaxamt1
	    oSostrs.nextnddprc=oSostrs.nsalesamt
	ELSE
		oSostrs.NDISCRATE	=oOrderLin.discrate
		oSostrs.Nprice		=ROUND(oOrderLin.PriceExcl 	,2)
		oSostrs.NPRCINCTX=	ROUND(oOrderLin.PriceIncl 	,2)
		oSostrs.NFPRCINCTX=	ROUND(oSostrs.NPRCINCTX*oSosOrd.NXCHGRATE  ,2)
		oSostrs.NFPRICE=	ROUND(oSosTrs.nPrice*oSosOrd.NXCHGRATE   ,2)
		oSostrs.NSALESAMT=	ROUND(oSostrs.Nprice*oSostrs.NORDQTY,2)
		oSostrs.NFSALESAMT=	ROUND(oSostrs.NSALESAMT *oSosOrd.NXCHGRATE,2)
		oSostrs.NDISCAMT=	ROUND(oSostrs.NSALESAMT*oSostrs.NDISCRATE/100,2)
		oSostrs.NFDISCAMT=	ROUND(oSostrs.NFSALESAMT*oSostrs.NDISCRATE/100,2)
		oSostrs.NTAXAMT1 =	ROUND((oSostrs.NSALESAMT-oSostrs.NDISCAMT)*((oOrderLin.vatRate/100)) ,2)    && ROUND(oOrderlin.LineTax,2)	&&
		oSostrs.NFTAXAMT1=	ROUND(oSostrs.NTAXAMT1  * oSosOrd.NXCHGRATE,2)
		oSostrs.nExtnddprc = oSostrs.NORDQTY * oSostrs.NFPRICE
	
	ENDIF
	
	IF !ISNULLOREMPTY(oOrderLin.AttributesXml)
		lcAwd=STREXTRACT(oOrderLin.AttributesXml,;
		[<AwardProductAttributeValue><Value>],;
		[</Value></AwardProductAttributeValue>])
		IF !EMPTY(lcAwd)
			oSostrs.cRevnCode="MLM-000"
			lcFastStart=lcFastStart+lcAwd+","
		ENDIF
	ENDIF	
	oSostrs.CTAXCODE		=IIF(oOrderlin.vatrate=0,'VAT - 0%  ',oArcust.CTAXCODE)
	oSostrs.LUPSELL			=0
	oSostrs.LMODIKIT		=0
	oSostrs.LTAXABLE1		=0
	oSostrs.LTAXABLE2		=0
	oSostrs.LOWRMK			=0
	oSostrs.LPTRMK			=0
	oSostrs.NQTYDEC			=0
	oSostrs.NTAXVER			= 1
	oSostrs.NTAXAMT2		=0
	oSostrs.NTAXAMT3		=0
	oSostrs.NFTAXAMT2		=0
	oSostrs.NFTAXAMT3		=0
	oSostrs.NBUILTQTY		=0
	oSostrs.NORDQTY			=oOrderLin.Quantity
	oSostrs.NSHIPQTY		=0
	oSostrs.NADVQTY			=0
	oSostrs.NITMCNVQTY		=1
	oSostrs.NTRSCNVQTY		=1
	oSostrs.NWEIGHT			=icitem.NWEIGHT
	oSostrs.NCOST			=iciwhs.NCOST
	oSostrs.NSEQ			=lnSeq

	*oSostrs.lcusxitm=oOrderlin.lcusxitm
	*oSostrs.ixitmID=NVL(oOrderLin.iCusxitmID,0)
	oSosOrd.NWEIGHT=oSosOrd.NWEIGHT+(icitem.NWEIGHT*oSostrs.NORDQTY)
	

	nqtyAvailable = iciwhs.nOnhand - iciwhs.nbook
	IF oSostrs.LKITITEM=1
		IF loIcikit.LoadComponents(oSostrs.cItemno,,'CurTemp')
			SELECT CurTemp
			SCAN
				INSERT INTO Soskit(cuid, cSono, CLINEITEM, cItemno, CSPECCODE1, CSPECCODE2, ;
					cDescript,  lPrint, nQty, NSEQ, NCOST,  LSTOCK ) ;
					VALUES(THIS.sp_AssignUID(), oSostrs.cSono, oSostrs.CLINEITEM, CurTemp.cCompNo, ;
					CurTemp.cCSpecCode1, CurTemp.cCSpecCode2, CurTemp.cDescript,  ;
					CurTemp.lPrint, CurTemp.nQty, CurTemp.NSEQ, CurTemp.NCOST,  CurTemp.LSTOCK)
			ENDSCAN

		ELSE
			THIS.lerror=.T.
			THIS.cerrormsg="Kit Not Found :"+oOrderLin.sku
			RETURN .F.
		ENDIF
	ENDIF
	
	oSostrs.csppruid=oOrderLin.cuid
	oSostrs.lfree=0 
	luRet=THIS.sp_AssignUID()
	IF VARTYPE(luRet)<>'C'
		THIS.lerror=.T.
		THIS.cerrormsg="Could not get UID:"+oOrderLin.sku
		RETURN .F.
	ENDIF

	oSostrs.cuid=luRet
	INSERT INTO sostrs FROM NAME oSostrs
	
	IF !ISNULLOREMPTY(oOrderLin.AttributeDescription) AND LOWER(oOrderLin.AttributeDescription)	='ticket'
		lhasticket=.T.
	ENDIF

	&& Check if we have any non standard discounts and add them as coupons- >
	&& ----------------------------------------------------------------------
	
	SELECT Tdiscount
	SCAN for !isnullorempty(Tdiscount.CouponCode) AND !ISNULL(OrderItemID) AND OrderItemID=oOrderLin.ID ;
				AND !TRANSFORM(DiscountID)+","$this.osettings.discount.standard
	 	lnSeq=lnSeq+10
		IF !loicitem.LOAD(Tdiscount.CouponCode)
			THIS.lerror=.T.
			THIS.cerrormsg="Could not get Item records :"+Tdiscount.CouponCode
			RETURN .F.
		ENDIF
		icitem=loicitem.oData
		IF !loIciwhs.loadbase("citemno='"+icitem.citemno+"' and cwarehouse = '"+this.osettings.ws.warehouse+"'")  
			THIS.cerrormsg="WAREHOUSE DOES_NOT_CARRY_ITEM :"+icitem.citemno
			RETURN .F.
		ENDIF
		iciwhs=loIciwhs.oData
		oSostrs.CLINEITEM		='_' + RIGHT(oSostrs.CLINEITEM, 9)
		THIS.AddCoupon(oSostrs.CLINEITEM,icitem.citemno,iciwhs.cRevnCode,lnSeq,TDiscount.DiscountAmount,tDiscount.name)
	ENDSCAN
	
	
ENDSCAN


&&-----------------------------------------------------------------------
&& Free Shipping
&&-----------------------------------------------------------------------

SELECT Tdiscount
*LOCATE FOR  DiscountID = VAL(this.osettings.discount.freeshipping)
LOCATE FOR ATC(PADL(TRANSFORM(DiscountID),2,'0'),this.osettings.discount.freeshipping)>0


IF FOUND()
	lnFreeShip=TDiscount.DiscountAmount
	oOrder.OrderShippingInclTax=lnFreeShip-oOrder.OrderShippingInclTax
	oOrder.OrderShippingExclTax=ROUND(lnFreeShip/1.15,2)
	oSosOrd.NFRTAMT=oOrder.OrderShippingExclTax
	oSosOrd.NFFRTAMT=ROUND(oSosOrd.NFRTAMT * oOrder.CurrencyRate,2)
	oSosOrd.NFRTTAX1 =oOrder.OrderShippingInclTax -oOrder.OrderShippingExclTax
	oSosOrd.NFFRTTAX1=oSosOrd.NFRTTAX1* oOrder.CurrencyRate
 	lnSeq=lnSeq+10
	IF !loicitem.LOAD(Tdiscount.CouponCode)
		THIS.lerror=.T.
		THIS.cerrormsg="Could not get Item records :"+Tdiscount.CouponCode
		RETURN .F.
	ENDIF
	icitem=loicitem.oData
	IF !loIciwhs.loadbase("citemno='"+icitem.citemno+"' and cwarehouse = '"+this.osettings.ws.warehouse+"'")  
		THIS.cerrormsg="WAREHOUSE DOES_NOT_CARRY_ITEM :"+icitem.citemno
		RETURN .F.
	ENDIF
	iciwhs=loIciwhs.oData
	SELECT sostrs
	SCATTER NAME oSosTrs BLANK
	oSostrs.CLINEITEM		='a' + RIGHT(SYS(2015), 9)
	THIS.AddCoupon(oSostrs.CLINEITEM,icitem.citemno,iciwhs.cRevnCode,lnSeq,lnFreeShip,tDiscount.name)
ENDIF


&&-----------------------------------------------------------------------
&& Add the discounts that dont have an orderline
&&-----------------------------------------------------------------------
&& 

SELECT Tdiscount
Scan for ISNULL(OrderItemID) AND ATC(PADL(TRANSFORM(DiscountID),2,'0'),this.osettings.discount.freeshipping)=0 AND ;
 !TRANSFORM(DiscountID)+","$this.osettings.discount.standard
	
	lnSeq=lnSeq+10
	IF !loicitem.LOAD(Tdiscount.CouponCode)
		THIS.lerror=.T.
		THIS.cerrormsg="Could not get Item records :"+Tdiscount.CouponCode
		RETURN .F.
	ENDIF
	icitem=loicitem.oData
	IF !loIciwhs.loadbase("citemno='"+icitem.citemno+"' and cwarehouse = '"+this.osettings.ws.warehouse+"'")  
		THIS.cerrormsg="WAREHOUSE DOES_NOT_CARRY_ITEM :"+icitem.citemno
		RETURN .F.
	ENDIF
	iciwhs=loIciwhs.oData
	
	SELECT sostrs
	SCATTER NAME oSosTrs BLANK
	oSostrs.CLINEITEM		='a' + RIGHT(SYS(2015), 9)
	THIS.AddCoupon(oSostrs.CLINEITEM,icitem.citemno,iciwhs.cRevnCode,lnSeq,TDiscount.DiscountAmount,tDiscount.Name,iciwhs.ncost)
	
ENDSCAN


&&-----------------------------------------------------------------------

SELECT Sostrs
SUM nsalesamt,nfsalesamt,ntaxamt1,nftaxamt1 ,ndiscamt;
	TO oSosOrd.nsalesamt,oSosOrd.nfsalesamt,oSosOrd.ntaxamt1,oSosOrd.nftaxamt1,oSosOrd.ndiscamt
		
oSosOrd.ntaxamt1=oSosOrd.ntaxamt1+oSosOrd.NFRTTAX1
oSosOrd.NFSALESAMT=oSosOrd.NSALESAMT * oOrder.CurrencyRate
oSosOrd.NFTAXAMT1=oSosOrd.NTAXAMT1 
oSosOrd.NFDISCAMT=oSosOrd.NDISCAMT* oOrder.CurrencyRate

luRet=this.PostTOAM()
IF luRet
	IF !this.Update_Nop()
		=LOGSTRING("Could not create shipment "+TRANSFORM(oOrder.ID),ERROR_LOG)
	ENDIF	
	
ENDIF	
IF !INLIST(oSosord.ccustno ,'ASHOP1','ASHOP2','ASTAFF1')
	THIS.DeactivateStarter()
	IF !EMPTY(lcFastStart)
		THIS.RemoveAwardFromWebstore(pnOrderID,oArcust.ccustno)
	ENDIF
ENDIF

ENDFUNC


&&-----------------------------------------------------------------------
FUNCTION PostTOAM
&&-----------------------------------------------------------------------

oAMSQL.begintransaction()
llError=.F.

TRY 

DO WHILE .T.

&& Check again for duplicate

	IF loSosord.loadbase("cPono='"+TRANSFORM(oOrder.ID)+"' AND ccustno='"+lcCustno+"'")
		THIS.lerror=.T.
		THIS.cerrormsg="Already Processed :"+TRANSFORM(oOrder.ID)
		llError=.T.
		EXIT
	ENDIF

	loSosord.nUpdateMode = 2
	loSosord.oData=oSosOrd
	IF !loSosord.SAVE()
		llError=.T.
		THIS.cerrormsg=loSosord.cerrormsg
		EXIT
	ENDIF

*!*		IF !ISNULLOREMPTY(oWSosork.oData.msoremark)
*!*			loSosork.SAVE()
*!*		ENDIF

	SELECT Sostrs
	IF RECCOUNT()<>lnOrderLines
		SET STEP ON 
		THIS.cerrormsg=" Order Line Mismatch"
		llError=.T.
		LOGSTRING(oSosOrd.cSono+THIS.cerrormsg,"ordersync.log")
		EXIT
	ENDIF
	SCAN
		SCATTER NAME oSostrs MEMO
		oSostrs.cSono=oSosOrd.cSono
		loSostrs.cSkipFieldsforupdates	="DREQUEST"
		loSostrs.nUpdateMode = 2
		loSostrs.oData=oSostrs
		IF !loSostrs.SAVE()
			THIS.cerrormsg=loSostrs.cerrormsg
			llError=.T.
			EXIT
		ENDIF

		IF !loIciwhs.UpdateBooked(Sostrs.cItemno,this.oSettings.ws.warehouse,oSostrs.NORDQTY)
			THIS.Seterror("Could not update booked")
			llError=.T.
			EXIT
		ENDIF

*!*			IF !EMPTY(oSostrs.csppruid) AND !ISNULL(oSostrs.csppruid)
*!*				IF !losppr.UpdateSales(oSostrs.cItemno,oSostrs.csppruid,oSostrs.NORDQTY)
*!*					THIS.seterror("Could not sales on campaign booked")
*!*					llError=.T.
*!*					EXIT
*!*				ENDIF
*!*			ENDIF

	ENDSCAN

	IF llError
		EXIT
	ENDIF

	SELECT Soskit
	SCAN
		SCATTER NAME oSoskit MEMO
		oSoskit.cSono=oSosOrd.cSono
		oSoskit.CUID=THIS.sp_AssignUID()
		loSoskit.nUpdateMode = 2
		loSoskit.oData=oSoskit
		IF !loSoskit.SAVE()
			THIS.Seterror("Could not update kits ")
			llError=.T.
			EXIT
		ENDIF
	ENDSCAN
	IF lcCustno<>"STAFF1" 
		IF !This.Update_ArCash(op)
			llError=.T.
			EXIT
		ENDIF
	ENDIF	
	IF !This.Update_SoPortal()
		llError=.T.
		EXIT
	ENDIF
	
	IF llError
		EXIT
	ENDIF
	
	
	EXIT
ENDDO	

CATCH TO oErr 

      =LOGSTRING([  Message: ] + oErr.Message + CHR(13)+CHR(10)+;
      [  Procedure: ] + oErr.Procedure  + CHR(13)+CHR(10)+;
      [  Details: ] + oErr.Details  + CHR(13)+CHR(10)+;
      [  LineContents: ] + oErr.LineContents ;
			+TRANSFORM(TOrderLines.ID),ERROR_LOG)

	llError=.t.

ENDTRY
IF llError
	oAMSQL.ROLLBACK()
	=LOGSTRING(TRANSFORM(oOrder.ID)+ "Fail",ERROR_LOG)
	RETURN .F.
ELSE
	oAMSQL.Commit()
	=LOGSTRING(TRANSFORM(oOrder.ID)+ "Success",ERROR_LOG)
ENDIF

ENDFUNC


&&-----------------------------------------------------------------------
FUNCTION Update_ArCash(op)
&&-----------------------------------------------------------------------
&& 

FOR EACH lop IN op
		
		
		luRcptno = THIS.new_rcptno()
		IF VARTYPE(luRcptno)<>'C'
			THIS.Seterror("Could not get rec #")
			llError=.T.
			RETURN .F.
		ENDIF

		loArcash.New(.T.)
		luRet=THIS.sp_AssignUID()
		IF VARTYPE(luRet)<>'C'
			llError=.T.
			RETURN .F.
		ENDIF
		isEft=.F.
		oArCash=loArcash.oData
		oArCash.CUID=PADR(luRet,15)
		oArCash.cCustno=oSosOrd.cCustno
		oArCash.crcptno=luRcptno
		IF loP.PaymentMethod="eft"
			isEft=.T.
		ENDIF

		oArCash=loArcash.oData
		oArCash.cuid=PADR(luRet,15)
		oArCash.cCustno=oSosOrd.cCustno
		oArCash.crcptno=luRcptno
		oArCash.cdepno=" "
		
		oArCash.CPAYCODE="PAYUC"
		oArCash.CBANKNO="NED (469)"
		oArCash.CCHKNO=" "
		oArCash.CPAYREF=PADR(lop.PaymentMethod+NVL(oOrder.AuthorizationTransactionID,""),20)
		IF isEft
			oArCash.CPAYCODE="PAYUE"
		ENDIF
		IF lop.PaymentMethod="payflex"
			oArCash.CPAYCODE="PAYFLEX"
		ENDIF

		IF lop.PaymentMethod="giftcard"
			oArCash.CPAYCODE="AFF"
			oArCash.CBANKNO="AFF000"
		ENDIF


		oArCash.cCurrCode=oSosOrd.cCurrCode
		oArCash.ctogl=" "
		oArCash.ctoglmc=" "
		oArCash.dCreate=DATE()
		oArCash.dpaid=DATE()
		oArCash.dlastapp=DATE()
		oArCash.LVOID=0
		oArCash.lprtrcpt=0
		oArCash.npaytype=3
		oArCash.npaidamt=loP.paymentamount
		oArCash.nappamt=.00001
		oArCash.nfpaidamt=oArCash.npaidamt &&*oOrder.nExchange
		oArCash.nfappamt=.00001
		oArCash.ntotmcvar=0
		oArCash.nmcround=0
		oArCash.NXCHGRATE=oSosOrd.NXCHGRATE
		oArCash.nbpaidamt=oArCash.npaidamt
		oArCash.cSono=oSosOrd.cSono
		oArCash.CENTERBY="Web Order"
		loArcash.cSkipFieldsforupdates		="dlastapp"
		IF !loArcash.SAVE()
			llError=.T.
			RETURN .F.
		ENDIF
		IF !loArcash.SAVE()
			llError=.T.
			THIS.Seterror("Could not update arcash")
			RETURN .F.
		ENDIF

		IF !loArcust.LOAD(oSosOrd.cCustno)
			llError=.T.
			THIS.Seterror("Could not load :"+oSosOrd.cCustno)
			EXIT
		ENDIF

		loArcust.oData.nbalance = loArcust.oData.nbalance - oArCash.npaidamt
		loArcust.oData.nopencr =  loArcust.oData.nopencr + oArCash.npaidamt
		IF !loArcust.SAVE()
			THIS.Seterror("Could not save :"+oSosOrd.cCustno)
			llError=.T.
			RETURN .F.
		ENDIF

	ENDFOR
ENDFUNC

&&---------------------------------------------------------------------------------------------------------
FUNCTION Update_SoPortal ()
&&---------------------------------------------------------------------------------------------------------
	losoportal.New(.T.)
	losoportal.oData.cSono=oSosOrd.cSono
	losoportal.oData.dCreate=DATETIME()
	losoportal.oData.cStatus=''
	losoportal.oData.dprinted=.NULL.
	IF !losoportal.SAVE()
		llError=.T.
		THIS.Seterror("Could not update soPortal")
		RETURN .F.
	ENDIF
ENDFUNC	

&&---------------------------------------------------------------------------------------------------------
FUNCTION new_rcptno
&&---------------------------------------------------------------------------------------------------------
cDocno=""
oAMsql.AddParameter("crcptno","csystfield","IN")
oAMsql.AddParameter("lAutoSoNo","cautodocno","IN")
oAMsql.AddParameter("arcash/arcashh","calias","IN")
oAMsql.AddParameter("crcptno","ckeyfield","IN")
oAMsql.AddParameter("","ckeyvalue")
oAMsql.AddParameter(1,"bfromfront")
oAMsql.AddParameter(10,"nkeylength","IN")
oAMsql.AddParameter("arsyst","csystalias","IN")
oAMsql.AddParameter(1,"bfromio")
oAMsql.AddParameter("","cdocno","OUT")
IF !oAMsql.ExecuteStoredProcedure("vsp_am_getnewdocno")
	RETURN .F.
ENDIF
lnResultValue = oAMsql.oParameters["cdocno"].VALUE
RETURN lnResultValue
ENDFUNC

&&---------------------------------------------------------------------------------------------------------
FUNCTION sp_AssignUID()
&&---------------------------------------------------------------------------------------------------------
IF oAMSql.EXECUTE("select RIGHT(REPLACE(NEWID(),'-',''),15) Guid","tGuid")=-1
	RETURN .f.
ENDIF
lcGuid=tGuid.Guid
USE IN TGUID
RETURN lcGuid	

ENDFUNC

&&---------------------------------------------------------------------------------------------------------
FUNCTION AddCoupon(lCLINEITEM,lcItemno,lcRevnCode,lnSeq,lnValue,lcDescrip,lnCOst)
&&---------------------------------------------------------------------------------------------------------
	oSostrs.CLINEITEM		= lCLINEITEM
	oSostrs.cCustno			=lccustno
	oSostrs.cItemno			=lcItemno
	oSostrs.cWarehouse		=this.osettings.ws.warehouse &&"4400"   &&gosettings.common.warehouse
	oSostrs.cDescript 		=lcDescrip
	oSostrs.CMEASURE		="EACH"
	oSostrs.CCOMMISS		=""
	oSostrs.cRevnCode		=lcRevnCode
	oSostrs.NSEQ			=lnSeq
	oSostrs.nordqty			=1
	oSostrs.LSTOCK			=0
	oSostrs.NWEIGHT			=0
	oSostrs.NDISCRATE		=0
	oSostrs.CTAXCODE		=oArcust.CTAXCODE
	oSostrs.LUPSELL			=0
	oSostrs.LMODIKIT		=0
	oSostrs.LTAXABLE1		=0
	oSostrs.LTAXABLE2		=0
	oSostrs.LOWRMK			=0
	oSostrs.LPTRMK			=0
	oSostrs.NQTYDEC			=0
	oSostrs.NTAXVER			= 1
	oSostrs.Nprice			=-ROUND(lnValue/1.15,4)
	oSostrs.NCost 			= IIF(EMPTY(lnCost),0,lnCost)	
	oSostrs.NPRCINCTX	=-lnValue
	oSostrs.NFPRCINCTX	=ROUND(oSostrs.NPRCINCTX*oSosOrd.NXCHGRATE  ,2)
	oSostrs.NFPRICE=	ROUND(oSosTrs.nPrice*oSosOrd.NXCHGRATE   ,2)
	oSostrs.NSALESAMT=	ROUND(oSostrs.Nprice*oSostrs.NORDQTY,2)
	oSostrs.NFSALESAMT=	ROUND(oSostrs.NSALESAMT *oSosOrd.NXCHGRATE,2)
	oSostrs.NDISCAMT=	0
	oSostrs.NFDISCAMT=	0
	oSostrs.NTAXAMT1 =	ROUND((oSostrs.NSALESAMT-oSostrs.NDISCAMT)*((This.vatRate/100)) ,2)    && ROUND(oOrderlin.LineTax,2)	&&
	oSostrs.NFTAXAMT1=	ROUND(oSostrs.NTAXAMT1  * oSosOrd.NXCHGRATE,2)
	oSostrs.nExtnddprc = oSostrs.NORDQTY * oSostrs.NFPRICE
	luRet=THIS.sp_AssignUID()
	IF VARTYPE(luRet)<>'C'
		THIS.lerror=.T.
		THIS.cerrormsg="Could not get UID:"+oOrderLin.sku
		RETURN .F.
	ENDIF

	oSostrs.cuid=luRet
	INSERT INTO sostrs FROM NAME oSostrs
	lnOrderLines=lnOrderLines+1

ENDFUNC	


&&--------------------------------------------------------------------------------------------------------
FUNCTION Set_Affiliate  && if not set on order
&&--------------------------------------------------------------------------------------------------------

	IF oOrder.AffiliateID<>0
		IF !loAffiliate.Load(oOrder.AffiliateID)
			RETURN ""&& Perhaps a log here
		ENDIF
		lnAffCheck=loArcust.Query("SELECT cCustno FROM ARCUST WHERE cCustno='"+ALLTRIM(loAffiliate.oData.FriendlyUrlName)+"'","ChkAff")
		IF lnAffCheck=0
			RETURN ""
		ENDIF	
		RETURN loAffiliate.oData.FriendlyUrlName
	ENDIF
	IF oWssql.Execute("EXEC sp_locaterefsponsor '"+loBill.oData.ZipPostalCode   +"',1","GSPON")<>1
		RETURN ""
	ENDIF
		
	IF RECCOUNT("GSPON")=0
		RETURN ""
	ENDIF
	IF ISNULL(GSpon.cSponsor)
		RETURN
	ENDIF

&&------------------------------------------------------------
		&&REPLACE cSponsor WITH 'TEST01' IN GSPON  && Test
&&---------------------------------------------------------		
	IF !loAffiliate.LoadBase("FriendlyUrlName='"+GSpon.cSponsor+"'")
		RETURN ""&& Perhaps a log here
	ENDIF
	loCust.oData.AffiliateID=loAffiliate.oData.ID
	IF !loCust.SAVE()
		RETURN ""
	ENDIF	
	loOrder.oData.AffiliateID=loAffiliate.oData.ID
	loOrder.Save()
	RETURN GSpon.cSponsor
ENDFUNC
*/-------------------------------- Affiliate ------------------


FUNCTION DESTROY
oAMSQL.ROLLBACK()
ENDFUNC



FUNCTION Update_Nop
	
	
	IF lhasticket
		lcSql="UPDATE ANQ_Booking SET cSono='"+oSosOrd.cSono+"' where Orderid="+TRANSFORM(oOrder.ID)
		IF !oNopSql.EXECUTENONQUERY(lcSql)
			=LOGSTRING("Could not Update Booking "+TRANSFORM(oOrder.ID),ERROR_LOG)
		ENDIF
		IF oSosOrd.CFOB="tickets"  && Only Tickets
*!*					lcSql="UPDATE [Order] SET ShippingStatusId=30,OrderStatusID=30  where id="+TRANSFORM(oOrder.ID)
*!*					IF !oNopSql.EXECUTENONQUERY(lcSql)
*!*					=LOGSTRING("Could not Update Booking Ship Status "+TRANSFORM(oOrder.ID),ERROR_LOG)
*!*					ENDIF
				RETURN
		ENDIF		
	ENDIF
	
	luRet=this.oNop.ShipMent_Create(;
	TEXTMERGE([{"order_id": <<oOrder.ID>>,"admin_comment": "In Warehouse","created_on_utc": '<<TOISODATESTRING(DATETIME(),.t.,.t.)>>'}]))
	IF VARTYPE(luRet)<>"O"
		THIS.Seterror("Could not create shipment")
		RETURN .F.
	ENDIF
	lnSHipID=luret.ID
	
	
	llError=.f.
	SELECT TOrderLines
	SCAN FOR LOWER(AttributeDescription)<>'ticket'
	TEXT TO lcJson TEXTMERGE NOSHOW
    {"shipment_id":  <<TRANSFORM(INT(lnShipID))>>, "order_item_id": <<TOrderLines.ID>>,
    "quantity" : <<TOrderLines.Quantity>>,  "warehouse_id": 0,"id": 0}
	ENDTEXT
	luRet=this.oNop.ShipMentItem_Create(ALLTRIM(lcJson))
	IF VARTYPE(luRet)<>"O"
		=LOGSTRING("Could not create shipment item "+TRANSFORM(TOrderLines.ID),ERROR_LOG)
		THIS.Seterror("Could not create shipment item "+TRANSFORM(TOrderLines.ID))
		llError=.T.
*!*			EXIT
		LOOP
	ENDIF
	ENDSCAN
	RETURN !llError
	
ENDFUNC


FUNCTION DeactivateStarter
loCust.oData.createdonutc=IIF(VARTYPE(loCust.oData.createdonutc)='C',;
					CTOT(loCust.oData.createdonutc),loCust.oData.createdonutc)

&& ----------------- Starter Kits	-------------------
IF oOrder.OrderTotal>0 AND (DATE()-TTOD(loCust.oData.createdonutc))<90;
			and (oArcust.dstarter =<{^1900-01-01} OR ISNULL(oArcust.dstarter))
TEXT TO lcSql TEXTMERGE NOSHOW		
EXEC [NopIntegration]..sp_DeactivateStarterKits 
		@cCustno='<<oArcust.ccustno>>',@StoreID=<<loCust.oData.RegisteredinStoreID>>
ENDTEXT		

	IF !oAMsql.EXECUTENONQUERY(lcSql)
		=LOGSTRING("Could not detactivate starter "+oAMsql.cErrorMsg,"ordSync.log")
	ENDIF
ENDIF	
ENDFUNC	

FUNCTION RemoveAwardFromWebstore(pnOrderID,lcCustno)

	
ENDFUNC


&&-------------------------------------------------------------------------------
&& Cancel Unpaid Orders
&&-------------------------------------------------------------------------------

FUNCTION CancelOrders(lnid)
&&-------------------------------------------------------------------------------
IF !EMPTY(lnid)
	this.oNop.Order_Cancel(lnid)

ELSE


IF oNopSql.EXECUTE("EXEC ANQ_UnpaidOrders 4","TProcess")>0
SELECT TProcess
SCAN &&FOR id=802684
	this.oNop.Order_Cancel(tProcess.id)


ENDSCAN
ENDIF

ENDIF

ENDFUNC
&&-------------------------------------------------------------------------------


ENDDEFINE

