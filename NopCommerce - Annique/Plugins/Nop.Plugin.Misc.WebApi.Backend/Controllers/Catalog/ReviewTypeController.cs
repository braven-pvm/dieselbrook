using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Catalog
{
    public partial class ReviewTypeController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IReviewTypeService _reviewTypeService;

        #endregion

        #region Ctor

        public ReviewTypeController(IReviewTypeService reviewTypeService)
        {
            _reviewTypeService = reviewTypeService;
        }

        #endregion

        #region Methods

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var reviewType = await _reviewTypeService.GetReviewTypeByIdAsync(id);

            if (reviewType == null)
                return NotFound($"Review type Id={id} not found");

            await _reviewTypeService.DeleteReviewTypeAsync(reviewType);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ReviewTypeDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var reviewType = await _reviewTypeService.GetReviewTypeByIdAsync(id);

            if (reviewType == null)
                return NotFound($"Review type Id={id} not found");

            return Ok(reviewType.ToDto<ReviewTypeDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(IList<ReviewTypeDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll()
        {
            var reviewTypes = await _reviewTypeService.GetAllReviewTypesAsync();
            var reviewTypesDto = reviewTypes.Select(r => r.ToDto<ReviewTypeDto>());

            return Ok(reviewTypesDto);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ReviewTypeDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ReviewTypeDto model)
        {
            var reviewType = model.FromDto<ReviewType>();

            await _reviewTypeService.InsertReviewTypeAsync(reviewType);

            var reviewTypeDto = reviewType.ToDto<ReviewTypeDto>();

            return Ok(reviewTypeDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] ReviewTypeDto model)
        {
            var reviewType = await _reviewTypeService.GetReviewTypeByIdAsync(model.Id);

            if (reviewType == null)
                return NotFound($"Review type Id={model.Id} is not found");

            reviewType = model.FromDto<ReviewType>();

            await _reviewTypeService.UpdateReviewTypeAsync(reviewType);

            return Ok();
        }

        #endregion
    }
}
