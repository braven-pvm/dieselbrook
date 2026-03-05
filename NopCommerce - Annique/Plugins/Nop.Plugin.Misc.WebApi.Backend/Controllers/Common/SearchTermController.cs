using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Common;
using Nop.Data;
using Nop.Plugin.Misc.WebApi.Backend.Dto.Common;
using Nop.Plugin.Misc.WebApi.Framework.Dto;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Services.Common;

namespace Nop.Plugin.Misc.WebApi.Backend.Controllers.Common
{
    public partial class SearchTermController : BaseNopWebApiBackendController
    {
        #region Fields

        private readonly IRepository<SearchTerm> _searchTermRepository;
        private readonly ISearchTermService _searchTermService;

        #endregion

        #region Ctor

        public SearchTermController(IRepository<SearchTerm> searchTermRepository,
            ISearchTermService searchTermService)
        {
            _searchTermRepository = searchTermRepository;
            _searchTermService = searchTermService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a search term record by keyword
        /// </summary>
        /// <param name="keyword">Search term keyword</param>
        /// <param name="storeId">Store identifier</param>
        [HttpGet("{storeId}")]
        [ProducesResponseType(typeof(SearchTermDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetSearchTermByKeyword([FromQuery][Required] string keyword, int storeId)
        {
            var item = await _searchTermService.GetSearchTermByKeywordAsync(keyword, storeId);

            return Ok(item.ToDto<SearchTermDto>());
        }

        /// <summary>
        /// Gets a search term statistics
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        [HttpGet]
        [ProducesResponseType(typeof(PagedListDto<SearchTermReportLine, SearchTermReportLineDto>), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetStats([FromQuery] int pageIndex = 0, [FromQuery] int pageSize = int.MaxValue)
        {
            var searchTerms = await _searchTermService.GetStatsAsync(pageIndex, pageSize);

            return Ok(new PagedListDto<SearchTermReportLine, SearchTermReportLineDto>(searchTerms));
        }

        [HttpPost]
        [ProducesResponseType(typeof(SearchTermDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Create([FromBody] SearchTermDto model)
        {
            var searchTerm = model.FromDto<SearchTerm>();

            await _searchTermService.InsertSearchTermAsync(searchTerm);

            var searchTermDto = searchTerm.ToDto<SearchTermDto>();

            return Ok(searchTermDto);
        }

        [HttpPut]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Update([FromBody] SearchTermDto model)
        {
            var searchTerm = await _searchTermRepository.GetByIdAsync(model.Id);

            if (searchTerm == null)
                return NotFound($"Search term Id={model.Id} is not found");

            searchTerm = model.FromDto<SearchTerm>();

            await _searchTermService.UpdateSearchTermAsync(searchTerm);

            return Ok();
        }

        #endregion
    }
}
