using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
    public partial class ProductAttributeCombinationController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IProductAttributeService _productAttributeService;

        #endregion

        #region Ctor

        public ProductAttributeCombinationController(IProductAttributeService productAttributeService)
        {
            _productAttributeService = productAttributeService;
        }

        #endregion

        #region Methods
        
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var productAttributeCombination = await _productAttributeService.GetProductAttributeCombinationByIdAsync(id);

            if (productAttributeCombination == null)
                return NotFound($"Product attribute combination Id={id} not found");

            await _productAttributeService.DeleteProductAttributeCombinationAsync(productAttributeCombination);

            return Ok();
        }

        /// <summary>
        /// Gets a product attribute combination by SKU
        /// </summary>
        /// <param name="sku">SKU</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProductAttributeCombinationDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetBySku([FromQuery][Required] string sku)
        {
            if (string.IsNullOrEmpty(sku))
                return BadRequest();

            var productAttributeCombination = await _productAttributeService.GetProductAttributeCombinationBySkuAsync(sku);

            if (productAttributeCombination == null)
                return NotFound($"Product attribute combination sku={sku} not found");

            return Ok(productAttributeCombination.ToDto<ProductAttributeCombinationDto>());
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProductAttributeCombinationDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var productAttributeCombination = await _productAttributeService.GetProductAttributeCombinationByIdAsync(id);

            if (productAttributeCombination == null)
                return NotFound($"Product attribute combination Id={id} not found");

            return Ok(productAttributeCombination.ToDto<ProductAttributeCombinationDto>());
        }
        
        [HttpGet("{productId}")]
        [ProducesResponseType(typeof(IList<ProductAttributeCombinationDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll(int productId)
        {
            var productAttributeCombinations = await _productAttributeService.GetAllProductAttributeCombinationsAsync(productId);
            var productAttributeCombinationsDto = productAttributeCombinations.Select(p => p.ToDto<ProductAttributeCombinationDto>());

            return Ok(productAttributeCombinationsDto);
        }

        [HttpPost]
        [ProducesResponseType(typeof(ProductAttributeCombinationDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ProductAttributeCombinationDto model)
        {
            var productAttributeCombination = model.FromDto<ProductAttributeCombination>();

            await _productAttributeService.InsertProductAttributeCombinationAsync(productAttributeCombination);

            var productAttributeCombinationDto = productAttributeCombination.ToDto<ProductAttributeCombinationDto>();

            return Ok(productAttributeCombinationDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] ProductAttributeCombinationDto model)
        {
            var productAttributeCombination = await _productAttributeService.GetProductAttributeCombinationByIdAsync(model.Id);

            if (productAttributeCombination == null)
                return NotFound($"Product attribute combination Id={model.Id} is not found");

            productAttributeCombination = model.FromDto<ProductAttributeCombination>();

            await _productAttributeService.UpdateProductAttributeCombinationAsync(productAttributeCombination);

            return Ok();
        }

        #endregion
    }
}
