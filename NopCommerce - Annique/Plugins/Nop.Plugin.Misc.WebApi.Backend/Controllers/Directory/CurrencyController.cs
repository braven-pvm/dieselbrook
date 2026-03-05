using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Directory;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Directory;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Directory
{
    public partial class CurrencyController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly ICurrencyService _currencyService;

        #endregion

        #region Ctor

        public CurrencyController(ICurrencyService currencyService)
        {
            _currencyService = currencyService;
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

            var currency = await _currencyService.GetCurrencyByIdAsync(id);

            if (currency == null)
                return NotFound($"Currency Id={id} not found");

            await _currencyService.DeleteCurrencyAsync(currency);

            return Ok();
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CurrencyDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest();

            var currency = await _currencyService.GetCurrencyByIdAsync(id);

            if (currency == null)
                return NotFound($"Currency Id={id} not found");

            return Ok(currency.ToDto<CurrencyDto>());
        }

        /// <summary>
        /// Gets a currency by code
        /// </summary>
        /// <param name="currencyCode">Currency code</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(CurrencyDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCurrencyByCode([FromQuery][Required] string currencyCode)
        {
            if (string.IsNullOrEmpty(currencyCode))
                return BadRequest();

            var currency = await _currencyService.GetCurrencyByCodeAsync(currencyCode);

            if (currency == null)
                return NotFound($"Currency code={currencyCode} not found");

            return Ok(currency.ToDto<CurrencyDto>());
        }

        /// <summary>
        /// Gets all currencies
        /// </summary>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="storeId">Load records allowed only in a specified store; pass 0 to load all records</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<Currency, CurrencyDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetAll([FromQuery] int storeId = 0, [FromQuery] bool showHidden = false)
        {
            var recurrences = await _currencyService.GetAllCurrenciesAsync(showHidden, storeId);

            return Ok(recurrences.Select(c => c.ToDto<CurrencyDto>()));
        }

        [HttpPost]
        [ProducesResponseType(typeof(CurrencyDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] CurrencyDto model)
        {
            var currency = model.FromDto<Currency>();

            await _currencyService.InsertCurrencyAsync(currency);

            var currencyDto = currency.ToDto<CurrencyDto>();

            return Ok(currencyDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] CurrencyDto model)
        {
            var currency = await _currencyService.GetCurrencyByIdAsync(model.Id);

            if (currency == null)
                return NotFound($"Currency Id={model.Id} is not found");

            currency = model.FromDto<Currency>();

            await _currencyService.UpdateCurrencyAsync(currency);

            return Ok();
        }

        #region Conversions

        /// <summary>
        /// Gets live rates regarding the passed currency
        /// </summary>
        /// <param name="currencyCode">Currency code; pass null to use primary exchange rate currency</param>
        [HttpGet]
        [ProducesResponseType(typeof(IList<ExchangeRateDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetCurrencyLiveRates([FromQuery][Required] string currencyCode)
        {
            var rates = await _currencyService.GetCurrencyLiveRatesAsync(currencyCode);
            var ratesDto = rates.Select(r => r.ToDto<ExchangeRateDto>()).ToList();

            return Ok(ratesDto);
        }

        /// <summary>
        /// Converts currency
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="exchangeRate">Currency exchange rate</param>
        [HttpGet]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        public IActionResult ConvertCurrency([FromQuery][Required] decimal amount,
                                             [FromQuery][Required] decimal exchangeRate)
        {
            return Ok(_currencyService.ConvertCurrency(amount, exchangeRate));
        }

        /// <summary>
        /// Converts to primary store currency 
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="currencyId">Source currency Id</param>
        [HttpGet("{currencyId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ConvertToPrimaryStoreCurrency([FromQuery][Required] decimal amount, int currencyId)
        {
            if (currencyId <= 0)
                return BadRequest();

            var currency = await _currencyService.GetCurrencyByIdAsync(currencyId);

            if (currency == null)
                return NotFound($"Currency Id={currencyId} is not found");

            var rez = await _currencyService.ConvertToPrimaryStoreCurrencyAsync(amount, currency);

            return Ok(rez);
        }

        /// <summary>
        /// Converts from primary store currency
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="currencyId">Target currency Id</param>
        [HttpGet("{currencyId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ConvertFromPrimaryStoreCurrency([FromQuery][Required] decimal amount, int currencyId)
        {
            if (currencyId <= 0)
                return BadRequest();

            var currency = await _currencyService.GetCurrencyByIdAsync(currencyId);

            if (currency == null)
                return NotFound($"Currency Id={currencyId} is not found");

            var rez = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(amount, currency);

            return Ok(rez);
        }

        /// <summary>
        /// Converts currency
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="sourceCurrencyId">Source currency code</param>
        /// <param name="targetCurrencyId">Target currency code</param>
        [HttpGet("{sourceCurrencyId}/{targetCurrencyId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ConvertCurrency([FromQuery][Required] decimal amount, int sourceCurrencyId, int targetCurrencyId)
        {
            if (sourceCurrencyId <= 0 || targetCurrencyId <= 0)
                return BadRequest();

            var sourceCurrency = await _currencyService.GetCurrencyByIdAsync(sourceCurrencyId);

            if (sourceCurrency == null)
                return NotFound($"Currency Id={sourceCurrencyId} is not found");

            var targetCurrency = await _currencyService.GetCurrencyByIdAsync(targetCurrencyId);

            if (targetCurrency == null)
                return NotFound($"Currency Id={targetCurrencyId} is not found");

            var rez = await _currencyService.ConvertCurrencyAsync(amount, sourceCurrency, targetCurrency);

            return Ok(rez);
        }

        /// <summary>
        /// Converts to primary exchange rate currency 
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="currencyId">Source currency Id</param>
        [HttpGet("{currencyId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ConvertToPrimaryExchangeRateCurrency([FromQuery][Required] decimal amount, int currencyId)
        {
            if (currencyId <= 0)
                return BadRequest();

            var currency = await _currencyService.GetCurrencyByIdAsync(currencyId);

            if (currency == null)
                return NotFound($"Currency Id={currencyId} is not found");

            var rez = await _currencyService.ConvertToPrimaryExchangeRateCurrencyAsync(amount, currency);

            return Ok(rez);
        }

        /// <summary>
        /// Converts from primary exchange rate currency
        /// </summary>
        /// <param name="amount">Amount</param>
        /// <param name="currencyId">Target currency code</param>
        [HttpGet("{currencyId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ConvertFromPrimaryExchangeRateCurrency([FromQuery][Required] decimal amount, int currencyId)
        {
            if (currencyId <= 0)
                return BadRequest();

            var currency = await _currencyService.GetCurrencyByIdAsync(currencyId);

            if (currency == null)
                return NotFound($"Currency Id={currencyId} is not found");

            var rez = await _currencyService.ConvertFromPrimaryExchangeRateCurrencyAsync(amount, currency);

            return Ok(rez);
        }

        #endregion

        #endregion
    }
}
