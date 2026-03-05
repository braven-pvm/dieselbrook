using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Discounts;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Discounts;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Discounts;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Discounts
{
    public partial class DiscountRequirementController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IDiscountService _discountService;

        #endregion

        #region Ctor

        public DiscountRequirementController(IDiscountService discountService)
        {
            _discountService = discountService;
        }

        #endregion

        #region Methods
        
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id, [FromQuery][Required] bool recursively)
        {
            if (id <= 0)
                return BadRequest();

            var discountRequirement = await _discountService.GetDiscountRequirementByIdAsync(id);

            if (discountRequirement == null)
                return NotFound($"Discount requirement Id={id} not found");

            await _discountService.DeleteDiscountRequirementAsync(discountRequirement, recursively);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(DiscountRequirementDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var discountRequirement = await _discountService.GetDiscountRequirementByIdAsync(id);

            if (discountRequirement == null)
                return NotFound($"Discount requirement Id={id} not found");

            return Ok(discountRequirement.ToDto<DiscountRequirementDto>());
        }

        /// <summary>
        /// Gets child discount requirements
        /// </summary>
        /// <param name="discountRequirementId">Parent discount requirement Id</param>
        [HttpGet("{discountRequirementId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(IList<DiscountRequirementDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetDiscountRequirementsByParent(int discountRequirementId)
        {
            if (discountRequirementId <= 0)
                return BadRequest();

            var discountRequirement = await _discountService.GetDiscountRequirementByIdAsync(discountRequirementId);

            if (discountRequirement == null)
                return NotFound($"Discount requirement Id={discountRequirementId} not found");

            var requirements = await _discountService.GetDiscountRequirementsByParentAsync(discountRequirement);
            var requirementsDto = requirements.Select(r => r.ToDto<DiscountRequirementDto>()).ToList();

            return Ok(requirementsDto);
        }

        /// <summary>
        /// Get all discount requirements
        /// </summary>
        /// <param name="discountId">Discount identifier</param>
        /// <param name="topLevelOnly">Whether to load top-level requirements only (without parent identifier)</param>
        [HttpGet]
        [ProducesResponseType(typeof(IList<DiscountRequirementDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] int discountId = 0, [FromQuery] bool topLevelOnly = false)
        {
            var discountRequirements = await _discountService.GetAllDiscountRequirementsAsync(discountId, topLevelOnly);
            var discountRequirementsDto =
                discountRequirements.Select(dr => dr.ToDto<DiscountRequirementDto>()).ToList();

            return Ok(discountRequirementsDto);
        }

        [HttpPost]
        [ProducesResponseType(typeof(DiscountRequirementDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] DiscountRequirementDto model)
        {
            var discountRequirement = model.FromDto<DiscountRequirement>();

            await _discountService.InsertDiscountRequirementAsync(discountRequirement);

            var discountRequirementDto = discountRequirement.ToDto<DiscountRequirementDto>();

            return Ok(discountRequirementDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] DiscountRequirementDto model)
        {
            var discountRequirement = await _discountService.GetDiscountRequirementByIdAsync(model.Id);

            if (discountRequirement == null)
                return NotFound($"Discount requirement Id={model.Id} is not found");

            discountRequirement = model.FromDto<DiscountRequirement>();

            await _discountService.UpdateDiscountRequirementAsync(discountRequirement);

            return Ok();
        }

        #endregion
    }
}
