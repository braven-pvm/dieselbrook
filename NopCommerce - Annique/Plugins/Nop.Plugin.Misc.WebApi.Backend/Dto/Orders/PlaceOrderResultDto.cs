using System.Collections.Generic;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Backend.Dto.Orders
{
    public partial class PlaceOrderResultDto : BaseDto
    {
        /// <summary>
        /// Gets a value indicating whether request has been completed successfully
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Errors
        /// </summary>
        public IList<string> Errors { get; set; }

        /// <summary>
        /// Gets or sets the placed order
        /// </summary>
        public OrderDto PlacedOrder { get; set; }
    }
}
