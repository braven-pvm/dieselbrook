using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Discounts;
using Nop.Data;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Catalog
{
    public partial class ProductDiscountsController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IRepository<Discount> _discountRepository;
        private readonly IRepository<DiscountProductMapping> _discountProductMappingRepository;

        #endregion

        #region Ctor

        public ProductDiscountsController(IProductService productService,
            IRepository<Discount> discountRepository,
            IRepository<DiscountProductMapping> discountProductMappingRepository)
        {
            _productService = productService;
            _discountRepository = discountRepository;
            _discountProductMappingRepository = discountProductMappingRepository;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Clean up product references for a specified discount
        /// </summary>
        /// <param name="discountId">Discount identifier</param>
        [HttpGet("{discountId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> ClearDiscountProductMapping(int discountId)
        {
            if (discountId <= 0)
                return BadRequest();

            var discount = await _discountRepository.GetByIdAsync(discountId);

            if (discount == null)
                return NotFound($"Discount Id={discountId} not found");

            await _productService.ClearDiscountProductMappingAsync(discount);

            return Ok();
        }

        /// <summary>
        /// Get a discount-product mapping records by product identifier
        /// </summary>
        /// <param name="productId">Product identifier</param>
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(IList<DiscountProductMappingDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAllDiscountsAppliedToProduct(int productId)
        {
            var discountProductMappings = await _productService.GetAllDiscountsAppliedToProductAsync(productId);
            var discountProductMappingsDto =
                discountProductMappings.Select(dpm => dpm.ToDto<DiscountProductMappingDto>());

            return Ok(discountProductMappingsDto);
        }

        /// <summary>
        /// Get a discount-product mapping record
        /// </summary>
        /// <param name="productId">Product identifier</param>
        /// <param name="discountId">Discount identifier</param>
        [HttpGet("{productId}/{discountId}")]
        [ProducesResponseType(typeof(DiscountProductMappingDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetDiscountAppliedToProduct(int productId, int discountId)
        {
            var discountProductMapping = await _productService.GetDiscountAppliedToProductAsync(productId, discountId);

            return Ok(discountProductMapping.ToDto<DiscountProductMappingDto>());
        }
        
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var discountProductMapping = await _discountProductMappingRepository.GetByIdAsync(id);

            if (discountProductMapping == null)
                return NotFound($"Discount product mapping Id={id} not found");

            await _productService.DeleteDiscountProductMappingAsync(discountProductMapping);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(DiscountProductMappingDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var discountProductMapping = await _discountProductMappingRepository.GetByIdAsync(id);

            if (discountProductMapping == null)
                return NotFound($"Discount product mapping Id={id} not found");

            return Ok(discountProductMapping.ToDto<DiscountProductMappingDto>());
        }
        
        [HttpPost]
        [ProducesResponseType(typeof(DiscountProductMappingDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] DiscountProductMappingDto model)
        {
            var discountProductMapping = model.FromDto<DiscountProductMapping>();

            await _productService.InsertDiscountProductMappingAsync(discountProductMapping);

            var discountProductMappingDto = discountProductMapping.ToDto<DiscountProductMappingDto>();

            return Ok(discountProductMappingDto);
        }

        #endregion
    }
}
