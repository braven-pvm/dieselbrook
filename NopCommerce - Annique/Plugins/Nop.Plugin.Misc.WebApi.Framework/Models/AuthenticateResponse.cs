using Newtonsoft.Json;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Framework.Models
{
    public class AuthenticateResponse : BaseDto
    {
        public AuthenticateResponse(string token)
        {
            Token = token;
        }

        public string Username { get; set; }

        public int CustomerId { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Token { get; set; }
    }
}
