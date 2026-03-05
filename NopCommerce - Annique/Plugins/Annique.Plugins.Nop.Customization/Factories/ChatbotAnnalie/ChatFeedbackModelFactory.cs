using Annique.Plugins.Nop.Customization.Domain.ChatbotAnnalie;
using Annique.Plugins.Nop.Customization.Models.ChatbotAnnalie;
using Annique.Plugins.Nop.Customization.Services.ChatbotAnnalie;
using Nop.Services.Helpers;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Factories.ChatbotAnnalie
{
    public class ChatFeedbackModelFactory : IChatFeedbackModelFactory
    {
        private readonly IChatbotService _chatbotService;
        private readonly IDateTimeHelper _dateTimeHelper;

        public ChatFeedbackModelFactory(IChatbotService chatbotService,
            IDateTimeHelper dateTimeHelper)
        {
            _chatbotService = chatbotService;
            _dateTimeHelper = dateTimeHelper;
        }

        /// <summary>
        /// Prepare feedback search model
        /// </summary>
        /// <param name="searchModel">feedback search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the feedback search model
        /// </returns>
        public virtual Task<ChatFeedbackSearchModel> PrepareFeedbackSearchModelAsync(ChatFeedbackSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return Task.FromResult(searchModel);
        }

        /// <summary>
        /// Prepare paged feedback list model
        /// </summary>
        /// <param name="searchModel">feedback search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the feedback list model
        /// </returns>
        public virtual async Task<ChatFeedbackListModel> PrepareFeedbackListModelAsync(ChatFeedbackSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var feedbackItems = await _chatbotService.GetAllChatbotFeedbackAsync(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new ChatFeedbackListModel().PrepareToGridAsync(searchModel, feedbackItems, () =>
            {
                //fill in model values from the entity
                return feedbackItems.SelectAwait(async feedbackItem =>
                {
                    //fill in model values from the entity
                    var feedbackModel = new ChatFeedbackModel() {
                        Id = feedbackItem.Id,
                        OriginalMessage = feedbackItem.OriginalMessage,
                        AiResponse = feedbackItem.AiResponse,
                        Username = feedbackItem.Username,
                        Status = feedbackItem.Status,
                        IpAddress = feedbackItem.IpAddress,
                    };

                    //convert dates to the user time
                    feedbackModel.CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(feedbackItem.CreatedOnUtc, DateTimeKind.Utc);

                    return feedbackModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare feedback model
        /// </summary>
        /// <param name="model">Log model</param>
        /// <param name="feedback">feedback</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the feedback model
        /// </returns>
        public virtual async Task<ChatFeedbackModel> PrepareFeedbackModelAsync(ChatFeedbackModel model, ChatbotFeedback chatbotFeedback)
        {
            if (chatbotFeedback != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = new ChatFeedbackModel()
                    {
                        Id = chatbotFeedback.Id,
                        OriginalMessage = chatbotFeedback.OriginalMessage,
                        AiResponse = chatbotFeedback.AiResponse,
                        Username = chatbotFeedback.Username,
                        Status = chatbotFeedback.Status,
                        IpAddress= chatbotFeedback.IpAddress,
                    };

                    model.CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(chatbotFeedback.CreatedOnUtc, DateTimeKind.Utc);
                }
            }
            return model;
        }
    }
}
