using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.ShoppingCart
{
    public partial class CheckoutAttributeModelDto : ModelWithIdDto
    {
        public string Name { get; set; }

        public string DefaultValue { get; set; }

        public string TextPrompt { get; set; }

        public bool IsRequired { get; set; }

        public int? SelectedDay { get; set; }
        
        public int? SelectedMonth { get; set; }
        
        public int? SelectedYear { get; set; }

        public IList<string> AllowedFileExtensions { get; set; }

        public AttributeControlType AttributeControlType { get; set; }

        public IList<CheckoutAttributeValueModelDto> Values { get; set; }
    }
}
