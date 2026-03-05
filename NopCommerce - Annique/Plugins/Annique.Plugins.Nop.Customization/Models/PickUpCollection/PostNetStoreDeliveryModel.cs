using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Annique.Plugins.Nop.Customization.Models.PickUpCollection
{
    /// <summary>
    /// postnet store delivery model
    /// </summary>
    public record PostNetStoreDeliveryModel : BaseNopModel
    {
        public PostNetStoreDeliveryModel()
        {
            PickupPoints = new List<CustomCheckoutPickupPointModel>();
        }
        //gets sets customer First Name
        [NopResourceDisplayName("Annique.Plugin.PostNetStoreDelivery.Fields.FirstName")]
        public string FirstName { get; set; }

        //gets sets customer Last Name
        [NopResourceDisplayName("Annique.Plugin.PostNetStoreDelivery.Fields.LastName")]
        public string LastName { get; set; }

        //gets sets customer Cell number
        [NopResourceDisplayName("Annique.Plugin.PostNetStoreDelivery.Fields.Cell")]
        public string Cell { get; set; }

        //gets sets confirm cell
        [NopResourceDisplayName("Annique.Plugin.PostNetStoreDelivery.Fields.ConfirmCell")]
        public string ConfirmCell { get; set; }

        //gets sets Postal code
        [NopResourceDisplayName("Annique.Plugin.PostNetStoreDelivery.Fields.Location")]
        public string Location { get; set; }

        public IList<CustomCheckoutPickupPointModel> PickupPoints { get; set; }
    }
}
