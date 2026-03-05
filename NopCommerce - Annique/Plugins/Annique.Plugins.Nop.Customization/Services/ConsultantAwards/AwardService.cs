using Annique.Plugins.Nop.Customization.Domain.ConsultantAwards;
using Annique.Plugins.Nop.Customization.Services.PrivateMessagePopUp;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Catalog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Annique.Plugins.Nop.Customization.Services.ConsultantAwards
{
    public class AwardService : IAwardService
    {
        #region Fields

        private readonly IRepository<Award> _awardRepository;
        private readonly IRepository<AwardShoppingCartItem> _awardShoppingCartItemRepository;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly ICustomPrivateMessageService _customPrivateMessageService;

        #endregion

        #region Properties

        protected string RootElementName { get; set; } = "Attributes";

        protected string ChildElementName { get; set; } = "AwardProductAttribute";

        #endregion

        #region Ctor

        public AwardService(IRepository<Award> awardRepository,
            IRepository<AwardShoppingCartItem> awardShoppingCartItemRepository,
            IProductAttributeParser productAttributeParser,
            ICustomPrivateMessageService customPrivateMessageService)
        {
            _awardRepository = awardRepository;
            _awardShoppingCartItemRepository = awardShoppingCartItemRepository;
            _productAttributeParser = productAttributeParser;
            _customPrivateMessageService = customPrivateMessageService;
        }

        #endregion

        #region Award Methods

        /// <summary>
        /// Get awards by customer id
        /// </summary>
        /// <param name="customerId">Customer Identifier</param>
        /// </param>
        /// <returns>
        /// The task result contains the awards assigned to customer
        /// </returns>
        public IList<Award> GetAwardsByCustomerId(int customerId)
        {
            var awards = from a in _awardRepository.Table
                         where a.CustomerId == customerId
                            && DateTime.UtcNow <= a.ExpiryDate
                            && a.OrderId == null || a.OrderId == 0
                         select a;

            return awards.ToList();
        }

        /// <summary>
        /// Gets a Award by Id
        /// </summary>
        /// <param name="id">Id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Award
        /// </returns>
        public async Task<Award> GetAwardByIdAsync(int id)
        {
            return await _awardRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Update Award
        /// </summary>
        /// <param name="award">Award</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// </returns>
        public async Task UpdateAwardAsync(Award award)
        {
            await _awardRepository.UpdateAsync(award);
        }

        #endregion

        #region Award Shopping cart item methods

        /// <summary>
        /// Inserts a Award Shopping cart Item
        /// </summary>
        /// <param name="awardId">award Id</param>
        /// <param name="shoppingCartItem">Shopping Cart item</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task InsertAwardShoppingCartItemAsync(int awardId, ShoppingCartItem shoppingCartItem)
        {
            var awardShoppingCartItem = new AwardShoppingCartItem()
            {
                AwardId = awardId,
                ShoppingCartItemId = shoppingCartItem.Id,
                CustomerId = shoppingCartItem.CustomerId,
                ProductId = shoppingCartItem.ProductId,
                Quantity = shoppingCartItem.Quantity,
                StoreId = shoppingCartItem.StoreId
            };

            await _awardShoppingCartItemRepository.InsertAsync(awardShoppingCartItem);
        }

        /// <summary>
        /// updates a Award Shopping cart Item
        /// </summary>
        /// <param name="awardShoppingCartItem">award Shopping cart item</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task UpdateAwardShoppingCartItemsAsync(AwardShoppingCartItem awardShoppingCartItem)
        {
            await _awardShoppingCartItemRepository.UpdateAsync(awardShoppingCartItem);
        }

        /// <summary>
        /// Get list of Award Shopping cart Items
        /// </summary>
        /// <param name="customerId">CustomerId</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IList<AwardShoppingCartItem>> GetAwardShoppingCartItemsByCustomerIdAsync(int customerId)
        {
            var query = from asci in _awardShoppingCartItemRepository.Table
                        where asci.CustomerId == customerId
                        select asci;

            return await query.ToListAsync();
        }

        /// <summary>
        /// Get list of Award Shopping cart Items by AwardId
        /// </summary>
        /// <param name="awardId">award id</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IList<AwardShoppingCartItem>> GetAwardShoppingCartItemsByAwardIdAsync(int awardId)
        {
            var query = from asci in _awardShoppingCartItemRepository.Table
                        where asci.AwardId == awardId
                        select asci;

            return await query.ToListAsync();
        }

        /// <summary>
        /// Gets a Award Shopping cart item by shopping cart item Id
        /// </summary>
        /// <param name="sciId">Shopping cart item Id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Award
        /// </returns>
        public async Task<AwardShoppingCartItem> GetAwardScibyShoppingCartItemIdAsync(int sciId)
        {
            var query = from asci in _awardShoppingCartItemRepository.Table
                        where asci.ShoppingCartItemId == sciId
                        select asci;

            return await query.FirstOrDefaultAsync();
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Gets selected product attribute values with the quantity entered by the customer
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="productAttributeMappingId">Product attribute mapping identifier</param>
        /// <param name="productAttributeType">Product attribute Type</param>
        ///  <param name="productAttributeTypeValue">Product attribute Type Value</param>
        /// <returns>Collections of pairs of product attribute values and their quantity</returns>
        protected IList<Tuple<string, string>> ParseValuesWithQuantity(string attributesXml, int productAttributeMappingId, string productAttributeType, string productAttributeTypeValue)
        {
            var selectedValues = new List<Tuple<string, string>>();
            if (string.IsNullOrEmpty(attributesXml))
                return selectedValues;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributesXml);

                foreach (XmlNode attributeNode in xmlDoc.SelectNodes(@"//Attributes/" + productAttributeType))
                {
                    if (attributeNode.Attributes?["ID"] == null)
                        continue;

                    if (!int.TryParse(attributeNode.Attributes["ID"].InnerText.Trim(), out var attributeId) ||
                        attributeId != productAttributeMappingId)
                        continue;

                    foreach (XmlNode attributeValue in attributeNode.SelectNodes(productAttributeTypeValue))
                    {
                        var value = attributeValue.SelectSingleNode("Value").InnerText.Trim();
                        var quantityNode = attributeValue.SelectSingleNode("Quantity");
                        selectedValues.Add(new Tuple<string, string>(value, quantityNode != null ? quantityNode.InnerText.Trim() : string.Empty));
                    }
                }
            }
            catch
            {
                // ignored
            }

            return selectedValues;
        }

        /// <summary>
        /// Gets selected attribute identifiers
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <returns>Selected attribute identifiers</returns>
        protected virtual IList<int> ParseAttributeIds(string attributesXml)
        {
            var ids = new List<int>();
            if (string.IsNullOrEmpty(attributesXml))
                return ids;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributesXml);

                var elements = xmlDoc.SelectNodes(@$"//{RootElementName}/{ChildElementName}");

                if (elements == null)
                    return Array.Empty<int>();

                foreach (XmlNode node in elements)
                {
                    if (node.Attributes?["ID"] == null)
                        continue;

                    var attributeValue = node.Attributes["ID"].InnerText.Trim();
                    if (int.TryParse(attributeValue, out var id))
                        ids.Add(id);
                }
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            return ids;
        }

        //parse attributes
        public async Task<object> ParseAttributesAsync(string attributeXml)
        {
            if (!string.IsNullOrEmpty(attributeXml))
            {
                bool containsProductAttribute = ContainsProductAttribute(attributeXml);
                if (containsProductAttribute)
                {
                    return await _productAttributeParser.ParseProductAttributeMappingsAsync(attributeXml);
                }

                bool containsAwardProductAttribute = ContainsAwardProductAttribute(attributeXml);
                if (containsAwardProductAttribute)
                {
                    return await ParseAwardProductAttributeAsync(attributeXml);
                }
            }

            // If neither ProductAttribute nor AwardProductAttribute is found, return null
            return null;
        }

        #endregion

        #region Award Attribute methods

        //Check for product attribute in attributeXml
        public bool ContainsProductAttribute(string attributeXml)
        {
            XDocument xdoc = XDocument.Parse(attributeXml);
            return xdoc.Descendants("ProductAttribute").Any();
        }

        //check for Award product attribute in attributeXml
        public bool ContainsAwardProductAttribute(string attributeXml)
        {
            if (!string.IsNullOrEmpty(attributeXml))
            {
                XDocument xdoc = XDocument.Parse(attributeXml);
                return xdoc.Descendants("AwardProductAttribute").Any();
            }
            return false;
        }

        /// <summary>
        /// Gets selected awards
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the selected Awards
        /// </returns>
        public virtual async Task<IList<Award>> ParseAwardProductAttributeAsync(string attributesXml)
        {
            var result = new List<Award>();
            if (string.IsNullOrEmpty(attributesXml))
                return result;

            var ids = ParseAttributeIds(attributesXml);
            foreach (var id in ids)
            {
                var award = await GetAwardByIdAsync(id);
                if (award != null)
                    result.Add(award);
            }

            return result;
        }

        /// <summary>
        /// Adds an Award attribute
        /// </summary>
        /// <param name="attributesXml">Attributes in XML format</param>
        /// <param name="awardId">Award id</param>
        /// <param name="value">Value</param>
        /// <returns>Updated result (XML format)</returns>
        public string AddAwardProductAttribute(string attributesXml, int awardId, string value)
        {
            var result = string.Empty;
            try
            {
                var xmlDoc = new XmlDocument();
                if (string.IsNullOrEmpty(attributesXml))
                {
                    var element1 = xmlDoc.CreateElement("Attributes");
                    xmlDoc.AppendChild(element1);
                }
                else
                {
                    xmlDoc.LoadXml(attributesXml);
                }

                var rootElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes");

                XmlElement attributeElement = null;
                //find existing
                var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/AwardProductAttribute");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes?["ID"] == null)
                        continue;

                    var str1 = node1.Attributes["ID"].InnerText.Trim();
                    if (!int.TryParse(str1, out var id))
                        continue;

                    if (id != awardId)
                        continue;

                    attributeElement = (XmlElement)node1;
                    break;
                }

                //create new one if not found
                if (attributeElement == null)
                {
                    attributeElement = xmlDoc.CreateElement("AwardProductAttribute");
                    attributeElement.SetAttribute("ID", awardId.ToString());
                    rootElement.AppendChild(attributeElement);
                }

                var attributeValueElement = xmlDoc.CreateElement("AwardProductAttributeValue");
                attributeElement.AppendChild(attributeValueElement);

                var attributeValueValueElement = xmlDoc.CreateElement("Value");
                attributeValueValueElement.InnerText = value;
                attributeValueElement.AppendChild(attributeValueValueElement);

                result = xmlDoc.OuterXml;
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }

            return result;
        }

        /// <summary>
        /// Are attributes equal
        /// </summary>
        /// <param name="attributesXml1">The attributes of the first product</param>
        /// <param name="attributesXml2">The attributes of the second product</param>
        /// <param name="ignoreNonCombinableAttributes">A value indicating whether we should ignore non-combinable attributes</param>
        /// <param name="ignoreQuantity">A value indicating whether we should ignore the quantity of attribute value entered by the customer</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the result
        /// </returns>
        public virtual async Task<bool> AreProductAttributesEqualAsync(string attributesXml1, string attributesXml2, bool ignoreNonCombinableAttributes, bool ignoreQuantity = true)
        {
            var attributes1 = await ParseAttributesAsync(attributesXml1);
            if (attributes1 is List<ProductAttributeMapping> productAttributes1 && ignoreNonCombinableAttributes)
                productAttributes1 = productAttributes1.Where(x => !x.IsNonCombinable()).ToList();

            var attributes2 = await ParseAttributesAsync(attributesXml2);
            if (attributes2 is List<ProductAttributeMapping> productAttributes2 && ignoreNonCombinableAttributes)
                productAttributes2 = productAttributes2.Where(x => !x.IsNonCombinable()).ToList();

            var attributesEqual = true;
            if (attributes1 is List<ProductAttributeMapping> && attributes2 is List<ProductAttributeMapping>)
            {
                // Both attributes1 and attributes2 are of type List<ProductAttributeMapping>
                var productAttribute1 = (List<ProductAttributeMapping>)attributes1;
                var productAttribute2 = (List<ProductAttributeMapping>)attributes2;

                if (productAttribute1.Count != productAttribute2.Count)
                    return false;

                foreach (var a1 in productAttribute1)
                {
                    var hasAttribute = false;
                    foreach (var a2 in productAttribute2)
                    {
                        if (a1.Id != a2.Id)
                            continue;

                        hasAttribute = true;
                        var values1Str = ParseValuesWithQuantity(attributesXml1, a1.Id, "ProductAttribute", "ProductAttributeValue");
                        var values2Str = ParseValuesWithQuantity(attributesXml2, a2.Id, "ProductAttribute", "ProductAttributeValue");
                        if (values1Str.Count == values2Str.Count)
                        {
                            foreach (var str1 in values1Str)
                            {
                                var hasValue = false;
                                foreach (var str2 in values2Str)
                                {
                                    //case insensitive? 
                                    //if (str1.Trim().ToLowerInvariant() == str2.Trim().ToLowerInvariant())
                                    if (str1.Item1.Trim() != str2.Item1.Trim())
                                        continue;

                                    hasValue = ignoreQuantity || str1.Item2.Trim() == str2.Item2.Trim();
                                    break;
                                }

                                if (hasValue)
                                    continue;

                                attributesEqual = false;
                                break;
                            }
                        }
                        else
                        {
                            attributesEqual = false;
                            break;
                        }
                    }

                    if (hasAttribute)
                        continue;

                    attributesEqual = false;
                    break;
                }

                return attributesEqual;
            }
            else if (attributes1 is List<Award> && attributes2 is List<Award>)
            {
                // Both attributes1 and attributes2 are of type List<Award>
                var awardAttributes1 = (List<Award>)attributes1;
                var awardAttributes2 = (List<Award>)attributes2;

                if (awardAttributes1.Count != awardAttributes2.Count)
                    return false;

                foreach (var a1 in awardAttributes1)
                {
                    var hasAttribute = false;
                    foreach (var a2 in awardAttributes2)
                    {
                        if (a1.Id != a2.Id)
                            continue;

                        hasAttribute = true;
                        var values1Str = ParseValuesWithQuantity(attributesXml1, a1.Id, "AwardProductAttribute", "AwardProductAttributeValue");
                        var values2Str = ParseValuesWithQuantity(attributesXml2, a2.Id, "AwardProductAttribute", "AwardProductAttributeValue");
                        if (values1Str.Count == values2Str.Count)
                        {
                            foreach (var str1 in values1Str)
                            {
                                var hasValue = false;
                                foreach (var str2 in values2Str)
                                {
                                    //case insensitive? 
                                    //if (str1.Trim().ToLowerInvariant() == str2.Trim().ToLowerInvariant())
                                    if (str1.Item1.Trim() != str2.Item1.Trim())
                                        continue;

                                    hasValue = ignoreQuantity || str1.Item2.Trim() == str2.Item2.Trim();
                                    break;
                                }

                                if (hasValue)
                                    continue;

                                attributesEqual = false;
                                break;
                            }
                        }
                        else
                        {
                            attributesEqual = false;
                            break;
                        }
                    }

                    if (hasAttribute)
                        continue;

                    attributesEqual = false;
                    break;
                }

                return attributesEqual;
            }
            else if (string.IsNullOrEmpty(attributesXml1) && string.IsNullOrEmpty(attributesXml2))
            {
                attributesEqual = true;
            }
            else
            {
                attributesEqual = false;
            }

            return attributesEqual;
        }

        /// <summary>
        /// Get distinct award id from order items
        /// </summary>
        /// <param name="orderItems">Order items</param>
        /// <returns>
        /// The task result retuns distincts award ids
        public IList<int> GetDistinctAwardProductAttributeIdsFromOrderItems(IList<OrderItem> orderItems)
        {
            var distinctIds = new List<int>();

            foreach (var orderItem in orderItems)
            {
                var attributeXml = orderItem.AttributesXml;

                if (string.IsNullOrEmpty(attributeXml))
                    continue;

                if (ContainsAwardProductAttribute(attributeXml))
                {
                    var ids = ParseAttributeIds(attributeXml);
                    distinctIds.AddRange(ids);
                }
            }

            distinctIds = distinctIds.Distinct().ToList();
            return distinctIds;
        }

        /// <summary>
        /// Process award takens on order paid
        /// </summary>
        /// <param name="orderItems">Order items</param>
        /// <returns>
        /// The task process awards taken on order paid
        public async Task ProcessAwardsTakenAsync(IList<OrderItem> orderItems)
        {
            var selectedAwardIds = GetDistinctAwardProductAttributeIdsFromOrderItems(orderItems);
            var orderId = orderItems.Select(o => o.OrderId).FirstOrDefault();

            if (selectedAwardIds.Count > 0)
            {
                foreach (var awardId in selectedAwardIds)
                {
                    //get award by id
                    var award = await GetAwardByIdAsync(awardId);
                    if (award != null && !award.dtaken.HasValue)
                    {
                        //update award order it and taken fields
                        award.OrderId = orderId;
                        award.dtaken = DateTime.Now;
                        await UpdateAwardAsync(award);

                        //handle private message
                        await _customPrivateMessageService.HandlePrivateMessageAsync(awardId: award.Id);
                    }
                }
            }
        }

        #endregion
    }
}