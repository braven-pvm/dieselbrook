const API_URL = '';

const number0 = value => $.number(value, 0);
const number2 = value => $.number(value, 2);
const yesNo = value => (value == 1 ? 'Yes' : 'No');

const rightAlignedNumberField = (key, label, decimals = 0) => ({
    key,
    label,
    formatter: value => $.number(value, decimals),
    thClass: 'text-right',
    tdClass: 'text-right'
});

const vm = {
    value: 0,
    isBusy: false,
    appName: 'Campaign / Product Exposure',
    tabIndex: 0,
    show: false,
    isVisible: false,
    itemSearch: '',
    settings: {},
    itemlookups: '',
    visibleRows: [],
    visibleRowsDetail: [],
    visibleRowsUsedin: [],
    visibleRowsKit: [],
    visibleRowsCat: [],
    visibleRowsSpon: [],
    visibleRowsBrand: [],
    visibleItemRows: [],
    dfrom: '2021-07-01',
    dto: '2030-07-01',
    category: '',
    filter: null,
    filterOn: ['cbrand', 'cdescript', 'citemno'],
    lineState: null,
    linefields: [
        { key: 'month', label: 'Month' },
        rightAlignedNumberField('naop', 'AOP'),
        rightAlignedNumberField('npfx', 'PFX'),
        rightAlignedNumberField('ntarget', 'Marketing'),
        rightAlignedNumberField('ngptarget', 'GP%', 2),
        rightAlignedNumberField('ocat[0].ntargetsplit', 'Skincare', 2),
        rightAlignedNumberField('ocat[1].ntargetsplit', 'Bodycare', 2),
        rightAlignedNumberField('ocat[2].ntargetsplit', 'Lifestyle', 2),
        rightAlignedNumberField('ocat[3].ntargetsplit', 'Cosmetics', 2),
        rightAlignedNumberField('ocat[4].ntargetsplit', 'Fragrance', 2),
        rightAlignedNumberField('ocat[5].ntargetsplit', 'Other', 2),
        rightAlignedNumberField('ospon[0].ntargetsplit', 'Standard', 2),
        rightAlignedNumberField('ospon[1].ntargetsplit', 'New', 2),
        rightAlignedNumberField('ospon[2].ntargetsplit', 'Promo', 2),
        rightAlignedNumberField('ospon[3].ntargetsplit', 'Other', 2),
        rightAlignedNumberField('nactualsales', 'Sales'),
        rightAlignedNumberField('nactualsales', 'Sales'),
        rightAlignedNumberField('nactualgpp', 'GP%'),
        { key: 'actions', label: '' }
    ],
    skufields: [
        {
            key: 'citemno',
            label: 'Item Code',
            sortable: true,
            stickyColumn: true,
            sortByFormatted: (value, key, item) => `${item.cbrand}-${item.citemno}-${item.dfrom}`
        },
        { key: 'cbrand', label: 'Brand' },
        { key: 'cdescript', label: 'Description', sortable: true, stickyColumn: true },
        {
            key: 'cmonthname',
            label: 'Month',
            sortable: true,
            stickyColumn: true,
            sortByFormatted: (value, key, item) => `${item.dfrom}-${item.cbrand}-${item.citemno}`
        },
        rightAlignedNumberField('nforecasts', 'Forecast (Std)'),
        rightAlignedNumberField('nforecastn', 'Forecast (New)'),
        rightAlignedNumberField('nforecastp', 'Forecast (Promo)'),
        rightAlignedNumberField('nforecastO', 'Forecast (O)'),
        rightAlignedNumberField('nforecast', 'Forecast (Total)'),
        rightAlignedNumberField('nprice', 'Price (AVG)', 2),
        rightAlignedNumberField('nrsp', 'RSP (AVG)', 2),
        {
            key: 'ntarget',
            label: 'Forecast RSP',
            thClass: 'text-right',
            tdClass: 'text-right'
        },
        rightAlignedNumberField('ncost', 'Cost', 2),
        rightAlignedNumberField('ngp', 'GP%', 2),
        { key: 'lonstore', label: 'On Store', formatter: yesNo },
        { key: 'lcanbuy', label: 'Can Buy', formatter: yesNo },
        { key: 'lkititem', label: 'Kit Item', tdClass: 'text-left' },
        { key: 'usedin', label: 'Used In', tdClass: 'text-left' },
        { key: 'actions', label: 'Actions' }
    ],
    usedinfields: [
        { key: 'ckitcode', label: 'Kit Code' },
        { key: 'cdescript', label: 'Description' },
        { key: 'nqty', label: 'Qty' },
        { key: 'nforecast', label: 'Forecast' }
    ],
    kititemfields: [
        { key: 'citemno', label: 'Item Code' },
        { key: 'compdescript', label: 'Description' },
        { key: 'nqty', label: 'Qty' }
    ],
    detailfields: [
        { key: 'csponcat', label: 'Sponcat' },
        { key: 'cspontype', label: 'Spontype' },
        { key: 'dfrom', label: 'From Date' },
        { key: 'dto', label: 'To Date' },
        {
            key: 'nforecast',
            label: 'Forecast',
            formatter: value => number0(value)
        },
        {
            key: 'nrsp',
            label: 'Price (Inc)',
            formatter: value => value.toFixed(2),
            thStyle: 'width: 5em;'
        },
        {
            key: 'nqtylimit',
            label: 'Qty Limit',
            formatter: value => number0(value)
        },
        { key: 'coffer', label: 'Major Promo' },
        { key: 'cpageno', label: 'Page' },
        {
            key: 'ngp',
            label: 'GP%',
            formatter: value => number2(value)
        },
        { key: 'linactive', label: 'Inactive' },
        { key: 'lgwp', label: 'Gift', thStyle: 'width: 5em;' },
        { key: 'actions', label: '' }
    ],
    detailfieldsview: [
        { key: 'csponcat', label: 'Sponcat' },
        { key: 'cspontype', label: 'Spontype' },
        { key: 'dfrom', label: 'From Date' },
        { key: 'dto', label: 'To Date' },
        {
            key: 'nforecast',
            label: 'Forecast',
            formatter: value => number0(value)
        },
        {
            key: 'nrsp',
            label: 'Price',
            formatter: value => value.toFixed(2),
            thStyle: 'width: 5em;'
        },
        {
            key: 'nqtylimit',
            label: 'Qty-Limit',
            formatter: value => number0(value)
        },
        { key: 'coffer', label: 'Major Promo' },
        { key: 'cpageno', label: 'Page' },
        {
            key: 'ngp',
            label: 'GP%',
            formatter: value => number2(value)
        },
        {
            key: 'lgwp',
            label: 'Gift',
            thStyle: 'width: 5em;',
            formatter: value => (value ? 'Yes' : 'No')
        },
        { key: 'linactive', label: 'Inactive' }
    ],
    totalCatPlanFmt: '',
    catfields: [
        { key: 'category', label: 'Category', tdClass: 'W-50', thClass: 'W-50' },
        {
            key: 'ntargetsplit',
            label: 'Split %',
            formatter: value => number2(value),
            tdClass: 'col-10perc'
        },
        {
            key: 'ntarget',
            label: 'Target',
            formatter: value => number0(value),
            tdClass: 'text-right'
        },
        {
            key: 'nptarget',
            label: 'Campaign',
            formatter: value => number0(value),
            tdClass: 'text-right'
        },
        {
            key: 'nptargetsplit',
            label: 'Split',
            formatter: value => number0(value),
            tdClass: 'text-right'
        },
        {
            key: 'npvariance',
            label: 'Variance %',
            formatter: value => number0(value),
            tdClass: 'text-right'
        },
        {
            key: 'ngptarget',
            label: 'Target GP%',
            formatter: value => number0(value),
            tdClass: 'col-10perc'
        },
        {
            key: 'ngpp',
            label: 'Campaign GP%',
            formatter: value => number0(value)
        },
        {
            key: 'ngppvariance',
            label: 'Variance %',
            formatter: value => number0(value)
        }
    ],
    sponfields: [
        { key: 'csponcat', label: 'SPON Cat' },
        {
            key: 'ntargetsplit',
            label: 'Split %',
            formatter: value => number2(value),
            tdClass: 'col-10perc'
        },
        {
            key: 'ntarget',
            label: 'Target',
            formatter: value => number0(value),
            tdClass: 'text-right'
        },
        {
            key: 'nptarget',
            label: 'Campaign',
            formatter: value => number0(value),
            tdClass: 'text-right'
        },
        {
            key: 'nptargetsplit',
            label: 'Split',
            formatter: value => number0(value),
            tdClass: 'text-right'
        },
        {
            key: 'npvariance',
            label: 'Variance %',
            formatter: value => number0(value),
            tdClass: 'text-right'
        },
        {
            key: 'ngptarget',
            label: 'Target GP%',
            formatter: value => number0(value),
            tdClass: 'col-10perc'
        },
        {
            key: 'ngpp',
            label: 'Campaign GP%',
            formatter: value => number0(value)
        },
        {
            key: 'ngppvariance',
            label: 'Variance %',
            formatter: value => number0(value)
        }
    ],
    brandfields: [
        { key: 'category', label: 'Category' },
        { key: 'cclass', label: 'Brand' },
        {
            key: 'nrrbudget',
            label: 'Reward Budget (R)',
            formatter: value => number0(value),
            tdClass: 'text-right'
        },
        { key: 'actions', label: '' }
    ],
    dashfields: [
        { key: 'label', label: '' },
        rightAlignedNumberField('tot', 'Total'),
        rightAlignedNumberField('c1', 'Skincare'),
        rightAlignedNumberField('c2', 'Body Care'),
        rightAlignedNumberField('c3', 'Life Style'),
        rightAlignedNumberField('c4', 'Cosmetics'),
        rightAlignedNumberField('c5', 'Fine Fr'),
        rightAlignedNumberField('c6', 'Other'),
        rightAlignedNumberField('s1', 'Standard'),
        rightAlignedNumberField('s2', 'New'),
        rightAlignedNumberField('s3', 'Promo'),
        rightAlignedNumberField('s4', 'Obselete')
    ],
    hasError: false,
    updateErrors: [],
    camp_updateErrors: [],
    detail_updateErrors: [],
    item_updateErrors: [],
    odata: {
        campaigns: [],
        campaign: {},
        skus: [],
        brands: [],
        sku: {}
    },
    campaign: {},
    campaignlookup: [],
    vmlookup: {},
    spontypes: [],
    sponcats: [],
    sku: {
        index: 0,
        cmonthname: '',
        category: 0,
        orderseq: 0,
        dfrom: '',
        id: 0,
        campaignid: 0,
        campcatid: 0,
        lpending: 0,
        citemno: '',
        cdescript: '',
        cbrand: '',
        lkititem: 0,
        nprice: 0,
        ndiscrate: 0,
        ndrate: 0,
        nmlmrate: 0,
        nrsp: 0,
        lonstore: 0,
        lcanbuy: 0,
        lcalculated: 0,
        ncost: 0,
        nforecasts: 0,
        nforecastp: 0,
        nforecasto: 0,
        nforecastn: 0,
        nforecast: 0,
        odetail: [],
        okit: [],
        ousedin: [],
        selected: []
    },
    newsku: {
        citemno: '',
        cdescript: '',
        lPending: 0,
        selected: []
    },
    skuempty: {
        cmonthname: '',
        category: 0,
        orderseq: 0,
        dfrom: '',
        id: 0,
        campaignid: 0,
        campcatid: 0,
        lpending: 0,
        citemno: '',
        cdescript: '',
        cbrand: '',
        lkititem: 0,
        nprice: 0,
        ndiscrate: 0,
        ndrate: 0,
        nmlmrate: 0,
        nrsp: 0,
        lonstore: 0,
        lcanbuy: 0,
        lcalculated: 0,
        ncost: 0,
        nforecasts: 0,
        nforecastp: 0,
        nforecasto: 0,
        nforecastn: 0,
        nforecast: 0,
        odetail: [],
        okit: [],
        ousedin: []
    },
    giftitem: {},
    brand: {},
    isnpd: 'No',
    infoModal: {
        id: 'info-modal',
        title: '',
        content: '',
        item: ''
    }
};

VeeValidate.extend('required', {
    validate(value) {
        return {
            required: true,
            valid: !['', null, undefined].includes(value)
        };
    },
    computesRequired: true
});

VeeValidate.extend('required', VeeValidateRules.required);
VeeValidate.extend('min', VeeValidateRules.min);
VeeValidate.extend('integer', VeeValidateRules.integer);
VeeValidate.extend('min_value', VeeValidateRules.min_value);
VeeValidate.extend('max_value', VeeValidateRules.max_value);

Vue.component('ValidationObserver', VeeValidate.ValidationObserver);
Vue.component('ValidationProvider', VeeValidate.ValidationProvider);
Vue.component('vue-bootstrap-typeahead', VueTypeaheadBootstrap);

vm.vmlookup = {
    baseUrl: './',
    categories: [],
    category: '',
    suburbs: [],
    brands: [],
    brand: '',
    ready: false,
    addRowVisible: false,
    searchText: '',
    items: [],
    item: '',
    lines: [],
    line: '',
    nextLine: 1,
    showkits: false,
    itemselected: { citemno: '' },
    codeonly: false
};

let _ = '';

const App = new Vue({
    el: '#app',
    data() {
        return vm;
    },
    methods: {
        getValidationState({ dirty, validated, valid = null }) {
            return dirty || validated ? valid : null;
        },

        resetForm() {
            this.form = {
                name: null,
                food: null
            };

            this.$nextTick(() => {
                this.$refs.observer.reset();
            });
        },

        onSubmit() {},

        handleVisibility(isVisible) {
            this.isVisible = isVisible;
        },

        info(item, index, button) {
            this.infoModal.title = `Row index: ${index}`;
            this.infoModal.content = JSON.stringify(item, null, 2);
            this.infoModal.item = item;
            this.$root.$emit('bv::show::modal', this.infoModal.id, button);
        },

        resetInfoModal() {
            this.infoModal.title = '';
            this.infoModal.content = '';
        },

        logtime(method) {
            const currentDateTime = new Date();
            const resultInSeconds = currentDateTime.getTime() / 1000;
            return { method, currentDateTime, resultInSeconds };
        },

        camp_onRowSelected(items) {
            if (this.skufields[0].key !== 'category') {
                this.skufields.unshift({
                    key: 'category',
                    label: 'Category',
                    sortable: true,
                    stickyColumn: true,
                    sortByFormatted: (value, key, item) =>
                        `${item.category}-${item.cbrand}-${item.citemno}-${item.dfrom}`
                });
            }

            this.getSkus('', items.dfrom, items.dto);
            this.tabIndex = 1;
        },

        camp_edit(item, index, button) {
            this.updateErrors = [];
            this.odata.campaign = item;
            this.campaign = $.extend(true, {}, item);
            this.campaign.index = index;
            this.odata.campaign.index = index;
            this.show = true;
            this.$root.$emit('bv::show::modal', 'campaign', button);
        },

        camp_handleClose() {
            const oItem = this.campaign;
            this.odata.campaigns[oItem.index] = $.extend(true, {}, oItem);
            this.$set(this.odata.campaigns, oItem.index, this.campaign);
        },

        camp_handleOk(bvModalEvt) {
            bvModalEvt.preventDefault();
            this.camp_handleSave();
        },

        camp_handleSave() {
            this.updateCamp('UPDATE', this.odata.campaign);
        },

        updateCamp(action, data) {
            this.updateErrors = [];
            this.camp_updateErrors = [];
            this.hasError = false;
            this.isBusy = true;

            const idx = data.index;

            ajaxCallMethod(
                'jsonCallbackst.ann',
                'camp_update',
                [action, data],
                ret => {
                    if (ret.length > 0 && action !== 'DELETE') {
                        App.$set(App.odata.campaigns, idx, ret[0]);
                        App.updateComplete();
                        return;
                    }

                    if (ret.errors) {
                        for (const i of ret.errors.aitems) {
                            if (i.cmessage === 'Not Authenticated') {
                                alert('You are not Authentcated');
                                window.location.href = 'login.ann';
                                return;
                            }

                            App.camp_updateErrors.push(i.cmessage);
                        }

                        App.hasError = true;
                        App.isBusy = false;
                    }
                },
                error => {
                    App.hasError = true;
                    App.updateErrors.push(error.message);
                    App.isBusy = false;
                },
                { timeout: 30000 }
            );

            return !this.hasError;
        },

        async updateComplete() {
            this.$bvModal.hide('campaign');
            this.isBusy = false;
        },

        camp_synctostage(id) {
            this.updateErrors = [];
            this.camp_updateErrors = [];
            this.hasError = false;
            this.isBusy = true;

            ajaxCallMethod(
                'jsonCallbackst.ann',
                'camp_synctostage',
                [id],
                ret => {
                    if (ret.errors) {
                        for (const i of ret.errors.aitems) {
                            if (i.cmessage === 'Not Authenticated') {
                                alert('You are not Authentcated');
                                window.location.href = 'login.ann';
                                return;
                            }

                            App.camp_updateErrors.push(i.cmessage);
                        }

                        App.hasError = true;
                        App.isBusy = false;
                    } else {
                        App.isBusy = false;
                        alert('Stage Updated');
                    }
                },
                error => {
                    App.hasError = true;
                    App.updateErrors.push(error.message);
                    App.isBusy = false;
                },
                { timeout: 30000 }
            );

            return !this.hasError;
        },

        changeSponForecast() {
            for (let i = 0; i < this.odata.campaign.ospon.length; i += 1) {
                this.odata.campaign.ospon[i].ntarget =
                    (this.odata.campaign.ospon[i].ntargetsplit / 100) * this.odata.campaign.ntarget;
            }
        },

        changeCatForecast() {
            for (let i = 0; i < this.odata.campaign.ocat.length; i += 1) {
                this.odata.campaign.ocat[i].ntarget =
                    (this.odata.campaign.ocat[i].ntargetsplit / 100) * this.odata.campaign.ntarget;
            }
        },

        brand_addRowHandler() {
            this.brand = { category: '', cclass: '', nnrrbudget: 0.0 };
            this.odata.campaign.obrand.push(this.brand);
        },

        brand_removeRowHandler(index) {
            this.odata.campaign.obrand = this.odata.campaign.obrand.filter((item, i) => i !== index);
            this.$emit('input', this.odata.campaign.obrand);
        },

        totalTarget(camp) {
            return camp.ocat.reduce((accum, item) => accum + parseFloat(item.nptarget), 0.0);
        },

        totalCost(camp) {
            return camp.ocat.reduce((accum, item) => accum + parseFloat(item.ntcost), 0.0);
        },

        totalDisc(camp) {
            return camp.ocat.reduce((accum, item) => accum + parseFloat(item.ntdisc), 0.0);
        },

        gpp(sales, cost, disc, mlm) {
            if (cost == 0) {
                return '**';
            }

            if (!disc) {
                disc = 0;
            }

            if (!mlm) {
                mlm = 0;
            }

            const d = sales * (disc / 100);
            const m = sales * (mlm / 100);
            sales = sales - (d + m);

            return $.number(((sales - cost) / sales) * 100, 2);
        },

        gppactual(sales, cost, disc, mlm) {
            if (cost == 0) {
                return '**';
            }

            if (!disc) {
                disc = 0;
            }

            if (!mlm) {
                mlm = 0;
            }

            const d = disc;
            const m = mlm;
            sales = sales - (d + m);

            return $.number(((sales - cost) / sales) * 100, 2);
        },

        formatter_2dec(value) {
            return parseFloat(value).toFixed(2);
        },

        formatter_com(value) {
            return $.number(value, 0);
        },

        detail_onRowSelected() {},

        detail_edit(item, index) {
            if (
                this.settings.viewonly ||
                (!this.settings.editinfrozen && item.cstatus == 'F') ||
                (!this.settings.editlive && item.cstatus == 'C') ||
                item.cstatus == 'H'
            ) {
                return;
            }

            this.detail_updateErrors = [];
            this.sku = item;
            this.odata.sku = $.extend(true, {}, item);
            this.sku.index = index;
            this.odata.sku.index = index;
            this.$root.$emit('bv::show::modal', 'detail');
        },

        detail_addRowHandler() {
            const newRow = $.extend(true, {}, this.odata.sku.odetail[0]);
            newRow.id = 0;
            newRow.nforecast = 0;
            this.odata.sku.odetail.push(newRow);
        },

        detail_handleClose() {},

        detail_handleOk(bvModalEvt) {
            bvModalEvt.preventDefault();
            this.detail_handleSave();
        },

        detail_handleSave() {
            this.detail_update('UPDATE', this.odata.sku);
        },

        detail_update(action, data) {
            this.updateErrors = [];
            this.detail_updateErrors = [];
            this.hasError = false;
            this.isBusy = true;

            let idx = data.index;

            ajaxCallMethod(
                'jsonCallbackst.ann',
                'camp_updatesku',
                [action, data],
                ret => {
                    if (ret.length > 0 && action !== 'DELETE') {
                        idx = App.odata.skus.findIndex(x => x.id === ret[0].id);
                        App.$set(App.odata.skus, idx, ret[0]);
                        App.detail_updateComplete();
                        return;
                    }

                    if (ret.errors) {
                        for (const i of ret.errors.aitems) {
                            if (i.cmessage === 'Not Authenticated') {
                                alert('You are not Authentcated');
                                window.location.href = 'login.ann';
                                return;
                            }

                            App.detail_updateErrors.push(i.cmessage);
                        }

                        App.hasError = true;
                        App.isBusy = false;
                    }
                },
                error => {
                    App.hasError = true;
                    App.updateErrors.push(error.message);
                    App.isBusy = false;
                },
                { timeout: 90000 }
            );

            return !this.hasError;
        },

        async detail_updateComplete() {
            this.$bvModal.hide('detail');
            this.isBusy = false;
        },

        detail_removeRowHandler(data) {
            let deleteyn = '';

            this.$bvModal
                .msgBoxConfirm(
                    `Please confirm that you want to delete ${data.item.csponcat} from ${data.item.dfrom} to ${data.item.dto}`,
                    {
                        title: 'Please Confirm',
                        size: 'sm',
                        buttonSize: 'sm',
                        okVariant: 'danger',
                        okTitle: 'YES',
                        cancelTitle: 'NO',
                        footerClass: 'p-2',
                        hideHeaderClose: false,
                        centered: true
                    }
                )
                .then(value => {
                    deleteyn = value;
                    if (deleteyn) {
                        this.odata.sku.odetail = this.odata.sku.odetail.filter((item, i) => i !== data.index);
                        this.$emit('input', this.odata.sku.odetail);
                    }
                })
                .catch(() => {});
        },

        jumpto(value) {
            const result = this.odata.skus.findIndex(obj => obj.id === value);

            if (result) {
                this.detail_edit(this.odata.skus[result], result);
            }
        },

        detail_updatesponcat(o) {
            if (!o) {
                return;
            }

            const result = this.sponcats.filter(obj => obj.cspontype === o.cspontype);
            if (result) {
                o.csponcat = result[0].csponcat;
            }

            const aSpons = ['OFFER', 'AFFILIATE', 'EXPORT', 'VOUCHER'];
            if (aSpons.includes(o.cspontype)) {
                o.linactive = true;
            }
        },

        item_handleClose() {},

        item_handleOk(bvModalEvt) {
            bvModalEvt.preventDefault();
            this.item_update();
        },

        item_addsku() {
            this.item_info();
        },

        item_info() {
            this.updateErrors = [];
            this.itemSearch = '';
            this.newsku = { citemno: '', cdescript: '', selected: [] };
            this.$root.$emit('bv::show::modal', 'new-item');
        },

        item_valid() {
            let lerror = false;
            this.item_updateErrors = [];

            if (!this.newsku.citemno) {
                item_updateErrors.push('Select an item');
                lerror = true;
            }

            if (!this.newsku.selected.length == 0) {
                item_updateErrors.push('Select a campaign');
                lerror = true;
            }

            return lerror;
        },

        item_update() {
            this.item_updateErrors = [];
            this.updateErrors = [];
            this.hasError = false;
            this.isBusy = true;

            const data = this.newsku;

            ajaxCallMethod(
                'jsonCallbackst.ann',
                'camp_updatesku',
                ['ADD', data],
                ret => {
                    if (ret.errors) {
                        for (const i of ret.errors.aitems) {
                            if (i.cmessage === 'Not Authenticated') {
                                alert('You are not Authentcated');
                                window.location.href = 'login.ann';
                                return;
                            }
                            App.item_updateErrors.push(i.cmessage);
                        }
                        App.hasError = true;
                        App.isBusy = false;
                        return;
                    }

                    if (ret.length == 1) {
                        App.odata.skus.push(ret[0]);
                    } else {
                        App.getSkus(App.category, App.dfrom, App.dto);
                    }

                    App.item_updateComplete();
                },
                error => {
                    App.hasError = true;
                    App.item_updateErrors.push(error.message);
                    App.isBusy = false;
                },
                { timeout: 60000 }
            );

            return !this.hasError;
        },

        async item_updateComplete() {
            this.$bvModal.hide('new-item');
            this.isBusy = false;
        },

        item_removeRowHandler(index) {
            let deleteyn = '';

            this.$bvModal
                .msgBoxConfirm(
                    `Please confirm that you want to delete ${index.item.citemno} from the campaign`,
                    {
                        title: 'Please Confirm',
                        size: 'sm',
                        buttonSize: 'sm',
                        okVariant: 'danger',
                        okTitle: 'YES',
                        cancelTitle: 'NO',
                        footerClass: 'p-2',
                        hideHeaderClose: false,
                        centered: true
                    }
                )
                .then(value => {
                    deleteyn = value;
                    if (deleteyn) {
                        this.updateErrors = [];
                        ajaxCallMethod(
                            'jsonCallbackst.ann',
                            'camp_updatesku',
                            ['DELETE', index.item],
                            ret => {
                                if (ret.errors) {
                                    for (const i of ret.errors.aitems) {
                                        App.updateErrors.push(i.cmessage);
                                    }
                                    return;
                                }

                                this.odata.skus = this.odata.skus.filter((item, i) => i !== index.index);
                                this.$emit('input', this.odata.skus);
                            },
                            error => {
                                App.hasError = true;
                                App.updateErrors.push(error.message);
                            },
                            { timeout: 30000 }
                        );
                    }
                })
                .catch(() => {});
        },

        gift_edit(item) {
            this.giftitem = item;
            this.$root.$emit('bv::show::modal', 'gift-item');
        },

        gift_handleClose() {},
        gift_handleOk() {},

        loadUserSettings() {
            ajaxCallMethod(
                'jsonCallbacks.ann',
                'Camp_GetSettings',
                [],
                req => {
                    App.settings = req;
                    if (App.settings.defaultcat) {
                        App.category = App.settings.defaultcat;
                    }
                },
                () => {}
            );
        },

        loadCampaignLookup() {
            ajaxCallMethod(
                'jsonCallbacks.ann',
                'Camp_GetCamps',
                [],
                req => {
                    vm.campaignlookup = req;
                },
                () => {}
            );
        },

        loadItemLookups() {
            ajaxCallMethod(
                'jsonCallbacks.ann',
                'Camp_GetItemLookups',
                [],
                req => {
                    App.itemlookups = req;
                },
                () => {}
            );
        },

        loadSpontype() {
            ajaxCallMethod(
                'jsonCallbacks.ann',
                'camp_getspontypes',
                [],
                req => {
                    vm.spontypes = req.spontypes;
                    vm.sponcats = req.sponcats;
                },
                () => {}
            );
        },

        getSkus(category, dfrom, dto) {
            this.isBusy = true;
            this.category = category;
            this.dfrom = dfrom;
            this.dto = dto;

            ajaxCallMethod(
                'jsonCallbacks.ann',
                'camp_getskubymonthvert',
                [category, dfrom, dto],
                ret => {
                    if (ret.errors) {
                        for (const i of ret.errors.aitems) {
                            App.updateErrors.push(i.cmessage);
                        }

                        App.hasError = true;
                        App.isBusy = false;
                    } else {
                        App.odata.skus = ret;
                        App.isBusy = false;
                    }
                },
                () => {
                    App.hasError = true;
                    App.isBusy = false;
                }
            );

            if (this.hasError) {
                this.isBusy = false;
            }
        },

        lookupItem: debounce(function (addr) {
            let l = 5;
            if (addr.length > 0 && addr.substring(0, 1) == '*') {
                l = 3;
            }

            if (addr.length < l) {
                return;
            }

            this.loadItems(addr);
        }, 500),

        loadItems(searchText) {
            ajaxCallMethod(
                'jsonCallbacks.ann',
                'Camp_getItems',
                [searchText],
                items => {
                    App.vmlookup.items = items;
                    App.vmlookup.ready = true;
                },
                () => {}
            );
        },

        dashRowClass(item, type) {
            if (!item || type !== 'row') {
                return;
            }
            if (item.label === 'Variance') {
                return 'table-warning';
            }
        },

        campRowClass(item, type) {
            if (!item || type !== 'row') {
                return;
            }
            if (item.cstatus === 'C') {
                return 'table-danger';
            }
            if (item.cstatus === 'F') {
                return 'table-primary';
            }
            if (item.cstatus === 'H') {
                return 'table-success';
            }
        }
    },

    watch: {
        'odata.sku.odetail': {
            handler(after) {
                let tota = after.reduce((accum, item) => accum + parseFloat(item.nforecast), 0.0);
                this.odata.sku.nforecast = tota;

                tota = after.reduce((accum, item) => accum + parseFloat(item.nforecast * item.nprice), 0.0);
                if (this.odata.sku.nforecast > 0) {
                    this.odata.sku.nprice = (tota / this.odata.sku.nforecast).toFixed(2);
                }

                tota = after.reduce((accum, item) => accum + parseFloat(item.nforecast * item.nrsp), 0.0);
                if (this.odata.sku.nforecast > 0) {
                    this.odata.sku.nrsp = (tota / this.odata.sku.nforecast).toFixed(2);
                }

                let s = after.filter(item => item.csponcat === 'STANDARD');
                this.odata.sku.nforecasts = s.reduce((accum, item) => accum + parseFloat(item.nforecast), 0.0);

                s = after.filter(item => item.csponcat === 'PROMOTION');
                this.odata.sku.nforecastp = s.reduce((accum, item) => accum + parseFloat(item.nforecast), 0.0);

                s = after.filter(item => item.csponcat === 'OBSOLETE');
                this.odata.sku.nforecasto = s.reduce((accum, item) => accum + parseFloat(item.nforecast), 0.0);

                s = after.filter(item => item.csponcat === 'NEW');
                this.odata.sku.nforecastn = s.reduce((accum, item) => accum + parseFloat(item.nforecast), 0.0);
            },
            deep: true
        }
    },

    computed: {
        totalCat() {
            return this.visibleRowsCat.reduce((accum, item) => accum + parseFloat(item.ntargetsplit), 0.0);
        },

        totalCatPlan() {
            return this.visibleRowsCat.reduce((accum, item) => accum + parseFloat(item.nptarget), 0.0);
        },

        totalSpon() {
            return this.visibleRowsSpon.reduce(
                (accum, item) => accum + parseFloat(item.ntargetsplit ? item.ntargetsplit : 0),
                0.0
            );
        },

        codedesc() {
            return `${this.odata.sku.citemno} ${this.odata.sku.cdescript}`;
        },

        totaldrp() {
            return (
                this.visibleRowsDetail.reduce((accum, item) => accum + parseFloat(item.nforecast), 0.0) +
                this.visibleRowsUsedin.reduce((accum, item) => accum + parseFloat(item.nforecast), 0.0)
            );
        }
    },

    mounted() {
        this.isBusy = true;
        this.loadUserSettings();
        this.loadSpontype();
        this.loadCampaignLookup();
        this.loadItemLookups();

        ajaxCallMethod(
            'jsonCallbacks.ann',
            'camp_getsummary',
            [this.dfrom, this.dto],
            ret => {
                if (ret.errors) {
                    for (const i of ret.errors.aitems) {
                        App.updateErrors.push(i.cmessage);
                    }

                    App.hasError = true;
                    App.isBusy = false;
                } else {
                    App.odata.campaigns = ret;
                    App.isBusy = false;
                }
            },
            () => {
                App.hasError = true;
                App.isBusy = false;
            }
        );

        if (this.hasError) {
            this.isBusy = false;
        }
    }
});
