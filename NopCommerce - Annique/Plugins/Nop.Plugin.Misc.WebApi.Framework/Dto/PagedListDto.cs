using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;

namespace Nop.Plugin.Misc.WebApi.Framework.Dto
{
    public class PagedListDto<TEntity, T> : PagedDto
        where T: BaseDto
    {
        public PagedListDto()
        {
        }
        
        public PagedListDto(IPagedList<TEntity> items)
        {
            PageIndex = items.PageIndex;
            PageSize = items.PageSize;
            TotalCount = items.TotalCount;
            TotalPages = items.TotalPages;
            HasPreviousPage = items.HasPreviousPage;
            HasNextPage = items.HasNextPage;

            Items.AddRange(items.Select(p => p.ToDto<T>()));
        }
        
        /// <summary>
        /// Total count
        /// </summary>
        public int TotalCount { get; }

        /// <summary>
        /// Total pages
        /// </summary>
        public int TotalPages { get; }

        /// <summary>
        /// Has previous page
        /// </summary>
        public bool HasPreviousPage { get; }

        /// <summary>
        /// Has next age
        /// </summary>
        public bool HasNextPage { get; }

        /// <summary>
        /// Items
        /// </summary>
        public List<T> Items { get; set; } = new();
    }
}
