using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Forums;
using Nop.Core.Rss;
using Nop.Plugin.Misc.WebApi.Framework.Infrastructure.Mapper.Extensions;
using Nop.Plugin.Misc.WebApi.Frontend.Dto.Boards;
using Nop.Services.Customers;
using Nop.Services.Forums;
using Nop.Services.Localization;
using Nop.Web.Factories;
using Nop.Web.Models.Boards;

namespace Nop.Plugin.Misc.WebApi.Frontend.Controllers
{
    public partial class BoardsController : BaseNopWebApiFrontendController
    {
        #region Fields

        private readonly ForumSettings _forumSettings;
        private readonly ICustomerService _customerService;
        private readonly IForumModelFactory _forumModelFactory;
        private readonly IForumService _forumService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public BoardsController(ForumSettings forumSettings,
            ICustomerService customerService,
            IForumModelFactory forumModelFactory,
            IForumService forumService,
            ILocalizationService localizationService,
            IStoreContext storeContext,
            IWebHelper webHelper,
            IWorkContext workContext)
        {
            _forumSettings = forumSettings;
            _customerService = customerService;
            _forumModelFactory = forumModelFactory;
            _forumService = forumService;
            _localizationService = localizationService;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _workContext = workContext;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Index
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(BoardsIndexModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Index()
        {
            if (!_forumSettings.ForumsEnabled)
                return NotFound($"The setting {nameof(_forumSettings.ForumsEnabled)} is not enabled.");

            var model = await _forumModelFactory.PrepareBoardsIndexModelAsync();

            return Ok(model.ToDto<BoardsIndexModelDto>());
        }

        /// <summary>
        /// Active discussions
        /// </summary>
        /// <param name="forumId">Forum identifier</param>
        /// <param name="pageNumber">Number of forum topics page</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ActiveDiscussionsModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ActiveDiscussions([FromQuery] int forumId = 0, [FromQuery] int pageNumber = 1)
        {
            if (!_forumSettings.ForumsEnabled)
                return NotFound($"The setting {nameof(_forumSettings.ForumsEnabled)} is not enabled.");

            var model = await _forumModelFactory.PrepareActiveDiscussionsModelAsync(forumId, pageNumber);

            return Ok(model.ToDto<ActiveDiscussionsModelDto>());
        }

        /// <summary>
        /// Active discussions RSS
        /// </summary>
        /// <param name="forumId">Forum identifier</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ActiveDiscussionsRss([FromQuery] int forumId = 0)
        {
            if (!_forumSettings.ForumsEnabled)
                return NotFound($"The setting {nameof(_forumSettings.ForumsEnabled)} is not enabled.");

            if (!_forumSettings.ActiveDiscussionsFeedEnabled)
                return NotFound($"The setting {nameof(_forumSettings.ActiveDiscussionsFeedEnabled)} is not enabled.");

            var topics = await _forumService.GetActiveTopicsAsync(forumId, 0, _forumSettings.ActiveDiscussionsFeedCount);
            var url = Url.RouteUrl("ActiveDiscussionsRSS", null, _webHelper.GetCurrentRequestProtocol());

            var feedTitle = await _localizationService.GetResourceAsync("Forum.ActiveDiscussionsFeedTitle");
            var feedDescription = await _localizationService.GetResourceAsync("Forum.ActiveDiscussionsFeedDescription");
            var store = await _storeContext.GetCurrentStoreAsync();
            var feed = new RssFeed(
                string.Format(feedTitle, await _localizationService.GetLocalizedAsync(store, x => x.Name)),
                feedDescription,
                new Uri(url),
                DateTime.UtcNow);

            var items = new List<RssItem>();

            var viewsText = await _localizationService.GetResourceAsync("Forum.Views");
            var repliesText = await _localizationService.GetResourceAsync("Forum.Replies");

            foreach (var topic in topics)
            {
                var topicUrl = Url.RouteUrl("TopicSlug", new { id = topic.Id, slug = await _forumService.GetTopicSeNameAsync(topic) }, _webHelper.GetCurrentRequestProtocol());
                var content = $"{repliesText}: {(topic.NumPosts > 0 ? topic.NumPosts - 1 : 0)}, {viewsText}: {topic.Views}";

                items.Add(new RssItem(topic.Subject, content, new Uri(topicUrl),
                    $"urn:store:{store.Id}:activeDiscussions:topic:{topic.Id}", topic.LastPostTime ?? topic.UpdatedOnUtc));
            }
            feed.Items = items;

            return Ok(feed.GetContent());
        }

        /// <summary>
        /// Gets a forum group
        /// </summary>
        /// <param name="id">The forum group identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ForumGroupModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ForumGroup(int id)
        {
            if (!_forumSettings.ForumsEnabled)
                return NotFound($"The setting {nameof(_forumSettings.ForumsEnabled)} is not enabled.");

            var forumGroup = await _forumService.GetForumGroupByIdAsync(id);
            if (forumGroup == null)
                return NotFound($"The forum group by id={id} is not found.");

            var model = await _forumModelFactory.PrepareForumGroupModelAsync(forumGroup);

            return Ok(model.ToDto<ForumGroupModelDto>());
        }

        /// <summary>
        /// Get forum page
        /// </summary>
        /// <param name="id">Forum identifier</param>
        /// <param name="pageNumber">Number of forum page</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ForumPageModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Forum(int id, [FromQuery] int pageNumber = 1)
        {
            if (!_forumSettings.ForumsEnabled)
                return NotFound($"The setting {nameof(_forumSettings.ForumsEnabled)} is not enabled.");

            var forum = await _forumService.GetForumByIdAsync(id);
            if (forum == null)
                return NotFound($"The forum by id={id} is not found.");

            var model = await _forumModelFactory.PrepareForumPageModelAsync(forum, pageNumber);

            return Ok(model.ToDto<ForumPageModelDto>());
        }

        /// <summary>
        /// Forum RSS
        /// </summary>
        /// <param name="id">Forum identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ForumRss(int id)
        {
            if (!_forumSettings.ForumsEnabled)
                return NotFound($"The setting {nameof(_forumSettings.ForumsEnabled)} is not enabled.");

            if (!_forumSettings.ForumFeedsEnabled)
                return NotFound($"The setting {nameof(_forumSettings.ForumFeedsEnabled)} is not enabled.");

            var topicLimit = _forumSettings.ForumFeedCount;
            var forum = await _forumService.GetForumByIdAsync(id);

            if (forum != null)
            {
                //Order by newest topic posts & limit the number of topics to return
                var topics = await _forumService.GetAllTopicsAsync(forum.Id, 0, string.Empty,
                     ForumSearchType.All, 0, 0, topicLimit);

                var url = Url.RouteUrl("ForumRSS", new { id = forum.Id }, _webHelper.GetCurrentRequestProtocol());

                var feedTitle = await _localizationService.GetResourceAsync("Forum.ForumFeedTitle");
                var feedDescription = await _localizationService.GetResourceAsync("Forum.ForumFeedDescription");
                var store = await _storeContext.GetCurrentStoreAsync();
                var feed = new RssFeed(
                    string.Format(feedTitle, await _localizationService.GetLocalizedAsync(store, x => x.Name), forum.Name),
                    feedDescription,
                    new Uri(url),
                    DateTime.UtcNow);

                var items = new List<RssItem>();

                var viewsText = await _localizationService.GetResourceAsync("Forum.Views");
                var repliesText = await _localizationService.GetResourceAsync("Forum.Replies");

                foreach (var topic in topics)
                {
                    var topicUrl = Url.RouteUrl("TopicSlug", new { id = topic.Id, slug = await _forumService.GetTopicSeNameAsync(topic) }, _webHelper.GetCurrentRequestProtocol());
                    var content = $"{repliesText}: {(topic.NumPosts > 0 ? topic.NumPosts - 1 : 0)}, {viewsText}: {topic.Views}";

                    items.Add(new RssItem(topic.Subject, content, new Uri(topicUrl), $"urn:store:{store.Id}:forum:topic:{topic.Id}", topic.LastPostTime ?? topic.UpdatedOnUtc));
                }

                feed.Items = items;

                return Ok(feed.GetContent());
            }

            return Ok(string.Empty);
        }

        /// <summary>
        /// Forum watch
        /// </summary>
        /// <param name="id">Forum identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ForumWatchResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> ForumWatch(int id)
        {
            var watchTopic = await _localizationService.GetResourceAsync("Forum.WatchForum");
            var unwatchTopic = await _localizationService.GetResourceAsync("Forum.UnwatchForum");
            var returnText = watchTopic;

            var forum = await _forumService.GetForumByIdAsync(id);
            if (forum == null)
                return Ok(new ForumWatchResponse { Subscribed = false, Text = returnText, Error = true });

            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _forumService.IsCustomerAllowedToSubscribeAsync(customer))
                return Ok(new ForumWatchResponse { Subscribed = false, Text = returnText, Error = true });

            var forumSubscription = (await _forumService.GetAllSubscriptionsAsync(customer.Id,
                forum.Id, 0, 0, 1)).FirstOrDefault();

            bool subscribed;
            if (forumSubscription == null)
            {
                forumSubscription = new ForumSubscription
                {
                    SubscriptionGuid = Guid.NewGuid(),
                    CustomerId = customer.Id,
                    ForumId = forum.Id,
                    CreatedOnUtc = DateTime.UtcNow
                };
                await _forumService.InsertSubscriptionAsync(forumSubscription);
                subscribed = true;
                returnText = unwatchTopic;
            }
            else
            {
                await _forumService.DeleteSubscriptionAsync(forumSubscription);
                subscribed = false;
            }

            return Ok(new ForumWatchResponse { Subscribed = subscribed, Text = returnText, Error = false });
        }

        /// <summary>
        /// Get topic page
        /// </summary>
        /// <param name="id">Topic identifier</param>
        /// <param name="pageNumber">Number of topic page</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ForumTopicPageModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Topic(int id, [FromQuery] int pageNumber = 1)
        {
            if (!_forumSettings.ForumsEnabled)
                return NotFound($"The setting {nameof(_forumSettings.ForumsEnabled)} is not enabled.");

            var forumTopic = await _forumService.GetTopicByIdAsync(id);
            if (forumTopic == null)
                return NotFound($"The forum topic by id={id} is not found.");

            var model = await _forumModelFactory.PrepareForumTopicPageModelAsync(forumTopic, pageNumber);

            //update view count
            forumTopic.Views += 1;
            await _forumService.UpdateTopicAsync(forumTopic);

            return Ok(model.ToDto<ForumTopicPageModelDto>());
        }

        /// <summary>
        /// Topic watch
        /// </summary>
        /// <param name="id">Topic identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(TopicWatchResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> TopicWatch(int id)
        {
            var watchTopic = await _localizationService.GetResourceAsync("Forum.WatchTopic");
            var unwatchTopic = await _localizationService.GetResourceAsync("Forum.UnwatchTopic");
            var returnText = watchTopic;

            var forumTopic = await _forumService.GetTopicByIdAsync(id);
            if (forumTopic == null)
                return Ok(new TopicWatchResponse { Subscribed = false, Text = returnText, Error = true });

            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _forumService.IsCustomerAllowedToSubscribeAsync(customer))
                return Ok(new TopicWatchResponse { Subscribed = false, Text = returnText, Error = true });

            var forumSubscription = (await _forumService.GetAllSubscriptionsAsync(customer.Id,
                0, forumTopic.Id, 0, 1)).FirstOrDefault();

            bool subscribed;
            if (forumSubscription == null)
            {
                forumSubscription = new ForumSubscription
                {
                    SubscriptionGuid = Guid.NewGuid(),
                    CustomerId = customer.Id,
                    TopicId = forumTopic.Id,
                    CreatedOnUtc = DateTime.UtcNow
                };
                await _forumService.InsertSubscriptionAsync(forumSubscription);
                subscribed = true;
                returnText = unwatchTopic;
            }
            else
            {
                await _forumService.DeleteSubscriptionAsync(forumSubscription);
                subscribed = false;
            }

            return Ok(new TopicWatchResponse { Subscribed = subscribed, Text = returnText, Error = false });
        }

        /// <summary>
        /// Get topic move model
        /// </summary>
        /// <param name="id">Topic identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(TopicMoveModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> TopicMove(int id)
        {
            if (!_forumSettings.ForumsEnabled)
                return NotFound($"The setting {nameof(_forumSettings.ForumsEnabled)} is not enabled.");

            var forumTopic = await _forumService.GetTopicByIdAsync(id);
            if (forumTopic == null)
                return NotFound($"The forum topic by id={id} is not found.");

            var model = await _forumModelFactory.PrepareTopicMoveAsync(forumTopic);

            return Ok(model.ToDto<TopicMoveModelDto>());
        }

        /// <summary>
        /// Topic delete
        /// </summary>
        /// <param name="id">Topic identifier</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> TopicDelete(int id)
        {
            if (!_forumSettings.ForumsEnabled)
                return NotFound($"The setting {nameof(_forumSettings.ForumsEnabled)} is not enabled.");

            var forumTopic = await _forumService.GetTopicByIdAsync(id);
            if (forumTopic != null)
            {
                if (!await _forumService.IsCustomerAllowedToDeleteTopicAsync(await _workContext.GetCurrentCustomerAsync(), forumTopic))
                    return BadRequest("Customer is not allowed to delete topic");

                var forum = await _forumService.GetForumByIdAsync(forumTopic.ForumId);

                await _forumService.DeleteTopicAsync(forumTopic);

                if (forum != null)
                    return Ok(Url.RouteUrl("ForumSlug", new { id = forum.Id, slug = await _forumService.GetForumSeNameAsync(forum) }));
            }

            return Ok(Url.RouteUrl("Boards"));
        }

        /// <summary>
        /// Prepare the forum topic create model
        /// </summary>
        /// <param name="id">The forum identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(EditForumTopicModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> TopicCreate(int id)
        {
            if (!_forumSettings.ForumsEnabled)
                return NotFound($"The setting {nameof(_forumSettings.ForumsEnabled)} is not enabled.");

            var forum = await _forumService.GetForumByIdAsync(id);
            if (forum == null)
                return NotFound($"The forum by id={id} is not found.");

            if (await _forumService.IsCustomerAllowedToCreateTopicAsync(await _workContext.GetCurrentCustomerAsync(), forum) == false)
                return BadRequest("Customer is not allowed to create topic");

            var model = new EditForumTopicModel();
            await _forumModelFactory.PrepareTopicCreateModelAsync(forum, model);

            return Ok(model.ToDto<EditForumTopicModelDto>());
        }

        /// <summary>
        /// Topic create
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(EditForumTopicModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> TopicCreate([FromBody] EditForumTopicModelDto model)
        {
            if (!_forumSettings.ForumsEnabled)
                return NotFound($"The setting {nameof(_forumSettings.ForumsEnabled)} is not enabled.");

            var forum = await _forumService.GetForumByIdAsync(model.ForumId);
            if (forum == null)
                return NotFound($"The forum by id={model.ForumId} is not found.");

            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _forumService.IsCustomerAllowedToCreateTopicAsync(customer, forum))
                return BadRequest("Customer is not allowed to create topic");

            var subject = model.Subject;
            var maxSubjectLength = _forumSettings.TopicSubjectMaxLength;
            if (maxSubjectLength > 0 && subject.Length > maxSubjectLength) 
                subject = subject[0..maxSubjectLength];

            var text = model.Text;
            var maxPostLength = _forumSettings.PostMaxLength;
            if (maxPostLength > 0 && text.Length > maxPostLength)
                text = text[0..maxPostLength];

            var topicType = ForumTopicType.Normal;
            var ipAddress = _webHelper.GetCurrentIpAddress();
            var nowUtc = DateTime.UtcNow;

            if (await _forumService.IsCustomerAllowedToSetTopicPriorityAsync(await _workContext.GetCurrentCustomerAsync()))
                topicType = (ForumTopicType)Enum.ToObject(typeof(ForumTopicType), model.TopicTypeId);

            //forum topic
            var forumTopic = new ForumTopic
            {
                ForumId = forum.Id,
                CustomerId = customer.Id,
                TopicTypeId = (int)topicType,
                Subject = subject,
                CreatedOnUtc = nowUtc,
                UpdatedOnUtc = nowUtc
            };
            await _forumService.InsertTopicAsync(forumTopic, true);

            //forum post
            var forumPost = new ForumPost
            {
                TopicId = forumTopic.Id,
                CustomerId = customer.Id,
                Text = text,
                IPAddress = ipAddress,
                CreatedOnUtc = nowUtc,
                UpdatedOnUtc = nowUtc
            };
            await _forumService.InsertPostAsync(forumPost, false);

            //update forum topic
            forumTopic.NumPosts = 1;
            forumTopic.LastPostId = forumPost.Id;
            forumTopic.LastPostCustomerId = forumPost.CustomerId;
            forumTopic.LastPostTime = forumPost.CreatedOnUtc;
            forumTopic.UpdatedOnUtc = nowUtc;
            await _forumService.UpdateTopicAsync(forumTopic);

            //subscription                
            if (await _forumService.IsCustomerAllowedToSubscribeAsync(customer))
                if (model.Subscribed)
                {
                    var forumSubscription = new ForumSubscription
                    {
                        SubscriptionGuid = Guid.NewGuid(),
                        CustomerId = customer.Id,
                        TopicId = forumTopic.Id,
                        CreatedOnUtc = nowUtc
                    };

                    await _forumService.InsertSubscriptionAsync(forumSubscription);
                }

            //redisplay form
            var editForumTopicModel = model.FromDto<EditForumTopicModel>();
            await _forumModelFactory.PrepareTopicCreateModelAsync(forum, editForumTopicModel);

            return Ok(editForumTopicModel.ToDto<EditForumTopicModelDto>());
        }

        /// <summary>
        /// Edit the forum topic
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(EditForumTopicModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> TopicEdit(int id)
        {
            if (!_forumSettings.ForumsEnabled)
                return NotFound($"The setting {nameof(_forumSettings.ForumsEnabled)} is not enabled.");

            var forumTopic = await _forumService.GetTopicByIdAsync(id);
            if (forumTopic == null)
                return NotFound($"The forum topic by id={id} is not found.");

            if (!await _forumService.IsCustomerAllowedToEditTopicAsync(await _workContext.GetCurrentCustomerAsync(), forumTopic))
                return BadRequest("Customer is not allowed to edit forum topic");

            var model = new EditForumTopicModel();
            await _forumModelFactory.PrepareTopicEditModelAsync(forumTopic, model, false);

            return Ok(model.ToDto<EditForumTopicModelDto>());
        }

        /// <summary>
        /// Edit the forum topic
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(EditForumTopicModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> TopicEdit([FromBody] EditForumTopicModelDto model)
        {
            if (!_forumSettings.ForumsEnabled)
                return NotFound($"The setting {nameof(_forumSettings.ForumsEnabled)} is not enabled.");

            var forumTopic = await _forumService.GetTopicByIdAsync(model.Id);
            if (forumTopic == null)
                return NotFound($"The forum topic by id={model.Id} is not found.");

            var forum = await _forumService.GetForumByIdAsync(forumTopic.ForumId);
            if (forum == null)
                return NotFound($"The forum by id={model.Id} is not found.");

            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _forumService.IsCustomerAllowedToEditTopicAsync(customer, forumTopic))
                return BadRequest("Customer is not allowed to edit forum topic");

            var subject = model.Subject;
            var maxSubjectLength = _forumSettings.TopicSubjectMaxLength;
            if (maxSubjectLength > 0 && subject.Length > maxSubjectLength) subject = subject[0..maxSubjectLength];

            var text = model.Text;
            var maxPostLength = _forumSettings.PostMaxLength;
            if (maxPostLength > 0 && text.Length > maxPostLength)
                text = text[0..maxPostLength];

            var topicType = ForumTopicType.Normal;
            var ipAddress = _webHelper.GetCurrentIpAddress();
            var nowUtc = DateTime.UtcNow;

            if (await _forumService.IsCustomerAllowedToSetTopicPriorityAsync(customer))
                topicType = (ForumTopicType)Enum.ToObject(typeof(ForumTopicType), model.TopicTypeId);

            //forum topic
            forumTopic.TopicTypeId = (int)topicType;
            forumTopic.Subject = subject;
            forumTopic.UpdatedOnUtc = nowUtc;
            await _forumService.UpdateTopicAsync(forumTopic);

            //forum post                
            var firstPost = await _forumService.GetFirstPostAsync(forumTopic);
            if (firstPost != null)
            {
                firstPost.Text = text;
                firstPost.UpdatedOnUtc = nowUtc;
                await _forumService.UpdatePostAsync(firstPost);
            }
            else
            {
                //error (not possible)
                firstPost = new ForumPost
                {
                    TopicId = forumTopic.Id,
                    CustomerId = forumTopic.CustomerId,
                    Text = text,
                    IPAddress = ipAddress,
                    UpdatedOnUtc = nowUtc
                };

                await _forumService.InsertPostAsync(firstPost, false);
            }

            //subscription
            if (await _forumService.IsCustomerAllowedToSubscribeAsync(customer))
            {
                var forumSubscription = (await _forumService.GetAllSubscriptionsAsync(customer.Id,
                    0, forumTopic.Id, 0, 1)).FirstOrDefault();
                if (model.Subscribed)
                {
                    if (forumSubscription == null)
                    {
                        forumSubscription = new ForumSubscription
                        {
                            SubscriptionGuid = Guid.NewGuid(),
                            CustomerId = customer.Id,
                            TopicId = forumTopic.Id,
                            CreatedOnUtc = nowUtc
                        };

                        await _forumService.InsertSubscriptionAsync(forumSubscription);
                    }
                }
                else
                {
                    if (forumSubscription != null) await _forumService.DeleteSubscriptionAsync(forumSubscription);
                }
            }

            //redisplay form
            var editForumTopicModel = model.FromDto<EditForumTopicModel>();
            await _forumModelFactory.PrepareTopicEditModelAsync(forumTopic, editForumTopicModel, true);

            return Ok(editForumTopicModel.ToDto<EditForumTopicModelDto>());
        }

        /// <summary>
        /// Delete the forum post
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(EditForumTopicModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> PostDelete(int id)
        {
            if (!_forumSettings.ForumsEnabled)
                return NotFound($"The setting {nameof(_forumSettings.ForumsEnabled)} is not enabled.");

            var forumPost = await _forumService.GetPostByIdAsync(id);
            if (forumPost == null)
                return NotFound($"The forum post by id={id} is not found.");

            if (!await _forumService.IsCustomerAllowedToDeletePostAsync(await _workContext.GetCurrentCustomerAsync(), forumPost))
                return BadRequest("Customer is not allowed to delete forum topic");

            await _forumService.DeletePostAsync(forumPost);

            return Ok();
        }

        /// <summary>
        /// Prepare forum post model
        /// </summary>
        /// <param name="id">Forum topic identifier</param>
        /// <param name="quoteId">Identifier of the quoted post; pass 0 to load the empty text</param>
        [HttpGet("{id}/{quote}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(EditForumPostModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> PostCreate(int id, int quoteId)
        {
            if (!_forumSettings.ForumsEnabled)
                return NotFound($"The setting {nameof(_forumSettings.ForumsEnabled)} is not enabled.");

            var forumTopic = await _forumService.GetTopicByIdAsync(id);
            if (forumTopic == null)
                return NotFound($"The forum topic by id={id} is not found.");

            if (!await _forumService.IsCustomerAllowedToCreatePostAsync(await _workContext.GetCurrentCustomerAsync(), forumTopic))
                return BadRequest("Customer is not allowed to create post");

            var model = await _forumModelFactory.PreparePostCreateModelAsync(forumTopic, quoteId, false);

            return Ok(model.ToDto<EditForumPostModelDto>());
        }

        /// <summary>
        /// Create forum post
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(EditForumPostModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> PostCreate([FromBody] EditForumPostModelDto model)
        {
            if (!_forumSettings.ForumsEnabled)
                return NotFound($"The setting {nameof(_forumSettings.ForumsEnabled)} is not enabled.");

            var forumTopic = await _forumService.GetTopicByIdAsync(model.ForumTopicId);
            if (forumTopic == null)
                return NotFound($"The forum topic by id={model.ForumTopicId} is not found.");

            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _forumService.IsCustomerAllowedToCreatePostAsync(customer, forumTopic))
                return BadRequest("Customer is not allowed to create post");

            var text = model.Text;
            var maxPostLength = _forumSettings.PostMaxLength;
            if (maxPostLength > 0 && text.Length > maxPostLength)
                text = text[0..maxPostLength];

            var ipAddress = _webHelper.GetCurrentIpAddress();

            var nowUtc = DateTime.UtcNow;

            var forumPost = new ForumPost
            {
                TopicId = forumTopic.Id,
                CustomerId = customer.Id,
                Text = text,
                IPAddress = ipAddress,
                CreatedOnUtc = nowUtc,
                UpdatedOnUtc = nowUtc
            };
            await _forumService.InsertPostAsync(forumPost, true);

            //subscription
            if (await _forumService.IsCustomerAllowedToSubscribeAsync(customer))
            {
                var forumSubscription = (await _forumService.GetAllSubscriptionsAsync(customer.Id,
                    0, forumPost.TopicId, 0, 1)).FirstOrDefault();
                if (model.Subscribed)
                {
                    if (forumSubscription == null)
                    {
                        forumSubscription = new ForumSubscription
                        {
                            SubscriptionGuid = Guid.NewGuid(),
                            CustomerId = customer.Id,
                            TopicId = forumPost.TopicId,
                            CreatedOnUtc = nowUtc
                        };

                        await _forumService.InsertSubscriptionAsync(forumSubscription);
                    }
                }
                else
                {
                    if (forumSubscription != null)
                        await _forumService.DeleteSubscriptionAsync(forumSubscription);
                }
            }

            //redisplay form
            var editForumPostModel = await _forumModelFactory.PreparePostCreateModelAsync(forumTopic, 0, true);

            return Ok(editForumPostModel.ToDto<EditForumPostModelDto>());
        }

        /// <summary>
        /// Prepare the forum post edit model
        /// </summary>
        /// <param name="id">The forum post identifier</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(EditForumPostModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> PostEdit(int id)
        {
            if (!_forumSettings.ForumsEnabled)
                return NotFound($"The setting {nameof(_forumSettings.ForumsEnabled)} is not enabled.");

            var forumPost = await _forumService.GetPostByIdAsync(id);
            if (forumPost == null)
                return NotFound($"The forum post by id={id} is not found.");

            if (!await _forumService.IsCustomerAllowedToEditPostAsync(await _workContext.GetCurrentCustomerAsync(), forumPost))
                return BadRequest("Customer is not allowed to edit forum post");

            var model = await _forumModelFactory.PreparePostEditModelAsync(forumPost, false);

            return Ok(model.ToDto<EditForumPostModelDto>());
        }

        /// <summary>
        /// Edit forum post
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(EditForumPostModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> PostEdit([FromBody] EditForumPostModelDto model)
        {
            if (!_forumSettings.ForumsEnabled)
                return NotFound($"The setting {nameof(_forumSettings.ForumsEnabled)} is not enabled.");

            var forumPost = await _forumService.GetPostByIdAsync(model.Id);
            if (forumPost == null)
                return NotFound($"The forum post by id={model.Id} is not found.");

            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _forumService.IsCustomerAllowedToEditPostAsync(customer, forumPost))
                return BadRequest("Customer is not allowed to edit forum post");

            var forumTopic = await _forumService.GetTopicByIdAsync(forumPost.TopicId);
            if (forumTopic == null)
                return NotFound($"The forum topic by id={forumPost.TopicId} is not found.");

            var forum = await _forumService.GetForumByIdAsync(forumTopic.ForumId);
            if (forum == null)
                return NotFound($"The forum by id={forumTopic.ForumId} is not found.");

            var nowUtc = DateTime.UtcNow;

            var text = model.Text;
            var maxPostLength = _forumSettings.PostMaxLength;
            if (maxPostLength > 0 && text.Length > maxPostLength) text = text[0..maxPostLength];

            forumPost.UpdatedOnUtc = nowUtc;
            forumPost.Text = text;
            await _forumService.UpdatePostAsync(forumPost);

            //subscription
            if (await _forumService.IsCustomerAllowedToSubscribeAsync(customer))
            {
                var forumSubscription = (await _forumService.GetAllSubscriptionsAsync(customer.Id,
                    0, forumPost.TopicId, 0, 1)).FirstOrDefault();
                if (model.Subscribed)
                {
                    if (forumSubscription == null)
                    {
                        forumSubscription = new ForumSubscription
                        {
                            SubscriptionGuid = Guid.NewGuid(),
                            CustomerId = customer.Id,
                            TopicId = forumPost.TopicId,
                            CreatedOnUtc = nowUtc
                        };
                        await _forumService.InsertSubscriptionAsync(forumSubscription);
                    }
                }
                else
                {
                    if (forumSubscription != null) await _forumService.DeleteSubscriptionAsync(forumSubscription);
                }
            }

            //redisplay form
            var editForumPostModel = await _forumModelFactory.PreparePostEditModelAsync(forumPost, true);

            return Ok(editForumPostModel.ToDto<EditForumPostModelDto>());
        }

        /// <summary>
        /// Search terms in forum post
        /// </summary>
        /// <param name="searchTerms">Search terms</param>
        /// <param name="advs">Whether to use the advanced search</param>
        /// <param name="forumId">Forum identifier</param>
        /// <param name="within">String representation of int value of ForumSearchType</param>
        /// <param name="limitDays">Limit by the last number days; 0 to load all topics</param>
        /// <param name="page">Number of items page</param>
        [HttpGet("{forumId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(SearchBoardsModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> Search([FromQuery, Required] string searchTerms,
            [FromQuery] bool? advs,
            string forumId,
            [FromQuery, Required] string within,
            [FromQuery, Required] string limitDays,
            [FromQuery, Required] int page)
        {
            if (!_forumSettings.ForumsEnabled)
                return NotFound($"The setting {nameof(_forumSettings.ForumsEnabled)} is not enabled.");

            var model = await _forumModelFactory.PrepareSearchModelAsync(searchTerms, advs, forumId, within, limitDays,
                page);

            return Ok(model.ToDto<SearchBoardsModelDto>());
        }

        /// <summary>
        /// Prepare the customer forum subscriptions model
        /// </summary>
        /// <param name="pageNumber">Number of items page; pass null to load the first page</param>
        [HttpGet]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CustomerForumSubscriptionsModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> CustomerForumSubscriptions([FromQuery] int? pageNumber)
        {
            if (!_forumSettings.AllowCustomersToManageSubscriptions)
                return NotFound($"The setting {nameof(_forumSettings.AllowCustomersToManageSubscriptions)} is not enabled.");

            var model = await _forumModelFactory.PrepareCustomerForumSubscriptionsModelAsync(pageNumber);

            return Ok(model.ToDto<CustomerForumSubscriptionsModelDto>());
        }

        /// <summary>
        /// Customer forum subscriptions POST
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(CustomerForumSubscriptionsModelDto), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> CustomerForumSubscriptionsPOST([FromBody] IDictionary<string, string> formCollection)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            foreach (var key in formCollection.Keys)
            {
                var value = formCollection[key];

                if (value.Equals("on") && key.StartsWith("fs", StringComparison.InvariantCultureIgnoreCase))
                {
                    var id = key.Replace("fs", "").Trim();
                    if (int.TryParse(id, out var forumSubscriptionId))
                    {
                        var forumSubscription = await _forumService.GetSubscriptionByIdAsync(forumSubscriptionId);
                        if (forumSubscription != null && forumSubscription.CustomerId == customer.Id) await _forumService.DeleteSubscriptionAsync(forumSubscription);
                    }
                }
            }

            if (!_forumSettings.AllowCustomersToManageSubscriptions)
                return NotFound($"The setting {nameof(_forumSettings.AllowCustomersToManageSubscriptions)} is not enabled.");

            var model = await _forumModelFactory.PrepareCustomerForumSubscriptionsModelAsync(null);

            return Ok(model.ToDto<CustomerForumSubscriptionsModelDto>());
        }

        /// <summary>
        /// POst vote
        /// </summary>
        /// <param name="postId">Post identifier</param>
        /// <param name="isUp">Is up</param>
        [HttpGet("{postId}")]
        [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(PostVoteResponse), StatusCodes.Status200OK)]
        public virtual async Task<IActionResult> PostVote(int postId, [FromQuery][Required] bool isUp)
        {
            if (!_forumSettings.AllowPostVoting)
                return NotFound($"The setting {nameof(_forumSettings.AllowPostVoting)} is not enabled.");

            var forumPost = await _forumService.GetPostByIdAsync(postId);
            if (forumPost == null)
                return NotFound($"The forum post by id={postId} is not found.");

            var customer = await _workContext.GetCurrentCustomerAsync();

            if (!await _customerService.IsRegisteredAsync(customer))
                return Ok(new PostVoteResponse
                {
                    Error = await _localizationService.GetResourceAsync("Forum.Votes.Login"),
                    VoteCount = forumPost.VoteCount
                });

            if (customer.Id == forumPost.CustomerId)
                return Ok(new PostVoteResponse
                {
                    Error = await _localizationService.GetResourceAsync("Forum.Votes.OwnPost"),
                    VoteCount = forumPost.VoteCount
                });

            var forumPostVote = await _forumService.GetPostVoteAsync(postId, customer);
            if (forumPostVote != null)
            {
                if ((forumPostVote.IsUp && isUp) || (!forumPostVote.IsUp && !isUp))
                    return Ok(new PostVoteResponse
                    {
                        Error = await _localizationService.GetResourceAsync("Forum.Votes.AlreadyVoted"),
                        VoteCount = forumPost.VoteCount
                    });

                await _forumService.DeletePostVoteAsync(forumPostVote);
                return Ok(new PostVoteResponse { VoteCount = forumPost.VoteCount });
            }

            if (await _forumService.GetNumberOfPostVotesAsync(customer, DateTime.UtcNow.AddDays(-1)) >= _forumSettings.MaxVotesPerDay)
                return Ok(new PostVoteResponse
                {
                    Error = string.Format(await _localizationService.GetResourceAsync("Forum.Votes.MaxVotesReached"), _forumSettings.MaxVotesPerDay),
                    VoteCount = forumPost.VoteCount
                });

            await _forumService.InsertPostVoteAsync(new ForumPostVote
            {
                CustomerId = customer.Id,
                ForumPostId = postId,
                IsUp = isUp,
                CreatedOnUtc = DateTime.UtcNow
            });

            return Ok(new PostVoteResponse { VoteCount = forumPost.VoteCount, IsUp = isUp });
        }

        #endregion
    }
}
