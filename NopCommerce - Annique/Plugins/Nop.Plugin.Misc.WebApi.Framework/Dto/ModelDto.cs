using System.Collections.Generic;

namespace Nop.Plugin.Misc.WebApi.Framework.Dto
{
    public abstract class ModelDto : BaseDto
    {
        /// <summary>
        /// Gets or sets property to store any custom values for models 
        /// </summary>
        public Dictionary<string, string> CustomProperties { get; set; }
    }
}
