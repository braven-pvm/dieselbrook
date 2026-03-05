using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.News;
using Nop.Core.Events;
using Nop.Core.Rss;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.News;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.News;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Models.News;

namespace Nop.Plugin.Misc.WebApi.Frontend.Controllers
{
    public partial class NewsController : BaseNopWebApiFrontendController
    {
        #region Fields

        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILocalizationService _localizationService;
        private readonly INewsModelFactory _newsModelFactory;
        private readonly INewsService _newsService;
        private readonly INopUrlHelper _nopUrlHelper;
        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;
        private readonly NewsSettings _newsSettings;

        #endregion

        #region Ctor

        public NewsController(
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IEventPublisher eventPublisher,
            ILocalizationService localizationService,
            INewsModelFactory newsModelFactory,
            INewsService newsService,
            INopUrlHelper nopUrlHelper,
            IPermissionService permissionService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings,
            NewsSettings newsSettings)
        {
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _eventPublisher = eventPublisher;
            _localizationService = localizationService;
            _newsModelFactory = newsModelFactory;
            _newsService = newsService;
            _nopUrlHelper = nopUrlHelper;
            _permissionService = permissionService;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _urlRecordService = urlRecordService;
            _webHelper = webHelper;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
            _newsSettings = newsSettings;
        }

        #endregion

        #region Methods

        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NewsItemListModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> List([FromBody] NewsPagingFilteringModelDto command)
        {
            if (!_newsSettings.Enabled)
                return BadRequest($"The setting {nameof(_newsSettings.Enabled)} is not enabled");

            var model = await _newsModelFactory.PrepareNewsItemListModelAsync(command.FromDto<NewsPagingFilteringModel>());
            
            return Ok(model.ToDto<NewsItemListModelDto>());
        }

        [HttpGet("{languageId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ListRss(int languageId)
        {
            if (!_newsSettings.Enabled)
                return BadRequest($"The setting {nameof(_newsSettings.Enabled)} is not enabled");

            var store = await _storeContext.GetCurrentStoreAsync();
            var feed = new RssFeed(
                $"{await _localizationService.GetLocalizedAsync(store, x => x.Name)}: News",
                "News",
                new Uri(_webHelper.GetStoreLocation()),
                DateTime.UtcNow);

            var items = new List<RssItem>();
            var newsItems = await _newsService.GetAllNewsAsync(languageId, store.Id);
            foreach (var n in newsItems)
            {
                var seName = await _urlRecordService.GetSeNameAsync(n, n.LanguageId, ensureTwoPublishedLanguages: false);
                var newsUrl = await _nopUrlHelper.RouteGenericUrlAsync<NewsItem>(new { SeName = seName }, _webHelper.GetCurrentRequestProtocol());
                items.Add(new RssItem(n.Title, n.Short, new Uri(newsUrl), $"urn:store:{store.Id}:news:blog:{n.Id}", n.CreatedOnUtc));
            }

            feed.Items = items;

            return Ok(feed.GetContent());
        }

        [HttpGet("{newsItemId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(NewsItemModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetNewsItem(int newsItemId)
        {
            if (!_newsSettings.Enabled)
                return BadRequest($"The setting {nameof(_newsSettings.Enabled)} is not enabled");

            var newsItem = await _newsService.GetNewsByIdAsync(newsItemId);
            if (newsItem == null)
                return NotFound($"News item Id={newsItemId} not found");

            var notAvailable =
                //published?
                !newsItem.Published ||
                //availability dates
                !_newsService.IsNewsAvailable(newsItem) ||
                //Store mapping
                !await _storeMappingService.AuthorizeAsync(newsItem);
            //Check whether the current user has a "Manage news" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel) && await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageNews);
            if (notAvailable && !hasAdminAccess)
                return NotFound($"News item Id={newsItemId} not found");

            var model = new NewsItemModel();
            model = await _newsModelFactory.PrepareNewsItemModelAsync(model, newsItem, true);
            
            return Ok(model.ToDto<NewsItemModelDto>());
        }

        [HttpPost("{newsItemId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> NewsCommentAdd([FromBody] NewsItemModelDto model, int newsItemId)
        {
            if (!_newsSettings.Enabled)
                return BadRequest($"The setting {nameof(_newsSettings.Enabled)} is not enabled");

            var newsItem = await _newsService.GetNewsByIdAsync(newsItemId);

            if (newsItem == null)
                return NotFound($"News item Id={newsItemId} not found");

            if (!newsItem.Published || !newsItem.AllowComments)
                return BadRequest($"The property {nameof(newsItem.AllowComments)} is not allowed or news item is not published.");

            var customer = await _workContext.GetCurrentCustomerAsync();

            if (await _customerService.IsGuestAsync(customer) &&
                !_newsSettings.AllowNotRegisteredUsersToLeaveComments)
                return BadRequest(await _localizationService.GetResourceAsync("News.Comments.OnlyRegisteredUsersLeaveComments"));

            var modelNews = model.FromDto<NewsItemModel>();

            var store = await _storeContext.GetCurrentStoreAsync();

            var comment = new NewsComment
            {
                NewsItemId = newsItem.Id,
                CustomerId = customer.Id,
                CommentTitle = modelNews.AddNewComment.CommentTitle,
                CommentText = modelNews.AddNewComment.CommentText,
                IsApproved = !_newsSettings.NewsCommentsMustBeApproved,
                StoreId = store.Id,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _newsService.InsertNewsCommentAsync(comment);

            //notify a store owner;
            if (_newsSettings.NotifyAboutNewNewsComments)
                await _workflowMessageService.SendNewsCommentStoreOwnerNotificationMessageAsync(comment,
                    _localizationSettings.DefaultAdminLanguageId);

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.AddNewsComment",
                await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddNewsComment"), comment);

            //raise event
            if (comment.IsApproved)
                await _eventPublisher.PublishAsync(new NewsCommentApprovedEvent(comment));

            return Ok();
        }

        #endregion
    }
}