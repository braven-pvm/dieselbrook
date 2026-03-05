#DEFINE ERROR_LOG     "Reports"+DTOS(DATE())+".log"
#INCLUDE EXCEL.H

EXTERNAL ARRAY aFld

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


FUNCTION MonthNameToNumber(lcMonth)
lcMonth=UPPER(lcMonth)
RETURN ICASE(lcMonth="JANUARY",1,lcMonth="FEBRUARY",2,lcMonth="MARCH",3,lcMonth="APRIL",4, lcMonth="MAY",5,;
	lcMonth="JUNE",6, lcMonth="JULY",7,  lcMonth="AUGUST",8, lcMonth="SEPTEMBER",9, lcMonth="OCTOBER",10,;
	lcMonth="NOVEMBER",11, lcMonth="DECEMBER",12,0)

ENDFUNC


FUNCTION ReportFields(lo,aFld,lnID,loSql)

IF loSql.Execute("Select * from NopIntegration..NopReportDetail where ReportId="+TRANSFORM(lnID)+;
		"  and displayorder>0 Order by DisplayOrder","TLineFields")#1
	THIS.ErrorResponse("No Report Detail",  "400 No Report Available")
	RETURN .F.
ENDIF


ADDPROPERTY(lo,"LineFields",CREATEOBJECT("collection"))

LOCAL ii
IF RECCOUNT("TLineFields")=0

	FOR ii=1 TO ALEN(aFld,1)
		IF UPPER(aFld[ii,1])="HDF"
			LOOP
		ENDIF
		lobj=CREATEOBJECT("EMPTY")
		lcLabel=PROPER(CHRTRAN(aFld[ii,1],"_"," "))
		IF LEFT(aFld[ii,1],1)="_"
			x=	ATC("_",aFld[ii,1],2)
			IF x>0
				lcLabel=PROPER(CHRTRAN(SUBSTR(aFld[ii,1],ATC("_",aFld[ii,1],2)+1),"_"," "))
			ENDIF
		ENDIF

		ADDPROPERTY(lobj,"key",LOWER(aFld[ii,1]))
		ADDPROPERTY(lobj,"label",lcLabel)
		lo.LineFields.ADD(lobj)
	NEXT


ELSE

	DIMENSION aLFld[1]
	SELECT TLineFields
	=AFIELDS(aLFld,"TLINEFIELDS")
	SCAN
		SCATTER MEMVAR MEMO
		STORE "" TO lcKey,lcLabel
		lobj=CREATEOBJECT("EMPTY")
		IF m._Key="_"
			FOR ii=1 TO ALEN(aFld,1)
				IF UPPER(ALLTRIM(m._Key))==UPPER(LEFT(aFld[ii,1],LEN(ALLTRIM(m._Key))))
					lcLabel=PROPER(CHRTRAN(SUBSTR(aFld[ii,1],ATC("_",aFld[ii,1],2)+1),"_"," "))
					lcKey=aFld[ii,1]
				ENDIF
			NEXT
			IF EMPTY(lcKey)
				LOOP
			ENDIF
		ELSE
			lcKey=ALLTRIM(m._Key)
			lcLabel=ALLTRIM(m._Label)
		ENDIF
		ADDPROPERTY(lobj,"key",LOWER(lcKey))
		ADDPROPERTY(lobj,"label",TEXTMERGE(lcLabel))


		FOR ii=1 TO ALEN(aLFld,1)
			IF INLIST(UPPER(aLFld[ii,1]),"REPORTID","_KEY","ID","_LABEL","DISPLAYORDER","DYNAMICCOLUMN")
				LOOP
			ENDIF
			lcFld="m."+aLFld[ii,1]
			IF ISNULL(&lcFld)
				LOOP
			ENDIF

			ADDPROPERTY(lobj,CHRTRAN(aLFld[ii,1],"_"," "),&lcFld)

		NEXT
		
		lAdd=.t.
		IF !ISNULLOREMPTY(m.condition)
			TRY 
				lAdd=EVALUATE(m.condition)
			
			CATCH
			
				lAdd=.f.
			ENDTR
		ENDIF
		IF lAdd
			lo.LineFields.ADD(lobj)
		ENDIF
	ENDSCAN
ENDIF
lcJson=oSer.Serialize(lo.LineFields,.T.)
oSer.MapPropertyName(@lcJson, "tdclass","tdClass")
lo.LineFields=oSer.Deserialize(lcJson)


ENDFUNC



DEFINE CLASS Reports AS CUSTOM

cerrormsg = ""
lerror = .F.

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

	FUNCTION Inactive_Recruits(lo)
	IF !PEMSTATUS(lo.FOOTER,"count",5)
		RETURN
	ENDIF
	LOCAL ii,jj
	FOR ii=1 TO lo.DETAIL.COUNT
		lFound=.F.
		FOR jj=1 TO lo.FOOTER.COUNT
			IF lo.FOOTER[jj].recruitno=lo.DETAIL[ii].recruitno
				IF !PEMSTATUS(lo.DETAIL[ii],'oUp',5)
					lx=lo.DETAIL[ii]
					ADDPROPERTY(lx,"oUp",CREATE("Collection"))
				ENDIF
				lo.DETAIL[ii].oUp.ADD(COPYOBJECT(lo.FOOTER[jj]))

			ENDIF
		NEXT
	NEXT


	ENDFUNC


	FUNCTION Chile_Report(lo)

	TEXT TO lcSql TEXTMERGE NOSHOW
EXEC <<db>>..sp_NOP_rep_ChileDeactivations @cSponsor='<<lcCustno>>'
	ENDTEXT
	luret=oAMSQL.Execute(lcSql,"TFSD" )
	IF luret=1
		loD = CursorToCollection("TFSD")
	ELSE
		loD=CREATEOBJECT("COLLECTION")
	ENDIF
	ADDPROPERTY(lo.FOOTER,"DeactivationList",loD)

	TEXT TO lcSql TEXTMERGE NOSHOW
EXEC <<db>>..sp_NOP_rep_ChileRecruits @cSponsor='<<lcCustno>>'
	ENDTEXT
	luret=oAMSQL.Execute(lcSql,"TFSD" )
	IF luret=1
		loD = CursorToCollection("TFSD")
	ELSE
		loD=CREATEOBJECT("COLLECTION")
	ENDIF
	ADDPROPERTY(lo.FOOTER,"RecruitList",loD)

	ENDFUNC

&&---------------------------------------------------------------------
	FUNCTION Backoffice_VDS(lo)
&&---------------------------------------------------------------------
	LOGSTRING("Backoffice_VDS","TFS.log")
	_a2_pvd=""

	oData=lo.HEADER
	ADDPROPERTY(oData,"_nCGQV",LTRIM(TRANSFORM(ROUND(oData.nCGQV,0) ,'99,999,999')))
	ADDPROPERTY(oData,"_nCDqv",LTRIM(TRANSFORM(ROUND(oData.nCDqv,0)  ,'99,999,999')))
	ADDPROPERTY(oData,"_nCGrsp",LTRIM(TRANSFORM(ROUND(oData.nCGrsp,0)  ,'99,999,999')))
	ADDPROPERTY(oData,"_nCCrsp",LTRIM(TRANSFORM(ROUND(oData.nCCrsp,0),'99,999,999')))
	ADDPROPERTY(oData,"_iActiveLegs",TRANSFORM(NVL(oData.iActiveLegs,0) ,'9999'))
	ADDPROPERTY(oData,"_nTotCOM",LTRIM(TRANSFORM(NVL(oData.nTotCOM-oData.nTotCOMShop,0),'99,999,999.99')))
	ADDPROPERTY(oData,"_nTotCOMShop",LTRIM(TRANSFORM(NVL(oData.nTotCOMShop,0),'99,999,999.99')))

	
	
	ADDPROPERTY(oData,"_nTotTB",LTRIM(TRANSFORM(NVL(oData.nTotTB1,0)+NVL(oData.nTotTB2,0),'99,999,999.99')))
	ADDPROPERTY(oData,"_nTotGB",LTRIM(TRANSFORM(NVL(oData.nTotGB1+oData.nTotGB2,0),'99,999,999.99')))
	ADDPROPERTY(oData,"_nTotFSB",LTRIM(TRANSFORM(ROUND(NVL(oData.nTotFSB,0)*(1+oData.pnVatRate),2) ,'99,999,999.99')))
	ADDPROPERTY(oData,"_nTotBSB",LTRIM(TRANSFORM(ROUND(NVL(oData.nTotBSB,0)*(1+oData.pnVatRate),2) ,'99,999,999.99')))
	ADDPROPERTY(oData,"_nTOTBSM",LTRIM(TRANSFORM(ROUND((NVL(oData.nTOTBSM,0)+NVL(oData.nTotMPB,0)+NVL(oData.nTotMDB,0))*(1+oData.pnVatRate),2) ,'99,999,999.99')))
	oData.nTOTBSM=NVL(oData.nTOTBSM,0)+NVL(oData.nTotMPB,0)+NVL(oData.nTotMDB,0)
	ADDPROPERTY(oData,"_nTotgr",LTRIM(TRANSFORM(NVL(oData.nTotgr,0),'99,999,999.99')))
	ADDPROPERTY(oData,"_nTotfs1",TRANSFORM(oData.nTotfs1       ,'99'))
	ADDPROPERTY(oData,"_nTOtfs2",TRANSFORM(oData.nTOtfs2       ,'99'))
	ADDPROPERTY(oData,"_nTotfs3",TRANSFORM(oData.nTotfs3       ,'99'))
	ADDPROPERTY(oData,"_nTotComRsp",LTRIM(TRANSFORM(NVL(oData.nTotComRsp,0),'99,999,999.99')))
	ADDPROPERTY(oData,"_nCEDQV",LTRIM(TRANSFORM(oData.nCEDQV       ,'99,999,999')))
	ADDPROPERTY(oData,"_nDsalesnd",LTRIM(TRANSFORM(ROUND(oData.nDSalesnd,0),'999,999')))
	ADDPROPERTY(oData,"_nGsalesnd",LTRIM(TRANSFORM(ROUND(oData.nGSalesnd,0),'999,999')))
	ADDPROPERTY(oData,"_nTotDisc",LTRIM(TRANSFORM(NVL(oData.nTotDisc,0)*(1+oData.pnVatRate),'999,999.99')))
	oData.nTotDisc=NVL(oData.nTotDisc,0)
	ADDPROPERTY(oData,"_nTotDiscP",IIF(oData.nTotRsp<>0,TRANSFORM(oData.nTotDisc/oData.nTotRsp*100,'999,999.99'),""))
	ADDPROPERTY(oData,"_nTotComShopP",IIF(oData.nPSalesAff<>0,TRANSFORM((oData.nTotCOMShop*(1+oData.pnVatRate))/oData.nPSalesAff*100,'999,999.99'),""))
	
	ADDPROPERTY(oData,"_nTotComP",IIF(oData.nTotRsp<>0,TRANSFORM((oData.nTotCOM-oData.nTotCOMShop)/oData.nTotRsp*100,'999,999.99'),""))
	ADDPROPERTY(oData,"_dstarter",DTOC(oData.dstarter))
	ADDPROPERTY(oData,"_legC",TRANSFORM(NVL(oData.legC ,0)  ,'9999'))
	ADDPROPERTY(oData,"_legC4",TRANSFORM(NVL(oData.legC4,0) ,'9999'))
	ADDPROPERTY(oData,"_legC1",TRANSFORM(NVL(oData.legC1,0) ,'9999'))
	ADDPROPERTY(oData,"_legC2",TRANSFORM(NVL(oData.legC2,0) ,'9999'))
	ADDPROPERTY(oData,"_legC3",TRANSFORM(NVL(oData.legC3,0) ,'9999'))
	ADDPROPERTY(oData,"_legM",TRANSFORM(NVL(oData.legM,0)   ,'9999'))
	ADDPROPERTY(oData,"_legD",TRANSFORM(NVL(oData.legD,0)   ,'9999'))
	ADDPROPERTY(oData,"_legR",TRANSFORM(NVL(oData.legR,0)   ,'9999'))
	ADDPROPERTY(oData,"_legE",TRANSFORM(NVL(oData.legE,0)   ,'9999'))
	ADDPROPERTY(oData,"_legx",TRANSFORM(NVL(oData.legx,0)   ,'9999'))
	ADDPROPERTY(oData,"_Pin1",TRANSFORM(oData.Pin1+oData.Pin2    ,'9999'))
	ADDPROPERTY(oData,"_Pin2",TRANSFORM(oData.Pin2   ,'9999'))
	ADDPROPERTY(oData,"_Pin3",TRANSFORM(oData.Pin3   ,'9999'))
	ADDPROPERTY(oData,"_Pin4",TRANSFORM(oData.Pin4   ,'9999'))
	ADDPROPERTY(oData,"_Pin6",TRANSFORM(oData.Pin6   ,'9999'))
	ADDPROPERTY(oData,"_Pin7",TRANSFORM(oData.Pin7   ,'9999'))
	ADDPROPERTY(oData,"_Pin8",TRANSFORM(oData.Pin8   ,'9999'))
	ADDPROPERTY(oData,"_Pin9",TRANSFORM(oData.Pin9   ,'9999'))
	ADDPROPERTY(oData,"_Pin10",TRANSFORM(oData.Pin10  ,'9999'))
	ADDPROPERTY(oData,"_Pin11",TRANSFORM(oData.Pin11  ,'9999'))
	ADDPROPERTY(oData,"_Pat1",TRANSFORM(oData.Pat1+oData.Pat2   ,'9999'))
	ADDPROPERTY(oData,"_Pat2",TRANSFORM(oData.Pat2   ,'9999'))
	ADDPROPERTY(oData,"_Pat3",TRANSFORM(oData.Pat3   ,'9999'))
	ADDPROPERTY(oData,"_Pat4",TRANSFORM(oData.Pat4   ,'9999'))
	ADDPROPERTY(oData,"_Pat5",TRANSFORM(oData.Pat5   ,'9999'))
	ADDPROPERTY(oData,"_Pat6",TRANSFORM(oData.Pat6   ,'9999'))
	ADDPROPERTY(oData,"_Pat7",TRANSFORM(oData.Pat7   ,'9999'))
	ADDPROPERTY(oData,"_Pat8",TRANSFORM(oData.Pat8   ,'9999'))
	ADDPROPERTY(oData,"_Pat9",TRANSFORM(oData.Pat9   ,'9999'))
	ADDPROPERTY(oData,"_Pat10",TRANSFORM(oData.Pat10  ,'9999'))
	ADDPROPERTY(oData,"_Pat11",TRANSFORM(oData.Pat11  ,'9999'))
	ADDPROPERTY(oData,"_nTotRsp",LTRIM(TRANSFORM(NVL(oData.ndRsp,0),'9,999,999')))
	ADDPROPERTY(oData,"_nPQV3",LTRIM(TRANSFORM(NVL(oData.nPqv3,0),'9,999,999')))


	ADDPROPERTY(oData,"_nPSalesAff",LTRIM(TRANSFORM(ROUND(NVL(oData.nPSalesAff,0),0),'9,999,999')))
	lnCPrsp=ROUND(oData.nCPQV,0)-ROUND(oData.nPSalesnd,0)  && -ROUND(NVL(oData.nPSalesAff,0),0)
	nPsalesSub=lnCPrsp+ROUND(NVL(oData.nPSalesAff,0),0)
	nPTotal=ROUND(oData.nCPQV,0)- ROUND(NVL(oData.nPSalesAff,0),0)
	ADDPROPERTY(oData,"nPtotal",nPTotal)
	ADDPROPERTY(oData,"_nPsalesSub",LTRIM(TRANSFORM(nPsalesSub,'999,999')))
	*ADDPROPERTY(oData,"_npSalesnd",LTRIM(TRANSFORM(ROUND(oData.nPSalesnd,0),'999,999')))
	ADDPROPERTY(oData,"_npSalesnd",LTRIM(TRANSFORM(lnCPrsp+ROUND(oData.nPSalesnd,0),'999,999')))
*ADDPROPERTY(oData,"nCPQV",LTRIM(TRANSFORM(ROUND(oData.nCPQV,0) ,'99,999,999'))
	ADDPROPERTY(oData,"_nCPQV",LTRIM(TRANSFORM(ROUND(oData.nCPQV,0) ,'99,999,999')))
	ADDPROPERTY(oData,"_nCCsp",LTRIM(TRANSFORM(ROUND(oData.nCCsp*(1+oData.pnVatRate),0) ,'99,999,999')))
	ADDPROPERTY(oData,"_nCPrsp",LTRIM(TRANSFORM(lnCPrsp,'99,999,999')))


	ADDPROPERTY(oData,"l1",TRANSFORM(oData.nL1,'9999'))
	ADDPROPERTY(oData,"team",TRANSFORM(oData.nTeam,'9999'))



	TEXT TO lcSql NOSHOW TEXTMERGE
select cCompType,max(nPerc) from <<db>>..ctcomp
	where iyear=<<TRANSFORM(YEAR(oData.dend))>>
	and iMOnth=<<TRANSFORM(MONTH(oData.dend))>> and nperc<>0
	and ccustno='<<lccustno>>' group by cCompType
UNION
	select cCompType,max(nPerc) from <<db>>..ctcomph
	where iyear=<<TRANSFORM(YEAR(oData.dend))>>
	and iMOnth=<<TRANSFORM(MONTH(oData.dend))>> and nperc<>0
	and ccustno='<<lccustno>>' group by cCompType

	ENDTEXT
	IF oAMSQL.Execute(lcSql,'cComp')=-1
		RESPONSE.WRITE("Sql Error:"+SERVER.oAMSQL.cErrormsg)
		RETURN
	ENDIF


	SELECT cComp
	IF RECCOUNT()>0
		COPY TO ARRAY ac
	ELSE
		DIMENSION ac[1,2]
		ac[1,1]=" "
		ac[1,2]=0
	ENDIF



	pcGrd=""
	ADDPROPERTY(oData,"pcGrd",pcGrd)

	ENDFUNC

	
&&---------------------------------------------------------------------
	FUNCTION Volume_Discount_Statement(lo)
&&---------------------------------------------------------------------

	_a2_pvd=""

	oData=lo.HEADER
	ADDPROPERTY(oData,"_nCGQV",LTRIM(TRANSFORM(ROUND(oData.nCGQV,0) ,'99,999,999')))
	ADDPROPERTY(oData,"_nCDqv",LTRIM(TRANSFORM(ROUND(oData.nCDqv,0)  ,'99,999,999')))
	ADDPROPERTY(oData,"_nCGrsp",LTRIM(TRANSFORM(ROUND(oData.nCGrsp,0)  ,'99,999,999')))
	ADDPROPERTY(oData,"_nCCrsp",LTRIM(TRANSFORM(ROUND(oData.nCCrsp,0),'99,999,999')))
	ADDPROPERTY(oData,"_iActiveLegs",TRANSFORM(NVL(oData.iActiveLegs,0) ,'9999'))
	ADDPROPERTY(oData,"_nTotCOM",LTRIM(TRANSFORM(NVL(oData.nTotCOM-oData.nTotCOMShop,0),'99,999,999.99')))
	ADDPROPERTY(oData,"_nTotCOMShop",LTRIM(TRANSFORM(NVL(oData.nTotCOMShop,0),'99,999,999.99')))

	
	
	ADDPROPERTY(oData,"_nTotTB",LTRIM(TRANSFORM(NVL(oData.nTotTB1,0)+NVL(oData.nTotTB2,0),'99,999,999.99')))
	ADDPROPERTY(oData,"_nTotGB",LTRIM(TRANSFORM(NVL(oData.nTotGB1+oData.nTotGB2,0),'99,999,999.99')))
	ADDPROPERTY(oData,"_nTotFSB",LTRIM(TRANSFORM(ROUND(NVL(oData.nTotFSB,0)*(1+oData.pnVatRate),2) ,'99,999,999.99')))
	ADDPROPERTY(oData,"_nTotBSB",LTRIM(TRANSFORM(ROUND(NVL(oData.nTotBSB,0)*(1+oData.pnVatRate),2) ,'99,999,999.99')))
	ADDPROPERTY(oData,"_nTOTBSM",LTRIM(TRANSFORM(ROUND((NVL(oData.nTOTBSM,0)+NVL(oData.nTotMPB,0)+NVL(oData.nTotMDB,0))*(1+oData.pnVatRate),2) ,'99,999,999.99')))
	oData.nTOTBSM=NVL(oData.nTOTBSM,0)+NVL(oData.nTotMPB,0)+NVL(oData.nTotMDB,0)
	ADDPROPERTY(oData,"_nTotgr",LTRIM(TRANSFORM(NVL(oData.nTotgr,0),'99,999,999.99')))
	ADDPROPERTY(oData,"_nTotfs1",TRANSFORM(oData.nTotfs1       ,'99'))
	ADDPROPERTY(oData,"_nTOtfs2",TRANSFORM(oData.nTOtfs2       ,'99'))
	ADDPROPERTY(oData,"_nTotfs3",TRANSFORM(oData.nTotfs3       ,'99'))
	ADDPROPERTY(oData,"_nTotComRsp",LTRIM(TRANSFORM(NVL(oData.nTotComRsp,0),'99,999,999.99')))
	ADDPROPERTY(oData,"_nCEDQV",LTRIM(TRANSFORM(oData.nCEDQV       ,'99,999,999')))
	ADDPROPERTY(oData,"_nDsalesnd",LTRIM(TRANSFORM(ROUND(oData.nDSalesnd,0),'999,999')))
	ADDPROPERTY(oData,"_nGsalesnd",LTRIM(TRANSFORM(ROUND(oData.nGSalesnd,0),'999,999')))
	ADDPROPERTY(oData,"_nTotDisc",LTRIM(TRANSFORM(NVL(oData.nTotDisc,0)*(1+oData.pnVatRate),'999,999.99')))
	oData.nTotDisc=NVL(oData.nTotDisc,0)
	ADDPROPERTY(oData,"_nTotDiscP",IIF(oData.nTotRsp<>0,TRANSFORM(oData.nTotDisc/oData.nTotRsp*100,'999,999.99'),""))
	ADDPROPERTY(oData,"_nTotComShopP",IIF(oData.nPSalesAff<>0,TRANSFORM((oData.nTotCOMShop*(1+oData.pnVatRate))/oData.nPSalesAff*100,'999,999.99'),""))
	
	ADDPROPERTY(oData,"_nTotComP",IIF(oData.nTotRsp<>0,TRANSFORM((oData.nTotCOM-oData.nTotCOMShop)/oData.nTotRsp*100,'999,999.99'),""))
	ADDPROPERTY(oData,"_dstarter",DTOC(oData.dstarter))
	ADDPROPERTY(oData,"_legC",TRANSFORM(NVL(oData.legC ,0)  ,'9999'))
	ADDPROPERTY(oData,"_legC4",TRANSFORM(NVL(oData.legC4,0) ,'9999'))
	ADDPROPERTY(oData,"_legC1",TRANSFORM(NVL(oData.legC1,0) ,'9999'))
	ADDPROPERTY(oData,"_legC2",TRANSFORM(NVL(oData.legC2,0) ,'9999'))
	ADDPROPERTY(oData,"_legC3",TRANSFORM(NVL(oData.legC3,0) ,'9999'))
	ADDPROPERTY(oData,"_legM",TRANSFORM(NVL(oData.legM,0)   ,'9999'))
	ADDPROPERTY(oData,"_legD",TRANSFORM(NVL(oData.legD,0)   ,'9999'))
	ADDPROPERTY(oData,"_legR",TRANSFORM(NVL(oData.legR,0)   ,'9999'))
	ADDPROPERTY(oData,"_legE",TRANSFORM(NVL(oData.legE,0)   ,'9999'))
	ADDPROPERTY(oData,"_legx",TRANSFORM(NVL(oData.legx,0)   ,'9999'))
	ADDPROPERTY(oData,"_Pin1",TRANSFORM(oData.Pin1+oData.Pin2    ,'9999'))
	ADDPROPERTY(oData,"_Pin2",TRANSFORM(oData.Pin2   ,'9999'))
	ADDPROPERTY(oData,"_Pin3",TRANSFORM(oData.Pin3   ,'9999'))
	ADDPROPERTY(oData,"_Pin4",TRANSFORM(oData.Pin4   ,'9999'))
	ADDPROPERTY(oData,"_Pin6",TRANSFORM(oData.Pin6   ,'9999'))
	ADDPROPERTY(oData,"_Pin7",TRANSFORM(oData.Pin7   ,'9999'))
	ADDPROPERTY(oData,"_Pin8",TRANSFORM(oData.Pin8   ,'9999'))
	ADDPROPERTY(oData,"_Pin9",TRANSFORM(oData.Pin9   ,'9999'))
	ADDPROPERTY(oData,"_Pin10",TRANSFORM(oData.Pin10  ,'9999'))
	ADDPROPERTY(oData,"_Pin11",TRANSFORM(oData.Pin11  ,'9999'))
	ADDPROPERTY(oData,"_Pat1",TRANSFORM(oData.Pat1+oData.Pat2   ,'9999'))
	ADDPROPERTY(oData,"_Pat2",TRANSFORM(oData.Pat2   ,'9999'))
	ADDPROPERTY(oData,"_Pat3",TRANSFORM(oData.Pat3   ,'9999'))
	ADDPROPERTY(oData,"_Pat4",TRANSFORM(oData.Pat4   ,'9999'))
	ADDPROPERTY(oData,"_Pat5",TRANSFORM(oData.Pat5   ,'9999'))
	ADDPROPERTY(oData,"_Pat6",TRANSFORM(oData.Pat6   ,'9999'))
	ADDPROPERTY(oData,"_Pat7",TRANSFORM(oData.Pat7   ,'9999'))
	ADDPROPERTY(oData,"_Pat8",TRANSFORM(oData.Pat8   ,'9999'))
	ADDPROPERTY(oData,"_Pat9",TRANSFORM(oData.Pat9   ,'9999'))
	ADDPROPERTY(oData,"_Pat10",TRANSFORM(oData.Pat10  ,'9999'))
	ADDPROPERTY(oData,"_Pat11",TRANSFORM(oData.Pat11  ,'9999'))
	ADDPROPERTY(oData,"_nTotRsp",LTRIM(TRANSFORM(NVL(oData.ndRsp,0),'9,999,999')))
	ADDPROPERTY(oData,"_nPQV3",LTRIM(TRANSFORM(NVL(oData.nPqv3,0),'9,999,999')))


	ADDPROPERTY(oData,"_nPSalesAff",LTRIM(TRANSFORM(ROUND(NVL(oData.nPSalesAff,0),0),'9,999,999')))
	lnCPrsp=ROUND(oData.nCPQV,0)-ROUND(oData.nPSalesnd,0)  && -ROUND(NVL(oData.nPSalesAff,0),0)
	nPsalesSub=lnCPrsp+ROUND(NVL(oData.nPSalesAff,0),0)
	nPTotal=ROUND(oData.nCPQV,0)- ROUND(NVL(oData.nPSalesAff,0),0)
	ADDPROPERTY(oData,"nPtotal",nPTotal)
	ADDPROPERTY(oData,"_nPsalesSub",LTRIM(TRANSFORM(nPsalesSub,'999,999')))
	*ADDPROPERTY(oData,"_npSalesnd",LTRIM(TRANSFORM(ROUND(oData.nPSalesnd,0),'999,999')))
	ADDPROPERTY(oData,"_npSalesnd",LTRIM(TRANSFORM(lnCPrsp+ROUND(oData.nPSalesnd,0),'999,999')))
*ADDPROPERTY(oData,"nCPQV",LTRIM(TRANSFORM(ROUND(oData.nCPQV,0) ,'99,999,999'))
	ADDPROPERTY(oData,"_nCPQV",LTRIM(TRANSFORM(ROUND(oData.nCPQV,0) ,'99,999,999')))
	ADDPROPERTY(oData,"_nCCsp",LTRIM(TRANSFORM(ROUND(oData.nCCsp*(1+oData.pnVatRate),0) ,'99,999,999')))
	ADDPROPERTY(oData,"_nCPrsp",LTRIM(TRANSFORM(lnCPrsp,'99,999,999')))


	ADDPROPERTY(oData,"l1",TRANSFORM(oData.nL1,'9999'))
	ADDPROPERTY(oData,"team",TRANSFORM(oData.nTeam,'9999'))



	TEXT TO lcSql NOSHOW TEXTMERGE
select cCompType,max(nPerc) from <<db>>..ctcomp
	where iyear=<<TRANSFORM(YEAR(oData.dend))>>
	and iMOnth=<<TRANSFORM(MONTH(oData.dend))>> and nperc<>0
	and ccustno='<<lccustno>>' group by cCompType
UNION
	select cCompType,max(nPerc) from <<db>>..ctcomph
	where iyear=<<TRANSFORM(YEAR(oData.dend))>>
	and iMOnth=<<TRANSFORM(MONTH(oData.dend))>> and nperc<>0
	and ccustno='<<lccustno>>' group by cCompType

	ENDTEXT
	IF oAMSQL.Execute(lcSql,'cComp')=-1
		RESPONSE.WRITE("Sql Error:"+SERVER.oAMSQL.cErrormsg)
		RETURN
	ENDIF


	SELECT cComp
	IF RECCOUNT()>0
		COPY TO ARRAY ac
	ELSE
		DIMENSION ac[1,2]
		ac[1,1]=" "
		ac[1,2]=0
	ENDIF



	pcGrd=""
	ADDPROPERTY(oData,"pcGrd",pcGrd)

	ENDFUNC

&&---------------------------------------------------------------------
	FUNCTION VDS_Stage(lo)
&&---------------------------------------------------------------------

	_a2_pvd=""

	oData=lo.HEADER
	ADDPROPERTY(oData,"_nCGQV",LTRIM(TRANSFORM(ROUND(oData.nCGQV,0) ,'99,999,999')))
	ADDPROPERTY(oData,"_nCDqv",LTRIM(TRANSFORM(ROUND(oData.nCDqv,0)  ,'99,999,999')))
	ADDPROPERTY(oData,"_nCGrsp",LTRIM(TRANSFORM(ROUND(oData.nCGrsp,0)  ,'99,999,999')))
	ADDPROPERTY(oData,"_nCCrsp",LTRIM(TRANSFORM(ROUND(oData.nCCrsp,0),'99,999,999')))
	ADDPROPERTY(oData,"_iActiveLegs",TRANSFORM(NVL(oData.iActiveLegs,0) ,'9999'))
	ADDPROPERTY(oData,"_nTotCOM",LTRIM(TRANSFORM(NVL(oData.nTotCOM-oData.nTotCOMShop,0),'99,999,999.99')))
	ADDPROPERTY(oData,"_nTotCOMShop",LTRIM(TRANSFORM(NVL(oData.nTotCOMShop,0),'99,999,999.99')))

	
	
	ADDPROPERTY(oData,"_nTotTB",LTRIM(TRANSFORM(NVL(oData.nTotTB1,0)+NVL(oData.nTotTB2,0),'99,999,999.99')))
	ADDPROPERTY(oData,"_nTotGB",LTRIM(TRANSFORM(NVL(oData.nTotGB1+oData.nTotGB2,0),'99,999,999.99')))
	ADDPROPERTY(oData,"_nTotFSB",LTRIM(TRANSFORM(ROUND(NVL(oData.nTotFSB,0)*(1+oData.pnVatRate),2) ,'99,999,999.99')))
	ADDPROPERTY(oData,"_nTotBSB",LTRIM(TRANSFORM(ROUND(NVL(oData.nTotBSB,0)*(1+oData.pnVatRate),2) ,'99,999,999.99')))
	ADDPROPERTY(oData,"_nTOTBSM",LTRIM(TRANSFORM(ROUND((NVL(oData.nTOTBSM,0)+NVL(oData.nTotMPB,0)+NVL(oData.nTotMDB,0))*(1+oData.pnVatRate),2) ,'99,999,999.99')))
	oData.nTOTBSM=NVL(oData.nTOTBSM,0)+NVL(oData.nTotMPB,0)+NVL(oData.nTotMDB,0)
	ADDPROPERTY(oData,"_nTotgr",LTRIM(TRANSFORM(NVL(oData.nTotgr,0),'99,999,999.99')))
	ADDPROPERTY(oData,"_nTotfs1",TRANSFORM(oData.nTotfs1       ,'99'))
	ADDPROPERTY(oData,"_nTOtfs2",TRANSFORM(oData.nTOtfs2       ,'99'))
	ADDPROPERTY(oData,"_nTotfs3",TRANSFORM(oData.nTotfs3       ,'99'))
	ADDPROPERTY(oData,"_nTotComRsp",LTRIM(TRANSFORM(NVL(oData.nTotComRsp,0),'99,999,999.99')))
	ADDPROPERTY(oData,"_nCEDQV",LTRIM(TRANSFORM(oData.nCEDQV       ,'99,999,999')))
	ADDPROPERTY(oData,"_nDsalesnd",LTRIM(TRANSFORM(ROUND(oData.nDSalesnd,0),'999,999')))
	ADDPROPERTY(oData,"_nGsalesnd",LTRIM(TRANSFORM(ROUND(oData.nGSalesnd,0),'999,999')))
	ADDPROPERTY(oData,"_nTotDisc",LTRIM(TRANSFORM(NVL(oData.nTotDisc,0)*(1+oData.pnVatRate),'999,999.99')))
	oData.nTotDisc=NVL(oData.nTotDisc,0)
	ADDPROPERTY(oData,"_nTotDiscP",IIF(oData.nTotRsp<>0,TRANSFORM(oData.nTotDisc/oData.nTotRsp*100,'999,999.99'),""))
	ADDPROPERTY(oData,"_nTotComShopP",IIF(oData.nPSalesAff<>0,TRANSFORM((oData.nTotCOMShop*(1+oData.pnVatRate))/oData.nPSalesAff*100,'999,999.99'),""))
	
	ADDPROPERTY(oData,"_nTotComP",IIF(oData.nTotRsp<>0,TRANSFORM((oData.nTotCOM-oData.nTotCOMShop)/oData.nTotRsp*100,'999,999.99'),""))
	ADDPROPERTY(oData,"_dstarter",DTOC(oData.dstarter))
	ADDPROPERTY(oData,"_legC",TRANSFORM(NVL(oData.legC ,0)  ,'9999'))
	ADDPROPERTY(oData,"_legC4",TRANSFORM(NVL(oData.legC4,0) ,'9999'))
	ADDPROPERTY(oData,"_legC1",TRANSFORM(NVL(oData.legC1,0) ,'9999'))
	ADDPROPERTY(oData,"_legC2",TRANSFORM(NVL(oData.legC2,0) ,'9999'))
	ADDPROPERTY(oData,"_legC3",TRANSFORM(NVL(oData.legC3,0) ,'9999'))
	ADDPROPERTY(oData,"_legM",TRANSFORM(NVL(oData.legM,0)   ,'9999'))
	ADDPROPERTY(oData,"_legD",TRANSFORM(NVL(oData.legD,0)   ,'9999'))
	ADDPROPERTY(oData,"_legR",TRANSFORM(NVL(oData.legR,0)   ,'9999'))
	ADDPROPERTY(oData,"_legE",TRANSFORM(NVL(oData.legE,0)   ,'9999'))
	ADDPROPERTY(oData,"_legx",TRANSFORM(NVL(oData.legx,0)   ,'9999'))
	ADDPROPERTY(oData,"_Pin1",TRANSFORM(oData.Pin1+oData.Pin2    ,'9999'))
	ADDPROPERTY(oData,"_Pin2",TRANSFORM(oData.Pin2   ,'9999'))
	ADDPROPERTY(oData,"_Pin3",TRANSFORM(oData.Pin3   ,'9999'))
	ADDPROPERTY(oData,"_Pin4",TRANSFORM(oData.Pin4   ,'9999'))
	ADDPROPERTY(oData,"_Pin6",TRANSFORM(oData.Pin6   ,'9999'))
	ADDPROPERTY(oData,"_Pin7",TRANSFORM(oData.Pin7   ,'9999'))
	ADDPROPERTY(oData,"_Pin8",TRANSFORM(oData.Pin8   ,'9999'))
	ADDPROPERTY(oData,"_Pin9",TRANSFORM(oData.Pin9   ,'9999'))
	ADDPROPERTY(oData,"_Pin10",TRANSFORM(oData.Pin10  ,'9999'))
	ADDPROPERTY(oData,"_Pin11",TRANSFORM(oData.Pin11  ,'9999'))
	ADDPROPERTY(oData,"_Pat1",TRANSFORM(oData.Pat1+oData.Pat2   ,'9999'))
	ADDPROPERTY(oData,"_Pat2",TRANSFORM(oData.Pat2   ,'9999'))
	ADDPROPERTY(oData,"_Pat3",TRANSFORM(oData.Pat3   ,'9999'))
	ADDPROPERTY(oData,"_Pat4",TRANSFORM(oData.Pat4   ,'9999'))
	ADDPROPERTY(oData,"_Pat5",TRANSFORM(oData.Pat5   ,'9999'))
	ADDPROPERTY(oData,"_Pat6",TRANSFORM(oData.Pat6   ,'9999'))
	ADDPROPERTY(oData,"_Pat7",TRANSFORM(oData.Pat7   ,'9999'))
	ADDPROPERTY(oData,"_Pat8",TRANSFORM(oData.Pat8   ,'9999'))
	ADDPROPERTY(oData,"_Pat9",TRANSFORM(oData.Pat9   ,'9999'))
	ADDPROPERTY(oData,"_Pat10",TRANSFORM(oData.Pat10  ,'9999'))
	ADDPROPERTY(oData,"_Pat11",TRANSFORM(oData.Pat11  ,'9999'))
	ADDPROPERTY(oData,"_nTotRsp",LTRIM(TRANSFORM(NVL(oData.ndRsp,0),'9,999,999')))
	ADDPROPERTY(oData,"_nPQV3",LTRIM(TRANSFORM(NVL(oData.nPqv3,0),'9,999,999')))


	ADDPROPERTY(oData,"_nPSalesAff",LTRIM(TRANSFORM(ROUND(NVL(oData.nPSalesAff,0),0),'9,999,999')))
	lnCPrsp=ROUND(oData.nCPQV,0)-ROUND(NVL(oData.nPSalesAff,0),0)-ROUND(oData.nPSalesnd,0)
	nPsalesSub=lnCPrsp+ROUND(NVL(oData.nPSalesAff,0),0)
	nPTotal=ROUND(oData.nCPQV,0)- ROUND(NVL(oData.nPSalesAff,0),0)
	ADDPROPERTY(oData,"nPtotal",nPTotal)
	ADDPROPERTY(oData,"_nPsalesSub",LTRIM(TRANSFORM(nPsalesSub,'999,999')))
	ADDPROPERTY(oData,"_npSalesnd",LTRIM(TRANSFORM(ROUND(oData.nPSalesnd,0),'999,999')))
*ADDPROPERTY(oData,"nCPQV",LTRIM(TRANSFORM(ROUND(oData.nCPQV,0) ,'99,999,999'))
	ADDPROPERTY(oData,"_nCPQV",LTRIM(TRANSFORM(ROUND(oData.nCPQV,0) ,'99,999,999')))
	ADDPROPERTY(oData,"_nCCsp",LTRIM(TRANSFORM(ROUND(oData.nCCsp*(1+oData.pnVatRate),0) ,'99,999,999')))
	ADDPROPERTY(oData,"_nCPrsp",LTRIM(TRANSFORM(lnCPrsp,'99,999,999')))


	ADDPROPERTY(oData,"l1",TRANSFORM(oData.nL1,'9999'))
	ADDPROPERTY(oData,"team",TRANSFORM(oData.nTeam,'9999'))



	TEXT TO lcSql NOSHOW TEXTMERGE
select cCompType,max(nPerc) from <<db>>..ctcomp
	where iyear=<<TRANSFORM(YEAR(oData.dend))>>
	and iMOnth=<<TRANSFORM(MONTH(oData.dend))>> and nperc<>0
	and ccustno='<<lccustno>>' group by cCompType
UNION
	select cCompType,max(nPerc) from <<db>>..ctcomph
	where iyear=<<TRANSFORM(YEAR(oData.dend))>>
	and iMOnth=<<TRANSFORM(MONTH(oData.dend))>> and nperc<>0
	and ccustno='<<lccustno>>' group by cCompType

	ENDTEXT
	IF oAMSQL.Execute(lcSql,'cComp')=-1
		RESPONSE.WRITE("Sql Error:"+oAMSQL.cErrormsg)
		RETURN
	ENDIF


	SELECT cComp
	IF RECCOUNT()>0
		COPY TO ARRAY ac
	ELSE
		DIMENSION ac[1,2]
		ac[1,1]=" "
		ac[1,2]=0
	ENDIF



	pcGrd=""
	#if .f.
	FOR EACH cData IN lo.DETAIL


		cClass=""
		IF BETWEEN(cData.dstarter,cData.dstart,cData.dend)
			cClass='class="table-striped"'
		ENDIF
		cClassName=""
		IF cData.ilevel=1
			cClassName='level1'
		ENDIF
		TEXT TEXTMERGE TO lcTR NOSHOW
<tr <<cclass>> >
<td class="<<cclassname>> vdsdrill" ><<cData.ccustno>></td>
<td class="<<cclassname>> contact" data-email="<<cDAta.cemail>>" data-cell="<<cDAta.cphone2>>"
 data-city="<<cDAta.ccity>>" data-state="<<cDAta.cstate>>">
 <<cDAta.ccompany>>

</td>
<td class="<<cclassname>>" ><<REPLICATE(".",cdata.ilevel)+TRANSFORM(cdata.iLevel)>></td>
<td class="<<cclassname>>" ><<DTOC(cdata.dstarter)>></td>
<td class="<<cclassname>>" ><<ICASE(cData.ctitlecode="C1","S1",cData.ctitlecode="C2","S2",cData.ctitlecode="C3","S3",cData.ctitlecode="C4","S4",cData.ctitlecode)>></td>
<td class="<<cclassname>>" ><<ICASE(cData.cpatitlecode="C1","S1",cData.cpatitlecode="C2","S2",cData.cpatitlecode="C3","S3",cData.cpatitlecode="C4","S4",cData.cpatitlecode))>></td>
<td class="text-right <<cclassname>>"><<TRANSFORM(ROUND(cData.nCCsp*(1+odata.pnVatRate),0),'99,999,999')>></td>
<td class="text-right <<cclassname>>"><<TRANSFORM(ROUND(cdata.nCPQV,0)  ,'99,999,999')>></td>
<td class="text-right <<cclassname>>"><<TRANSFORM(ROUND(cdata.nCGQV,0)  ,'99,999,999')>></td>
<td class="text-right <<cclassname>>"><<TRANSFORM(ROUND(cdata.nCDqv,0)  ,'99,999,999')>></td>
<td class="text-right <<cclassname>>"><<TRANSFORM(ROUND(cdata.nPqv3,0) ,'99,999,999')>></td>
<td class="text-right <<cclassname>>"><<TRANSFORM(ROUND(cdata.ncPQV-cdata.npSalesND,0),'99,999,999')>></td>
<td class="text-right <<cclassname>>"><<TRANSFORM(NVL(cdata.nTotTB1,0),'99,999,999.99')>></td>
<td class="text-right <<cclassname>>"><<TRANSFORM(IIF(ASCAN(ac,'TB1',1,ALEN(ac,1),1,8)>0,ac[ASCAN(ac,'TB1',1,ALEN(ac,1),1,8),2],0),'99.99')>></td>
<td class="text-right <<cclassname>>"><<TRANSFORM(NVL(cdata.nTotTB2,0),'99,999,999.99')>></td>
<td class="text-right <<cclassname>>"><<TRANSFORM(IIF(ASCAN(ac,'TB2',1,ALEN(ac,1),1,8)>0,ac[ASCAN(ac,'TB2',1,ALEN(ac,1),1,8),2],0),'99.99')>></td>
<td class="text-right <<cclassname>>"><<TRANSFORM(NVL(cdata.nTotgr,0),'99,999,999.99')>></td>
<td class="text-right <<cclassname>>"><<TRANSFORM(IIF(ASCAN(ac,'GR',1,ALEN(ac,1),1,8)>0,ac[ASCAN(ac,'GR',1,ALEN(ac,1),1,8),2],0),'99.99')>></td>
<td class="text-right <<cclassname>>"><<TRANSFORM(NVL(cdata.nTotGB1,0),'99,999,999.99')>></td>
<td class="text-right <<cclassname>>"><<TRANSFORM(IIF(ASCAN(ac,'GE1',1,ALEN(ac,1),1,8)>0,ac[ASCAN(ac,'GE1',1,ALEN(ac,1),1,8),2],0),'99.99')>></td>
<td class="text-right <<cclassname>>"><<TRANSFORM(NVL(cdata.nTotGB2,0),'99,999,999.99')>></td>
<td class="text-right <<cclassname>>"><<TRANSFORM(IIF(ASCAN(ac,'GE2',1,ALEN(ac,1),1,8)>0,ac[ASCAN(ac,'GE2',1,ALEN(ac,1),1,8),2],0),'99.99')>></td>
<td class="text-right <<cclassname>>"><<TRANSFORM(NVL(cdata.nTotfsb ,0)      ,'99,999,999.99')>></td>
<td class="text-right <<cclassname>> <<IIF(cdata.vbal>0,'bg-success text-white','')>>"><<TRANSFORM(ROUND(NVL(cData.nPSalesAff,0),0) ,'99,999,999.99')>></td>
<td class="text-right <<cclassname>> "><<TRANSFORM(ROUND(NVL(cData.nPending,0),0) ,'99,999,999.99')>></td>
</tr>
		ENDTEXT
		pcGrd=pcGrd+lcTR

	NEXT
#endif

	ADDPROPERTY(oData,"pcGrd",pcGrd)

	ENDFUNC



&&---------------------------------------------------------------------
	FUNCTION vdsDrill(lo)
&&---------------------------------------------------------------------

	_a2_pvd=""

	oData=lo.HEADER
	ADDPROPERTY(oData,"_nCGQV",LTRIM(TRANSFORM(ROUND(oData.nCGQV,0) ,'99,999,999')))
	ADDPROPERTY(oData,"_nCDqv",LTRIM(TRANSFORM(ROUND(oData.nCDqv,0)  ,'99,999,999')))
	ADDPROPERTY(oData,"_nCGrsp",LTRIM(TRANSFORM(ROUND(oData.nCGrsp,0)  ,'99,999,999')))
	ADDPROPERTY(oData,"_nCCrsp",LTRIM(TRANSFORM(ROUND(oData.nCCrsp,0),'99,999,999')))
	ADDPROPERTY(oData,"_iActiveLegs",TRANSFORM(NVL(oData.iActiveLegs,0) ,'9999'))
	ADDPROPERTY(oData,"_nTotCOM",LTRIM(TRANSFORM(NVL(oData.nTotCOM,0),'99,999,999.99')))
	ADDPROPERTY(oData,"_nTotCOMShop",LTRIM(TRANSFORM(NVL(oData.nTotCOMShop,0),'99,999,999.99')))
	ADDPROPERTY(oData,"_nTotTB",LTRIM(TRANSFORM(NVL(oData.nTotTB1,0)+NVL(oData.nTotTB2,0),'99,999,999.99')))
	ADDPROPERTY(oData,"_nTotGB",LTRIM(TRANSFORM(NVL(oData.nTotGB1+oData.nTotGB2,0),'99,999,999.99')))
	ADDPROPERTY(oData,"_nTotFSB",LTRIM(TRANSFORM(ROUND(NVL(oData.nTotFSB,0)*(1+oData.pnVatRate),2) ,'99,999,999.99')))
	ADDPROPERTY(oData,"_nTotBSB",LTRIM(TRANSFORM(ROUND(NVL(oData.nTotBSB,0)*(1+oData.pnVatRate),2) ,'99,999,999.99')))
	ADDPROPERTY(oData,"_nTOTBSM",LTRIM(TRANSFORM(ROUND((NVL(oData.nTOTBSM,0)+NVL(oData.nTotMPB,0)+NVL(oData.nTotMDB,0))*(1+oData.pnVatRate),2) ,'99,999,999.99')))
	oData.nTOTBSM=NVL(oData.nTOTBSM,0)+NVL(oData.nTotMPB,0)+NVL(oData.nTotMDB,0)
	ADDPROPERTY(oData,"_nTotgr",LTRIM(TRANSFORM(NVL(oData.nTotgr,0),'99,999,999.99')))
	ADDPROPERTY(oData,"_nTotfs1",TRANSFORM(oData.nTotfs1       ,'99'))
	ADDPROPERTY(oData,"_nTOtfs2",TRANSFORM(oData.nTOtfs2       ,'99'))
	ADDPROPERTY(oData,"_nTotfs3",TRANSFORM(oData.nTotfs3       ,'99'))
	ADDPROPERTY(oData,"_nTotComRsp",LTRIM(TRANSFORM(NVL(oData.nTotComRsp,0),'99,999,999.99')))
	ADDPROPERTY(oData,"_nCEDQV",LTRIM(TRANSFORM(oData.nCEDQV       ,'99,999,999')))
	ADDPROPERTY(oData,"_nDsalesnd",LTRIM(TRANSFORM(ROUND(oData.nDSalesnd,0),'999,999')))
	ADDPROPERTY(oData,"_nGsalesnd",LTRIM(TRANSFORM(ROUND(oData.nGSalesnd,0),'999,999')))
	ADDPROPERTY(oData,"_nTotDisc",LTRIM(TRANSFORM(NVL(oData.nTotDisc,0)*(1+oData.pnVatRate),'999,999.99')))
	oData.nTotDisc=NVL(oData.nTotDisc,0)
	ADDPROPERTY(oData,"_nTotDiscP",IIF(oData.nTotRsp<>0,TRANSFORM(oData.nTotDisc/oData.nTotRsp*100,'999,999.99'),""))
	ADDPROPERTY(oData,"_nTotComP",IIF(oData.nTotRsp<>0,TRANSFORM(oData.nTotCOM/oData.nTotRsp*100,'999,999.99'),""))
	ADDPROPERTY(oData,"_nTotComShopP",IIF(oData.nPSalesAff<>0,TRANSFORM((oData.nTotCOMShop*(1+oData.pnVatRate))/oData.nPSalesAff*100,'999,999.99'),""))
	
	ADDPROPERTY(oData,"_dstarter",DTOC(oData.dstarter))
	ADDPROPERTY(oData,"_legC",TRANSFORM(NVL(oData.legC ,0)  ,'9999'))
	ADDPROPERTY(oData,"_legC4",TRANSFORM(NVL(oData.legC4,0) ,'9999'))
	ADDPROPERTY(oData,"_legC1",TRANSFORM(NVL(oData.legC1,0) ,'9999'))
	ADDPROPERTY(oData,"_legC2",TRANSFORM(NVL(oData.legC2,0) ,'9999'))
	ADDPROPERTY(oData,"_legC3",TRANSFORM(NVL(oData.legC3,0) ,'9999'))
	ADDPROPERTY(oData,"_legM",TRANSFORM(NVL(oData.legM,0)   ,'9999'))
	ADDPROPERTY(oData,"_legD",TRANSFORM(NVL(oData.legD,0)   ,'9999'))
	ADDPROPERTY(oData,"_legR",TRANSFORM(NVL(oData.legR,0)   ,'9999'))
	ADDPROPERTY(oData,"_legE",TRANSFORM(NVL(oData.legE,0)   ,'9999'))
	ADDPROPERTY(oData,"_legx",TRANSFORM(NVL(oData.legx,0)   ,'9999'))
	ADDPROPERTY(oData,"_Pin1",TRANSFORM(oData.Pin1+oData.Pin2    ,'9999'))
	ADDPROPERTY(oData,"_Pin2",TRANSFORM(oData.Pin2   ,'9999'))
	ADDPROPERTY(oData,"_Pin3",TRANSFORM(oData.Pin3   ,'9999'))
	ADDPROPERTY(oData,"_Pin4",TRANSFORM(oData.Pin4   ,'9999'))
	ADDPROPERTY(oData,"_Pin6",TRANSFORM(oData.Pin6   ,'9999'))
	ADDPROPERTY(oData,"_Pin7",TRANSFORM(oData.Pin7   ,'9999'))
	ADDPROPERTY(oData,"_Pin8",TRANSFORM(oData.Pin8   ,'9999'))
	ADDPROPERTY(oData,"_Pin9",TRANSFORM(oData.Pin9   ,'9999'))
	ADDPROPERTY(oData,"_Pin10",TRANSFORM(oData.Pin10  ,'9999'))
	ADDPROPERTY(oData,"_Pin11",TRANSFORM(oData.Pin11  ,'9999'))
	ADDPROPERTY(oData,"_Pat1",TRANSFORM(oData.Pat1+oData.Pat2   ,'9999'))
	ADDPROPERTY(oData,"_Pat2",TRANSFORM(oData.Pat2   ,'9999'))
	ADDPROPERTY(oData,"_Pat3",TRANSFORM(oData.Pat3   ,'9999'))
	ADDPROPERTY(oData,"_Pat4",TRANSFORM(oData.Pat4   ,'9999'))
	ADDPROPERTY(oData,"_Pat5",TRANSFORM(oData.Pat5   ,'9999'))
	ADDPROPERTY(oData,"_Pat6",TRANSFORM(oData.Pat6   ,'9999'))
	ADDPROPERTY(oData,"_Pat7",TRANSFORM(oData.Pat7   ,'9999'))
	ADDPROPERTY(oData,"_Pat8",TRANSFORM(oData.Pat8   ,'9999'))
	ADDPROPERTY(oData,"_Pat9",TRANSFORM(oData.Pat9   ,'9999'))
	ADDPROPERTY(oData,"_Pat10",TRANSFORM(oData.Pat10  ,'9999'))
	ADDPROPERTY(oData,"_Pat11",TRANSFORM(oData.Pat11  ,'9999'))
	ADDPROPERTY(oData,"_nTotRsp",LTRIM(TRANSFORM(NVL(oData.ndRsp,0),'9,999,999')))
	ADDPROPERTY(oData,"_nPQV3",LTRIM(TRANSFORM(NVL(oData.nPqv3,0),'9,999,999')))


	ADDPROPERTY(oData,"_nPSalesAff",LTRIM(TRANSFORM(ROUND(NVL(oData.nPSalesAff,0),0),'9,999,999')))
	lnCPrsp=ROUND(oData.nCPQV,0)-ROUND(NVL(oData.nPSalesAff,0),0)-ROUND(oData.nPSalesnd,0)
	nPsalesSub=lnCPrsp+ROUND(NVL(oData.nPSalesAff,0),0)
	nPTotal=ROUND(oData.nCPQV,0)- ROUND(NVL(oData.nPSalesAff,0),0)
	ADDPROPERTY(oData,"nPtotal",nPTotal)
	ADDPROPERTY(oData,"_nPsalesSub",LTRIM(TRANSFORM(nPsalesSub,'999,999')))
	ADDPROPERTY(oData,"_npSalesnd",LTRIM(TRANSFORM(ROUND(oData.nPSalesnd,0),'999,999')))
*ADDPROPERTY(oData,"nCPQV",LTRIM(TRANSFORM(ROUND(oData.nCPQV,0) ,'99,999,999'))
	ADDPROPERTY(oData,"_nCPQV",LTRIM(TRANSFORM(ROUND(oData.nCPQV,0) ,'99,999,999')))
	ADDPROPERTY(oData,"_nCCsp",LTRIM(TRANSFORM(ROUND(oData.nCCsp*(1+oData.pnVatRate),0) ,'99,999,999')))
	ADDPROPERTY(oData,"_nCPrsp",LTRIM(TRANSFORM(lnCPrsp,'99,999,999')))


	ADDPROPERTY(oData,"l1",TRANSFORM(oData.nL1,'9999'))
	ADDPROPERTY(oData,"team",TRANSFORM(oData.nTeam,'9999'))

ENDFUNC


	FUNCTION DownlineTraded


	ENDFUNC

&&---------------------------------------------------------------------
	FUNCTION DashBoard(lo)
&&---------------------------------------------------------------------
	ln=AFIELDS(af,'TFS1')
	ADDPROPERTY(lo,"LineFields",CREATEOBJECT("collection"))

	lobj=CREATEOBJECT("EMPTY")
	ADDPROPERTY(lobj,"key","cdesciption")
	ADDPROPERTY(lobj,"label","Description")
	lo.LineFields.ADD(lobj)
	lobj=CREATEOBJECT("EMPTY")
*!*		ADDPROPERTY(lobj,"key","ctype")
*!*		ADDPROPERTY(lobj,"formatter","dashRow")
*!*		lo.LineFields.ADD(lobj)

	DIMENSION aColH[18,2]
	dt=BEGINOFMONTH(1,DATE())
	istart=0
	ic=0
	ilc=0
	DO WHILE ilc<18
		ilc=ilc+1
		dt=BEGINOFMONTH(-1,dt)
		iy=YEAR(dt)
		im=MONTH(dt)
		cVar="N"+TRANSFORM(iy)+"_"+RIGHT("0"+TRANSFORM(im),2)
		FOR ii=1 TO ln
			IF af[ii,1]=cVar
				ic=ic+1
				lobj=CREATEOBJECT("EMPTY")
				ADDPROPERTY(lobj,"label",LEFT(CMONTH(dt),3)+" "+TRANSFORM(iy))
				ADDPROPERTY(lobj,"key","_"+TRANSFORM(iy)+"_"+RIGHT("0"+TRANSFORM(im),2))
				ADDPROPERTY(lobj,"tdclass","tdCallBack")
*!*					IF lo.DETAIL[xx].cvartype$"IN"
*!*							lobj.tdclass="text-right")
*!*						ENDIF
*!*					IF lo.DETAIL[1].cvartype$"IN"
*!*						ADDPROPERTY(lobj,"formatter","format_number")
*!*					ENDIF
				FOR xx=1 TO lo.DETAIL.COUNT
					
					ADDPROPERTY(lo.DETAIL[xx],"_"+TRANSFORM(iy)+"_"+RIGHT("0"+TRANSFORM(im),2))
					DO CASE	
						CASE lo.DETAIL[xx].cvartype="C"

						MVAR="lo.Detail[xx]._"+TRANSFORM(iy)+"_"+RIGHT("0"+TRANSFORM(im),2)
						cVar="lo.Detail[xx].C"+TRANSFORM(iy)+"_"+RIGHT("0"+TRANSFORM(im),2)
						&MVAR=NVL(&cVar,"")

					CASE lo.DETAIL[xx].cvartype="I"
						MVAR="lo.Detail[xx]._"+TRANSFORM(iy)+"_"+RIGHT("0"+TRANSFORM(im),2)
						cVar="lo.Detail[xx].N"+TRANSFORM(iy)+"_"+RIGHT("0"+TRANSFORM(im),2)
						&MVAR=NVL(TRANSFORM(&cVar,lo.DETAIL[xx].cmask),"")

					CASE lo.DETAIL[xx].cvartype="N"
						MVAR="lo.Detail[xx]._"+TRANSFORM(iy)+"_"+RIGHT("0"+TRANSFORM(im),2)
						cVar="lo.Detail[xx].N"+TRANSFORM(iy)+"_"+RIGHT("0"+TRANSFORM(im),2)
						&MVAR=NVL(TRANSFORM(&cVar,lo.DETAIL[xx].cmask),"")
						
					OTHERWISE	
						MVAR="lo.Detail[xx]._"+TRANSFORM(iy)+"_"+RIGHT("0"+TRANSFORM(im),2)
						&MVAR=""
					ENDCASE
				NEXT


				lo.LineFields.ADD(lobj)
				istart=ic
				EXIT
			ENDIF
		NEXT
	ENDDO



	ENDFUNC


	FUNCTION Support(lo)
	
	oSql=CREATEOBJECT("wwSQL")
	IF !process.DBConnect(process.oConfig.cSqlconnectstring,oSql)
		=LOGSTRING(this.cerrormsg,ERROR_LOG)
		RETURN .F.
	ENDIF	

	oNopSql=CREATEOBJECT("wwSQL")
	IF !process.DBConnect(process.oConfig.cNopSqlconnectstring,oNopSql)
		=LOGSTRING(this.cerrormsg,ERROR_LOG)
		RETURN .F.
	ENDIF	
	oNopSql.EnableUnicodeToAnsiMapping()
		lcUrl=""
		oSSo=NEWOBJECT("Support","ssoClass.prg")
		luret= oSSo.Autologin(lo.header.ccustno,@lcUrl)
		IF luret 
			ADDPROPERTY(lo.header,"url",lcUrl)
		ELSE
			ADDPROPERTY(lo.header,"url",lcUrl)
			ADDPROPERTY(lo.header,"cErrorMsg",oSSO.cErrorMsg)
		ENDIF
		IF ISNULLOREMPTY(lo.header.url)
		=LOGSTRING(osso.cerrormsg,ERROR_LOG)
		ENDIF
	
	ENDFUNC
	
	FUNCTION Academy(lo)
	
	oSql=CREATEOBJECT("wwSQL")
	IF !process.DBConnect(process.oConfig.cSqlconnectstring,oSql)
		=LOGSTRING(this.cerrormsg,ERROR_LOG)
		RETURN .F.
	ENDIF	

	oNopSql=CREATEOBJECT("wwSQL")
	IF !process.DBConnect(process.oConfig.cNopSqlconnectstring,oNopSql)
		=LOGSTRING(this.cerrormsg,ERROR_LOG)
		RETURN .F.
	ENDIF	
	oNopSql.EnableUnicodeToAnsiMapping()
		lcUrl=""
		oSSo=NEWOBJECT("Academy","ssoClass.prg")
		luret= oSSo.Autologin(lo.header.ccustno,@lcUrl)
		IF luret 
			ADDPROPERTY(lo.header,"url",lcUrl)
		ELSE
			ADDPROPERTY(lo.header,"url",lcUrl)
			ADDPROPERTY(lo.header,"cErrorMsg",oSSO.cErrorMsg)
		ENDIF
	
		IF ISNULLOREMPTY(lo.header.url)
		=LOGSTRING(osso.cerrormsg,ERROR_LOG)
		ENDIF
	
	ENDFUNC



ENDDEFINE


DEFINE CLASS VDS_Excel AS GENXL

FUNCTION beforegenrows()
		this.currentrow=this.currentrow+1
		LOCAL zz
*!*			FOR zz=1 TO 29 STEP 2
*!*			.RANGE(THIS.xlrange(this.currentrow,6+zz,this.currentrow,7+zz)).Merge()
*!*	    	.RANGE(THIS.xlrange(this.currentrow,6+zz,this.currentrow,7+zz)).HorizontalAlignment = xlCenter
*!*	    	.RANGE(THIS.xlrange(this.currentrow,6+zz,this.currentrow,7+zz)).interior.COLOR= RGB(245,245,245)
*!*	    	.RANGE(THIS.xlrange(this.currentrow,6+zz,this.currentrow,7+zz)).Borders(xlEdgeLeft).LineStyle = xlContinuous
*!*	  		.RANGE(THIS.xlrange(this.currentrow,6+zz,this.currentrow,7+zz)).Borders(xlEdgeRight).LineStyle = xlContinuous
*!*			.RANGE(THIS.xlrange(this.currentrow,6+zz,this.currentrow,7+zz)).Borders(xlEdgeBottom).LineStyle = xlContinuous
*!*	    	.RANGE(THIS.xlrange(this.currentrow,6+zz,this.currentrow,7+zz)).Borders(xlEdgeTop).LineStyle = xlContinuous
*!*			NEXT
*!*			
*!*			.RANGE(THIS.xlrange(this.currentrow,7)).VALUE="1st"
*!*		   	.RANGE(THIS.xlrange(this.currentrow,9)).VALUE="2nd"
*!*	    	.RANGE(THIS.xlrange(this.currentrow,11)).VALUE="3rd"
*!*			.RANGE(THIS.xlrange(this.currentrow,13)).VALUE="4th"
*!*			.RANGE(THIS.xlrange(this.currentrow,15)).VALUE="5th"
*!*			.RANGE(THIS.xlrange(this.currentrow,17)).VALUE="6th"
*!*			.RANGE(THIS.xlrange(this.currentrow,19)).VALUE="7th"
*!*			.RANGE(THIS.xlrange(this.currentrow,21)).VALUE="8th"
*!*			.RANGE(THIS.xlrange(this.currentrow,23)).VALUE="9th"
*!*			.RANGE(THIS.xlrange(this.currentrow,25)).VALUE="10th"
*!*			.RANGE(THIS.xlrange(this.currentrow,27)).VALUE="11th"
*!*			.RANGE(THIS.xlrange(this.currentrow,29)).VALUE="12th"

*!*	    	
    	
    	
    	this.currentrow=this.currentrow-1
  		* .VerticalAlignment = xlCenter
		*
		*+"   SALE STATUS:"+NVL(TFS.SaleStatus,"All");
		*+"   MANDATE STATUS:"+NVL(TFS.MANDATEStatus,"All");
		*+"   INSTALMENT STATUS:"+NVL(TFS.INSTALMENTStatus,"All")
		*.RANGE(THIS.xlrange(this.currentrow,6)).VALUE="FROM DATE:"+NVL(TFS.FROMSALEDATE,"")
		*.RANGE(THIS.xlrange(this.currentrow,8)).VALUE="TO DATE:"+NVL(TFS.TOSALEDATE,"")
ENDFUNC
ENDDEFINE