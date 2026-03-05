using Newtonsoft.Json;

namespace Annique.Plugins.Nop.Customization.Models.PickUpCollection
{
    public record PostalCodesResponseModel
    {
        [JsonProperty("postalCodes")]
        public PostalCodes[] PostalCodes { get; set; }
    }

    public class PostalCodes
    {
        [JsonProperty("lng")]
        public float Longitude { get; set; }

        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("postalcode")]
        public string PostalCode { get; set; }

        [JsonProperty("placeName")]
        public string PlaceName { get; set; }

        [JsonProperty("lat")]
        public float Latitude { get; set; }
    }
}
