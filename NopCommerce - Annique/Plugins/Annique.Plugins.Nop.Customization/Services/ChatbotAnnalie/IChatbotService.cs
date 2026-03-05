using Annique.Plugins.Nop.Customization.Domain.ChatbotAnnalie;
using Nop.Core;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.ChatbotAnnalie
{
    public interface IChatbotService
    {
        /// <summary>
        /// Inserts a feedback
        /// </summary>
        /// <param name="feedback">feedback</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task InsertFeedbackAsync(ChatbotFeedback chatbotFeedback);

        /// <summary>
        /// Checks customer is allowed to use chatbot or not
        /// </summary>
        /// <returns>A task returns true or false based on roles</returns>
        Task<bool> IsCustomerAllowedAsync();

        /// <summary>
        /// Gets all feedbacks
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the chatbot feedback items
        /// </returns>
        Task<IPagedList<ChatbotFeedback>> GetAllChatbotFeedbackAsync(int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Gets a feedback item
        /// </summary>
        /// <param name="feedbackId">Log item identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the feedback item
        /// </returns>
        Task<ChatbotFeedback> GetFeedbackByIdAsync(int feedbackId);
    }
}
