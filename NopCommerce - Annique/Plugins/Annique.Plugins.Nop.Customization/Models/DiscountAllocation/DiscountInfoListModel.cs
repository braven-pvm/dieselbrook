using Nop.Core.Domain.Discounts;
using System.Collections.Generic;

namespace Annique.Plugins.Nop.Customization.Models.DiscountAllocation
{
    public class DiscountInfoListModel
    {
        public DiscountInfoListModel()
        {
            AvailableDiscounts = new List<AvailableDiscountModel>();
            AppliedDiscountNames = new List<string>();
        }

        public IList<AvailableDiscountModel> AvailableDiscounts { get; set; }
        public IList<string> AppliedDiscountNames { get; set; }

        public bool HasAutoAppliedDiscount { get; set; }
    }

    public class AvailableDiscountModel
    {
        public string Name { get; set; }
        public string CouponCode { get; set; }

        public DiscountType DiscountType { get; set; }

        // full discount entity
        public Discount Discount { get; set; }

        // controls if show radio button for selecton or not
        public bool ShowRadioButton { get; set; } = true;
    }
}
