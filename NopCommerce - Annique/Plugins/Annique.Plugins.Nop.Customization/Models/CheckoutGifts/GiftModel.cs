using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;
using System.Collections.Generic;

namespace Annique.Plugins.Nop.Customization.Models.CheckoutGifts
{
    public record GiftModel : BaseNopModel
    {
        #region Ctor

        public GiftModel()
        {
            GiftItems = new List<GiftItemsModel>();
            ExclusiveItems = new List<ExclusiveItemsModel>();
            SpecialOffers = new List<SpecialOfferModel>();
        }

        #endregion

        #region Property

        public string DonationButtonPrice1 { get; set; }
        public string DonationButtonPrice2 { get; set; }
        public string DonationButtonPrice3 { get; set; }
        
        public int DonationProductQtyInCart { get; set; }
        public IList<GiftItemsModel> GiftItems { get; set; }

        public IList<ExclusiveItemsModel> ExclusiveItems { get; set; }

        public IList<SpecialOfferModel> SpecialOffers { get; set; }


        #endregion

        #region Nested Class

        public record GiftItemsModel : BaseNopModel
        {
            public GiftItemsModel()
            {
                AvailableQuantities = new List<SelectListItem>();
            }

            public int ProductId { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            public bool IsAlreadyInCart { get; set; }

            public IList<SelectListItem> AvailableQuantities { get; set; }

            public int AvailableQuanitity { get; set; }

            public int GiftQtyLimit { get; set; }

            public string OldPrice { get; set; }

            public string Price { get; set; }

            public PictureModel PictureModel { get; set; }

            public bool IsDonationProduct { get; set; }
        }

        public record ExclusiveItemsModel : BaseNopModel
        {
            public ExclusiveItemsModel()
            {
                AvailableQuantities = new List<SelectListItem>();
            }

            public int ProductId { get; set; }

            public string Name { get; set; }

            public string Description { get; set; }

            public bool IsAlreadyInCart { get; set; }

            public IList<SelectListItem> AvailableQuantities { get; set; }

            public int AvailableQuanitity { get; set; }

            public int QtyLimit { get; set; }

            public string Price { get; set; }

            public PictureModel PictureModel { get; set; }
        }

        public record SpecialOfferModel : BaseNopModel 
        {
            public int OfferId { get; set; }
            public int DiscountId { get; set; }
            public string DiscountName { get; set; }

            public int AllowedSelections { get; set; }

            public string BackgroundImageUrl { get; set; }
        }

        #endregion
    }
}
