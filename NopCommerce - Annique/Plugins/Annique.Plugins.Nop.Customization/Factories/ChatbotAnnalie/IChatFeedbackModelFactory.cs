using Annique.Plugins.Nop.Customization.Domain.ChatbotAnnalie;
using Annique.Plugins.Nop.Customization.Models.ChatbotAnnalie;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Factories.ChatbotAnnalie
{
    public interface IChatFeedbackModelFactory
    {
        /// <summary>
        /// Prepare feedback search model
        /// </summary>
        /// <param name="searchModel">feedback search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the feedback search model
        /// </returns>
        Task<ChatFeedbackSearchModel> PrepareFeedbackSearchModelAsync(ChatFeedbackSearchModel searchModel);

        /// <summary>
        /// Prepare paged feedback list model
        /// </summary>
        /// <param name="searchModel">feedback search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the feedback list model
        /// </returns>
        Task<ChatFeedbackListModel> PrepareFeedbackListModelAsync(ChatFeedbackSearchModel searchModel);

        /// <summary>
        /// Prepare feedback model
        /// </summary>
        /// <param name="model">Log model</param>
        /// <param name="feedback">feedback</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the feedback model
        /// </returns>
        Task<ChatFeedbackModel> PrepareFeedbackModelAsync(ChatFeedbackModel model, ChatbotFeedback chatbotFeedback);
    }
}
