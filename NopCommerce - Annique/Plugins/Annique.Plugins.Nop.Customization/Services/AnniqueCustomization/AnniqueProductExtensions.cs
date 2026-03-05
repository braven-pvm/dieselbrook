using Annique.Plugins.Nop.Customization.Domain.Enums;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Data;
using System.Linq;

namespace Annique.Plugins.Nop.Customization.Services.AnniqueCustomization
{
    public static class AnniqueProductExtensions
    {
        /// <summary>
        /// Sorts the elements of a sequence in order according to a product sorting rule
        /// </summary>
        /// <param name="productsQuery">A sequence of products to order</param>
        /// <param name="currentLanguage">Current language</param>
        /// <param name="orderBy">Product sorting rule</param>
        /// <param name="localizedPropertyRepository">Localized property repository</param>
        /// <returns>An System.Linq.IOrderedQueryable`1 whose elements are sorted according to a rule.</returns>
        /// <remarks>
        /// If <paramref name="orderBy"/> is set to <c>Position</c> and passed <paramref name="productsQuery"/> is
        /// ordered sorting rule will be skipped
        /// </remarks>
        public static IQueryable<Product> OrderBy(this IQueryable<Product> productsQuery, IRepository<LocalizedProperty> localizedPropertyRepository, Language currentLanguage, AnniqueProductSortingEnum orderBy)
        {
            if (orderBy == AnniqueProductSortingEnum.NameAsc || orderBy == AnniqueProductSortingEnum.NameDesc)
            {
                var currentLanguageId = currentLanguage.Id;

                var query =
                    from product in productsQuery
                    join localizedProperty in localizedPropertyRepository.Table on new
                    {
                        product.Id,
                        languageId = currentLanguageId,
                        keyGroup = nameof(Product),
                        key = nameof(Product.Name)
                    }
                        equals new
                        {
                            Id = localizedProperty.EntityId,
                            languageId = localizedProperty.LanguageId,
                            keyGroup = localizedProperty.LocaleKeyGroup,
                            key = localizedProperty.LocaleKey
                        } into localizedProperties
                    from localizedProperty in localizedProperties.DefaultIfEmpty(new LocalizedProperty { LocaleValue = product.Name })
                    select new { localizedProperty, product };

                if (orderBy == AnniqueProductSortingEnum.NameAsc)
                    productsQuery = from item in query
                                    orderby item.localizedProperty.LocaleValue, item.product.Name
                                    select item.product;
                else
                    productsQuery = from item in query
                                    orderby item.localizedProperty.LocaleValue descending, item.product.Name descending
                                    select item.product;

                return productsQuery;
            }

            return orderBy switch
            {
                AnniqueProductSortingEnum.PriceAsc => productsQuery.OrderBy(p => p.Price),
                AnniqueProductSortingEnum.PriceDesc => productsQuery.OrderByDescending(p => p.Price),
                AnniqueProductSortingEnum.CreatedOn => productsQuery.OrderByDescending(p => p.CreatedOnUtc),
                AnniqueProductSortingEnum.ItemCode => productsQuery.OrderBy(p => p.Sku),
                AnniqueProductSortingEnum.Position when productsQuery is IOrderedQueryable => productsQuery,
                _ => productsQuery.OrderBy(p => p.DisplayOrder).ThenBy(p => p.Id)
            };
        }
    }
}
