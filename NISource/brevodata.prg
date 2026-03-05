
DEFINE CLASS BrevoDATA AS busBase

cDatabaseName=''
cSkipFieldsforUpdates="ID"
cPkField="ID"
laudit = .F.
auditexclude = "lastuser,dlastupdate"
oaudit = .NULL.
linternalerror = .F.
*lvalidateonsave = .T.


ENDDEFINE

&&---------------------------------------------------------------
&& Campaign
&&---------------------------------------------------------------
DEFINE CLASS BrevoCampaign AS BrevoData
	cpkfield = "ID"
	calias = "BrevoCampaign"
	cfilename = "BrevoCampaign"
	ckeyfield = "ID"
	nDataMode = 2
	lCompareUpdates = .t.
	Name = "BrevoCampaign"

	
ENDDEFINE

&&---------------------------------------------------------------
&& Campaign Detail
&&---------------------------------------------------------------
DEFINE CLASS BrevoDetail AS BrevoData
	cpkfield = "ID"
	calias = "BrevoDetail"
	cfilename = "BrevoDetail"
	ckeyfield = "ID"
	nDataMode = 2
	lCompareUpdates = .t.
	Name = "BrevoDetail"

ENDDEFINE


&&---------------------------------------------------------------
&& Campaign Log
&&---------------------------------------------------------------
DEFINE CLASS BrevoLog AS BrevoData
	cpkfield = "ID"
	calias = "BrevoLog"
	cfilename = "BrevoLog"
	ckeyfield = "ID"
	nDataMode = 2
	Name = "BrevoLog"

	
ENDDEFINE

