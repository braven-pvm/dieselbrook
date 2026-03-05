namespace Nop.Plugin.Misc.WebApi.Framework.Dto
{
    public abstract class PagedDto : BaseDto
    {
        /// <summary>
        /// Page index
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// Page size
        /// </summary>
        public int PageSize { get; set; }
    }
}
