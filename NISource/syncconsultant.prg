#DEFINE ERROR_LOG     "Synchlog"+DTOS(DATE())+".log"
#DEFINE INI_FILE	  "nop.ini"
#DEFINE LOGTIME		  STRTRAN(LEFT(TIME(),5),":","")
#INCLUDE wconnect.h
DO wconnect
DO WWUTILS
SET PROCEDURE TO WWSQL ADDIT
SET PROCEDURE TO wwJsonSerializer ADDIT
SET PROCEDURE TO wwBusinessObject ADDIT
SET PROCEDURE TO BaseData ADDIT
SET PROCEDURE TO NOPAPI ADDIT
SET PROCEDURE TO NOPData ADDIT
SET PROCEDURE TO SyncClass ADDIT
SET PROCEDURE TO AMData ADDIT
SET DATE YMD
CLEAR


DEFINE CLASS SyncConsultants AS SyncClass
	oCust=NULL
	StoreID=1
	oAddress=NULL
	oCustomerAddresses=NULL
	oArCust=NULL
	oArCadr=NULL
	oProfile=NULL

&&------------------------------------------------------------------------------------
	FUNCTION SETUP(lcUrl,lnStoreID)
&&------------------------------------------------------------------------------------
	IF !DODEFAULT(lcUrl)
		RETURN .F.
	ENDIF
	THIS.StoreID=lnStoreID

	WITH THIS
		.oCust=CREATEOBJECT("Customer")
		.oCust.SetSqlObject(oNopSql)
		.oAddress=CREATEOBJECT("Address")
		.oAddress.SetSqlObject(oNopSql)
		.oCustomerAddresses=CREATEOBJECT("CustomerAddresses")
		.oCustomerAddresses.SetSqlObject(oNopSql)
		oNopSql.EXECUTE("select * from StateProvince where countryid="+;
			TRANSFORM(IIF(this.StoreID=2,155,207)),"Tstates")
		.oArCust=CREATEOBJECT("arcust")
		.oArCust.SetSqlObject(oAMSql)
		.oArCadr=CREATEOBJECT("arcadr")
		.oArCadr.SetSqlObject(oAMSql)
		.oProfile=CREATEOBJECT("ANQ_UserProfileAdditionalInfo")
		.oProfile.SetSqlObject(oNopSql)
		

	ENDWITH

	ENDFUNC
&&------------------------------------------------------------------------------------
	PROCEDURE SyncALL()
&&------------------------------------------------------------------------------------

	ENDPROC


&&------------------------------------------------------------------------------------
	PROCEDURE SyncOne (lcAccount)
&&------------------------------------------------------------------------------------

	WITH THIS
		IF !.oArCust.LOAD(lcAccount)
			THIS.lerror=.T.
			THIS.cErrormsg="Could not get arcust record "+THIS.cErrormsg
			RETURN .F.	&& Set error flags etc
		ENDIF


		IF !.oCust.loadbase("username='"+.oArCust.odata.ccustno+"'")
			IF .oCust.osql.lerror
				THIS.lerror=.T.
				THIS.cErrormsg=.oCust.oSql.cErrormsg
				RETURN .F.	&& Set error flags etc
			ENDIF
			lNew=.T.
			lnCustomerID=-1
		ELSE
			lNew=.F.
			lnCustomerID=.oCust.odata.ID
			lcn=.oCust.odata.firstname
		ENDIF

		WITH .oArCust.odata
			m.ccustno=UPPER(ALLTRIM(.ccustno))

			SELECT Tstates
			LOCATE FOR NAME=.cState OR Abbreviation=.cState
			IF FOUND()
				lnStateID=ID
			ELSE
				lnStateID=IIF(THIS.StoreID=2,1848,1820)
			ENDIF

			IF lNew
				TEXT TO lcJson TEXTMERGE NOSHOW
{
  "customer_guid": "<<x8guid(36)>>",
  "username": "<<.cCustno>>",
  "email": "<<ALLTRIM(.cEmail)>>",
  "active": true,
  "deleted": false,
  "first_name": "<<oxml.EncodeXML(ALLTRIM(.cFname))>>",
  "last_name": "<<oxml.EncodeXML(ALLTRIM(.cLname))>>",
  "gender": "<<.cGender>>",
  "date_of_birth": "<<.dBirthday>>",
  "company": "<<oxml.EncodeXML(ALLTRIM(.cCompany))>>",
  "street_address": "<<oxml.EncodeXML(ALLTRIM(.cAddr1))>>",
  "street_address2": "<<oxml.EncodeXML(ALLTRIM(.cAddr2))>>",
  "zip_postal_code": "<<.cZip>>",
  "city": "<<.cCity>>",
  "county": null,
  "phone": "<<.cPhone2>>",
  "fax": "<<.cPhone2>>",
  "vat_number": "<<.ctaxFld1>>",
  "country_id": <<IIF(This.StoreID=2,155,207)>>,
  "state_province_id": <<lnStateID>>,
  "registered_in_store_id": <<This.StoreID>>,
  "created_on_utc" : '<<TOISODATESTRING(.dCreate,.t.,.t.)>>',
     "id": 0
}
				ENDTEXT

				luret=THIS.oNop.Customer_Create(lcJson)
				IF VARTYPE(luret)<>"O"
					=LOGSTRING("Failed Add Consultant :"+oNop.cErrormsg,"Consultants.Log")
					THIS.SetError("Failed Add Consultant :"+oNop.cErrormsg)
					RETURN .F.
				ENDIF
				lnCustomerID=luret.ID
				lnBillingAddressID=0
				lnShippingAddressID=0
				lcPassword=ALLTRIM(.ccustno)+"Anq!"
				luret=THIS.oNop.Customer_SetPassword(lnCustomerID,lcPassword)
				IF !this.oCust.LOAD(lnCustomerID)
					THIS.SetError("Failed to Find New Consultant :"+this.oCust.cErrormsg)
					RETURN .F.
				ENDIF
			ELSE
				lnCustomerID=this.oCust.odata.ID
				lnBillingAddressID=IIF(this.oCust.odata.BillingAddress_ID=0,NULL,this.oCust.odata.BillingAddress_ID)
				lnShippingAddressID=IIF(this.oCust.odata.ShippingAddress_ID=0,NULL,this.oCust.odata.ShippingAddress_ID)
				THIS.AddressValidate ("B",lnCustomerID,@lnBillingAddressID)
				THIS.AddressValidate ("S",lnCustomerID,@lnShippingAddressID)
				this.oCust.odata.email=ALLTRIM(.cEmail)
				this.oCust.odata.ACTIVE= .cStatus="A"
				this.oCust.odata.firstname=ALLTRIM(.cFname)
				this.oCust.odata.lastname=ALLTRIM(.cLname)
				this.oCust.odata.company=.cCompany
				this.oCust.odata.streetaddress=ALLTRIM(.cAddr1)
				this.oCust.odata.streetaddress2=ALLTRIM(.cAddr2)
				this.oCust.odata.zippostalcode=.cZip
				this.oCust.odata.city=.cCity
				this.oCust.odata.phone=.cPhone2
				this.oCust.odata.fax=.cPhone2
				this.oCust.odata.vatnumber=.ctaxFld1
				this.oCust.odata.stateprovinceid=lnStateID
				


			ENDIF
		ENDWITH
&&---------------- Sync Billing from AM ----------------
		THIS.SyncAddress("AB",this.oArCust.odata.ccustno	,lnCustomerID,@lnBillingAddressID)
&&---------------- Sync 001 Shipping from AM ----------------
		THIS.SyncAddress("AS",this.oArCust.odata.ccustno	,lnCustomerID,@lnShippingAddressID)

&&------------------Update Customer -----------------
		lnBillingAddressID=IIF(lnBillingAddressID=0,NULL,lnBillingAddressID)
		lnShippingAddressID=IIF(lnShippingAddressID=0,NULL,lnShippingAddressID)
		.oCust.odata.BillingAddress_ID=lnBillingAddressID
		.oCust.odata.ShippingAddress_ID=lnShippingAddressID
		IF !.oCust.SAVE()
			THIS.SetError("Failed to Save Consultant :"+.oCust.cErrormsg)
			RETURN .F.
		ENDIF

&&------------------ Add Roles---------------------------------
		luret=THIS.oNop.Customer_AddRole(lnCustomerID,"Registered")
		luret=THIS.oNop.Customer_AddRole(lnCustomerID,"Annique Consultant")
&&------------------ Update Additional Info -------------------
		THIS.UpdateAdditionalInfo(lnCustomerID)
&&------------------ Add Starter Kits -------------------
		IF lNew
			THIS.UpdateStarterKits(lnCustomerID,this.oArCust.odata.ccustno)
		ENDIF
	ENDWITH

	ENDFUNC


&&---------------------------------------------------------------------------------
	FUNCTION AddressValidate (lcType,lnCustomerID,lnAddressID)
&&---------------------------------------------------------------------------------
	TEXT TO lcSql TEXTMERGE NOSHOW
select * from Address a where a.id=<<lnAddressID>>
	ENDTEXT
	IF oNopSql.EXECUTE(lcSql,"CheckAdd")=1
		IF RECCOUNT("CheckAdd")=0
			lnAdressID=0
		ENDIF
	ELSE
		lnAdressID=0
	ENDIF
	ENDFUNC

&&---------------------------------------------------------------------------------
	FUNCTION SyncAddress(lcType,lcCustno,lnCustomerID,lnAddressID)
&&---------------------------------------------------------------------------------
	DO CASE

	CASE lcType="AB"		&& Post
		IF oAMSql.EXECUTE("select * from arcadr with (nolock) where ccustno='" + ;
				lcCustno+"' AND cAddrno='POST'","Tadd")<>1
			THIS.SetError(oAMSql.cErrormsg)
			RETURN .F.
		ENDIF

	CASE lcType="AS"		&& 001
		IF oAMSql.EXECUTE("select * from arcadr with (nolock) where ccustno='" + ;
				lcCustno+"' AND cAddrno='001'","Tadd")<>1
			THIS.SetError(oAMSql.cErrormsg)
			RETURN .F.
		ENDIF

	CASE lcType="WB"		&& Webstore Billing
		IF oWSSql.EXECUTE("select * from Address where ccustno='"+;
				lcCustno+"' AND cAddrno='POST' ORDER BY ID","Tadd")<>1
			THIS.SetError(oWSSql.cErrormsg)
			RETURN .F.
		ENDIF

	CASE lcType="WS"		&& Webstore Shipping
		IF oWSSql.EXECUTE("select * from Address where ccustno='"+;
				lcCustno+"' AND cAddrno<>'POST' AND isPep=0 ORDER BY ID","Tadd")<>1
			THIS.SetError(oWSSql.cErrormsg)
			RETURN .F.
		ENDIF

	ENDCASE


	SELECT Tadd
	SCAN
		SCATTER NAME oAdd MEMO
		lnID=0
		IF lnAddressID<>0  && Update existing default Address
			TEXT TO lcSql TEXTMERGE NOSHOW
select c.* from CustomerAddresses c JOIN
 Address a ON c.address_id=a.id
where c.Customer_ID=<<lnCustomerID>> AND a.ID='<<TRANSFORM(lnAddressID)>>'
			ENDTEXT
			oNopSql.EXECUTE(lcSql,"CheckAdd")
			IF RECCOUNT("CheckAdd")>0
				lnID=CheckAdd.Address_ID
			ELSE
				lnID=0
			ENDIF
		ELSE

			IF INLIST(lcType,"WS","WB")  && Coming from Webstore

				TEXT TO lcSql TEXTMERGE NOSHOW
select c.* from CustomerAddresses c JOIN
 Address a ON c.address_id=a.id
where c.Customer_ID=<<lnCustomerID>> AND a.faxnumber='<<TRANSFORM(oAdd.id)>>'
				ENDTEXT
				oNopSql.EXECUTE(lcSql,"CheckAdd")
				IF RECCOUNT("CheckAdd")>0
					lnID=CheckAdd.Address_ID
				ELSE
					lnID=0
				ENDIF
			ENDIF

			SELECT Tstates
			LOCATE FOR NAME=Tadd.cState OR Abbreviation=Tadd.cState
			IF FOUND()
				lnStateID=ID
			ELSE
				lnStateID=IIF(THIS.StoreID=2,1848,1820)
			ENDIF

		ENDIF

		lHasCustomerAddress=IIF(lnID<>0,.T.,.F.)
		IF !this.oAddress.LOAD(lnID)
			oCustomerAddresses.New()
			this.oAddress.New()
		ELSE
			this.oAddress.New()
		ENDIF
		WITH THIS.oAddress.odata
			.firstname=ALLTRIM(this.oArCust.odata.cFname)
			.lastname=ALLTRIM(this.oArCust.odata.cLname)
			.email=ALLTRIM(this.oArCust.odata.cEmail)
			.company=ALLTRIM(this.oArCust.odata.cCompany)
			.address1=ALLTRIM(this.oArCust.odata.cAddr1)
			.address2=ALLTRIM(this.oArCust.odata.cAddr2)
			.zippostalcode=ALLTRIM(this.oArCust.odata.cZip)
			.city=ALLTRIM(this.oArCust.odata.cCity)
			.countryid=IIF(this.StoreID=2,155,207)
			.stateprovinceid=lnStateID
			.phonenumber=ALLTRIM(this.oArCust.odata.cPhone2)
*.faxnumber": "<<TRANSFORM(m.id)>>",
		ENDWITH
		IF !THIS.oAddress.SAVE()
			THIS.SetError=this.oAddress.cErrormsg
			RETURN .F.
		ENDIF
		IF INLIST(lcType,"AS","AB","WB") AND lnAddressID=0
			lnAddressID=THIS.oAddress.odata.ID
		ENDIF
		IF !lHasCustomerAddress
			this.oCustomerAddresses.New()
			this.oCustomerAddresses.odata.Address_ID=this.oAddress.odata.ID
			this.oCustomerAddresses.odata.Customer_id=lnCustomerID
			IF !this.oCustomerAddresses.SAVE()
				THIS.SetError=this.oAddress.cErrormsg
				RETURN .F.
			ENDIF
			RETURN
		ENDIF



	ENDSCAN

	ENDFUNC

&&---------------------------------------------------------------------------------
	FUNCTION UpdateRoles(lnCustomerID)
&&---------------------------------------------------------------------------------
	luret=THIS.oNop.Customer_AddRole(lnCustomerID,"Registered")
	luret=THIS.oNop.Customer_AddRole(lnCustomerID,"Annique Consultant")
	ENDFUNC

&& ------------------- Update Additional Info ------------
	FUNCTION UpdateAdditionalInfo(lnCustomerID)
&&---------------------------------------------------------------------------------
	IF !this.oProfile.loadbase("CustomerID="+TRANSFORM(lnCustomerID))
		this.oProfile.New()
		this.oProfile.odata.Accept=.f.
      	this.oProfile.odata.ProfileUpdated=.f.
	ENDIF
	WITH this.oProfile.odata
	
			.CustomerId=lnCustomerID
			 m.ctitle=IIF(LOWER(this.oArcust.odata.ctitle)='miss','Ms',this.oArcust.odata.ctitle)
		    .Title=ALLTRIM(this.oArcust.odata.cTitle)
		    .Nationality=ALLTRIM(this.oArcust.odata.lRsa) &&m.cNation
			.IdNumber=ALLTRIM(this.oArcust.odata.cIdno)
			 m.cLanguage=IIF(LOWER(this.oArcust.odata.cLanguage)='eng','English',this.oArcust.odata.cLanguage)
			 m.cLanguage=IIF(LOWER(this.oArcust.odata.cLanguage)='afr','Afrikaans',this.oArcust.odata.cLanguage)
			.Language=ALLTRIM(this.oArcust.odata.cLanguage)
			.Ethnicity=ALLTRIM(this.oArcust.odata.cRace)
		    .BankName=ALLTRIM(this.oArcust.odata.cBranchno)
      		.AccountHolder=ALLTRIM(this.oArcust.odata.chldrname)
      		.AccountNumber=this.oArcust.odata.cbankacct
      		.AccountType=ALLTRIM(this.oArcust.odata.caccttype)
      		.ActivationDate=this.oArcust.odata.dstarter
    
      		.WhatsappNumber=ALLTRIM(this.oArcust.odata.cPhone2)
	
	ENDWITH
		IF !this.oProfile.SAVE()
			THIS.SetError=this.oProfile.cErrormsg
			RETURN .F.
		ENDIF
	ENDFUNC

&&---------------------------------------------------------------------------------
	FUNCTION UpdateStarterKits(lnCustomerID,lcCustno)
&&---------------------------------------------------------------------------------
	TEXT TO lcSql TEXTMERGE NOSHOW
EXEC NopIntegration..ANQ_loadstarter @cCustno='<<lcCustno>>',@CustomerID=<<lnCustomerID>>,@StoreID=<<this.StoreID>>
	ENDTEXT
	lok=.T.
	IF !oAMSql.executenonquery(lcSql)
		=LOGSTRING("Could not load starter "+oAMSql.cErrormsg,"CustSync.log")
		RETURN .F.
	ENDIF
	ENDFUNC


&&---------------------------------------------------------------------------------
PROCEDURE SendWelcomeMail(loBus)
&&---------------------------------------------------------------------------------
STORE "" TO SponsorName,ConsultantName,ConsultantPhone,ConsultantEmail
podata=loBus.odata
IF  !EMPTY(loBus.odata.csponsor)
	lcSql="select * from arCust where cCustno=?podata.csponsor"
	IF loBus.Query(lcSql,"TSPON")>0
		SponsorName=TSpon.cCompany
		SponsorEmail=TSpon.cEmail
		SponsorPhone=TSpon.cPhone2
	ENDIF
ENDIF
ConsultantName=ALLTRIM(loBus.oData.cFname)+" "+ALLTRIM(loBus.oData.cLname)
ConsultantPhone=ALLTRIM(loBus.oData.cPhone2)
ConsultantEmail=ALLTRIM(loBus.oData.cEmail)
ConsultantNo=ALLTRIM(loBus.oData.cCustno)
ConsultantIDNo=ALLTRIM(loBus.oData.cIDno)

oMail=CREATEOBJECT("MailMessage")
oMail.SetsqlObject(oWSSql)
Custno=ConsultantNo
IDno=ALLTRIM(Custno)+"Anq!"
		ConsulantName=ALLTRIM(loBus.oData.cFname)+" "+ALLTRIM(loBus.oData.cLname)
		WITH oMail
			IF !.New()
				RETURN "Could not create mail"
			ENDIF
*!*				IF FILE(process.oConfig.cHTMLPagePath+"\templates\activateemail.txt")
*!*					lc=FILETOSTR(process.oConfig.cHTMLPagePath+"\templates\activateemail.txt")
*!*					lcAlternatetext=MERGETEXT(lc)
*!*				ELSE
				lcAlternatetext=""
			*ENDIF	
			IF FILE(process.oConfig.cHTMLPagePath+"\templates\activateemail.wct")
				lc=FILETOSTR(process.oConfig.cHTMLPagePath+"\templates\activateemail.wct")
				.odata.Details=MERGETEXT(lc)
				.odata.Content = "text/html"
			ELSE
				.odata.Details=lcAlternatetext
				.odata.Content = "text/plain"
			ENDIF	
			.odata.Details=CHRTRAN(.odata.Details,CHR(0),"")
			.odata.AlternateContentType = "text/plain"
			.odata.AlternateText=lcAlternateText
			.odata.ToEmail=loBus.odata.cEmail
			.odata.CcEmail=SponsorEmail
			.odata.BccEmail="itsupport@annique.com"
			.odata.Attachment=process.oConfig.cHTMLPagePath+"\templates\ABC PLan Grid Final_2025.pdf,"
			.odata.Attachment=.odata.Attachment+process.oConfig.cHTMLPagePath+"\templates\Roadmap_2025.pdf,"
			.odata.Attachment=.odata.Attachment+process.oConfig.cHTMLPagePath+"\templates\Starter packs_2025.pdf,"
			.odata.Attachment=.odata.Attachment+process.oConfig.cHTMLPagePath+"\templates\Fast start Ts and Cs_2025.pdf"
			.odata.Subject="Welcome to Annique Rooibos - Your Gateway to Success!"
			IF !.save()
				RETURN "Could not save mail"
			ENDIF
			
			
			IF !.New()
				RETURN "Could not create mail"
			ENDIF
			IF FILE(process.oConfig.cHTMLPagePath+"\templates\activatesms.txt")
				lc=FILETOSTR(process.oConfig.cHTMLPagePath+"\templates\activatesms.txt")
				.odata.Details=MERGETEXT(lc)
			ELSE
				.odata.Details="New Recruit"
			ENDIF	
			.odata.ToEmail="SMS" &&oData.cEmail
			.odata.CcEmail=""
			.odata.BccEmail=""
			.odata.Subject=ConsultantPhone
			IF !.save()
				RETURN "Could not save mail"
			ENDIF
		ENDWITH	
		RETURN ""
	ENDFUNC
ENDDEFINE



#IF .F.



PROCEDURE SyncALL

oCust=CREATEOBJECT("Customer")
oCust.SetSqlObject(oNopSql)


oNopSql.EXECUTE("select * from StateProvince where countryid=207","Tstates")
*!*		Belinda 200804
*!*	Adele D 506458
*!*	Michelle G 602854
*!*	Annamarie Cronje 504879
*!*	Petro Venter 200008
*!*	,'100007','100239','100332','101060',
*!*	'200008','200083','200915','202046','500061','504879','506458','509660','511736','518441',
*!*	'529617','601685','601782','602854','610289','633919') '200804','506458','602854','504879','200008''610289','506458')

TEXT TO lcSql NOSHOW TEXTMERGE
SELECT * FROM arcust WHERE ccustno in ('200008','100007','601782','516260')
ENDTEXT
IF oAMSql.EXECUTE(lcSql,"WSCUST")<1
	THIS.SetError(oWSBus.cErrormsg)
	RETURN .F.
ENDIF

SELECT WSCUST
SCAN
	SCATTER MEMVAR MEMO
	m.ccustno=UPPER(ALLTRIM(m.ccustno))

	SELECT Tstates
	LOCATE FOR NAME=m.cState OR Abbreviation=m.cState
	IF FOUND()
		lnStateID=ID
	ELSE
		lnStateID=1820
	ENDIF

	IF !oCust.loadbase("username='"+UPPER(m.ccustno)+"'")


		TEXT TO lcJson TEXTMERGE NOSHOW
{
  "customer_guid": "<<x8guid(36)>>",
  "username": "<<m.cCustno>>",
  "email": "<<ALLTRIM(m.cEmail)>>",
  "active": true,
  "deleted": false,
  "first_name": "<<oxml.EncodeXML(ALLTRIM(m.cFname))>>",
  "last_name": "<<oxml.EncodeXML(ALLTRIM(m.cLname))>>",
  "gender": "<<m.cGender>>",
  "date_of_birth": "<<m.dBirthday>>",
  "company": "<<m.cCompany>>",
  "street_address": "<<oxml.EncodeXML(ALLTRIM(m.cAddr1))>>",
  "street_address2": "<<oxml.EncodeXML(ALLTRIM(m.cAddr2))>>",
  "zip_postal_code": "<<m.cZip>>",
  "city": "<<m.cCity>>",
  "county": null,
  "phone": "<<m.cPhone2>>",
  "fax": "<<m.cPhone2>>",
  "vat_number": "<<m.ctaxFld1>>",
  "country_id": 207,
  "state_province_id": <<lnStateID>>,
	"registered_in_store_id": 1,	"created_on_utc" : '<<TOISODATESTRING(m.dCreate,.t.,.t.)>>',
     "id": 0
}
		ENDTEXT
&& "phone": <<ALLTRIM(m.cPhone2)>>,
		luret=THIS.oNop.Customer_Create(lcJson)
		IF VARTYPE(luret)<>"O"
			=LOGSTRING("Failed  Add Consultant :"+oNop.cErrormsg,"Consultants.Log")
			LOOP
		ENDIF
		lnCustomerID=luret.ID
		lnBillingAddressID=0
		lnShippingAddressID=0

&& Set Password
		luret=THIS.oNop.Customer_SetPassword(lnCustomerID,ALLTRIM(m.cIdno))
	ELSE
		lnCustomerID=oCust.odata.ID
		lnBillingAddressID=oCust.odata.BillingAddress_ID
		lnShippingAddressID=oCust.odata.ShippingAddress_ID
		TEXT TO lcSql TEXTMERGE NOSHOW
select from Address a where a.id=<<lnBillingAddressID>>
		ENDTEXT
		IF oNopSql.EXECUTE(lcSql,"CheckAdd")=1
			IF RECCOUNT("CheckAdd")=0
				lnBillingAddressID=0
			ENDIF
		ELSE
			lnBillingAddressID=0
		ENDIF
		TEXT TO lcSql TEXTMERGE NOSHOW
select from Address a where a.id=<<lnShippingAddressID>>
		ENDTEXT
		IF oNopSql.EXECUTE(lcSql,"CheckAdd")=1
			IF RECCOUNT("CheckAdd")=0
				lnShippingAddressID=0
			ENDIF
			lnShippingAddressID=0
		ENDIF

	ENDIF

&& Set Customer Mapping
	luret=THIS.oNop.Customer_AddRole(lnCustomerID,"Registered")
	luret=THIS.oNop.Customer_AddRole(lnCustomerID,"Annique Consultant")

	oWSSql.EXECUTE("select * from Address where ccustno='"+	m.ccustno+"' ORDER BY ID","Tadd")
	SELECT Tadd
	LOCATE FOR cAddrno='POST'
	IF FOUND()
		SCATTER MEMVAR MEMO
		SELECT Tstates
		LOCATE FOR NAME=m.cState OR Abbreviation=m.cState
		IF FOUND()
			lnStateID=ID
		ELSE
			lnStateID=1820
		ENDIF
		TEXT TO lcSql TEXTMERGE NOSHOW
select c.* from CustomerAddresses c JOIN
 Address a ON c.address_id=a.id
where c.Customer_ID=<<lnCustomerID>> AND a.faxnumber='<<TRANSFORM(m.id)>>'
		ENDTEXT
		oNopSql.EXECUTE(lcSql,"CheckAdd")
		IF RECCOUNT("CheckAdd")>0
			lnID=CheckAdd.Address_ID
		ELSE
			lnID=0
		ENDIF

&& Update Billing

		TEXT TO lcJson TEXTMERGE NOSHOW
{
  "first_name": "<<oxml.EncodeXML(ALLTRIM(WSCUST.cFname))>>",
  "last_name": "<<oxml.EncodeXML(ALLTRIM(WSCUST.cLname))>>",
  "email": "<<ALLTRIM(WSCUST.cEmail)>>",
  "company": "<<m.cCompany>>",
  "address1": "<<oxml.EncodeXML(ALLTRIM(m.cAddr1))>>",
  "address2": "<<oxml.EncodeXML(ALLTRIM(m.cAddr2))>>",
  "zip_postal_code": "<<m.cZip>>",
  "city": "<<m.cCity>>",
  "county": null,
  "country_id": 207,
  "state_province_id": <<lnStateID>>,
  "phone_number": "<<m.cPhone>>",
  "fax_number": "<<TRANSFORM(m.id)>>",
  "id": <<lnID>>
}
		ENDTEXT
		lUpdateAddress=.F.
		IF lnID=0
			luret=oNop.Address_Create(lcJson,lnCustomerID)
			IF VARTYPE(luret)<>"O"
				=LOGSTRING("Failed  Add  Billing:"+oNop.cErrormsg,"Consultants.Log")
			ELSE
				IF ISNULLOREMPTY(lnBillingAddressID)
					lnBillingAddressID=luret.ID
					lUpdateAddress=.T.
				ENDIF
			ENDIF
		ELSE
			luret=oNop.Address_UPDATE(lcJson,lnCustomerID)
			IF VARTYPE(luret)<>"O"
				=LOGSTRING("Failed Update Billing:"+oNop.cErrormsg,"Consultants.Log")
			ENDIF
			lnBillingAddressID=lnID
		ENDIF

	ENDIF
&& Update Shipping
	SELECT Tadd
	SCAN FOR cAddrno<>'POST' AND isPep=.F.
		SCATTER MEMVAR MEMO
		SELECT Tstates
		LOCATE FOR NAME=m.cState OR Abbreviation=m.cState
		IF FOUND()
			lnStateID=ID
		ELSE
			lnStateID=1820
		ENDIF
		TEXT TO lcSql TEXTMERGE NOSHOW
select c.* from CustomerAddresses c JOIN
 Address a ON c.address_id=a.id
where c.Customer_ID=<<lnCustomerID>> AND a.faxnumber='<<TRANSFORM(m.id)>>'
		ENDTEXT
		oNopSql.EXECUTE(lcSql,"CheckAdd")
		IF RECCOUNT("CheckAdd")>0
			lnID=CheckAdd.Address_ID
		ELSE
			lnID=0
		ENDIF


		TEXT TO lcJson TEXTMERGE NOSHOW
{
  "first_name": "<<oxml.EncodeXML(ALLTRIM(WSCUST.cFname))>>",
  "last_name": "<<oxml.EncodeXML(ALLTRIM(WSCUST.cLname))>>",
  "email": "<<ALLTRIM(WSCUST.cEmail)>>",
  "company": "<<m.cCompany>>",
  "address1": "<<oxml.EncodeXML(ALLTRIM(m.cAddr1))>>",
  "address2": "<<oxml.EncodeXML(ALLTRIM(m.cAddr2))>>",
  "zip_postal_code": "<<m.cZip>>",
  "city": "<<m.cCity>>",
  "county": null,
  "country_id": 207,
  "state_province_id": <<lnStateID>>,
  "phone_number": "<<m.cPhone>>",
  "fax_number": "<<TRANSFORM(m.id)>>",
  "id": <<lnID>>
}
		ENDTEXT

		IF lnID=0
			luret=oNop.Address_Create(lcJson,lnCustomerID)
			IF VARTYPE(luret)<>"O"
				=LOGSTRING("Failed  Add  Shipping:"+oNop.cErrormsg,"Consultants.Log")
			ELSE

				IF ISNULLOREMPTY(lnShippingAddressID) OR m.cAddrno='001'
					IF m.cAddrno='001'
						lnShippingAddressID=luret.ID
					ENDIF
					lUpdateAddress=.T.
				ENDIF
			ENDIF

		ELSE
			luret=oNop.Address_UPDATE(lcJson,lnCustomerID)
			IF VARTYPE(luret)<>"L" OR !luret
				=LOGSTRING("Failed Update Shipping:"+oNop.cErrormsg,"Consultants.Log")
			ENDIF
			IF ISNULLOREMPTY(lnShippingAddressID) OR m.cAddrno='001'
				IF m.cAddrno='001'
					lnShippingAddressID=lnID
				ENDIF
				lUpdateAddress=.T.
			ENDIF
		ENDIF


	ENDSCAN

	SELECT WSCUST
	SCATTER MEMVAR MEMO
	IF lUpdateAddress OR .T.
		luret=THIS.oNop.Customer_Get(lnCustomerID)
		IF VARTYPE(luret)<>"O"
			=LOGSTRING("Failed Reload Consultant:"+oNop.cErrormsg,"Consultants.Log")
			LOOP
		ENDIF
		luret.billing_address_id=INT(lnBillingAddressID)
		luret.shipping_address_id=INT(lnShippingAddressID)
		luret.first_name=oXml.EncodeXML(ALLTRIM(m.cFname))
		luret.last_name=oXml.EncodeXML(ALLTRIM(m.cLname))
		luret.street_address=oXml.EncodeXML(ALLTRIM(m.cAddr1))
		luret.street_address2=oXml.EncodeXML(ALLTRIM(m.cAddr2))
		luret.zip_postal_code= m.cZip
		luret.city= m.cCity
		luret.state_province_id=lnStateID

		luret.phone=m.cPhone2
		luret.fax=m.cPhone2
		luret.vat_number=m.ctaxFld1
		luret.created_on_utc=m.dCreate

		lcJson=oSer.Serialize(luret)
		luret=THIS.oNop.Customer_Update(lcJson)
		IF !luret
			=LOGSTRING("Failed Update Default Consultant Address :"+oNop.cErrormsg,"Consultants.Log")
			LOOP
		ENDIF

	ENDIF

&& ------------------- Update Additional Info ------------

	IF !oProfile.loadbase("CustomerID="+TRANSFORM(lnCustomerID))
		oProfile.New()
	ENDIF
	WITH oProfile.odata
		.CustomerId=lnCustomerID
		m.ctitle=IIF(LOWER(m.ctitle)='miss','Ms',m.ctitle)
		.TITLE=ALLTRIM(m.ctitle)
		.Nationality=ALLTRIM(m.lRsa) &&m.cNation
		.IdNumber=ALLTRIM(m.cIdno)
		m.cLanguage=IIF(LOWER(m.cLanguage)='eng','English',m.cLanguage)
		m.cLanguage=IIF(LOWER(m.cLanguage)='afr','Afrikaans',m.cLanguage)
		.LANGUAGE=ALLTRIM(m.cLanguage)
		.Ethnicity=ALLTRIM(m.cRace)
		.BankName=ALLTRIM(m.cBranchno)
		.AccountHolder=ALLTRIM(m.chldrname)
		.AccountNumber=m.cbankacct
		.AccountType=ALLTRIM(m.caccttype)
		.ActivationDate=m.dstarter
		.ACCEPT=m.lAccept=1
		.ProfileUpdated=.T.
		.WhatsappNumber=ALLTRIM(m.cPhone2)
	ENDWITH
	oProfile.SAVE()

ENDSCAN
ENDPROC


ENDDEFINE




&&------------------------------- LOAD STARTER ---------------------------------------
IF PEMSTATUS(goSettings.Common,"starterkitcategory",5) AND goSettings.Common.starterkitcategory<>0
	IF VARTYPE(SERVER.goSettings.Common.DATE)<>"D" OR EMPTY(SERVER.goSettings.Common.DATE)
		ldate=DATE()
	ELSE
		ldate=SERVER.goSettings.Common.DATE
	ENDIF
	=LOGSTRING('load starter '+loNew.odata.ccustno,"CustSync.log")
	lok=.T.
	lcSql="sp_ws_loadstarter @date='"+x8convchar(ldate,"C")+"',@cCustno='"+odata.ccustno+"'"
	=LOGSTRING(lcSql,"CustSync.log")
	IF !oAMSql.executenonquery("sp_ws_loadstarter @date='"+x8convchar(ldate,"C")+"',@cCustno='"+odata.ccustno+"'")
		=LOGSTRING("Could not load starter AM "+oAMSql.cErrormsg,"CustSync.log")
		lok=.F.
	ENDIF
	lcSql="exec sp_loadstarter @date='"+x8convchar(ldate,"C")+"',@cCustno='"+odata.ccustno+"'"
	=LOGSTRING(lcSql,"CustSync.log")
	IF lok AND !oSql.executenonquery(lcSql)
		=LOGSTRING("Could not load starter WS "+oSql.cErrormsg,"CustSync.log")
	ENDIF
ENDIF
&&-------------------------------------------------------------------------------------


ENDIF
=LOGSTRING('Update to webstore '+loNew.odata.ccustno,"CustSync.log")

IF !THIS.sync(loNew.odata.ccustno)
	=LOGSTRING('Could not create '+loNew.odata.ccustno+" "+THIS.cErrormsg,"CustSync.log")
	THIS.SetError('Could not create '+loNew.odata.ccustno+" "+THIS.cErrormsg)
	RETURN .F.
ENDIF

#ENDIF
