using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Vendors;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Helpers;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Vendors;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Vendors
{
    public partial class VendorController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IVendorService _vendorService;

        #endregion

        #region Ctor

        public VendorController(IVendorService vendorService)
        {
            _vendorService = vendorService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets all vendors
        /// </summary>
        /// <param name="name">Vendor name</param>
        /// <param name="email">Vendor email</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<Vendor, VendorDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] string name = "",
            [FromQuery] string email = "",
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue,
            [FromQuery] bool showHidden = false)
        {
            var vendors = await _vendorService.GetAllVendorsAsync(name, email, pageIndex, pageSize, showHidden);
            var vendorsDto = vendors.ToPagedListDto<Vendor, VendorDto>();

            return Ok(vendorsDto);
        }

        /// <summary>
        /// Gets a vendor by vendor identifier
        /// </summary>
        /// <param name="id">The vendor identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(VendorDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            var vendor = await _vendorService.GetVendorByIdAsync(id);

            if (vendor == null) 
                return NotFound($"Vendor Id={id} not found");

            var vendorDto = vendor.ToDto<VendorDto>();

            return Ok(vendorDto);
        }

        /// <summary>
        /// Gets a vendor by product identifier
        /// </summary>
        /// <param name="id">The vendor identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(VendorDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetByProductId(int id)
        {
            var vendor = await _vendorService.GetVendorByProductIdAsync(id);

            if (vendor == null) 
                return NotFound("Vendor not found");

            var vendorDto = vendor.ToDto<VendorDto>();

            return Ok(vendorDto);
        }

        /// <summary>
        /// Gets a vendor by product identifier
        /// </summary>
        /// <param name="ids">Array of vendor identifiers (separator - ;)</param>
        [HttpGet("{ids}")]
        [ProducesResponseType(typeof(IList<VendorDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetByProductIds(string ids)
        {
            var productIds = ids.Split(";").Where(s => int.TryParse(s, out _)).Select(str => int.Parse(str)).ToArray();
            var vendors = await _vendorService.GetVendorsByProductIdsAsync(productIds);

            var vendorsDto = vendors.Select(vendor => vendor.ToDto<VendorDto>()).ToList();

            return Ok(vendorsDto);
        }

        /// <summary>
        /// Gets a vendors by customers identifiers
        /// </summary>
        /// <param name="ids">Array of customer identifiers (separator - ;)</param>
        [HttpGet("{ids}")]
        [ProducesResponseType(typeof(IList<VendorDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetByCustomerIds(string ids)
        {
            var customerIds = ids.Split(";").Where(s => int.TryParse(s, out _)).Select(str => int.Parse(str)).ToArray();
            var vendors = await _vendorService.GetVendorsByCustomerIdsAsync(customerIds);

            var vendorsDto = vendors.Select(vendor => vendor.ToDto<VendorDto>()).ToList();

            return Ok(vendorsDto);
        }

        /// <summary>
        /// Delete a vendor
        /// </summary>
        /// <param name="id">Vendor identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var vendor = await _vendorService.GetVendorByIdAsync(id);

            if (vendor == null)
                return NotFound($"Vendor Id={id} not found");

            await _vendorService.DeleteVendorAsync(vendor);

            return Ok();
        }

        /// <summary>
        /// Create a vendor
        /// </summary>
        /// <param name="model">Vendor Dto</param>
        [HttpPost]
        [ProducesResponseType(typeof(VendorDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] VendorDto model)
        {
            var vendor = model.FromDto<Vendor>();

            await _vendorService.InsertVendorAsync(vendor);

            var vendorDto = vendor.ToDto<VendorDto>();

            return Ok(vendorDto);
        }

        /// <summary>
        /// Updates the vendor
        /// </summary>
        /// <param name="model">Vendor Dto model</param>
        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] VendorDto model)
        {
            var vendor = await _vendorService.GetVendorByIdAsync(model.Id);

            if (vendor == null)
                return NotFound("Vendor is not found");

            vendor = model.FromDto<Vendor>();

            await _vendorService.UpdateVendorAsync(vendor);

            return Ok();
        }

        /// <summary>
        /// Gets a vendor note
        /// </summary>
        /// <param name="vendorNoteId">The vendor note identifier</param>
        [HttpGet("{vendorNoteId}")]
        [ProducesResponseType(typeof(VendorNoteDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> GetVendorNoteById(int vendorNoteId)
        {
            var vendorNote = await _vendorService.GetVendorNoteByIdAsync(vendorNoteId);

            if (vendorNote == null)
                return NotFound($"Vendor note Id={vendorNoteId} not found");

            var vendorDto = vendorNote.ToDto<VendorNoteDto>();

            return Ok(vendorDto);
        }

        /// <summary>
        /// Gets all vendor notes
        /// </summary>
        /// <param name="vendorId">Vendor identifier</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet("{vendorId}")]
        [ProducesResponseType(typeof(PagedListDto<VendorNote, VendorNoteDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetVendorNotesByVendor(int vendorId,
            [FromQuery] int pageIndex = 0,
            [FromQuery] int pageSize = int.MaxValue)
        {
            var notes = await _vendorService.GetVendorNotesByVendorAsync(vendorId, pageIndex, pageSize);

            return Ok(new PagedListDto<VendorNote, VendorNoteDto>(notes));
        }

        /// <summary>
        /// Deletes a vendor note
        /// </summary>
        /// <param name="id">The vendor note id</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        public virtual async Task<IActionResult> DeleteVendorNote(int id)
        {
            if (id <= 0)
                return BadRequest();

            var note = await _vendorService.GetVendorNoteByIdAsync(id);

            if (note == null)
                return NotFound($"Vendor note Id={id} not found");

            await _vendorService.DeleteVendorNoteAsync(note);

            return Ok();
        }

        /// <summary>
        /// Inserts a vendor note
        /// </summary>
        /// <param name="vendorNote">Vendor note</param>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> InsertVendorNote([FromBody] VendorNoteDto vendorNote)
        {
            await _vendorService.InsertVendorNoteAsync(vendorNote.FromDto<VendorNote>());

            return Ok();
        }

        #endregion
    }
}
