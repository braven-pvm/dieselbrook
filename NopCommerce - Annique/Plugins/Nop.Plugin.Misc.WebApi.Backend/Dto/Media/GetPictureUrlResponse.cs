using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Backend.Dto.Media
{
    public partial class GetPictureUrlResponse : BaseDto
    {
        /// <summary>
        /// Picture URL
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Picture DTO
        /// </summary>
        public PictureDto Picture { get; set; }
    }
}
