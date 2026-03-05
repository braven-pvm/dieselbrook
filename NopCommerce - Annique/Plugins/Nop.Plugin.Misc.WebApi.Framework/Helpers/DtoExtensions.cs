using Nop.Core;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Framework.Helpers
{
    public static class DtoExtensions
    {
        public static PagedListDto<TEntity, T> ToPagedListDto<TEntity, T>(this IPagedList<TEntity> items) where T : BaseDto
        {
            return new(items);
        }
    }
}
