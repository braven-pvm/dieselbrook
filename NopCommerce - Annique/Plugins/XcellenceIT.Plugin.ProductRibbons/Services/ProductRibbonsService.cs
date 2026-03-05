// *************************************************************************
// *                                                                       *
// * Product Ribbon Plugin for nopCommerce                                 *
// * Copyright (c) Xcellence-IT. All Rights Reserved.                      *
// *                                                                       *
// *************************************************************************
// *                                                                       *
// * Email: info@nopaccelerate.com                                         *
// * Website: http://www.nopaccelerate.com                                 *
// *                                                                       *
// *************************************************************************
// *                                                                       *
// * This  software is furnished  under a license  and  may  be  used  and *
// * modified  only in  accordance with the terms of such license and with *
// * the  inclusion of the above  copyright notice.  This software or  any *
// * other copies thereof may not be provided or  otherwise made available *
// * to any  other  person.   No title to and ownership of the software is *
// * hereby transferred.                                                   *
// *                                                                       *
// * You may not reverse  engineer, decompile, defeat  license  encryption *
// * mechanisms  or  disassemble this software product or software product *
// * license.  Xcellence-IT may terminate this license if you don't comply *
// * with  any  of  the  terms and conditions set forth in  our  end  user *
// * license agreement (EULA).  In such event,  licensee  agrees to return *
// * licensor  or destroy  all copies of software  upon termination of the *
// * license.                                                              *
// *                                                                       *
// * Please see the  License file for the full End User License Agreement. *
// * The  complete license agreement is also available on  our  website at * 
// * http://www.nopaccelerate.com/enterprise-license                       *
// *                                                                       *
// *************************************************************************
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Events;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XcellenceIt.Core;
using XcellenceIT.Plugin.ProductRibbons.Domain;
using XcellenceIT.Plugin.ProductRibbons.Utilities;
using static XcellenceIT.Plugin.ProductRibbons.AssemblyAttributes;

namespace XcellenceIT.Plugin.ProductRibbons.Services
{
    public partial class ProductRibbonsService : IProductRibbonsService
    {
        #region Fields

        private readonly IRepository<ProductRibbonRecord> _productRibbonRecordRepository;
        private readonly IRepository<ProductPictureRibbon> _productPictureRepository;
        private readonly IRepository<ProductRibbonMapping> _productRibbonMappingRepository;
        private readonly IEventPublisher _eventPublisher;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly IStaticCacheManager _staticCacheManager;
        private string HashAlgorithm => "SHA1";

        #endregion

        #region Ctor

        public ProductRibbonsService(IRepository<ProductRibbonRecord> productRibbonRecordRepository,
            IRepository<ProductPictureRibbon> productPictureRepository,
            IRepository<ProductRibbonMapping> productRibbonMappingRepository,
            IEventPublisher eventPublisher, 
            IStoreContext storeContext, ISettingService settingService,
            IStaticCacheManager staticCacheManager)
        {
            _productRibbonRecordRepository = productRibbonRecordRepository;
            _productPictureRepository = productPictureRepository;
            _productRibbonMappingRepository = productRibbonMappingRepository;
            _eventPublisher = eventPublisher;
            _storeContext = storeContext;
            _settingService = settingService;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Caching methods

        protected virtual string CreateIdsHash(IEnumerable<int> ids)
        {
            var identifiers = ids.ToList();

            if (!identifiers.Any())
                return string.Empty;

            var identifiersString = string.Join(", ", identifiers.OrderBy(id => id));
            return HashHelper.CreateHash(Encoding.UTF8.GetBytes(identifiersString), HashAlgorithm);
        }

        protected virtual object CreateCacheKeyParameters(object parameter)
        {
            return parameter switch
            {
                null => "null",
                IEnumerable<int> ids => CreateIdsHash(ids),
                IEnumerable<BaseEntity> entities => CreateIdsHash(entities.Select(entity => entity.Id)),
                BaseEntity entity => entity.Id,
                decimal param => param.ToString(CultureInfo.InvariantCulture),
                _ => parameter
            };
        }

        public virtual CacheKey PrepareKeyForCustomCache(CacheKey cacheKey, params object[] cacheKeyParameters)
        {
            var key = cacheKey.Create(CreateCacheKeyParameters, cacheKeyParameters);
            key.CacheTime = 1440;
            return key;
        }

        #endregion

        #region Product Ribbon Record Service

        public async Task<IPagedList<ProductRibbonRecord>> GetAllProductRibbonsAsync(string name = null, DateTime? startDateUtc = null, DateTime? endDateUtc = null, int displayOrder = 0, bool enabled = false, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _productRibbonRecordRepository.Table;

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(c => c.RibbonName.Contains(name));

            if (startDateUtc.HasValue)
                query = query.Where(o => startDateUtc.Value <= o.StartDateUtc);

            if (endDateUtc.HasValue)
                query = query.Where(o => endDateUtc.Value >= o.EndDateUtc);

            if (enabled)
                query = query.Where(x => x.Enabled);

            query = query.OrderBy(t => t.DisplayOrder);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<ProductRibbonRecord> GetProductRibbonByIdAsync(int ribbonId)
        {
            if (ribbonId == 0)
                return null;

            return await _productRibbonRecordRepository.GetByIdAsync(ribbonId, cache => default);
        }

        public async Task InsertProductRibbonAsync(ProductRibbonRecord productRibbonRecord)
        {
            if (productRibbonRecord == null)
                throw new NotImplementedException();

            await _productRibbonRecordRepository.InsertAsync(productRibbonRecord);

            await _eventPublisher.EntityInsertedAsync(productRibbonRecord);
        }

        public async Task UpdateProductRibbonAsync(ProductRibbonRecord productRibbonRecord)
        {
            if (productRibbonRecord == null)
                throw new NotImplementedException();

            await _productRibbonRecordRepository.UpdateAsync(productRibbonRecord);

            await _eventPublisher.EntityUpdatedAsync(productRibbonRecord);
        }

        public async Task DeleteProductRibbonAsync(ProductRibbonRecord productRibbonRecord)
        {
            if (productRibbonRecord == null)
                throw new NotImplementedException();

            var productPicture = await GetProductPictureRibbonByIdAsync(productRibbonRecord.Id);
            if (productPicture != null)
            {
                await _productPictureRepository.DeleteAsync(productPicture);

                await _eventPublisher.EntityDeletedAsync(productPicture);
            }

            var productRibbon = await GetProductRibbonMappingByIdAsync(productRibbonRecord.Id);
            if (productRibbon != null)
            {
                await _productRibbonMappingRepository.DeleteAsync(productRibbon);
                await _eventPublisher.EntityDeletedAsync(productPicture);
            }

            await _productRibbonRecordRepository.DeleteAsync(productRibbonRecord);
            await _eventPublisher.EntityDeletedAsync(productRibbonRecord);
        }

        #endregion

        #region Product Picture Ribbon  Service

        public async Task<ProductPictureRibbon> GetProductPictureRibbonByIdAsync(int ribbonId)
        {
            if (ribbonId == 0)
                return null;

            return await _productPictureRepository.GetByIdAsync(ribbonId, cache => default);
        }

        public async Task InsertProductPictureRibbonAsync(ProductPictureRibbon productPictureRibbon)
        {
            if (productPictureRibbon == null)
                throw new NotImplementedException();

            await _productPictureRepository.InsertAsync(productPictureRibbon);
            await _eventPublisher.EntityInsertedAsync(productPictureRibbon);
        }

        public async Task UpdateProductPictureRibbonAsync(ProductPictureRibbon productPictureRibbon)
        {
            if (productPictureRibbon == null)
                throw new NotImplementedException();

            await _productPictureRepository.UpdateAsync(productPictureRibbon);
            await _eventPublisher.EntityUpdatedAsync(productPictureRibbon);
        }

        #endregion

        #region ProductRibbon Mapping

        public async Task DeleteProductRibbonMappingAsync(ProductRibbonMapping productRibbonMapping)
        {
            if (productRibbonMapping == null)
                throw new NotImplementedException();

            await _productRibbonMappingRepository.DeleteAsync(productRibbonMapping);
            await _eventPublisher.EntityDeletedAsync(productRibbonMapping);
        }

        public async Task InsertProductRibbonMappingAsync(ProductRibbonMapping productRibbonMapping)
        {
            if (productRibbonMapping == null)
                throw new NotImplementedException();

            await _productRibbonMappingRepository.InsertAsync(productRibbonMapping);
            await _eventPublisher.EntityInsertedAsync(productRibbonMapping);
        }

        public async Task<ProductRibbonMapping> FindProductRibbonAsync(IList<ProductRibbonMapping> source,
           int productId, int ribbonId)
        {
            foreach (var ribbonProduct in source)
                if (ribbonProduct.ProductId == productId && ribbonProduct.RibbonId == ribbonId)
                    return ribbonProduct;

            return null;
        }

        public async Task<IPagedList<ProductRibbonMapping>> GetProductRibbonMappingRibbonIdAsync(int ribbonId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var productRibbonMapping = from pr in _productRibbonMappingRepository.Table
                                       where pr.RibbonId == ribbonId
                                       select pr;

            if (productRibbonMapping == null)
                return null;

            return await productRibbonMapping.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<ProductRibbonMapping> GetProductRibbonMappingByIdAsync(int products_Ribbon_MappingId)
        {
            if (products_Ribbon_MappingId == 0)
                return null;

            return await _productRibbonMappingRepository.GetByIdAsync(products_Ribbon_MappingId);
        }

        #endregion

        #region Public Product Ribbon Service

        public async Task<IList<ProductRibbonRecord>> GetAllEnabledProductRibbonsAsync()
        {
            var currentStore = await _storeContext.GetCurrentStoreAsync();

            var key = PrepareKeyForCustomCache(ProductRibbonDefaults.ProductRibbonsAllCacheKey,
                currentStore.Id);

            var ribbons = await _staticCacheManager
                .GetAsync(key, async () => (await GetAllEnabledProductRibbonsAsync(currentStore.Id)).ToList());

            return ribbons;
        }

        public async Task<IList<ProductRibbonRecord>> GetAllEnabledProductRibbonsAsync(int storeId)
        {
            var currentUTC = DateTime.UtcNow;
            var query = from r in _productRibbonRecordRepository.Table
                       where r.Enabled
                          && (r.StartDateUtc == null || r.StartDateUtc <= currentUTC)
                          && (r.EndDateUtc == null || r.EndDateUtc >= currentUTC)
                          && (r.StoreIds == "0" || r.StoreIds.Contains(storeId.ToString()))
                       orderby r.DisplayOrder, r.Id
                       select r;
            return await query.ToListAsync();
        }

        public async Task<ProductPictureRibbon> GetProductPictureRibbonProductIdAsync(int ribbonId, int productId)
        {
            ProductPictureRibbon productPictureRibbon = new();

            var productRibbons = (await _productRibbonMappingRepository.GetAllAsync(query =>
            {
                //filter by ribbon id
                query = query.Where(pr => pr.RibbonId == ribbonId);
                return query;
            }, cache => PrepareKeyForCustomCache(ProductRibbonDefaults.ProductRibbonsMappingsCacheKey,
             ribbonId)))
            .AsQueryable();

            if (productRibbons != null && productRibbons.Any())
            {
                var productRibbon = productRibbons.FirstOrDefault(pr => pr.ProductId == productId);

                if (productRibbon != null)
                    productPictureRibbon = await GetProductPictureRibbonIdAsync(productRibbon.RibbonId);
            }

            return productPictureRibbon;
        }

        

        public async Task<List<ProductRibbonRecord>> GetRibbonByProductIdAsync(int productId)
        {
            List<ProductRibbonRecord> productRibbonList = new();
            var currentStore = await _storeContext.GetCurrentStoreAsync();
            var productRibbon = _productRibbonMappingRepository.Table
               .Where(pr => pr.ProductId == productId).ToList();

            if (productRibbon.Count() > 0)
            {
                productRibbonList = (from pr in productRibbon
                                     join pp in _productRibbonRecordRepository.Table on pr.RibbonId equals pp.Id
                                     where pp.Enabled
                                      && (pp.StartDateUtc <= DateTime.UtcNow || pp.StartDateUtc == null)
                                      && (pp.EndDateUtc >= DateTime.UtcNow || pp.EndDateUtc == null)
                                      && (pp.StoreIds == "0" || pp.StoreIds.Contains(currentStore.Id.ToString()))
                                     orderby pp.DisplayOrder
                                     select pp).ToList();
            }

            return productRibbonList;
        }

        public async Task<ProductPictureRibbon> GetProductPictureRibbonIdAsync(int ribbonId)
        {
            if (ribbonId == 0)
                return null;

            var key = PrepareKeyForCustomCache(ProductRibbonDefaults.ProductPictureByRibbonCacheKey, ribbonId);

            return await _staticCacheManager.GetAsync(key, async () => await GetProductPictureRibbonQuery(ribbonId).FirstOrDefaultAsync());
        }

        protected IQueryable<ProductPictureRibbon> GetProductPictureRibbonQuery(int ribbonId)
        {
            return from p in _productPictureRepository.Table
                   where p.RibbonId == ribbonId && p.Enabled
                   select p;
        }

        #endregion

        #region License 

        public async Task<bool> IsLicenseActiveAsync()
        {
            var cacheKey = PrepareKeyForCustomCache(ProductRibbonDefaults.XITProductRibbons_License);

            // Try to get the result from the cache
            bool cachedResult = await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var productRibbonSetting = await _settingService.LoadSettingAsync<ProductRibbonsSettings>(storeScope);

                var buildDate = GetBuildDate(Assembly.GetExecutingAssembly());
                LicenseImplementer licenseImplementer = new();
                return await licenseImplementer.IsLicenseActiveAsync("XcellenceIT.Plugin.ProductRibbons", productRibbonSetting.LicenseKey, buildDate);
            });

            return cachedResult;
        }

        public DateTime GetBuildDate(Assembly assembly)
        {
            var attribute = assembly.GetCustomAttribute<BuildDateAttribute>();
            return attribute != null ? attribute.DateTime : default;
        }

        #endregion
    }
}
