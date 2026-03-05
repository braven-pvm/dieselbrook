using Nop.Web.Framework.Models;

namespace Annique.Plugins.Nop.Customization.Models.UserLogin
{
    public record UserLoginTimeOutModel : BaseNopModel
    {
        public bool IsUserLoggedIn { get; set; }

        public int LoginTimeOutMinutes { get; set;}
    }
}
