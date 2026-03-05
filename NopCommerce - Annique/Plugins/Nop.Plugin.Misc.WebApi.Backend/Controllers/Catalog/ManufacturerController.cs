using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Data;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Catalog;
using Nop.Plugin.Misc.WebApi.Backend.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Discounts;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Catalog
{
    public partial class ManufacturerController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICustomerService _customerService;
        private readonly IDiscountService _discountService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IRepository<DiscountManufacturerMapping> _discountManufacturerMappingRepository;

        #endregion

        #region Ctor

        public ManufacturerController(ICustomerService customerService,
            IDiscountService discountService,
            IManufacturerService manufacturerService,
            IRepository<DiscountManufacturerMapping> discountManufacturerMappingRepository)
        {
            _customerService = customerService;
            _discountService = discountService;
            _manufacturerService = manufacturerService;
            _discountManufacturerMappingRepository = discountManufacturerMappingRepository;
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Gets the manufacturers by category identifier
        /// </summary>
        /// <param name="categoryId">Cateogry identifier</param>
        [HttpGet("{categoryId}")]
        [ProducesResponseType(typeof(IList<ManufacturerDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetManufacturersByCategoryId(int categoryId)
        {
            var manufacturers = await _manufacturerService.GetManufacturersByCategoryIdAsync(categoryId);

            var manufacturersDto = manufacturers.Select(c => c.ToDto<ManufacturerDto>());

            return Ok(manufacturersDto);
        }

        /// <summary>
        /// Gets manufacturers by identifier
        /// </summary>
        /// <param name="ids">Array of manufacturer identifiers (separator - ;)</param>
        [HttpGet("{ids}")]
        [ProducesResponseType(typeof(IList<ManufacturerDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetManufacturersByIds(string ids)
        {
            var manufacturersId = ids.ToIdArray();
            var manufacturers = await _manufacturerService.GetManufacturersByIdsAsync(manufacturersId);

            var categoriesDto = manufacturers.Select(c => c.ToDto<CategoryDto>());

            return Ok(categoriesDto);
        }

        /// <summary>
        /// Get manufacturers for which a discount is applied
        /// </summary>
        /// <param name="discountId">Discount identifier; pass null to load all records</param>
        /// <param name="showHidden">A value indicating whether to load deleted manufacturers</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<Manufacturer, ManufacturerDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetManufacturersWithAppliedDiscount([FromQuery] int? discountId = null,
            [FromQuery] bool showHidden = false,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue)
        {
            var manufacturers =
                await _manufacturerService.GetManufacturersWithAppliedDiscountAsync(discountId, showHidden, pageIndex,
                    pageSize);

            return Ok(manufacturers.ToPagedListDto<Manufacturer, ManufacturerDto>());
        }

        /// <summary>
        /// Returns a list of names of not existing manufacturers
        /// </summary>
        /// <param name="idsNames">The names and/or IDs of the manufacturers to check</param>
        [HttpGet("{idsNames}")]
        [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetNotExistingManufacturers(string idsNames)
        {
            var manufacturerIdsNames = idsNames.Split(";").Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();
            var notExisting = await _manufacturerService.GetNotExistingManufacturersAsync(manufacturerIdsNames);

            return Ok(notExisting.ToList());
        }

        /// <summary>
        /// Delete manufacturers
        /// </summary>
        /// <param name="ids">Array of manufacturer identifiers (separator - ;)</param>
        [HttpGet("{ids}")]
        public virtual async Task<IActionResult> DeleteManufacturers(string ids)
        {
            var manufacturersId = ids.ToIdArray();
            var manufacturers = await _manufacturerService.GetManufacturersByIdsAsync(manufacturersId);

            await _manufacturerService.DeleteManufacturersAsync(manufacturers);

            return Ok();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(id);

            if (manufacturer == null)
                return NotFound($"Manufacturer Id={id} not found");

            await _manufacturerService.DeleteManufacturerAsync(manufacturer);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ManufacturerDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(id);

            if (manufacturer == null)
                return NotFound($"Manufacturer Id={id} not found");

            return Ok(manufacturer.ToDto<ManufacturerDto>());
        }

        /// <summary>
        /// Gets all manufacturers
        /// </summary>
        /// <param name="manufacturerName">Manufacturer name</param>
        /// <param name="storeId">Store identifier; 0 if you want to get all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="overridePublished">
        /// null - process "Published" property according to "showHidden" parameter
        /// true - load only "Published" products
        /// false - load only "Unpublished" products
        /// </param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<Manufacturer, ManufacturerDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] string manufacturerName = "",
            [FromQuery] int storeId = 0,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue,
            [FromQuery] bool showHidden = false,
            [FromQuery] bool? overridePublished = null)
        {
            var manufacturers = await _manufacturerService.GetAllManufacturersAsync(manufacturerName, storeId,
                pageIndex, pageSize, showHidden, overridePublished);

            return Ok(manufacturers.ToPagedListDto<Manufacturer, ManufacturerDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(ManufacturerDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] ManufacturerDto model)
        {
            var manufacturer = model.FromDto<Manufacturer>();

            await _manufacturerService.InsertManufacturerAsync(manufacturer);

            var manufacturerDto = manufacturer.ToDto<ManufacturerDto>();

            return Ok(manufacturerDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] ManufacturerDto model)
        {
            var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(model.Id);

            if (manufacturer == null)
                return NotFound($"Manufacturer Id={model.Id} is not found");

            manufacturer = model.FromDto<Manufacturer>();

            await _manufacturerService.UpdateManufacturerAsync(manufacturer);

            return Ok();
        }

        /// <summary>
        /// Clean up manufacturer references for a specified discount
        /// </summary>
        /// <param name="discountId">Discount Id</param>
        [HttpDelete("{discountId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> ClearDiscountManufacturerMapping(int discountId)
        {
            if (discountId <= 0)
                return BadRequest();

            var discount = await _discountService.GetDiscountByIdAsync(discountId);

            await _manufacturerService.ClearDiscountManufacturerMappingAsync(discount);

            return Ok();
        }

        /// <summary>
        /// Deletes a discount-manufacturer mapping record
        /// </summary>
        /// <param name="mappingId">Discount-manufacturer mapping Id</param>
        [HttpDelete("{mappingId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> DeleteDiscountManufacturerMapping(int mappingId)
        {
            if (mappingId <= 0)
                return BadRequest();

            var mapping = await _discountManufacturerMappingRepository.GetByIdAsync(mappingId);

            if (mapping == null)
                return NotFound($"Mapping Id={mappingId} not found");

            await _manufacturerService.DeleteDiscountManufacturerMappingAsync(mapping);

            return Ok();
        }

        /// <summary>
        /// Get manufacturer identifiers to which a discount is applied
        /// </summary>
        /// <param name="discountId">Discount Id</param>
        /// <param name="customerId">Customer Id</param>
        [HttpGet("{discountId}/{customerId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(IList<int>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> GetAppliedManufacturerIds(int discountId, int customerId)
        {
            if (discountId <= 0)
                return BadRequest();

            if (customerId <= 0)
                return BadRequest();

            var discount = await _discountService.GetDiscountByIdAsync(discountId);

            if (discount == null)
                return NotFound($"Discount Id={discountId} not found");

            var customer = await _customerService.GetCustomerByIdAsync(customerId);

            if (customer == null)
                return NotFound($"Customer Id={customerId} not found");

            var ids = await _manufacturerService.GetAppliedManufacturerIdsAsync(discount, customer);

            return Ok(ids);
        }

        /// <summary>
        /// Get a discount-manufacturer mapping record
        /// </summary>
        /// <param name="manufacturerId">Manufacturer identifier</param>
        /// <param name="discountId">Discount identifier</param>
        [HttpGet("{manufacturerId}/{discountId}")]
        [ProducesResponseType(typeof(DiscountCategoryMappingDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetDiscountAppliedToManufacturer(int manufacturerId, int discountId)
        {
            var mapping = await _manufacturerService.GetDiscountAppliedToManufacturerAsync(manufacturerId, discountId);

            return Ok(mapping.ToDto<DiscountCategoryMappingDto>());
        }

        /// <summary>
        /// Inserts a discount-manufacturer mapping record
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> InsertDiscountManufacturerMapping([FromBody] DiscountManufacturerMappingDto model)
        {
            await _manufacturerService.InsertDiscountManufacturerMappingAsync(model.FromDto<DiscountManufacturerMapping>());

            return Ok();
        }

        #endregion
    }
}
