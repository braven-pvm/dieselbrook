using System.Collections.Generic;

namespace Annique.Plugins.Nop.Customization.Models.ChatbotAnnalie
{
    public class ChatbotAddProductToCartResponse
    {
        public IList<string> Errors { get; set; }

        public bool Success { get; set; }

        public string Message { get; set; }
    }

}
