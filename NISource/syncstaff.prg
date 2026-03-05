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
SET PROCEDURE TO Syncstaff ADDIT
SET PROCEDURE TO SyncClass ADDIT
SET DATE YMD
CLEAR


DEFINE CLASS SyncStaff AS SyncClass
VatRate=15


PROCEDURE SyncAll

oCust=CREATEOBJECT("Customer")
oCust.SetSqlObject(oNopSql)
oXml=this.oXml
oSer=THIS.oSer

TEXT TO lcSql NOSHOW TEXTMERGE
SELECT * FROM arcust WHERE cClass IN ('Staff','EXCO') AND cStatus='A' AND cEmail<>'' 
ENDTEXT
IF oAMSql.EXECUTE(lcSql,"WSCUST")<1
	THIS.seterror(oWSBus.cErrormsg)
	RETURN .F.
ENDIF

SELECT WSCUST
SCAN 
	SCATTER MEMVAR MEMO
	m.cCustno=UPPER(ALLTRIM(m.cCustno))

	IF !oCust.LOADBASE("username='"+UPPER(m.cCustno)+"'")


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
  "company": "Annique Roobos",
  "street_address": "29 Regency Drive",
  "street_address2": "Route 21 Corporate Park",
  "zip_postal_code": "0157",
  "city": "Irene",
  "county": null,
  "country_id": 207,
  "state_province_id": 1820,

     "id": 0
}
		ENDTEXT
&& "phone": <<ALLTRIM(m.cPhone2)>>,
		luret=THIS.oNop.Customer_Create(lcJson)
		IF VARTYPE(luret)<>"O"
			=LOGSTRING("Failed  Add Staff Customer :"+oNop.cErrormsg,"Staff.Log")
			LOOP
		ENDIF
		lnCustomerID=luret.ID
		lnBillingAddressID=0
		lnShippingAddressID=0


&& Set Password
		luret=THIS.oNop.Customer_SetPassword(lnCustomerID,ALLTRIM(m.cIdno))
	ELSE
		lnCustomerID=oCust.oData.ID
		lnBillingAddressID=oCust.oData.BillingAddress_ID
		lnShippingAddressID=oCust.oData.ShippingAddress_ID
	ENDIF

&& Set Customer Mapping
	luret=THIS.oNop.Customer_AddRole(lnCustomerID,"Registered")
	luret=THIS.oNop.Customer_AddRole(lnCustomerID,"AnniqueStaff")
	TEXT TO lcJson TEXTMERGE NOSHOW
{
  "first_name": "<<oxml.EncodeXML(ALLTRIM(m.cFname))>>",
  "last_name": "<<oxml.EncodeXML(ALLTRIM(m.cLname))>>",
  "email": "<<ALLTRIM(m.cEmail)>>",
  "company": "Annique Roobos",
  "country_id": 0,
  "address1": "29 Regency Drive",
  "address2": "Route 21 Corporate Park",
  "zip_postal_code": "0157",
  "city": "Irene",
  "county": null,
  "country_id": 207,
  "state_province_id": 1820,
  "phone_number": "",
  "id": 0
}
	ENDTEXT
	lUpdateAddress=.F.
	IF ISNULLOREMPTY(lnBillingAddressID)
		luret=oNop.Address_Create(lcJson,lnCustomerID)
		IF VARTYPE(luret)<>"O"
			=LOGSTRING("Failed  Add Staff Billing:"+oNop.cErrormsg,"Staff.Log")
			lnBillingAddress=0
		ELSE
			lnBillingAddressID=luret.ID
			lUpdateAddress=.T.
		ENDIF
	ENDIF

	IF ISNULLOREMPTY(lnShippingAddressID)
		luret=oNop.Address_Create(lcJson,lnCustomerID)
		IF VARTYPE(luret)<>"O"
			=LOGSTRING("Failed  Add Staff Shipping:"+oNop.cErrormsg,"Staff.Log")
			lnShippingAddress=0
		ELSE
			lnShippingAddressID=luret.ID
			lUpdateAddress=.T.
		ENDIF
	ENDIF
	IF lUpdateAddress
		luret=THIS.oNop.Customer_Get(lnCustomerID)
		IF VARTYPE(luret)<>"O"
			=LOGSTRING("Failed  Reload Staff:"+oNop.cErrormsg,"Staff.Log")
			LOOP
		ENDIF
		luret.billing_address_id=INT(lnBillingAddressID)
		luret.shipping_address_id=INT(lnShippingAddressID)
		lcJson=oSer.Serialize(luret)
		luret=THIS.oNop.Customer_Update(lcJson)
		IF !luret
			=LOGSTRING("Failed Update Staff Address :"+oNop.cErrormsg,"Staff.Log")
			LOOP
		ENDIF

	ENDIF
	
ENDSCAN	
ENDPROC


ENDDEFINE
