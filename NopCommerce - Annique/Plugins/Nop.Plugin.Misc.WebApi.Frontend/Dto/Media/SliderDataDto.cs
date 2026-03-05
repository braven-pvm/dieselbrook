using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Product;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Media
{
    public class SliderDataDto : ModelDto
    {
        public List<SliderProductDto> Products { get; set; } = new();

        #region Nested classes

        public class SliderProductDto : ModelDto
        {
            public PictureModelDto Picture { get; set; }

            public int ProductId { get; set; }
        }

        #endregion
    }
}
