using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Customer
{
    public partial class CustomerAttributeModelDto : ModelWithIdDto
    {
        public string Name { get; set; }

        public bool IsRequired { get; set; }

        /// <summary>
        /// Default value
        /// </summary>
        public string DefaultValue { get; set; }

        public AttributeControlType AttributeControlType { get; set; }

        public IList<CustomerAttributeValueModelDto> Values { get; set; }
    }
}
