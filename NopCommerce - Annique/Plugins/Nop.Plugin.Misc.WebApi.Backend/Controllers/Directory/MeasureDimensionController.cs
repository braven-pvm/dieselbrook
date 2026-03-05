using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Directory;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Directory;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Directory
{
    public partial class MeasureDimensionController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IMeasureService _measureService;

        #endregion

        #region Ctor

        public MeasureDimensionController(IMeasureService measureService)
        {
            _measureService = measureService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Converts dimension
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="sourceMeasureDimensionId">Source dimension</param>
        /// <param name="targetMeasureDimensionId">Target dimension</param>
        /// <param name="round">A value indicating whether a result should be rounded</param>
        [HttpGet("{sourceMeasureDimensionId}/{targetMeasureDimensionId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ConvertDimension([FromQuery, Required] decimal value,
            int sourceMeasureDimensionId,
            int targetMeasureDimensionId,
            [FromQuery] bool round = true)
        {
            if (sourceMeasureDimensionId <= 0)
                return BadRequest();

            var sourceMeasureDimension = await _measureService.GetMeasureDimensionByIdAsync(sourceMeasureDimensionId);

            if (sourceMeasureDimension == null)
                return NotFound($"Measure dimension Id={sourceMeasureDimensionId} not found");

            var targetMeasureDimension = await _measureService.GetMeasureDimensionByIdAsync(targetMeasureDimensionId);

            if (targetMeasureDimension == null)
                return NotFound($"Measure dimension Id={targetMeasureDimensionId} not found");

            var rez = await _measureService.ConvertDimensionAsync(value, sourceMeasureDimension,
                targetMeasureDimension);

            return Ok(rez);
        }

        /// <summary>
        /// Converts from primary dimension
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="measureDimensionId">Target dimension Id</param>
        [HttpGet("{measureDimensionId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ConvertFromPrimaryMeasureDimension([FromQuery, Required] decimal value,
            int measureDimensionId)
        {
            if (measureDimensionId <= 0)
                return BadRequest();

            var targetMeasureDimension = await _measureService.GetMeasureDimensionByIdAsync(measureDimensionId);

            if (targetMeasureDimension == null)
                return NotFound($"Measure dimension Id={measureDimensionId} not found");

            var rez = await _measureService.ConvertFromPrimaryMeasureDimensionAsync(value, targetMeasureDimension);

            return Ok(rez);
        }

        /// <summary>
        /// Converts to primary measure dimension
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="measureDimensionId">Source dimension Id</param>
        [HttpGet("{measureDimensionId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ConvertToPrimaryMeasureDimension([FromQuery][Required] decimal value, int measureDimensionId)
        {
            if (measureDimensionId <= 0)
                return BadRequest();

            var targetMeasureDimension = await _measureService.GetMeasureDimensionByIdAsync(measureDimensionId);

            if (targetMeasureDimension == null)
                return NotFound($"Measure dimension Id={measureDimensionId} not found");

            var rez = await _measureService.ConvertToPrimaryMeasureDimensionAsync(value, targetMeasureDimension);

            return Ok(rez);
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
                return BadRequest();

            var measureDimension = await _measureService.GetMeasureDimensionByIdAsync(id);

            if (measureDimension == null)
                return NotFound($"Measure dimension Id={id} not found");

            await _measureService.DeleteMeasureDimensionAsync(measureDimension);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MeasureDimensionDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var measureDimension = await _measureService.GetMeasureDimensionByIdAsync(id);

            if (measureDimension == null)
                return NotFound($"Measure dimension Id={id} not found");

            return Ok(measureDimension.ToDto<MeasureDimensionDto>());
        }

        /// <summary>
        /// Gets a measure dimension by system keyword
        /// </summary>
        /// <param name="systemKeyword">The system keyword</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MeasureDimensionDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetBySystemKeyword([FromQuery][Required] string systemKeyword)
        {
            if (string.IsNullOrEmpty(systemKeyword))
                return BadRequest();

            var measureDimension = await _measureService.GetMeasureDimensionBySystemKeywordAsync(systemKeyword);

            if (measureDimension == null)
                return NotFound($"Measure dimension system keyword={systemKeyword} not found");

            return Ok(measureDimension.ToDto<MeasureDimensionDto>());
        }

        /// <summary>
        /// Gets all measure dimensions
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IList<MeasureDimensionDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll()
        {
            var measureDimensions = await _measureService.GetAllMeasureDimensionsAsync();

            return Ok(measureDimensions.Select(md => md.ToDto<MeasureDimensionDto>()).ToList());
        }

        [HttpPost]
        [ProducesResponseType(typeof(MeasureDimensionDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] MeasureDimensionDto model)
        {
            var measureDimension = model.FromDto<MeasureDimension>();

            await _measureService.InsertMeasureDimensionAsync(measureDimension);

            var measureDimensionDto = measureDimension.ToDto<MeasureDimensionDto>();

            return Ok(measureDimensionDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] MeasureDimensionDto model)
        {
            var measureDimension = await _measureService.GetMeasureDimensionByIdAsync(model.Id);

            if (measureDimension == null)
                return NotFound($"Measure dimension Id={model.Id} is not found");

            measureDimension = model.FromDto<MeasureDimension>();

            await _measureService.UpdateMeasureDimensionAsync(measureDimension);

            return Ok();
        }

        #endregion
    }
}
