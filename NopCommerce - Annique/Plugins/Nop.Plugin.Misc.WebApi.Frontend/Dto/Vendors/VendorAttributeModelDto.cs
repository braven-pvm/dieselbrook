using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Vendors
{
    public partial class VendorAttributeModelDto : ModelWithIdDto
    {
        public string Name { get; set; }

        public bool IsRequired { get; set; }

        public string DefaultValue { get; set; }

        public int AttributeControlType { get; set; }

        public IList<VendorAttributeValueModelDto> Values { get; set; }
    }
}
