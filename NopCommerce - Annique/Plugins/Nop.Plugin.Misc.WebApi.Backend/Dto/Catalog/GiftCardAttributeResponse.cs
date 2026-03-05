using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog
{
    /// <summary>
    /// Represents a gift card attribute
    /// </summary>
    public partial class GiftCardAttributeResponse : DtoWithId
    {
        /// <summary>
        /// Recipient name
        /// </summary>
        public string RecipientName { get; set; }

        /// <summary>
        /// Recipient email
        /// </summary>
        public string RecipientEmail { get; set; }

        /// <summary>
        /// Sender name
        /// </summary>
        public string SenderName { get; set; }

        /// <summary>
        /// Sender email
        /// </summary>
        public string SenderEmail { get; set; }

        /// <summary>
        /// Message
        /// </summary>
        public string GiftCardMessage { get; set; }
    }
}
