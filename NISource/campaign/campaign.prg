LOCAL CRLF
CRLF = CHR(13) + CHR(10)

 pcPageTitle = "Campaign / Product Exposure Summary" 

 IF (!wwScriptIsLayout)
    wwScriptIsLayout = .T.
    wwScriptContentPage = ""
wwScriptSections.Add("headers",[] + CRLF + ;
 [] +  CRLF +;
 [] +  CRLF +;
 [] +  CRLF +;
 [<style>] +  CRLF +;
 [	 .sr-only {] +  CRLF +;
 [		  display: none !important] +  CRLF +;
 [	 }] +  CRLF +;
 [] +  CRLF +;
 [	 .input-group-text {] +  CRLF +;
 [		  border-radius: 0 !important;] +  CRLF +;
 [		  display: block;] +  CRLF +;
 [		  text-align: right;] +  CRLF +;
 [		  width: 100%;] +  CRLF +;
 [	 }] +  CRLF +;
 [] +  CRLF +;
 [	 .input-group-prepend {] +  CRLF +;
 [		  width: 25%;] +  CRLF +;
 [	 }] +  CRLF +;
 [] +  CRLF +;
 [	 .input-group-append > .btn {] +  CRLF +;
 [		  border-radius: 0 !important;] +  CRLF +;
 [	 }] +  CRLF +;
 [] +  CRLF +;
 [	 .input-group .form-control {] +  CRLF +;
 [		  border-radius: 0 !important;] +  CRLF +;
 [	 }] +  CRLF +;
 [] +  CRLF +;
 [	 .text-right {] +  CRLF +;
 [		  text-align: right;] +  CRLF +;
 [	 }] +  CRLF +;
 [] +  CRLF +;
 [	 .text-small {] +  CRLF +;
 [		  font-size: 0.9em;] +  CRLF +;
 [		  font-weight: 500;] +  CRLF +;
 [	 }] +  CRLF +;
 [] +  CRLF +;
 [	 .text-vsmall {] +  CRLF +;
 [		  font-size: 0.7em;] +  CRLF +;
 [		  font-weight: 500;] +  CRLF +;
 [	 }] +  CRLF +;
 [] +  CRLF +;
 [	 [v-cloak] + ']' + [ {] +  CRLF +;
 [		  display: none;] +  CRLF +;
 [	 }] +  CRLF +;
 [] +  CRLF +;
 [	 .b-table-sticky-header {] +  CRLF +;
 [		  max-height: 90vh;] +  CRLF +;
 [	 }] +  CRLF +;
 [] +  CRLF +;
 [	 .table-sm {] +  CRLF +;
 [		  font-size: 0.9em !important;] +  CRLF +;
 [		  padding: 0;] +  CRLF +;
 [		  font-weight: 500 !important;] +  CRLF +;
 [		  background-color: whitesmoke; /* #F3C7BA !important;*/] +  CRLF +;
 [	 }] +  CRLF +;
 [] +  CRLF +;
 [	 .table-vsm {] +  CRLF +;
 [		  font-size: 0.75em !important;] +  CRLF +;
 [		  padding: 0;] +  CRLF +;
 [		  font-weight: 300 !important;] +  CRLF +;
 [		  background-color: whitesmoke; /* #F3C7BA !important;*/] +  CRLF +;
 [	 }] +  CRLF +;
 [] +  CRLF +;
 [	 .ann_green {] +  CRLF +;
 [		  background-color: #CFE189 !important;] +  CRLF +;
 [	 }] +  CRLF +;
 [] +  CRLF +;
 [	 .ann_pink {] +  CRLF +;
 [		  background-color: #F3C7BA !important;] +  CRLF +;
 [	 }] +  CRLF +;
 [] +  CRLF +;
 [	 .nav-pills .nav-link.active, .nav-pills .show > .nav-link {] +  CRLF +;
 [		  color: white;] +  CRLF +;
 [		  background-color: #D84519;] +  CRLF +;
 [	 }] +  CRLF +;
 [] +  CRLF +;
 [	 .nav-link {] +  CRLF +;
 [		  color: #D84519;] +  CRLF +;
 [	 }] +  CRLF +;
 [] +  CRLF +;
 [	 .form-control {] +  CRLF +;
 [		  padding: 2px;] +  CRLF +;
 [		  height: auto;] +  CRLF +;
 [	 }] +  CRLF +;
 [] +  CRLF +;
 [	 .footer {] +  CRLF +;
 [		  margin: 0;] +  CRLF +;
 [	 }] +  CRLF +;
 [] +  CRLF +;
 [	 ul {] +  CRLF +;
 [		  list-style-type: none;] +  CRLF +;
 [	 }] +  CRLF +;
 [] +  CRLF +;
 [	 .col-10perc {] +  CRLF +;
 [		  width:10%;] +  CRLF +;
 [	 }] +  CRLF +;
 [</style>] +  CRLF +;
 [] +  CRLF +;
 [])
wwScriptSections.Add("scripts",[] + CRLF + ;
 [] +  CRLF +;
 [<script>] +  CRLF +;
 [] +  CRLF +;
 [</script>] +  CRLF +;
 [<!--<script src="scripts/lookupsvue.js"></script>-->] +  CRLF +;
 [<script src="scripts/campaign.js?v=1"></script>] +  CRLF +;
 [] +  CRLF +;
 [])
    wwScript.RenderAspScript("~/views/_layoutpageVue.wcs")
    RETURN
ENDIF 
Response.Write([]+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   [<div id="app" v-cloak>]+ CRLF +;
   []+ CRLF +;
   [	 <div class="d-flex justify-content-center mb-3" v-if="isBusy">]+ CRLF +;
   [		  <b-spinner label="Loading..."></b-spinner>]+ CRLF +;
   [	 </div>]+ CRLF +;
   [	 <div class="alert alert-warning danger-color" v-if="updateErrors.length>0">] + CRLF )
Response.Write([		  <ul>]+ CRLF +;
   [				<li v-for="error in updateErrors">{{ error }}</li>]+ CRLF +;
   [		  </ul>]+ CRLF +;
   [	 </div>]+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   [	 <b-container fluid class="m-0 p-0">]+ CRLF +;
   [		  <!--<b-row class="mb-2 bg-secondary">]+ CRLF +;
   [		  <b-col class="p-2">]+ CRLF +;
   [				<b-btn href="default.ann" class="mr-auto" >Home</b-btn>] + CRLF )
Response.Write([		  </b-col>]+ CRLF +;
   []+ CRLF +;
   [])

 if (Process.lIsAuthenticated) 
Response.Write([]+ CRLF +;
   [					 <div class="ml-auto">]+ CRLF +;
   [						  <a href="logout.ann" style="color: #C86154">]+ CRLF +;
   [								<i class="fa fa-unlock"></i>]+ CRLF +;
   [								Sign out]+ CRLF +;
   [						  </a>]+ CRLF +;
   [					 </div>]+ CRLF +;
   [])

 endif 
Response.Write([]+ CRLF +;
   []+ CRLF +;
   [	 </b-row>-->]+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   [		  <div>]+ CRLF +;
   [				<b-navbar toggleable="lg" type="dark" variant="dark">]+ CRLF +;
   [					 <b-navbar-brand href="default.ann">Home</b-navbar-brand>]+ CRLF +;
   []+ CRLF +;
   [] + CRLF )
Response.Write([						  <!-- Right aligned nav items -->]+ CRLF +;
   [						  <b-navbar-nav class="ml-auto">]+ CRLF +;
   []+ CRLF +;
   [								<b-nav-item-dropdown right>]+ CRLF +;
   [									 <!-- Using 'button-content' slot -->]+ CRLF +;
   [									 <template #button-content>]+ CRLF +;
   [										  <em> ])

Response.Write(TRANSFORM( EVALUATE([ Process.cAuthenticatedName ]) ))

Response.Write([</em>]+ CRLF +;
   [									 </template>]+ CRLF +;
   []+ CRLF +;
   [									 <b-dropdown-item href="logout.ann" style="color: #C86154">]+ CRLF +;
   [												<i class="fa fa-unlock"></i>]+ CRLF +;
   [												Sign out]+ CRLF +;
   [									 </b-dropdown-item>]+ CRLF +;
   [								</b-nav-item-dropdown>]+ CRLF +;
   [						  </b-navbar-nav>]+ CRLF +;
   [] + CRLF )
Response.Write([				</b-navbar>]+ CRLF +;
   [		  </div>]+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   [	 </b-container>]+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   [						  <b-tabs pills card class="ann_green" v-model="tabIndex">]+ CRLF +;
   [								<b-tab title="Campaign Summary" active>] + CRLF )
Response.Write([]+ CRLF +;
   [									 <b-row class="m-2 w-100">]+ CRLF +;
   [										  <!---->]+ CRLF +;
   [									 <b-table hover :items="odata.campaigns" :fields="linefields"]+ CRLF +;
   [												 class="table-vsm" ref="table" v-model="visibleRows"]+ CRLF +;
   [												 sticky-header id="my-table"]+ CRLF +;
   [												 bordered striped]+ CRLF +;
   [												 show-empty @row-clicked="camp_onRowSelected"]+ CRLF +;
   [												 :tbody-tr-class="campRowClass"]+ CRLF +;
   [												 small>] + CRLF )
Response.Write([]+ CRLF +;
   [										  <template #cell(month)="row">]+ CRLF +;
   [												{{ row.item.iyear+"-"+row.item.cmonthname.substr(0, 3) }}]+ CRLF +;
   [										  </template>]+ CRLF +;
   []+ CRLF +;
   [										  <template #cell(nactualgpp)="row">]+ CRLF +;
   [												<div v-if="(  row.item.cstatus=='C' || row.item.cstatus=='H')">]+ CRLF +;
   [													 {{ gppactual(row.item.nactualsales,row.item.nactualcost,row.item.nactualdisc,row.item.nactualmlm) }}]+ CRLF +;
   [												</div>]+ CRLF +;
   [] + CRLF )
Response.Write([												<div v-if="( !( row.item.cstatus=='C' || row.item.cstatus=='H'))">]+ CRLF +;
   [													 {{ gppactual(totalTarget(row.item),totalCost(row.item),totalDisc(row.item),0) }}]+ CRLF +;
   [												</div>]+ CRLF +;
   []+ CRLF +;
   [										  </template>]+ CRLF +;
   []+ CRLF +;
   [										  <!-- A custom formatted header cell for field 'name' -->]+ CRLF +;
   [										  <template #thead-top="data">]+ CRLF +;
   [												<b-tr>]+ CRLF +;
   [													 <b-th></b-th>] + CRLF )
Response.Write([													 <b-th colspan="4" class="text-center">Targets</b-th>]+ CRLF +;
   [													 <b-th colspan="6" class="text-center">Category Split %</b-th>]+ CRLF +;
   [													 <b-th colspan="4" class="text-center">Spon Split %</b-th>]+ CRLF +;
   [													 <b-th colspan="2" class="text-center">Actuals (Plan)</b-th>]+ CRLF +;
   [												</b-tr>]+ CRLF +;
   [										  </template>]+ CRLF +;
   []+ CRLF +;
   [										  <template #cell(actions)="row">]+ CRLF +;
   [												<b-button size="sm" @click="camp_edit(row.item, row.index, $event.target)"]+ CRLF +;
   [															 class="p-1 m-1" variant="primary"] + CRLF )
Response.Write([															 v-if="(]+ CRLF +;
   [													(settings.edit==true  ] + '&' + '&' + []+ CRLF +;
   [											  (row.item.cstatus=='' || row.item.cstatus=='G'))]+ CRLF +;
   [												|| (settings.editinlive==true ] + '&' + '&' + [ row.item.cstatus =='C')]+ CRLF +;
   [												|| (settings.editinfrozen==true ] + '&' + '&' + [ row.item.cstatus =='F')]+ CRLF +;
   [												)">]+ CRLF +;
   [													 <i class="fa fa-edit"></i>]+ CRLF +;
   [												</b-button>]+ CRLF +;
   [												{{ settings.editinlive}}{{row.item.cstatus}}]+ CRLF +;
   [												<b-button size="sm" @click="row.toggleDetails"] + CRLF )
Response.Write([															 class="p-1 m-1" variant="success">]+ CRLF +;
   [													 <i class="fa fa-bars"></i>]+ CRLF +;
   [													 <!--{{ row.detailsShowing ? 'Hide' : '' }}-->]+ CRLF +;
   [												</b-button>]+ CRLF +;
   []+ CRLF +;
   [										  </template>]+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   [										  <template #row-details="row">] + CRLF )
Response.Write([]+ CRLF +;
   [												<b-table hover :items="row.item.odash"]+ CRLF +;
   [															:fields="dashfields" class="table-sm"]+ CRLF +;
   [															:tbody-tr-class="dashRowClass"]+ CRLF +;
   [															show-empty]+ CRLF +;
   [															bordered]+ CRLF +;
   [															small>]+ CRLF +;
   [												</b-table>]+ CRLF +;
   [										  </template>]+ CRLF +;
   [] + CRLF )
Response.Write([]+ CRLF +;
   [									 </b-table>]+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   [		  </b-row>]+ CRLF +;
   [		  </b-tab>]+ CRLF +;
   [		  <b-tab title="Categories">]+ CRLF +;
   [				<!--@click="getSkus('SKINCARE',odata.dfrom,odata.dto)">[{ text:'Select Category', value: ''},]+ CRLF +;
   [									 'Skincare','Bodycare','Lifestyle','Cosmetics',]+ CRLF +;
   [										  'Fine Fragr','Other'] + ']' + [-->] + CRLF )
Response.Write([				<b-container fluid>]+ CRLF +;
   [					 <b-form inline class="mb-1">]+ CRLF +;
   [						  <b-form-row>]+ CRLF +;
   [								<b-col>]+ CRLF +;
   [									 <b-form-select size="sm" v-model="category" id="category-selection"]+ CRLF +;
   [														 plain class="mr2-1 form-control"]+ CRLF +;
   [														 :options="settings.categories"]+ CRLF +;
   [														 :value="null">]+ CRLF +;
   []+ CRLF +;
   [									 </b-form-select>] + CRLF )
Response.Write([								</b-col>]+ CRLF +;
   [								<b-col>]+ CRLF +;
   [									 <label for="daterange">Date Range</label>]+ CRLF +;
   [								</b-col>]+ CRLF +;
   [								<b-col>]+ CRLF +;
   [									 <b-input type="date" size="sm"]+ CRLF +;
   [												 v-model="dfrom" name="daterange"]+ CRLF +;
   [												 locale="en-gb">]+ CRLF +;
   [									 </b-input>]+ CRLF +;
   [								</b-col>-] + CRLF )
Response.Write([								<b-col>]+ CRLF +;
   [									 <b-input type="date" size="sm"]+ CRLF +;
   [												 v-model="dto"]+ CRLF +;
   [												 locale="en-gb">]+ CRLF +;
   [									 </b-input>]+ CRLF +;
   [								</b-col>]+ CRLF +;
   [								<b-col>]+ CRLF +;
   [									 <b-button size="sm" class="text-small"]+ CRLF +;
   [												  @click="getSkus(category,dfrom,dto)">Get Campaigns</b-button>]+ CRLF +;
   [								</b-col>] + CRLF )
Response.Write([						  </b-form-row>]+ CRLF +;
   [					 </b-form>]+ CRLF +;
   []+ CRLF +;
   [					 <b-form-row class="my-1" v-if="odata.skus.length>0">]+ CRLF +;
   [						  <b-col sm="2">]+ CRLF +;
   [								<b-form-group class="mb-0">]+ CRLF +;
   [									 <b-input-group size="sm">]+ CRLF +;
   [										  <b-form-input id="filter-input"]+ CRLF +;
   [															 v-model="filter"]+ CRLF +;
   [															 type="search"] + CRLF )
Response.Write([															 placeholder="Filter"></b-form-input>]+ CRLF +;
   []+ CRLF +;
   [										  <b-input-group-append>]+ CRLF +;
   [												<b-button :disabled="!filter" @click="filter = ''">Clear</b-button>]+ CRLF +;
   [										  </b-input-group-append>]+ CRLF +;
   [									 </b-input-group>]+ CRLF +;
   [								</b-form-group>]+ CRLF +;
   [						  </b-col>]+ CRLF +;
   [					 </b-form-row>]+ CRLF +;
   [				</b-container>] + CRLF )
Response.Write([				<b-row>]+ CRLF +;
   [					 <b-table hover :items="odata.skus"]+ CRLF +;
   [								 :fields="skufields"]+ CRLF +;
   [								 v-model="visibleRows" class="table-vsm"]+ CRLF +;
   [								 sticky-header]+ CRLF +;
   [								 bordered striped]+ CRLF +;
   [								 show-empty]+ CRLF +;
   [								 small @row-clicked="detail_edit"]+ CRLF +;
   [								 :filter="filter"]+ CRLF +;
   [								 :filter-included-fields="filterOn">] + CRLF )
Response.Write([]+ CRLF +;
   [						  <!-- A virtual composite column -->]+ CRLF +;
   [						  <template #cell(ntarget)="data">]+ CRLF +;
   [								{{ Math.round(data.item.nforecast * data.item.nprice) }}]+ CRLF +;
   [						  </template>]+ CRLF +;
   []+ CRLF +;
   [						  <template #cell(actions)="row">]+ CRLF +;
   [								<b-button size="sm" @click="item_removeRowHandler(row)"]+ CRLF +;
   [											 variant="danger" class="ml-auto"]+ CRLF +;
   [											 v-if="!settings.viewonly ] + '&' + '&' + [ settings.additems ] + '&' + '&' + [ (] + CRLF )
Response.Write([									 ( row.item.cstatus=='')]+ CRLF +;
   [									 || (settings.editinfrozen ] + '&' + '&' + [ row.item.cstatus =='F')]+ CRLF +;
   [									 )">]+ CRLF +;
   []+ CRLF +;
   [									 <i class="fa fa-trash"></i>]+ CRLF +;
   [								</b-button>]+ CRLF +;
   [								<b-button size="sm" @click="row.toggleDetails" class="text-small"]+ CRLF +;
   [											 v-if="settings.viewonly || !settings.additems">]+ CRLF +;
   [									 {{ row.detailsShowing ? 'Hide' : 'Details' }}]+ CRLF +;
   [								</b-button>] + CRLF )
Response.Write([]+ CRLF +;
   []+ CRLF +;
   [						  </template>]+ CRLF +;
   []+ CRLF +;
   [						  <template #cell(usedin)="row">]+ CRLF +;
   []+ CRLF +;
   [								<ul class="text-vsmall p-0">]+ CRLF +;
   []+ CRLF +;
   [									 <li class="text-vsmall p-0" v-for="value in row.item.ousedin">]+ CRLF +;
   [										  <b-button v-b-tooltip.hover.left] + CRLF )
Response.Write([														size="sm" @click="jumpto(value.campskuid)"]+ CRLF +;
   [														variant="outline-info" class="p-0 m-0">]+ CRLF +;
   [												{{ value.ckitcode }}]+ CRLF +;
   [										  </b-button>]+ CRLF +;
   [									 </li>]+ CRLF +;
   []+ CRLF +;
   [								</ul>]+ CRLF +;
   []+ CRLF +;
   [						  </template>]+ CRLF +;
   [] + CRLF )
Response.Write([						  <template #cell(lkititem)="row">]+ CRLF +;
   []+ CRLF +;
   [								<ul class="text-vsmall p-0" v-if="row.item.lkititem">]+ CRLF +;
   [									 <li class="text-vsmall p-0" v-for="value in row.item.okit">]+ CRLF +;
   [										  {{ value.citemno }}-{{ value.compdescript }}]+ CRLF +;
   [									 </li>]+ CRLF +;
   [								</ul>]+ CRLF +;
   [						  </template>]+ CRLF +;
   []+ CRLF +;
   [						  <template #cell(ngp)="row">] + CRLF )
Response.Write([								{{ gpp(row.item.nprice,row.item.ncost,row.item.ndrate,row.item.nmlmrate) }}]+ CRLF +;
   [						  </template>]+ CRLF +;
   []+ CRLF +;
   [						  <template #head(actions)>]+ CRLF +;
   [								<b-button v-b-tooltip.hover.left title="Add a new item to Campaigns"]+ CRLF +;
   [											 size="sm" @click="item_addsku()"]+ CRLF +;
   [											 variant="success" class="ml-auto"]+ CRLF +;
   [											 v-if="!settings.viewonly ] + '&' + '&' + [ settings.additems">]+ CRLF +;
   [									 <i class="fa fa-plus-circle"></i>]+ CRLF +;
   [								</b-button>] + CRLF )
Response.Write([						  </template>]+ CRLF +;
   []+ CRLF +;
   [						  <template #row-details="row">]+ CRLF +;
   []+ CRLF +;
   [								<!--<ul>]+ CRLF +;
   [				<li v-for="(value, key) in row.item.odetail" :key="key">{{ key }}: {{ value }}</li>]+ CRLF +;
   [		  </ul>-->]+ CRLF +;
   [								<b-table hover :items="row.item.odetail"]+ CRLF +;
   [											:fields="detailfieldsview" class="table-vsm"]+ CRLF +;
   [											show-empty] + CRLF )
Response.Write([											bordered]+ CRLF +;
   [											small>]+ CRLF +;
   [									 <template #cell(ngp)="row">]+ CRLF +;
   [										  {{ gpp(row.item.nprice,row.item.ncost,row.item.ndrate,row.item.nmlmrate) }}]+ CRLF +;
   [									 </template>]+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   [								</b-table>]+ CRLF +;
   []+ CRLF +;
   [						  </template>] + CRLF )
Response.Write([]+ CRLF +;
   [					 </b-table>]+ CRLF +;
   [				</b-row>]+ CRLF +;
   []+ CRLF +;
   [		  </b-tab>]+ CRLF +;
   []+ CRLF +;
   [		  <b-tab title="Offers">]+ CRLF +;
   [		  </b-tab>]+ CRLF +;
   [		  <template #tabs-end>]+ CRLF +;
   [				<b-link v-if="settings.createitem" ] + CRLF )
Response.Write([						  href="itemadd.ann" class="nav-item align-self-center nav-link"]+ CRLF +;
   [						  target="_parent">New Item</b-link>]+ CRLF +;
   [		  </template>]+ CRLF +;
   [	 </b-tabs>]+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   [	 <!-- Info modal -->]+ CRLF +;
   [	 <b-modal :id="infoModal.id" :title="infoModal.title" ok-only @hide="resetInfoModal">] + CRLF )
Response.Write([		  <label v-if="infoModal.item.lgwp">Qty Limit</label>]+ CRLF +;
   [		  <b-form-input size="sm"]+ CRLF +;
   [							 type="number" class="text-small form-control"]+ CRLF +;
   [							 :value="infoModal.item.ngqtylimit"]+ CRLF +;
   [							 v-model.number="infoModal.item.ngqtylimit">]+ CRLF +;
   [		  </b-form-input>]+ CRLF +;
   [		  <pre>{{ infoModal.content }}</pre>]+ CRLF +;
   [	 </b-modal>]+ CRLF +;
   []+ CRLF +;
   [	 <b-modal id="campaign" ok-only @ok="camp_handleOk" @close="camp_handleClose"] + CRLF )
Response.Write([				 title="Campaign" size="xl" no-close-on-backdrop no-close-on-esc]+ CRLF +;
   [				 class="ann_green">]+ CRLF +;
   [		  <b-container>]+ CRLF +;
   [				<div class="alert alert-warning danger-color" v-if="camp_updateErrors.length>0">]+ CRLF +;
   [					 <ul>]+ CRLF +;
   [						  <li v-for="error in camp_updateErrors">{{ error }}</li>]+ CRLF +;
   [					 </ul>]+ CRLF +;
   [				</div>]+ CRLF +;
   [				<form ref="form" @submit.stop.prevent="camp_handleSave">]+ CRLF +;
   [] + CRLF )
Response.Write([]+ CRLF +;
   [					 <b-row>]+ CRLF +;
   [						  <b-col sm="6">]+ CRLF +;
   [								<b-row>]+ CRLF +;
   [									 <b-col sm="5">]+ CRLF +;
   [										  <b-form-group label="Start Date" label-for="camp_dfrom">]+ CRLF +;
   [												<b-form-input id="camp_dfrom" plain]+ CRLF +;
   [																  name="camp_dfrom" type="date"]+ CRLF +;
   [																  v-model="odata.campaign.dfrom">]+ CRLF +;
   [												</b-form-input>] + CRLF )
Response.Write([										  </b-form-group>]+ CRLF +;
   [									 </b-col>]+ CRLF +;
   [									 <b-col sm="5">]+ CRLF +;
   [										  <b-form-group label="End Date" label-for="camp_dto">]+ CRLF +;
   [												<b-form-input id="camp_dto" plain]+ CRLF +;
   [																  name="camp_dtom" type="date"]+ CRLF +;
   [																  v-model="odata.campaign.dto">]+ CRLF +;
   [												</b-form-input>]+ CRLF +;
   [										  </b-form-group>]+ CRLF +;
   [									 </b-col>] + CRLF )
Response.Write([								</b-row>]+ CRLF +;
   [								<b-row>]+ CRLF +;
   [									 <b-col sm="5">]+ CRLF +;
   [										  <b-form-group label="Marketing Target" label-for="camp_ntarget">]+ CRLF +;
   [												<b-form-input id="camp_ntarget" plain]+ CRLF +;
   [																  name="camp_ntarget"]+ CRLF +;
   [																  v-model.number="odata.campaign.ntarget">]+ CRLF +;
   [												</b-form-input>]+ CRLF +;
   [										  </b-form-group>]+ CRLF +;
   [									 </b-col>] + CRLF )
Response.Write([									 <b-col sm="5">]+ CRLF +;
   [										  <b-form-group label="GP % Target" label-for="camp_ngptarget">]+ CRLF +;
   [												<b-form-input id="camp_ngptarget" plain]+ CRLF +;
   [																  name="camp_ngptarget"]+ CRLF +;
   [																  v-model.number="odata.campaign.ngptarget">]+ CRLF +;
   [												</b-form-input>]+ CRLF +;
   [										  </b-form-group>]+ CRLF +;
   [									 </b-col>]+ CRLF +;
   [								</b-row>]+ CRLF +;
   [] + CRLF )
Response.Write([						  </b-col>]+ CRLF +;
   [						  <b-col>]+ CRLF +;
   [								<b-row>]+ CRLF +;
   [									 <b-col sm="5">]+ CRLF +;
   [										  <b-form-group label="Discount %" label-for="camp_ndiscrate">]+ CRLF +;
   [												<b-form-input id="camp_ndiscrate" plain]+ CRLF +;
   [																  name="camp_ndiscrate"]+ CRLF +;
   [																  v-model.number="odata.campaign.ndiscrate">]+ CRLF +;
   [												</b-form-input>]+ CRLF +;
   [										  </b-form-group>] + CRLF )
Response.Write([									 </b-col>]+ CRLF +;
   [									 <b-col sm="5">]+ CRLF +;
   [										  <b-form-group label="MLM %" label-for="camp_nmlmrate">]+ CRLF +;
   [												<b-form-input id="camp_nmlmrate" plain]+ CRLF +;
   [																  name="camp_nmlmrate"]+ CRLF +;
   [																  v-model.number="odata.campaign.nmlmrate">]+ CRLF +;
   [												</b-form-input>]+ CRLF +;
   [										  </b-form-group>]+ CRLF +;
   [									 </b-col>]+ CRLF +;
   [								</b-row>] + CRLF )
Response.Write([								<b-form-group label="Campaign Status" label-for="status-selection" class="row">]+ CRLF +;
   [									 <b-col>]+ CRLF +;
   [										  <b-form-select size="sm" v-model="odata.campaign.cstatus" id="status-selection"]+ CRLF +;
   [															  plain class="mr2-1 form-control" :disabled="!settings.freeze"]+ CRLF +;
   [															  :options="[{ value : 'H',text : 'Historic', disabled: true},]+ CRLF +;
   [													{ value : 'C',text : 'Current' , disabled: true},]+ CRLF +;
   [													{ value : 'F',text : 'Frozen' },]+ CRLF +;
   [													{ value : '',text : 'Future' }] + ']' + [">]+ CRLF +;
   []+ CRLF +;
   [										  </b-form-select>] + CRLF )
Response.Write([									 </b-col>]+ CRLF +;
   [								</b-form-group>]+ CRLF +;
   []+ CRLF +;
   [								<b-button v-if="settings.tostage" variant="info"]+ CRLF +;
   [											 @click="camp_synctostage(odata.campaign.id)">]+ CRLF +;
   [									 Publish to Stage]+ CRLF +;
   [								</b-button>]+ CRLF +;
   [								<b-button v-if="settings.tonamiba" variant="success"]+ CRLF +;
   [											 @click="camp_synctonam(odata.campaign.id)">]+ CRLF +;
   [									 Copy to Namibia] + CRLF )
Response.Write([								</b-button>]+ CRLF +;
   [								<!--@click="camp_synctonam(odata.campaign)"-->]+ CRLF +;
   [						  </b-col>]+ CRLF +;
   [					 </b-row>]+ CRLF +;
   [					 <b-row>]+ CRLF +;
   [						  <b-col sm="12">]+ CRLF +;
   [								<b-table hover :items="odata.campaign.ocat"]+ CRLF +;
   [											:fields="catfields" class="table-vsm"]+ CRLF +;
   [											v-model="visibleRowsCat"]+ CRLF +;
   [											show-empty] + CRLF )
Response.Write([											bordered>]+ CRLF +;
   []+ CRLF +;
   [									 <template #cell(nptargetsplit)="row">]+ CRLF +;
   [										  {{ (row.item.nptarget / totalCatPlan * 100).toFixed(2) }}]+ CRLF +;
   [									 </template>]+ CRLF +;
   [									 <template #cell(npvariance)="row">]+ CRLF +;
   [										  {{ ((row.item.ntarget - row.item.nptarget)/row.item.nptarget  * 100).toFixed(2) }}]+ CRLF +;
   [									 </template>]+ CRLF +;
   [									 <template #cell(ngpp)="row">]+ CRLF +;
   [										  {{ (row.item.npgp).toFixed(2) }}] + CRLF )
Response.Write([									 </template>]+ CRLF +;
   [									 <template #cell(ngppvariance)="row">]+ CRLF +;
   [										  {{ ((row.item.npgp)-row.item.ngptarget).toFixed(2) }}]+ CRLF +;
   [									 </template>]+ CRLF +;
   [									 <template #cell(ntargetsplit)="row">]+ CRLF +;
   [										  <b-input-group>]+ CRLF +;
   [												<b-form-input sm="3" class="text-small"]+ CRLF +;
   [																  type="number" plain]+ CRLF +;
   [																  :value="row.item.ntargetsplit"]+ CRLF +;
   [																  v-model.number="row.item.ntargetsplit"] + CRLF )
Response.Write([																  @change="changeCatForecast">]+ CRLF +;
   [												</b-form-input>]+ CRLF +;
   [										  </b-input-group>]+ CRLF +;
   [									 </template>]+ CRLF +;
   []+ CRLF +;
   [									 <template #cell(ngptarget)="row">]+ CRLF +;
   [										  <b-input-group>]+ CRLF +;
   [												<b-form-input sm="5" class="text-small"]+ CRLF +;
   [																  type="number" plain]+ CRLF +;
   [																  :value="row.item.ngptarget"] + CRLF )
Response.Write([																  v-model.number="row.item.ngptarget">]+ CRLF +;
   [												</b-form-input>]+ CRLF +;
   [										  </b-input-group>]+ CRLF +;
   [									 </template>]+ CRLF +;
   []+ CRLF +;
   [									 <template slot="bottom-row">]+ CRLF +;
   [										  <td>Total</td>]+ CRLF +;
   [										  <td>]+ CRLF +;
   [												{{ totalCat.toFixed(2) }}]+ CRLF +;
   [												<div v-if="totalCat!=100">({{ (100-totalCat).toFixed(2) }})</div>] + CRLF )
Response.Write([										  </td>]+ CRLF +;
   [										  <td class="text-right"> {{ formatter_com(odata.campaign.ntarget) }}</td>]+ CRLF +;
   [										  <td class="text-right"> {{ formatter_com(totalCatPlan) }}</td>]+ CRLF +;
   [									 </template>]+ CRLF +;
   [								</b-table>]+ CRLF +;
   [						  </b-col>]+ CRLF +;
   [						  <b-col sm="12">]+ CRLF +;
   [								<b-table hover :items="odata.campaign.ospon"]+ CRLF +;
   [											:fields="sponfields" class="table-vsm"]+ CRLF +;
   [											v-model="visibleRowsSpon"] + CRLF )
Response.Write([											show-empty responsive]+ CRLF +;
   [											bordered>]+ CRLF +;
   [									 <template #cell(ntargetsplit)="row">]+ CRLF +;
   [										  <b-input-group>]+ CRLF +;
   [												<b-form-input size="sm" class="text-small"]+ CRLF +;
   [																  type="number"]+ CRLF +;
   [																  :value="row.item.ntargetsplit"]+ CRLF +;
   [																  v-model.number="row.item.ntargetsplit"]+ CRLF +;
   [																  @change="changeSponForecast">]+ CRLF +;
   [												</b-form-input>] + CRLF )
Response.Write([										  </b-input-group>]+ CRLF +;
   [									 </template>]+ CRLF +;
   [									 <template #cell(ngptarget)="row">]+ CRLF +;
   [										  <b-input-group>]+ CRLF +;
   [												<b-form-input size="sm" class="text-small"]+ CRLF +;
   [																  type="number"]+ CRLF +;
   [																  :value="row.item.ngptarget"]+ CRLF +;
   [																  v-model.number="row.item.ngptarget">]+ CRLF +;
   [												</b-form-input>]+ CRLF +;
   [										  </b-input-group>] + CRLF )
Response.Write([									 </template>]+ CRLF +;
   []+ CRLF +;
   [									 <template #cell(nptargetsplit)="row">]+ CRLF +;
   [										  {{ (row.item.nptarget / totalCatPlan * 100).toFixed(2) }}]+ CRLF +;
   [									 </template>]+ CRLF +;
   [									 <template #cell(npvariance)="row">]+ CRLF +;
   [										  {{ ((row.item.ntarget - row.item.nptarget)/row.item.nptarget  * 100).toFixed(2) }}]+ CRLF +;
   [									 </template>]+ CRLF +;
   [									 <template #cell(ngpp)="row">]+ CRLF +;
   [										  {{ (row.item.npgp).toFixed(2) }}] + CRLF )
Response.Write([									 </template>]+ CRLF +;
   [									 <template #cell(ngppvariance)="row">]+ CRLF +;
   [										  {{ (row.item.npgp).toFixed(2) }}]+ CRLF +;
   [									 </template>]+ CRLF +;
   []+ CRLF +;
   [									 <template slot="bottom-row">]+ CRLF +;
   [										  <td>Total</td>]+ CRLF +;
   [										  <td>]+ CRLF +;
   [												{{ totalSpon.toFixed(2) }}]+ CRLF +;
   [												<div v-if="totalSpon!=100">({{ (100-totalSpon).toFixed(2) }})</div>] + CRLF )
Response.Write([										  </td>]+ CRLF +;
   [										  <td class="text-right"> {{ formatter_com(odata.campaign.ntarget) }}</td>]+ CRLF +;
   [										  <td class="text-right"> {{ formatter_com(totalCatPlan) }}</td>]+ CRLF +;
   [									 </template>]+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   [								</b-table>]+ CRLF +;
   [						  </b-col>]+ CRLF +;
   [						  <b-col sm="12">]+ CRLF +;
   [								<b-table hover :items="odata.campaign.obrand"] + CRLF )
Response.Write([											:fields="brandfields" class="table-vsm"]+ CRLF +;
   [											v-model="visibleRowsBrand" caption="Rewards Budget"]+ CRLF +;
   [											show-empty responsive]+ CRLF +;
   [											bordered>]+ CRLF +;
   [									 <!--caption-top="true"-->]+ CRLF +;
   [									 <template #head(actions)>]+ CRLF +;
   [										  <b-button size="sm" @click="brand_addRowHandler()"]+ CRLF +;
   [														variant="success" class="ml-auto">]+ CRLF +;
   [												<i class="fa fa-plus-circle"></i>]+ CRLF +;
   [										  </b-button>] + CRLF )
Response.Write([									 </template>]+ CRLF +;
   []+ CRLF +;
   [									 <template #cell(actions)="row">]+ CRLF +;
   [										  <b-button size="sm" @click="brand_removeRowHandler(row.index)"]+ CRLF +;
   [														variant="danger" class="ml-auto">]+ CRLF +;
   [												<i class="fa fa-trash"></i>]+ CRLF +;
   [										  </b-button>]+ CRLF +;
   [									 </template>]+ CRLF +;
   []+ CRLF +;
   [] + CRLF )
Response.Write([									 <template #cell(category)="row">]+ CRLF +;
   [										  <b-form-select size="sm" v-model="row.item.category"]+ CRLF +;
   [															  plain class="text-small form-control"]+ CRLF +;
   [															  :options="settings.categories">]+ CRLF +;
   []+ CRLF +;
   [										  </b-form-select>]+ CRLF +;
   [									 </template>]+ CRLF +;
   []+ CRLF +;
   [									 <template #cell(cclass)="row">]+ CRLF +;
   [										  <b-form-select size="sm" v-model="row.item.cclass"] + CRLF )
Response.Write([															  plain class="text-small form-control"]+ CRLF +;
   [															  :options="itemlookups.brands">]+ CRLF +;
   []+ CRLF +;
   [										  </b-form-select>]+ CRLF +;
   [									 </template>]+ CRLF +;
   []+ CRLF +;
   [									 <template #cell(nrrbudget)="row">]+ CRLF +;
   [										  <b-form-input type="number" class="text-small form-control"]+ CRLF +;
   [															 :value="row.item.nrrbudget" size="sm"]+ CRLF +;
   [															 v-model.number="row.item.nrrbudget">] + CRLF )
Response.Write([										  </b-form-input>]+ CRLF +;
   [									 </template>]+ CRLF +;
   []+ CRLF +;
   [								</b-table>]+ CRLF +;
   [					 </b-row>]+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   [				</form>]+ CRLF +;
   [		  </b-container>] + CRLF )
Response.Write([	 </b-modal>]+ CRLF +;
   []+ CRLF +;
   [	 <b-modal id="detail" ok-only @ok="detail_handleOk" @close="detail_handleClose"]+ CRLF +;
   [				 v-bind:title="codedesc"]+ CRLF +;
   [				 size="xl" no-close-on-backdrop no-close-on-esc>]+ CRLF +;
   [		  <b-container>]+ CRLF +;
   [				<div class="alert alert-warning danger-color" v-if="detail_updateErrors.length>0">]+ CRLF +;
   [					 <ul>]+ CRLF +;
   [						  <li v-for="error in detail_updateErrors">{{ error }}</li>]+ CRLF +;
   [					 </ul>] + CRLF )
Response.Write([				</div>]+ CRLF +;
   [				<!--<form ref="detailform" @submit.stop.prevent="detail_handleSave">-->]+ CRLF +;
   []+ CRLF +;
   [				<b-row class="mb-0  ann_green">]+ CRLF +;
   [					 <b-col sm="2">]+ CRLF +;
   [						  Forecast:{{ odata.sku.nforecast}}]+ CRLF +;
   [					 </b-col>]+ CRLF +;
   [					 <b-col sm="3">]+ CRLF +;
   [						  Avg Price Ex:{{ odata.sku.nprice}}]+ CRLF +;
   [					 </b-col>] + CRLF )
Response.Write([					 <b-col sm="2">]+ CRLF +;
   [						  Avg RSP {{ formatter_2dec(odata.sku.nrsp) }}]+ CRLF +;
   [					 </b-col>]+ CRLF +;
   [					 <b-col sm="2">]+ CRLF +;
   [						  Cost:{{ odata.sku.ncost}}]+ CRLF +;
   [					 </b-col>]+ CRLF +;
   [					 <b-col sm="2">]+ CRLF +;
   [						  GP % {{]+ CRLF +;
   [ odata.sku.ncost > 0 ?]+ CRLF +;
   [						  gpp(odata.sku.nprice,odata.sku.ncost,odata.sku.ndrate,odata.sku.nmlmrate)] + CRLF )
Response.Write([							: "**"]+ CRLF +;
   [						  }}]+ CRLF +;
   [						  <!--(((odata.sku.nprice-odata.sku.ncost)/odata.sku.nprice)*100).toFixed(2)-->]+ CRLF +;
   [					 </b-col>]+ CRLF +;
   [				</b-row>]+ CRLF +;
   [				<b-row class="mb-1  ann_green">]+ CRLF +;
   []+ CRLF +;
   [					 <b-col sm="2">]+ CRLF +;
   [						  DRP Tot:  {{ totaldrp }}]+ CRLF +;
   [					 </b-col>] + CRLF )
Response.Write([					 <b-col sm="3">]+ CRLF +;
   [						  <b-form-checkbox v-model="odata.sku.lonstore">On Store</b-form-checkbox>]+ CRLF +;
   [					 </b-col>]+ CRLF +;
   [					 <b-col sm="3">]+ CRLF +;
   [						  <b-form-checkbox v-model="odata.sku.lcanbuy">Can Buy</b-form-checkbox>]+ CRLF +;
   [					 </b-col>]+ CRLF +;
   [					 <b-col sm="4">]+ CRLF +;
   [						  Disc %]+ CRLF +;
   [						  <b-form-select v-model.number="odata.sku.ndiscrate" class="col-3" size="sm">]+ CRLF +;
   [								<b-form-select-option value=20>20%</b-form-select-option>] + CRLF )
Response.Write([								<b-form-select-option value=15>15%</b-form-select-option>]+ CRLF +;
   [								<b-form-select-option value=10>10%</b-form-select-option>]+ CRLF +;
   [								<b-form-select-option value=5>5%</b-form-select-option>]+ CRLF +;
   [								<b-form-select-option value=1>1%</b-form-select-option>]+ CRLF +;
   [								<b-form-select-option value=0>0%</b-form-select-option>]+ CRLF +;
   [						  </b-form-select>]+ CRLF +;
   [				</b-col>]+ CRLF +;
   [				</b-row>]+ CRLF +;
   []+ CRLF +;
   [				<b-table hover :items="odata.sku.odetail"] + CRLF )
Response.Write([							:fields="detailfields" class="table-vsm"]+ CRLF +;
   [							v-model="visibleRowsDetail"]+ CRLF +;
   [							show-empty]+ CRLF +;
   [							bordered]+ CRLF +;
   [							small>]+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   [					 <template #head(actions)>]+ CRLF +;
   [						  <b-button size="sm" @click="detail_addRowHandler()"] + CRLF )
Response.Write([										variant="success" class="ml-auto">]+ CRLF +;
   [								<i class="fa fa-plus-circle"></i>]+ CRLF +;
   [						  </b-button>]+ CRLF +;
   [					 </template>]+ CRLF +;
   []+ CRLF +;
   [					 <template #cell(actions)="row">]+ CRLF +;
   [						  <b-button size="sm" @click="detail_removeRowHandler(row)"]+ CRLF +;
   [										variant="danger" class="ml-auto" v-if="odata.sku.odetail.length>1">]+ CRLF +;
   [								<i class="fa fa-trash"></i>]+ CRLF +;
   [						  </b-button>] + CRLF )
Response.Write([					 </template>]+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   [					 <template #cell(cspontype)="row">]+ CRLF +;
   []+ CRLF +;
   [						  <b-form-select size="sm" v-model="row.item.cspontype"]+ CRLF +;
   [											  plain class="text-small form-control"]+ CRLF +;
   [											  :options="spontypes"]+ CRLF +;
   [											  @change="detail_updatesponcat(row.item)">]+ CRLF +;
   [] + CRLF )
Response.Write([						  </b-form-select>]+ CRLF +;
   []+ CRLF +;
   [					 </template>]+ CRLF +;
   []+ CRLF +;
   [					 <template #cell(dfrom)="row">]+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   [						  <b-input type="date" :value="row.item.dfrom" size="sm"]+ CRLF +;
   [									  v-model="row.item.dfrom" class="text-small form-control"]+ CRLF +;
   [									  locale="en-gb">] + CRLF )
Response.Write([						  </b-input>]+ CRLF +;
   []+ CRLF +;
   [					 </template>]+ CRLF +;
   []+ CRLF +;
   [					 <template #cell(dto)="row">]+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   [						  <b-input type="date" :value="row.item.dto" size="sm"]+ CRLF +;
   [									  v-model="row.item.dto"]+ CRLF +;
   [									  class="text-small form-control"] + CRLF )
Response.Write([									  locale="en-gb">]+ CRLF +;
   [						  </b-input>]+ CRLF +;
   []+ CRLF +;
   [					 </template>]+ CRLF +;
   [					 <template #cell(nforecast)="row">]+ CRLF +;
   []+ CRLF +;
   [						  <b-form-input size="sm" class="text-small form-control"]+ CRLF +;
   [											 type="number"]+ CRLF +;
   [											 :value="row.item.nforecast"]+ CRLF +;
   [											 v-model.number="row.item.nforecast">] + CRLF )
Response.Write([						  </b-form-input>]+ CRLF +;
   []+ CRLF +;
   [					 </template>]+ CRLF +;
   [					 <template #cell(nqtylimit)="row">]+ CRLF +;
   []+ CRLF +;
   [						  <b-form-input size="sm"]+ CRLF +;
   [											 type="number" class="text-small form-control"]+ CRLF +;
   [											 :value="row.item.nqtylimit"]+ CRLF +;
   [											 v-model.number="row.item.nqtylimit">]+ CRLF +;
   [						  </b-form-input>] + CRLF )
Response.Write([]+ CRLF +;
   [					 </template>]+ CRLF +;
   [					 <template #cell(nrsp)="row">]+ CRLF +;
   []+ CRLF +;
   [						  <b-form-input type="number" class="text-small form-control"]+ CRLF +;
   [											 :value="row.item.nrsp.toFixed(2)" size="sm"]+ CRLF +;
   [											 v-model.number="row.item.nrsp">]+ CRLF +;
   [						  </b-form-input>]+ CRLF +;
   []+ CRLF +;
   [					 </template>] + CRLF )
Response.Write([					 <template #cell(coffer)="row">]+ CRLF +;
   [						  <b-form-input size="sm"]+ CRLF +;
   [											 class="text-small form-control"]+ CRLF +;
   [											 :value="row.item.coffer"]+ CRLF +;
   [											 v-model="row.item.coffer">]+ CRLF +;
   [						  </b-form-input>]+ CRLF +;
   []+ CRLF +;
   [					 </template>]+ CRLF +;
   [					 <template #cell(cpageno)="row">]+ CRLF +;
   [						  <b-form-input size="sm"] + CRLF )
Response.Write([											 class="text-small form-control"]+ CRLF +;
   [											 :value="row.item.cpageno"]+ CRLF +;
   [											 v-model="row.item.cpageno">]+ CRLF +;
   [						  </b-form-input>]+ CRLF +;
   []+ CRLF +;
   [					 </template>]+ CRLF +;
   []+ CRLF +;
   [					 <template #cell(ngp)="row">]+ CRLF +;
   [						  <b-form-input :value="gpp(row.item.nrsp/1.15,row.item.ncost,row.item.ndrate,row.item.nmlmrate) || 0"]+ CRLF +;
   [											 size="sm" type="text" readonly />] + CRLF )
Response.Write([]+ CRLF +;
   [					 </template>]+ CRLF +;
   [					 <template #cell(linactive)="row">]+ CRLF +;
   [						  <b-form-checkbox v-model="row.item.linactive" :disabled="row.item.cspontype=='OFFER'"></b-form-checkbox>]+ CRLF +;
   [					 </template>]+ CRLF +;
   [					 <template #cell(lgwp)="row">]+ CRLF +;
   [						  <b-button size="sm" @click="gift_edit(row.item,row.index)"]+ CRLF +;
   [										variant="light">]+ CRLF +;
   [								<i class="fa fa-gift" v-if="row.item.lgwp"></i>]+ CRLF +;
   [						  </b-button>] + CRLF )
Response.Write([]+ CRLF +;
   []+ CRLF +;
   [					 </template>]+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   [				</b-table>]+ CRLF +;
   []+ CRLF +;
   [				<b-table hover :items="odata.sku.ousedin"]+ CRLF +;
   [							:fields="usedinfields" class="table-sm"] + CRLF )
Response.Write([							caption="Used In" caption-top]+ CRLF +;
   [							bordered]+ CRLF +;
   [							v-model="visibleRowsUsedin"]+ CRLF +;
   [							v-show="visibleRowsUsedin.length > 0"]+ CRLF +;
   [							small>]+ CRLF +;
   [				</b-table>]+ CRLF +;
   []+ CRLF +;
   [				<b-table hover :items="odata.sku.okit"]+ CRLF +;
   [							:fields="kititemfields" class="table-sm"]+ CRLF +;
   [							caption="Components" caption-top] + CRLF )
Response.Write([							bordered]+ CRLF +;
   [							small v-if="odata.sku.okit"]+ CRLF +;
   [							v-model="visibleRowsKit"]+ CRLF +;
   [							v-show="visibleRowsKit.length > 0">]+ CRLF +;
   [				</b-table>]+ CRLF +;
   []+ CRLF +;
   [				<!--</form>-->]+ CRLF +;
   [		  </b-container>]+ CRLF +;
   [	 </b-modal>]+ CRLF +;
   [] + CRLF )
Response.Write([]+ CRLF +;
   [	 <b-modal id="new-item" ok-only @ok="item_handleOk" @close="item_handleClose"]+ CRLF +;
   [				 title="Add an item to Campaign" size="xl"]+ CRLF +;
   [				 :ok-disabled="(!newsku.citemno || !newsku.cdescript || newsku.selected.length==0)">]+ CRLF +;
   [		  <b-container>]+ CRLF +;
   []+ CRLF +;
   [				<div class="alert alert-warning danger-color" v-if="item_updateErrors.length>0">]+ CRLF +;
   [					 <ul>]+ CRLF +;
   [						  <li v-for="error in item_updateErrors">{{ error }}</li>]+ CRLF +;
   [					 </ul>] + CRLF )
Response.Write([				</div>]+ CRLF +;
   []+ CRLF +;
   [				<!--<form ref="form" @submit.stop.prevent="item_handleLineSave">-->]+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   [				<b-row>]+ CRLF +;
   [					 <b-input-group class="m-1">]+ CRLF +;
   [						  <b-input-group-prepend class="col-sm-2 p-0"><b-input-group-text>Code</b-input-group-text></b-input-group-prepend>]+ CRLF +;
   [						  <div class="col-sm-10 p-0">]+ CRLF +;
   [] + CRLF )
Response.Write([								<vue-bootstrap-typeahead id="citemno"]+ CRLF +;
   [																 :data="vmlookup.items"]+ CRLF +;
   [																 v-model="newsku.citemno"]+ CRLF +;
   [																 :serializer="s => s.citemno"]+ CRLF +;
   [																 @input="lookupItem(newsku.citemno)"]+ CRLF +;
   [																 :min-matching-chars="3"]+ CRLF +;
   [																 @hit="newsku.citemno=$event.citemno;newsku.cdescript=$event.cdescript;]+ CRLF +;
   [																	  newsku.lpending=$event.lpending;">]+ CRLF +;
   [									 <template slot="suggestion" slot-scope="{ data, htmlText }">]+ CRLF +;
   [										  <span v-html="htmlText"></span>&nbsp;] + CRLF )
Response.Write([										  <small>{{ data.cdescript }}</small>]+ CRLF +;
   [									 </template>]+ CRLF +;
   [								</vue-bootstrap-typeahead>]+ CRLF +;
   [						  </div>]+ CRLF +;
   []+ CRLF +;
   [					 </b-input-group>]+ CRLF +;
   [					 {{newsku.cdescript}}]+ CRLF +;
   [					 <b-form-group label="Add to Campaigns" v-slot="{ ariaDescribedby }">]+ CRLF +;
   [						  <b-form-checkbox-group id="campaign-selected"]+ CRLF +;
   [														 v-model="newsku.selected"] + CRLF )
Response.Write([														 :options="campaignlookup"]+ CRLF +;
   [														 :aria-describedby="ariaDescribedby"]+ CRLF +;
   [														 name="campaign-selected"></b-form-checkbox-group>]+ CRLF +;
   [					 </b-form-group>]+ CRLF +;
   []+ CRLF +;
   [				</b-row>]+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   [] + CRLF )
Response.Write([				<div class="mx-3">]+ CRLF +;
   [					 <pre>{{ updateErrors[0] + ']' + [ }}</pre>]+ CRLF +;
   [				</div>]+ CRLF +;
   [				<!--/form>-->]+ CRLF +;
   [		  </b-container>]+ CRLF +;
   [	 </b-modal>]+ CRLF +;
   []+ CRLF +;
   [	 <b-modal id="gift-item" ok-only @ok="gift_handleOk" @close="gift_handleClose"]+ CRLF +;
   [				 title="Gift Item" size="sm">]+ CRLF +;
   [		  <b-form-checkbox v-model="giftitem.lgwp">] + CRLF )
Response.Write([				Gift or Forced Item]+ CRLF +;
   []+ CRLF +;
   [		  </b-form-checkbox>]+ CRLF +;
   [		  <b-container v-if="giftitem.lgwp">]+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   [				<label for="giftitem_ngqtylimit">Qty Limit</label>]+ CRLF +;
   [				<b-form-input size="sm" id="giftitem_ngqtylimit"]+ CRLF +;
   [								  type="number" class="text-small form-control"] + CRLF )
Response.Write([								  :value="giftitem.ngqtylimit"]+ CRLF +;
   [								  v-model.number="giftitem.ngqtylimit">]+ CRLF +;
   [				</b-form-input>]+ CRLF +;
   []+ CRLF +;
   [				<label for="giftitem_nminsales">Min Sales</label>]+ CRLF +;
   [				<b-form-input size="sm" id="giftitem_nminsales"]+ CRLF +;
   [								  type="number" class="text-small form-control"]+ CRLF +;
   [								  :value="giftitem.nminsales"]+ CRLF +;
   [								  v-model.number="giftitem.nminsales">]+ CRLF +;
   [				</b-form-input>] + CRLF )
Response.Write([]+ CRLF +;
   [				<label for="giftitem_cgfttype">Type</label>]+ CRLF +;
   [				<b-form-select size="sm" id="giftitem_cgfttype"]+ CRLF +;
   [									v-model="giftitem.cgfttype"]+ CRLF +;
   [									plain class="text-small form-control"]+ CRLF +;
   [									:options="['', 'FORCE','STARTER','DONATION'] + ']' + [">]+ CRLF +;
   []+ CRLF +;
   [				</b-form-select>]+ CRLF +;
   []+ CRLF +;
   [		  </b-container>] + CRLF )
Response.Write([	 </b-modal>]+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   [</div>]+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   []+ CRLF +;
   [] + CRLF )
Response.Write([])
