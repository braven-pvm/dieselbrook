using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Blogs;
using Nop.Core.Domain.Localization;
using Nop.Core.Events;
using Nop.Core.Rss;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Blog;
using Nop.Services.Blogs;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Stores;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc.Routing;
using Nop.Web.Models.Blogs;

namespace Nop.Plugin.Misc.WebApi.Frontend.Controllers
{
    public partial class BlogController : BaseNopWebApiFrontendController
    {
        #region Fields

        private readonly BlogSettings _blogSettings;
        private readonly IBlogModelFactory _blogModelFactory;
        private readonly IBlogService _blogService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IEventPublisher _eventPublisher;
        private readonly ILocalizationService _localizationService;
        private readonly INopUrlHelper _nopUrlHelper;
        private readonly IPermissionService _permissionService;
        private readonly IStoreContext _storeContext;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly LocalizationSettings _localizationSettings;

        #endregion

        #region Ctor

        public BlogController(BlogSettings blogSettings,
            IBlogModelFactory blogModelFactory,
            IBlogService blogService,
            ICustomerActivityService customerActivityService,
            IEventPublisher eventPublisher,
            ILocalizationService localizationService,
            INopUrlHelper nopUrlHelper,
            IPermissionService permissionService,
            IStoreContext storeContext,
            IStoreMappingService storeMappingService,
            IUrlRecordService urlRecordService,
            IWebHelper webHelper,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            LocalizationSettings localizationSettings)
        {
            _blogSettings = blogSettings;
            _blogModelFactory = blogModelFactory;
            _blogService = blogService;
            _customerActivityService = customerActivityService;
            _eventPublisher = eventPublisher;
            _localizationService = localizationService;
            _nopUrlHelper = nopUrlHelper;
            _permissionService = permissionService;
            _storeContext = storeContext;
            _storeMappingService = storeMappingService;
            _urlRecordService = urlRecordService;
            _webHelper = webHelper;
            _workContext = workContext;
            _workflowMessageService = workflowMessageService;
            _localizationSettings = localizationSettings;
        }

        #endregion

        #region Methods

        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BlogPostListModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> List([FromBody] BlogPagingFilteringModelDto command)
        {
            if (!_blogSettings.Enabled)
                return BadRequest($"The setting {nameof(_blogSettings.Enabled)} is not true.");

            var model = await _blogModelFactory.PrepareBlogPostListModelAsync(command.FromDto<BlogPagingFilteringModel>());
            
            return Ok(model.ToDto<BlogPostListModelDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BlogPostListModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> BlogByTag([FromBody] BlogPagingFilteringModelDto command)
        {
            if (!_blogSettings.Enabled)
                return BadRequest($"The setting {nameof(_blogSettings.Enabled)} is not true.");

            var model = await _blogModelFactory.PrepareBlogPostListModelAsync(command.FromDto<BlogPagingFilteringModel>());
            
            return Ok(model.ToDto<BlogPostListModelDto>());
        }

        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BlogPostListModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> BlogByMonth([FromBody] BlogPagingFilteringModelDto command)
        {
            if (!_blogSettings.Enabled)
                return BadRequest($"The setting {nameof(_blogSettings.Enabled)} is not true.");

            var model = await _blogModelFactory.PrepareBlogPostListModelAsync(command.FromDto<BlogPagingFilteringModel>());
            
            return Ok(model.ToDto<BlogPostListModelDto>());
        }

        [HttpGet("{languageId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ListRss(int languageId)
        {  
            if (!_blogSettings.Enabled)
                return BadRequest($"The setting {nameof(_blogSettings.Enabled)} is not true.");
            var store = await _storeContext.GetCurrentStoreAsync();

            var feed = new RssFeed(
                $"{await _localizationService.GetLocalizedAsync(store, x => x.Name)}: Blog",
                "Blog",
                new Uri(_webHelper.GetStoreLocation()),
                DateTime.UtcNow);

            var items = new List<RssItem>();
            var blogPosts = await _blogService.GetAllBlogPostsAsync((await _storeContext.GetCurrentStoreAsync()).Id, languageId);
            foreach (var blogPost in blogPosts)
            {
                var seName = await _urlRecordService.GetSeNameAsync(blogPost, blogPost.LanguageId, ensureTwoPublishedLanguages: false);
                var blogPostUrl = await _nopUrlHelper.RouteGenericUrlAsync<BlogPost>(new { SeName = seName }, _webHelper.GetCurrentRequestProtocol());
                items.Add(new RssItem(blogPost.Title, blogPost.Body, new Uri(blogPostUrl), $"urn:store:{store.Id}:blog:post:{blogPost.Id}", blogPost.CreatedOnUtc));
            }

            feed.Items = items;

            return Ok(feed.GetContent());
        }

        [HttpGet("{blogPostId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(BlogPostModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> GetBlogPost(int blogPostId)
        {
            if (!_blogSettings.Enabled)
                return BadRequest($"The setting {nameof(_blogSettings.Enabled)} is not true.");

            var blogPost = await _blogService.GetBlogPostByIdAsync(blogPostId);
            if (blogPost == null)
                 return NotFound($"Blog post Id={blogPostId} not found");

            var notAvailable =
                //availability dates
                !_blogService.BlogPostIsAvailable(blogPost) ||
                //Store mapping
                !await _storeMappingService.AuthorizeAsync(blogPost);
            //Check whether the current user has a "Manage blog" permission (usually a store owner)
            //We should allows him (her) to use "Preview" functionality
            var hasAdminAccess = await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel) && await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageBlog);
            if (notAvailable && !hasAdminAccess)
                 return NotFound($"Blog post Id={blogPostId} is not available or current user has not a 'Manage blog' permission.");

            var model = new BlogPostModel();
            await _blogModelFactory.PrepareBlogPostModelAsync(model, blogPost, true);

            return Ok(model.ToDto<BlogPostModelDto>());
        }

        [HttpPost("{blogPostId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> BlogCommentAdd([FromBody] BlogPostModelDto model, int blogPostId)
        {
            if (!_blogSettings.Enabled)
                return BadRequest($"The setting {nameof(_blogSettings.Enabled)} is not true.");

            var blogPost = await _blogService.GetBlogPostByIdAsync(blogPostId);
            if (blogPost == null)
                return NotFound($"Blog post Id={blogPostId} not found");

            if (!blogPost.AllowComments)
                return BadRequest($"The setting {nameof(blogPost.AllowComments)} is not true.");

            var store = await _storeContext.GetCurrentStoreAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var comment = new BlogComment
            {
                BlogPostId = blogPost.Id,
                CustomerId = customer.Id,
                CommentText = model.AddNewComment.CommentText,
                IsApproved = !_blogSettings.BlogCommentsMustBeApproved,
                StoreId = store.Id,
                CreatedOnUtc = DateTime.UtcNow,
            };

            await _blogService.InsertBlogCommentAsync(comment);

            //notify a store owner
            if (_blogSettings.NotifyAboutNewBlogComments)
                await _workflowMessageService.SendBlogCommentStoreOwnerNotificationMessageAsync(comment,
                    _localizationSettings.DefaultAdminLanguageId);

            //activity log
            await _customerActivityService.InsertActivityAsync("PublicStore.AddBlogComment",
                await _localizationService.GetResourceAsync("ActivityLog.PublicStore.AddBlogComment"), comment);

            //raise event
            if (comment.IsApproved)
                await _eventPublisher.PublishAsync(new BlogCommentApprovedEvent(comment));

            return Ok();
        }

        #endregion
    }
}