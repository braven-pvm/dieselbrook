using Newtonsoft.Json;

namespace Annique.Plugins.Nop.Customization.Models.ShippingAddressValidation
{
    public record SubrubResponseModel
    {
        [JsonProperty("text")]
        public string Text { get; set; }

        [JsonProperty("value")]
        public string Value { get; set;}
    }
}
