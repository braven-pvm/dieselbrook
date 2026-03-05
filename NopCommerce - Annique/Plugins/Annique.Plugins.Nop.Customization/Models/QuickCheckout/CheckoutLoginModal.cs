using Nop.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Annique.Plugins.Nop.Customization.Models.QuickCheckout
{
    public partial record CheckoutLoginModal
    {
        [NopResourceDisplayName("Account.Login.Fields.Name")]
        [Required]
        public string Username { get; set; }

        [DataType(DataType.Password)]
        [NopResourceDisplayName("Account.Login.Fields.Password")]
        [Required]
        public string Password { get; set; }
    }
}
