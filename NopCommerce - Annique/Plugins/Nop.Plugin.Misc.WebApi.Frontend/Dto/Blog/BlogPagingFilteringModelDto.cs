using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Frontend.Dto.Blog
{
    public partial class BlogPagingFilteringModelDto : ModelDto
    {
        public string Month { get; set; }

        public string Tag { get; set; }

        /// <summary>
        /// The current page number (starts from 1)
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// The number of items in each page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// The total number of items.
        /// </summary>
        public int TotalItems { get; set; }

        /// <summary>
        /// The total number of pages.
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// The index of the first item in the page.
        /// </summary>
        public int FirstItem { get; set; }

        /// <summary>
        /// The index of the last item in the page.
        /// </summary>
        public int LastItem { get; set; }

        /// <summary>
        /// Whether there are pages before the current page.
        /// </summary>
        public bool HasPreviousPage { get; set; }

        /// <summary>
        /// Whether there are pages after the current page.
        /// </summary>
        public bool HasNextPage { get; set; }
    }
}
