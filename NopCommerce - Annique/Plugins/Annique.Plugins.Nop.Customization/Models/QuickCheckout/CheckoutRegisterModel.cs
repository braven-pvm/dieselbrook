using Nop.Web.Framework.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations;

namespace Annique.Plugins.Nop.Customization.Models.QuickCheckout
{
    public partial record CheckoutRegisterModel
    {
        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Account.Fields.Email")]
        public string Email { get; set; }

        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Account.Fields.ConfirmEmail")]
        public string ConfirmEmail { get; set; }


        [DataType(DataType.Password)]
        [NopResourceDisplayName("Account.Fields.Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [NopResourceDisplayName("Account.Fields.ConfirmPassword")]
        public string ConfirmPassword { get; set; }
        
        [NopResourceDisplayName("Account.Register.Fields.Name")]
        public string Name { get; set; }

        [DataType(DataType.PhoneNumber)]
        [NopResourceDisplayName("Account.Register.Fields.Phone")]
        public string Phone { get; set; }
    }
}
