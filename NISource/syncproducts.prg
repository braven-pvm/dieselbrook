#DEFINE ERROR_LOG     "Synchlog"+DTOS(DATE())+".log"
#DEFINE INI_FILE	  "nop.ini"
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
SET DATE YMD
CLEAR




oAMSql=  CREATEOBJECT("wwSQL")
oNopSql =CREATEOBJECT("wwSQL")
*oNopSql.EnableUnicodeToAnsiMapping(.t.)
oWSSql=  CREATEOBJECT("wwSQL")
oSql=CREATEOBJECT("wwSQL")
oSql.CONNECT("DRIVER={SQL Server};SERVER=196.3.178.122,62111;UID=sa;PWD=AnniQu3S@;database=NopIntegration")


dDate=DATE()
lstage=.t.

#IF .t.  && Stage
ApiUrl="https://stage.annique.com/api-backend/"
* oWSSql.CONNECT("DRIVER={SQL Server};SERVER=stage.AnniqueStore.co.za,61023;UID=sa;PWD=Difficult1;database=AnniqueStore")
oSql=CREATEOBJECT("wwSQL")
oSql.CONNECT("DRIVER={SQL Server};SERVER=196.3.178.122,62111;UID=sa;PWD=AnniQu3S@;database=NopIntegration")
? oAMSql.CONNECT("DRIVER={SQL Server};SERVER=196.3.178.122,62111;UID=sa;PWD=AnniQu3S@;database=amanniquelive")
? oNopSql.CONNECT("DRIVER={SQL Server};SERVER=20.87.212.38,63000;UID=sa;PWD=Difficult1;database=staging")
	*dDate={^2024-01-02}
	lStage=.t.
*!*		? beginofmonth(-1,dDate)
*!*		? endofmonth(-1,dDate)
*!*		? DATEADD(-1,dDate)
*!*		RETURN

	
	
*!*		=LOGSTRING("SyncAll UnPublish",ERROR_LOG)
*!*		oNopSql.ExecuteNonQuery("Update Product Set Published=0" )
*!*		=LOGSTRING("SyncAll UnPublish",ERROR_LOG)

#ENDIF

#if .f. && Live

ApiUrl="https://annique.com/api-backend/"
*? oWSSql.CONNECT("DRIVER={SQL Server};SERVER=stage.AnniqueStore.co.za,61023;UID=sa;PWD=Difficult1;database=AnniqueStore")
? oAMSql.CONNECT("DRIVER={SQL Server};SERVER=172.19.16.100;UID=sa;PWD=AnniQu3S@;database=amanniquelive")
? oNopSql.CONNECT("DRIVER={SQL Server};SERVER=20.87.212.38,63000;UID=sa;PWD=Difficult1;database=annique")

#ENDIF

oNop=CREATEOBJECT("EMPTY")
*!*	? oNop.Authenticate()

oSync=CREATEOBJECT("SyncProducts")
IF !oSync.SETUP(ApiUrl)
	? oSync.cErrormsg
	RETURN
ENDIF
oSync.dSyncDate=dDate
oSync.lstage=lStage
? oSync.SyncAll()
*? oSync.SyncOne('SCESS21132') &&,' scrsc0001, bcres0194, bcbas0005SCESS22139 
*LSFHE23003
*? oSync.SyncChanges()
 *oSync.SyncAvailability()
? oSync.cErrormsg



&&------------------------------------------------------------------------------------
DEFINE CLASS SyncProducts AS SyncClass
	VatRate=15
	dSyncDate=DATE()
	oProd=NULL
	oCategory=NULL
	oBrand=NULL
	cPData=NULL
	cPUpdate=NULL
	oCatMap=NULL
	oBrandMap=NULL
	oItem=NULL
	lStage=.f.
	StoreID=1



&&------------------------------------------------------------------------------------
	FUNCTION SETUP(lcUrl)
&&------------------------------------------------------------------------------------	
	IF !DODEFAULT(lcUrl)
		RETURN .F.
	ENDIF


	WITH THIS
		.dSyncDate=DATE()
		.oProd=CREATEOBJECT("Product")
		.oProd.SetSqlObject(oNopSql)
		.oItem=CREATEOBJECT("icitem")
		.oItem.SetSqlObject(oAMSql)
		.oCategory=CREATEOBJECT("Product_Category_Mapping")
		.oCategory.SetSqlObject(oNopSql)
		.oBrand=CREATEOBJECT("Product_Manufacturer_Mapping")
		.oBrand.SetSqlObject(oNopSql)

		.cPData=oNop.LoadJSonObjectTemplate("Product_Create")
		.cPUpdate=oNop.LoadJSonObjectTemplate("Product_Update")
		.oCatMap=oNop.LoadJSonObject("Category")
		.oBrandMap=oNop.LoadJSonObject("Brand")
		TEXT TO lcSql NOSHOW TEXTMERGE
SELECT * FROM ANQ_CategoryIntegration
		ENDTEXT
		lnret=.oProd.QUERY(lcSql,"Tcategory")
		IF lnret=0
			THIS.seterror(.oProd.oSql.cErrormsg)
			RETURN .F.
		ENDIF

		TEXT TO lcSql NOSHOW TEXTMERGE
SELECT * FROM ANQ_ManufacturerIntegration
		ENDTEXT
		lnret=.oProd.QUERY(lcSql,"Tbrand")
		IF lnret=0
			THIS.seterror(.oProd.oSql.cErrormsg)
			RETURN .F.
		ENDIF
	ENDWITH

	ENDFUNC


	FUNCTION SyncOne(lcItemno)
	IF USED("TItems")
		USE IN TItems
	ENDIF
	TEXT TO lcSql TEXTMERGE NOSHOW
EXEC sp_ws_getactiveNEW @ldate = '<<toisodatestring(this.dSyncDate,,.t.)>>',@citemno='<<lcItemno>>'
	ENDTEXT
	luret=oAMSql.EXECUTE(lcSql,"TItems")
	IF luret<1
		RETURN .F.
	ENDIF
	
	IF !this.oProd.LoadbySku(lcitemno)
			*RETURN .f.
	ENDIF
	IF RECCOUNT("TItems") =0
		this.oProd.odata.published=.f.
		this.oProd.Save()
		this.oProd.ExecuteNonQuery("DELETE FROM ANQ_Gift WHERE ProductID="+TRANSFORM(this.oProd.odata.ID))
		RETURN	
	ENDIF	
	
	
	SELECT DISTINCT ALLTRIM(UPPER(IntegrationField)) FROM Tcategory INTO ARRAY aIfields
	SELECT TItems
	SCATTER NAME oData MEMO
	
	=LOGSTRING("Item Loaded "+lcItemno+" "+TRANSFORM(oData.dto),ERROR_LOG)
	IF !THIS.Sync(oData)
		=LOGSTRING("Could not Sync "+THIS.cErrormsg,ERROR_LOG)
		RETURN .F.
	ENDIF
	
	IF !this.oProd.LoadbySku(lcitemno)
		RETURN .f.
	ENDIF
	
*!*		IF this.oProd.Query("select * from Anq_Gift where ProductID="+TRANSFORM(this.oProd.odata.id))>0
*!*			this.oProd.odata.published=.f.
*!*			this.oProd.Save()
*!*		ENDIF
	
	ENDFUNC
	
&&------------------------------------------------------------------------------------
	FUNCTION SyncAll
&&------------------------------------------------------------------------------------

#UNDEFINE ERROR_LOG
#DEFINE ERROR_LOG     "SynchlogItemsAll"+DTOS(DATE())+".log"
	=LOGSTRING("SyncAll START",ERROR_LOG)
	IF USED("TItems")
		USE IN TItems
	ENDIF
	TEXT TO lcSql TEXTMERGE NOSHOW
EXEC sp_ws_getactiveNEW @ldate = '<<toisodatestring(this.dSyncDate,,.t.)>>'
	ENDTEXT
	luret=oAMSql.EXECUTE(lcSql,"TItems")
	IF luret<1
		=LOGSTRING("SyncAll ERROR" + oAMSql.cErrorMsg,ERROR_LOG)
		RETURN .F.
	ENDIF

	SELECT DISTINCT ALLTRIM(UPPER(IntegrationField)) FROM Tcategory INTO ARRAY aIfields
	SELECT TItems
	SCAN && FOR citemno='SASAI0163'
		SCATTER NAME oData MEMO
*!*				IF INLIST(oData.citemno,'MKESS0025','SASAI0163','MKBEA0079')
*!*				SET STEP ON 
*!*				ENDIF
		=LOGSTRING("SyncAll "+odata.citemno,ERROR_LOG)
		IF !THIS.Sync(oData)
			=LOGSTRING("Could not Sync "+THIS.cErrormsg,ERROR_LOG)
		ENDIF
*!*			IF INLIST(oData.citemno,'MKESS0025','SASAI0163','MKBEA0079')
*!*				SET STEP ON 
*!*				ENDIF
	ENDSCAN
	=LOGSTRING("SyncAll Publish",ERROR_LOG)
	oNopSql.ExecuteNonQuery("EXEC ANQ_SyncPublished" )
	=LOGSTRING("SyncAll Done",ERROR_LOG)
	#UNDEFINE ERROR_LOG
	#DEFINE ERROR_LOG     "Synchlog"+DTOS(DATE())+".log"
	ENDFUNC

&&------------------------------------------------------------------------------------
	FUNCTION Sync(oData)
&&------------------------------------------------------------------------------------	



	WITH THIS
		oItem=.oItem
		oProd=.oProd
		oCategory=.oCategory
		oBrand=.oBrand
		cPData=.cPData
		cPUpdate=.cPUpdate
		oCatMap=.oCatMap
		oBrandMap=.oBrandMap
		oSer=.oSer
		oXml=.oXml
	ENDWITH

TRY


	WITH oData

*!*			lcJson=TEXTMERGE(cPUpdate)
*!*			oPOrig=THIS.oSer.deSerialize(lcJson)
*!*			oPUpdate=THIS.oSer.deSerialize(lcJson)
		
		IF !oItem.Load(.citemno)
			=LOGSTRING("Not Found on AM "+.citemno,ERROR_LOG)	
		ENDIF
		lNew=.F.
		IF !oProd.LoadbySku(.citemno)
			lNew=.T.
			
		ENDIF

&& Set Fields
		IF THIS.lStage AND this.dsyncdate>DATE()
			oData.dfrom=DATEADD(-1,oData.dfrom)
			oData.dto=DATEADD(-1,oData.dto)
		ENDIF


		oData.dto=DATETIME(YEAR(oData.dto),MONTH(oData.dto),DAY(oData.dto),23,59,59)
&&(odata.cstatus="A" and odata.lportal=1)
	

		llError=.F.
		IF lNew
			lcJson=TEXTMERGE(cPData)
			
			luret=oNop.Product_Create(lcJson)
			IF VARTYPE(luret)<>"O"
				llError=.T.
				THIS.seterror("Could not create product "+TRANSFORM(luret))
				=LOGSTRING("Could not create product "+.citemno+TRANSFORM(luret),ERROR_LOG)	
				RETURN .F.

			ENDIF
			oNopSql.ExecuteNonQuery("EXEC ANQ_SyncImage '"+ALLTRIM(.citemno)+"'")
			oNopSql.ExecuteNonQuery("EXEC ANQ_SyncProductSEO "+TRANSFORM(luret.ID))
			nProductID=luret.ID
			*IF .stockquantity<>0
			*	THIS.UpdateStock(nProductID,.stockquantity)
			*ENDIF
			IF !oProd.Load(nProductID)
				llError=.T.
				THIS.seterror("Could not find created product "+TRANSFORM(luret))
				=LOGSTRING("Could not create product "+.citemno+TRANSFORM(luret),ERROR_LOG)	
				RETURN .F.
			ENDIF
		ENDIF

		


			lCheck=COMPOBJ(oProd.oData,oProd.oOrigData)
*!*
			WITH oProd.oOrigData
				.Sku=CHRTRAN(.Sku,CHR(0),"")
				.Name=CHRTRAN(.Name,CHR(0),"")
				*.shortdescription=CHRTRAN(.shortdescription,CHR(0),"")
				.manufacturerpartnumber=CHRTRAN(.manufacturerpartnumber,CHR(0),"")
				.availablestartdatetimeutc=IIF(VARTYPE(.availablestartdatetimeutc)='C',;
					CTOT(.availablestartdatetimeutc),.availablestartdatetimeutc)
				.availableenddatetimeutc=IIF(VARTYPE(.availableenddatetimeutc)='C',;
					CTOT(.availableenddatetimeutc),.availableenddatetimeutc)
			ENDWITH


			WITH oProd.oData
				.Sku=CHRTRAN(.Sku,CHR(0),"")
*!*					.availablestartdatetimeutc=IIF(VARTYPE(odata.availablestartdatetimeutc)='C',;
*!*						CTOT(odata.availablestartdatetimeutc),odata.availablestartdatetimeutc)
*!*					.availableenddatetimeutc=IIF(VARTYPE(odata..availableenddatetimeutc)='C',;
*!*						CTOT(odata.availableenddatetimeutc),odata.availableenddatetimeutc)
				.NAME=ALLTRIM(oData.cdescript)
				*.shortdescription=ALLTRIM(oData.cdescript)
				.manufacturerpartnumber=oData.cuid
	*.stockquantity=odata.stockquantity   && Maybe do stock adjust
				.minstockquantity=oData.minstockquantity
				.price=ROUND(oData.npprice*(1+this.vatrate/100),2)
				.oldprice=oData.nprcinctx
				.weight=oData.nweight
				.availablestartdatetimeutc=oData.dfrom
				.availableenddatetimeutc=oData.dto
				.published=(oData.cstatus="A" AND oData.lportal=1) &&AND oData.lFree=0) &&	AND oData.lcusxitm<>1
				.visibleindividually=oData.lfree=0 && AND oData.lcusxitm<>1
				.visibleindividually=IIF( oData.lcusxitm<>1,.visibleindividually,1)
				.HasDiscountsApplied=IIF(oData.nDiscRate>0,.t.,.f.)
				.gtin=oItem.oData.cbarcode1
			ENDWITH
			
			
	
			lCheck=COMPOBJ(oProd.oData,oProd.oOrigData)

			IF !COMPOBJ(oProd.oData,oProd.oOrigData)

				luret=oNop.Product_Get(oProd.oData.ID)
				IF VARTYPE(luret)<>"O"
					THIS.seterror("Could not find product "+TRANSFORM(luret))
					RETURN
				ENDIF
				&&? oProd.oData.sku

*!*					luret.full_description=oXml.EncodeXML(luret.full_description)
*!*					luret.short_description=oXml.EncodeXML(luret.short_description)
*!*					IF !ISNULL(luret.meta_description)
*!*					luret.meta_description=oXml.EncodeXML(luret.meta_description)
*!*					ENDIF
*!*					IF !ISNULL(luret.meta_keywords)
*!*					luret.meta_keywords=oXml.EncodeXML(luret.meta_keywords)
*!*					ENDIF
*!*					IF !ISNULL(luret.meta_title)
*!*					luret.meta_title=oXml.EncodeXML(luret.meta_title)
*!*					ENDIF
				WITH luret

					.NAME=oData.cdescript
					*.short_description=oXml.EncodeXML(oData.cdescript)
					.manufacturer_part_number=oData.cuid
*.stock_quantity=odata.stockquantity   && Maybe do stock adjust
					.min_stock_quantity=oData.minstockquantity
					.price=ROUND(oData.npprice*(1+this.vatrate/100),2)
					.old_price=oData.nprcinctx
					.weight=oData.nweight
					.available_start_date_time_utc=TOISODATESTRING(oData.dfrom,.T.,.T.)+"Z"
					.available_end_date_time_utc=TOISODATESTRING(oData.dto,.T.,.T.)+"Z"
					.published=(oData.cstatus="A" AND oData.lportal=1) && AND oData.lFree=0 ) && AND oData.lcusxitm<>1
					.visible_individually=(oData.lfree=0) && AND oData.lcusxitm<>1)
					.visible_individually=IIF( oData.lcusxitm<>1,.visible_individually,1)
					.updated_on_utc=DATETIME()
					.Has_Discounts_Applied=IIF(oData.nDiscRate>0,.t.,.f.)
					.gtin=oItem.oData.cbarcode1
				ENDWITH
				lcJson=oSer.Serialize(luret)
				luret=oNop.Product_Update(STRCONV(lcJson, 9))
				IF !luret
					THIS.seterror("Could not update product "+TRANSFORM(luret))
					RETURN .f.
				ENDIF
					


*!*				IF !oProd.save()
*!*					llError=.t.
*!*					THIS.SetError("Could not Update product "+oProd.cErrormsg)
*!*					RETURN .F.
*!*				ENDIF
				IF oProd.oData.NAME<>oProd.oOrigData.NAME
					oNopSql.ExecuteNonQuery("EXEC ANQ_SyncProductSEO "+TRANSFORM(oProd.oData.ID))
				ENDIF
			ELSE
				*? "no update needed"
				*=LOGSTRING("No Update needed "+oProd.oData.Sku,"UpdateCheck.log")	
				*=LOGSTRING("Item Before"+oProd.oData.sku+" "+TRANSFORM(oProd.oOrigData.availableenddatetimeutc),"UpdateCheck.log")
				*=LOGSTRING("Item After "+oProd.oData.sku+" "+TRANSFORM(oProd.oData.availableenddatetimeutc),"UpdateCheck.log")	

			ENDIF
			nProductID=oProd.oData.ID
			THIS.UpdateStock(nProductID,oData.stockquantity)
		
			luret=oNopSql.ExecuteNonQuery("EXEC ANQ_UpdateStockStatus "+TRANSFORM(nProductID)+",'"+LEFT(odata.stockstatus,1)+"'")
				IF !luRet
					SET STEP ON 
				ENDIF


		&&ENDIF

		
*!*			IF oProd.Load(nProductID) AND !ISNULLOREMPTY(odata.StockStatus)
*!*				oProd.oData.StockStatus=UPPER(LEFT(odata.StockStatus,1))
*!*				oProd.save()
*!*			ENDIF	
TEXT TO lcSql NOSHOW TEXTMERGE
	 EXEC ANQ_UpdateSpecification @name='Stock Status',@value='<<UPPER(LEFT(odata.StockStatus,1))>>',@ProductID=<<nProductID>>
ENDTEXT
	IF !oNopSql.ExecuteNonQuery(lcSql)
			*SET STEP ON 
	ENDIF	
*!*			
*!*			
		
		
		lcIds=""

		FOR EACH cField IN aIfields
			uVal=""
			IF PEMSTATUS(oData,ALLTRIM(cField),5)
				uVal=EVAL("odata."+cField)
			ENDIF
			IF EMPTY(uVal)  AND PEMSTATUS(oProd.oData,ALLTRIM(cField),5)
				uVal=EVAL("oProd.oData."+cField)
				uval=IIF(VARTYPE(uVal)="L",IIF(uVal,1,0),uVal)
			ENDIF
			IF EMPTY(uVal)
				LOOP
			ENDIF	
				
			SELECT CategoryID FROM Tcategory INTO ARRAY aCats WHERE UPPER(IntegrationField)=UPPER(ALLTRIM(cField)) ;
				AND IntegrationValue==ALLTRIM(TRANSFORM(uVal))
			IF _TALLY>0
				FOR EACH CatID IN aCats
					IF ISNULLOREMPTY(CatID)
						LOOP
					ENDIF
					
					lcIds=lcIds+TRANSFORM(CatID)+','
					
					IF oCategory.LoadBase("Productid="+TRANSFORM(nProductID)+" AND CategoryID="+TRANSFORM(CatID))
						LOOP  && Possible Add ID to Array for cleanup
					ENDIF

					oCatMap.Product_Id=nProductID
					oCatMap.category_id=CatID
					oCatMap.is_featured_product= .F.
					oCatMap.display_order=0
					lcJson=oSer.Serialize(oCatMap)
					luret=oNop.ProductCategory_Create(lcJson)
					IF VARTYPE(luret)<>"O"
						LOOP
					ENDIF

				NEXT
				
			ENDIF
		NEXT
				IF LEN(lcIds)>1
					lcIds=LEFT(lcIds,LEN(lcIds)-1)
					DIMENSION aC[1]
					lnCount = AParseString(@aC,lcIds,",")
					lIsSA=.F.
				
					FOR x=1 TO lnCount
						IF INLIST(VAL(aC[x]),27,62)
						lIsSA=.T.
						ENDIF
					ENDFOR
					FOR x=1 TO lnCount
							IF INLIST(VAL(aC[x]),28)
							lcIDs="28"
							lIsSA=.F.
						ENDIF
					ENDFOR
					
					IF lIsSa 
						luret=oNopSql.ExecuteNonQuery("EXEC ANQ_UpdateACL "+TRANSFORM(nProductID))
						IF !luRet
							SET STEP ON 
						ENDIF
						
						lcIds=""
						FOR x=1 TO lnCount
							IF !INLIST(VAL(aC[x]),27,28,62,55,56,57,58,64)
								LOOP
							ENDIF
							lcIds=lcIDs+ac[x]+","
						ENDFOR
						lcIds=LEFT(lcIds,LEN(lcIds)-1)
					ENDIF
					
					
					
					
					oCategory.removedeleted(nProductID,lcIds)
				ENDIF	

&& Manufacturers / Brands
		SELECT TBrand
		LOCATE FOR UPPER(ALLTRIM(IntegrationValue))==UPPER(ALLTRIM(.cClass))
		IF FOUND()

			IF !oBrand.LoadBase("Productid="+TRANSFORM(nProductID)+" AND ManufacturerID="+TRANSFORM(TBrand.ManufacturerID))
				oBrandMap.Product_Id=nProductID
				oBrandMap.Manufacturer_id=TBrand.ManufacturerID
				oBrandMap.is_featured_product= .F.
				oBrandMap.display_order=10
				lcJson=oSer.Serialize(oBrandMap)
				luret=oNop.ProductManufacturer_Create(lcJson)
				IF VARTYPE(luret)<>"O"
					llError=.T.
					THIS.seterror("Could not create product "+TRANSFORM(luret))
				ENDIF
			ENDIF
		ENDIF

&& ---------------- Check its not in an ACL category --------------

	IF oProd.odata.HasDiscountsApplied=IIF(VARTYPE(oProd.odata.HasDiscountsApplied)="L",.t.,1)
		oNopSql.ExecuteNonQuery("EXEC ANQ_SyncProductDiscount "+TRANSFORM(nProductID)+",3")
		oNopSql.ExecuteNonQuery("EXEC ANQ_SyncProductDiscount "+TRANSFORM(nProductID)+",4,"+TRANSFORM(odata.nPdiscount))
		*oNopSql.ExecuteNonQuery("EXEC ANQ_SyncProductDiscount "+TRANSFORM(nProductID)+",5")
		*SyncProductDiscountTEST 
	ENDIF
	
	

	
	
	ENDWITH
	
	
CATCH TO oErr 

      =LOGSTRING([  Message: ] + oErr.Message + CHR(13)+CHR(10)+;
      [  Procedure: ] + oErr.Procedure  + CHR(13)+CHR(10)+;
      [  Details: ] + oErr.Details  + CHR(13)+CHR(10)+;
      [  Item: ] +  odata.citemno  +" ";
			,ERROR_LOG)

	llError=.t.

ENDTRY
	
	
	ENDFUNC

&&------------------------------------------------------------------------------------
	FUNCTION UpdateStock(lnProductID,lnstockquantity)
&&------------------------------------------------------------------------------------	

	IF oNopSql.EXECUTE("EXEC ANQ_UnprocessedProduct @ProductID="+TRANSFORM(lnProductID),"TStock")<>1
		RETURN  && Perhaps a log here could be an issue for un published items
		&& Also need to Send the StoreID for multi warehouse
	ENDIF
	IF lnProductID=450
	*SET STEP ON
	ENDIF
	lnDiff=0 &&lnstockquantity
	IF RECCOUNT("TStock")>0
		lnDiff=lnstockquantity-(TStock.stockquantity+TStock.quantity)-Tstock.minstockquantity
	ENDIF	
	IF lnDiff=0
		RETURN
	ENDIF
	luret=oNop.Product_Adjust(lnProductID,lnDiff,"HO Adjust Sync")
	IF !luret
		THIS.seterror("Could not adjust product "+TRANSFORM(lnProductID))
		RETURN
	ENDIF


	ENDFUNC
	
&&------------------------------------------------------------------------------------
	FUNCTION UpdateStockWH(lnProductID,lnstockquantity,lnStoreID)
&&------------------------------------------------------------------------------------	

	IF oNopSql.EXECUTE("EXEC ANQ_UnprocessedProduct @ProductID="+TRANSFORM(lnProductID),"TStock")<>1
		RETURN  && Perhaps a log here could be an issue for un published items
		&& Also need to Send the StoreID for multi warehouse
	ENDIF
	IF lnProductID=450
	*SET STEP ON
	ENDIF
	lnDiff=0 &&lnstockquantity
	IF RECCOUNT("TStock")>0
		lnDiff=lnstockquantity-(TStock.stockquantity+TStock.quantity)-Tstock.minstockquantity
	ENDIF	
	IF lnDiff=0
		RETURN
	ENDIF
	luret=oNop.Product_Adjust(lnProductID,lnDiff,"HO Adjust Sync")
	IF !luret
		THIS.seterror("Could not adjust product "+TRANSFORM(lnProductID))
		RETURN
	ENDIF


	ENDFUNC	
	
&&------------------------------------------------------------------------------------
FUNCTION ChangesComplete(lcGuid,lcitemno)
&&------------------------------------------------------------------------------------		
luret=oAMSql.ExecuteNonQuery(;
				"EXEC sp_ws_getupdatesComplete @cGuid='"+lcGuid+"',@cItemNo='"+lcitemno+"'")
			IF !luret
				SET STEP ON
			ENDIF
RETURN
			
&&------------------------------------------------------------------------------------
FUNCTION SyncChanges()
&&------------------------------------------------------------------------------------	
	IF USED("TChanges")
		USE IN TChanges
	ENDIF
	TEXT TO lcSql TEXTMERGE NOSHOW
exec sp_ws_getupdates @Filter='''ICITEM'' ,''CAMPSKU'',''CAMPDETAIL'''
	ENDTEXT
	luret=oAMSql.EXECUTE(lcSql,"TChanges")
	IF luret<1 OR !USED("TChanges")
		RETURN
	ENDIF

	SELECT TChanges
	SCAN
		lcGuid=cGuid
		IF !THIS.SyncOne(TChanges.citemno)
			=this.ChangesComplete(lcGuid,TChanges.citemno)
			=LOGSTRING("Could not Sync "+TChanges.citemno+" "+THIS.cErrormsg,ERROR_LOG)
			LOOP
		ENDIF
		IF !THIS.oProd.LoadbySku(TChanges.citemno)
			=this.ChangesComplete(lcGuid,TChanges.citemno)
			LOOP && Item not on Web
		ENDIF

		IF THIS.UpdateStock(THIS.oProd.oData.ID,TItems.stockquantity)
			=this.ChangesComplete(lcGuid,TChanges.citemno)
		ENDIF	
	ENDSCAN


ENDFUNC
	
&&------------------------------------------------------------------------------------
FUNCTION SyncAvailability()
&&------------------------------------------------------------------------------------	
	IF USED("TChanges")
		USE IN TChanges
	ENDIF
	IF USED("TItems")
		USE IN TItems
	ENDIF
	TEXT TO lcSql TEXTMERGE NOSHOW
exec sp_ws_getupdates @Filter='''ICIWHS'''
	ENDTEXT
	luret=oAMSql.EXECUTE(lcSql,"TChanges")
	IF luret<1 OR !USED("TChanges")
		RETURN .F.
	ENDIF
	lcGuid=''
	SELECT TChanges
	SCAN &&FOR cItemno='BCMTB23052'
		lcGuid=cGuid
		TEXT TO lcSql TEXTMERGE NOSHOW
EXEC sp_ws_getAvailability_CodeNEW @ddate = '<<toisodatestring(this.dSyncDate,,.t.)>>',@citemno='<<TChanges.cItemno>>'
		ENDTEXT
		luret=oAMSql.EXECUTE(lcSql,"TItems")
		IF luret<1
			=this.ChangesComplete(lcGuid,TChanges.citemno)
			LOOP  && Possible issue on Batch of Updates
		ENDIF
		IF RECCOUNT("TItems") =0
			=this.ChangesComplete(lcGuid,TChanges.citemno)
			LOOP
		ENDIF	
		IF !THIS.oProd.LoadbySku(TChanges.citemno)
			=this.ChangesComplete(lcGuid,TChanges.citemno)
			LOOP && Item not on Web
		ENDIF


		IF THIS.UpdateStock(THIS.oProd.oData.ID,TItems.stockquantity)
			=this.ChangesComplete(lcGuid,TChanges.citemno)
		ENDIF	
		luret=oNopSql.ExecuteNonQuery("EXEC ANQ_UpdateStockStatus "+TRANSFORM(THIS.oProd.oData.ID)+",'"+LEFT(TItems.cstatus,1)+"'")
		IF !luRet
		SET STEP ON 
		ENDIF
	ENDSCAN


ENDFUNC




ENDDEFINE

FUNCTION DATEADD(nMonth,ldDate)
TEXT TO lcSql TEXTMERGE NOSHOW
SELECT  DATEADD(month,<<nMonth>>,'<<toisodatestring(ldDate,,.t.)>>') ddate
ENDTEXT
IF oAmSql.Execute(lcSql,"TDadd")=1
	RETURN TDadd.ddate
ELSE
	RETURN ldDate
ENDIF
ENDFUNC


FUNCTION BEGINOFMONTH
LPARAMETERS nMonth,dDate
lcDateSet=SET("Date")
SET DATE BRITISH

LOCAL mCurrMonth,mYears,mMonth,mYearsBack,mYearsForward
mYears = 0
IF EMPTY(nMonth)
	nMonth = 0
ENDIF
IF EMPTY(dDate)
	dDate=DATE()
ENDIF
mCurrMonth = MONTH(dDate) && the current month
&& First determine YEAR
mMonth = mCurrMonth + nMonth
IF (mMonth) <= 0 && to the past
	mMonth = - mMonth && reverse, now contains the number of months to track back
	mYearsBack = INT(mMonth/12)
	IF mYearsBack > 0
		mYears = mYearsBack + 1
	ELSE
		mYears = 1
	ENDIF
	mMonth = 12 * mYears - mMonth
	cStr = "1/" + ALLTRIM(STR(mMonth,2,0)) + "/" + SUBSTR(STR(YEAR(dDate) - mYears,4,0),3,2)
	cRet=CTOD(cStr)
	SET DATE &lcDateSet
	RETURN cRet
ELSE  && To the future!!
	mYearsForward = INT(mMonth/12)
	IF mYearsForward > 0
		mYears = mYearsForward
	ENDIF
	mMonth = mMonth  - (12 * mYears)
	IF mMonth = 0
		mMonth = 12
		mYears = mYears -1
	ENDIF
	cStr = "1/" + ALLTRIM(STR(mMonth,2,0)) + "/" + SUBSTR(STR(YEAR(dDate) + mYears,4,0),3,2)
	cRet=CTOD(cStr)
	SET DATE &lcDateSet
	RETURN cRet
ENDIF
ENDFUNC

FUNCTION ENDOFMONTH
LPARAMETERS nMonth,dDate
IF EMPTY(nMonth)
	nMonth = 0
ENDIF
RETURN BEGINOFMONTH(nMonth + 1,dDate) - 1
ENDFUNC