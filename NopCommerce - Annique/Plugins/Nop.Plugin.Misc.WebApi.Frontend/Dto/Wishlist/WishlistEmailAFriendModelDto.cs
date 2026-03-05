using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Wishlist
{
    public partial class WishlistEmailAFriendModelDto : ModelDto
    {
        public string FriendEmail { get; set; }

        public string YourEmailAddress { get; set; }

        public string PersonalMessage { get; set; }

        public bool SuccessfullySent { get; set; }

        public string Result { get; set; }

        public bool DisplayCaptcha { get; set; }
    }
}
