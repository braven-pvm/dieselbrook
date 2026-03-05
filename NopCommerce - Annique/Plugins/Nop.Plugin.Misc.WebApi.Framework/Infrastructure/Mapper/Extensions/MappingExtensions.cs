using System;
using Nop.Core.Infrastructure.Mapper;
using Nop.Plugin.Misc.WebApi.Framework.Dto;

namespace Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions
{
    public static class MappingExtensions
    {
        #region Utilities

        /// <summary>
        /// Execute a mapping from the source object to a new destination object. The source type is inferred from the source object
        /// </summary>
        /// <typeparam name="TDestination">Destination object type</typeparam>
        /// <param name="source">Source object to map from</param>
        /// <returns>Mapped destination object</returns>
        private static TDestination Map<TDestination>(this object source)
        {
            //use AutoMapper for mapping objects
            return AutoMapperConfiguration.Mapper.Map<TDestination>(source);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Execute a mapping from the entity to the existing base JSON DTO
        /// </summary>
        /// <typeparam name="TBaseJsonDto">Base JSON DTO type</typeparam>
        /// <param name="entity">Entity to map from</param>
        /// <returns>Mapped base JSON DTO</returns>
        public static TBaseJsonDto ToDto<TBaseJsonDto>(this object entity)
            where TBaseJsonDto : BaseDto
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            return entity.Map<TBaseJsonDto>();
        }

        /// <summary>
        /// Execute a mapping from existing DTO to the entity 
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="dto">Dto to map from</param>
        /// <returns></returns>
        public static TEntity FromDto<TEntity>(this BaseDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            return dto.Map<TEntity>();
        }

        #endregion
    }
}
