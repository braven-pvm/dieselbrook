using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto
{
    public partial class BaseModelDtoRequest<ModelType> : BaseDto
    where ModelType : BaseDto
    {
        public ModelType Model { get; set; }
        
        public IDictionary<string, string> Form { get; set; }
    }
}
