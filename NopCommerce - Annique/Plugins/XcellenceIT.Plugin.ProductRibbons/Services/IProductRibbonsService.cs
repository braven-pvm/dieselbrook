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
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using XcellenceIT.Plugin.ProductRibbons.Domain;

namespace XcellenceIT.Plugin.ProductRibbons.Services
{
    public partial interface IProductRibbonsService
    {
        #region Product Ribbons Service

        /// <summary>
        /// Gets all productRibbon Record
        /// </summary>
        /// <param name="productRibbonRecord">The productRibbonRecord identifier</param>
        /// <returns>productRibbonRecord</returns>
        Task<IPagedList<ProductRibbonRecord>> GetAllProductRibbonsAsync(string name = null, DateTime? startDateUtc = null, DateTime? endDateUtc = null, int displayOrder = 0, bool enabled = false, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets a productRibbon Record
        /// </summary>
        /// <param name="productRibbonRecord">The productRibbonRecord identifier</param>
        /// <returns>productRibbonRecord</returns>
        Task<ProductRibbonRecord> GetProductRibbonByIdAsync(int ribbonId);

        /// <summary>
        /// insert a productRibbonRecord
        /// </summary>
        /// <param name="productRibbonRecord">productRibbonRecord</param>
        Task InsertProductRibbonAsync(ProductRibbonRecord productRibbonRecord);

        /// <summary>
        /// Update a productRibbonRecord
        /// </summary>
        /// <param name="ProductRibbon">productRibbonRecord</param>
        Task UpdateProductRibbonAsync(ProductRibbonRecord productRibbonRecord);

        /// <summary>
        /// Deletes a productRibbonRecord
        /// </summary>
        /// <param name="productRibbonRecord">productRibbonRecord</param>
        Task DeleteProductRibbonAsync(ProductRibbonRecord productRibbonRecord);

        #endregion

        #region product Picture Ribbon Ribbons Service

        /// <summary>
        /// Gets a productPictureRibbon Record
        /// </summary>
        /// <param name="productPictureRibbon">The productPictureRibbon identifier</param>
        /// <returns>productPictureRibbon</returns>
        Task<ProductPictureRibbon> GetProductPictureRibbonByIdAsync(int ribbonId);

        /// <summary>
        /// insert a productPictureRibbon
        /// </summary>
        /// <param name="productPictureRibbon">productPictureRibbon</param>
        Task InsertProductPictureRibbonAsync(ProductPictureRibbon productPictureRibbon);

        /// <summary>
        /// Update a productPictureRibbon
        /// </summary>
        /// <param name="productPictureRibbon">productPictureRibbon</param>
        Task UpdateProductPictureRibbonAsync(ProductPictureRibbon productPictureRibbon);

        #endregion

        #region ProductRibbon Mapping

        /// <summary>
        /// Deletes a productRibbonMapping
        /// </summary>
        /// <param name="ProductRibbonMapping">productRibbonMapping Record</param>
        Task DeleteProductRibbonMappingAsync(ProductRibbonMapping productRibbonMapping);

        Task<ProductRibbonMapping> GetProductRibbonMappingByIdAsync(int products_Ribbon_MappingId);
        /// <summary>
        /// Inserts a ProductRibbonMapping
        /// </summary>
        /// <param name="ProductRibbonMapping">ProductRibbonMapping</param>
        /// 
        Task InsertProductRibbonMappingAsync(ProductRibbonMapping productRibbonMapping);

        /// <summary>
        /// Returns a ProductCategory that has the specified values
        /// </summary>
        /// <param name="source">Source</param>
        /// <param name="productId">Product identifier</param>
        /// <param name="ribbonId">ribbonId identifier</param>
        /// <returns>A ProductRibbonMapping that has the specified values; otherwise null</returns>
        Task<ProductRibbonMapping> FindProductRibbonAsync(IList<ProductRibbonMapping> source,
           int productId, int ribbonId);

        /// <summary>
        /// Gets a productRibbonMapping Record
        /// </summary>
        /// <param name="ribbonId">The productRibbonMapping identifier</param>
        /// <returns>productRibbonMapping</returns>
        Task<IPagedList<ProductRibbonMapping>> GetProductRibbonMappingRibbonIdAsync(int ribbonId, int pageIndex = 0, int pageSize = int.MaxValue);

        #endregion

        #region Public Product Ribbon Service

        Task<IList<ProductRibbonRecord>> GetAllEnabledProductRibbonsAsync();

        Task<ProductPictureRibbon> GetProductPictureRibbonProductIdAsync(int ribbonId, int productId);
        Task<List<ProductRibbonRecord>> GetRibbonByProductIdAsync(int productId);

        Task<ProductPictureRibbon> GetProductPictureRibbonIdAsync(int ribbonId);

        #endregion

        #region License 
        Task<bool> IsLicenseActiveAsync();

        DateTime GetBuildDate(Assembly assembly);
        #endregion

        #region Custom caching

        CacheKey PrepareKeyForCustomCache(CacheKey cacheKey, params object[] cacheKeyParameters);

        #endregion

    }
}
