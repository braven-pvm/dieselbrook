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
    public partial class MeasureWeightController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IMeasureService _measureService;

        #endregion

        #region Ctor

        public MeasureWeightController(IMeasureService measureService)
        {
            _measureService = measureService;
        }

        #endregion

        #region Methods
        
        /// <summary>
        /// Gets a measure weight by system keyword
        /// </summary>
        /// <param name="systemKeyword">The system keyword</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MeasureWeightDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetBySystemKeyword([FromQuery][Required] string systemKeyword)
        {
            if (string.IsNullOrEmpty(systemKeyword))
                return BadRequest();

            var measureWeight = await _measureService.GetMeasureWeightBySystemKeywordAsync(systemKeyword);

            if (measureWeight == null)
                return NotFound($"Measure weight system keyword={systemKeyword} not found");

            return Ok(measureWeight.ToDto<MeasureWeightDto>());
        }

        /// <summary>
        /// Converts weight
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="sourceMeasureWeightId">Source weight</param>
        /// <param name="targetMeasureWeightId">Target weight</param>
        /// <param name="round">A value indicating whether a result should be rounded</param>
        [HttpGet("{sourceMeasureWeightId}/{targetMeasureWeightId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ConvertWeight([FromQuery, Required] decimal value,
            int sourceMeasureWeightId,
            int targetMeasureWeightId,
            [FromQuery] bool round = true)
        {
            if (sourceMeasureWeightId <= 0)
                return BadRequest();

            var sourceMeasureWeight = await _measureService.GetMeasureWeightByIdAsync(sourceMeasureWeightId);

            if (sourceMeasureWeight == null)
                return NotFound($"Measure weight Id={sourceMeasureWeightId} not found");

            var targetMeasureWeight = await _measureService.GetMeasureWeightByIdAsync(targetMeasureWeightId);

            if (targetMeasureWeight == null)
                return NotFound($"Measure weight Id={targetMeasureWeightId} not found");

            var rez = await _measureService.ConvertWeightAsync(value, sourceMeasureWeight,
                targetMeasureWeight);

            return Ok(rez);
        }

        /// <summary>
        /// Converts from primary weight
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="measureWeightId">Target weight Id</param>
        [HttpGet("{measureWeightId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ConvertFromPrimaryMeasureWeight([FromQuery][Required] decimal value, int measureWeightId)
        {
            if (measureWeightId <= 0)
                return BadRequest();

            var targetMeasureWeight = await _measureService.GetMeasureWeightByIdAsync(measureWeightId);

            if (targetMeasureWeight == null)
                return NotFound($"Measure weight Id={measureWeightId} not found");

            var rez = await _measureService.ConvertFromPrimaryMeasureWeightAsync(value, targetMeasureWeight);

            return Ok(rez);
        }

        /// <summary>
        /// Converts to primary measure weight
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <param name="measureWeightId">Source weight Id</param>
        [HttpGet("{measureWeightId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ConvertToPrimaryMeasureWeight([FromQuery][Required] decimal value, int measureWeightId)
        {
            if (measureWeightId <= 0)
                return BadRequest();

            var targetMeasureWeight = await _measureService.GetMeasureWeightByIdAsync(measureWeightId);

            if (targetMeasureWeight == null)
                return NotFound($"Measure weight Id={measureWeightId} not found");

            var rez = await _measureService.ConvertToPrimaryMeasureWeightAsync(value, targetMeasureWeight);

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

            var measureWeight = await _measureService.GetMeasureWeightByIdAsync(id);

            if (measureWeight == null)
                return NotFound($"Measure weight Id={id} not found");

            await _measureService.DeleteMeasureWeightAsync(measureWeight);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(MeasureWeightDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var measureWeight = await _measureService.GetMeasureWeightByIdAsync(id);

            if (measureWeight == null)
                return NotFound($"Measure weight Id={id} not found");

            return Ok(measureWeight.ToDto<MeasureWeightDto>());
        }

        [HttpGet]
        [ProducesResponseType(typeof(IList<MeasureWeightDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll()
        {
            var measureWeights = await _measureService.GetAllMeasureWeightsAsync();

            return Ok(measureWeights.Select(mw => mw.ToDto<MeasureWeightDto>()).ToList());
        }

        [HttpPost]
        [ProducesResponseType(typeof(MeasureWeightDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] MeasureWeightDto model)
        {
            var measureWeight = model.FromDto<MeasureWeight>();

            await _measureService.InsertMeasureWeightAsync(measureWeight);

            var measureWeightDto = measureWeight.ToDto<MeasureWeightDto>();

            return Ok(measureWeightDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] MeasureWeightDto model)
        {
            var measureWeight = await _measureService.GetMeasureWeightByIdAsync(model.Id);

            if (measureWeight == null)
                return NotFound($"Measure weight Id={model.Id} is not found");

            measureWeight = model.FromDto<MeasureWeight>();

            await _measureService.UpdateMeasureWeightAsync(measureWeight);

            return Ok();
        }

        #endregion
    }
}
