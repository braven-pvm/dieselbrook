using System;
using AutoMapper;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper
{
    public abstract class BaseMapperConfiguration: Profile, IOrderedMapperProfile
    {
        #region Utilites

        protected virtual void CreateDtoMap<EntityType, DtoType>(Action<IMappingExpression<DtoType, EntityType>> entityIgnoreRule = null, Action<IMappingExpression<EntityType, DtoType>> dtoIgnoreRule = null)
        where DtoType : BaseDto
        {
            var dtoMap = CreateMap<EntityType, DtoType>();
            if (dtoIgnoreRule != null)
                dtoIgnoreRule(dtoMap);

            var entityMap = CreateMap<DtoType, EntityType>();

            if (entityIgnoreRule == null)
                return;

            entityIgnoreRule(entityMap);
        }

        #endregion


        /// <summary>
        /// Order of this mapper implementation
        /// </summary>
        public int Order => 0;
    }
}
