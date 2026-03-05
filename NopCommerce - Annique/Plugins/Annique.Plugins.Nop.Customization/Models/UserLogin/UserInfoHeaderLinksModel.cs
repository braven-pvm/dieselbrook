using Nop.Web.Framework.Models;

namespace Annique.Plugins.Nop.Customization.Models.UserLogin
{
    public record UserInfoHeaderLinksModel : BaseNopModel
    {
        public string CustomerFullName { get; set; }
    }
}
