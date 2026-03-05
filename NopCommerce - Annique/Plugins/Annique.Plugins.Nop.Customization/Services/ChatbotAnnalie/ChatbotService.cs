using Annique.Plugins.Nop.Customization.Domain.ChatbotAnnalie;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.ChatbotAnnalie
{
    public class ChatbotService : IChatbotService
    {
        private readonly IRepository<ChatbotFeedback> _chatbotFeedbackRepository;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IStaticCacheManager _staticCacheManager;

        public ChatbotService(IRepository<ChatbotFeedback> chatbotFeedbackRepository,
            ISettingService settingService,
            IStoreContext storeContext,
            ICustomerService customerService,
            IWorkContext workContext,
            IStaticCacheManager staticCacheManager)
        {
            _chatbotFeedbackRepository = chatbotFeedbackRepository;
            _settingService = settingService;
            _storeContext = storeContext;
            _customerService = customerService;
            _workContext = workContext;
            _staticCacheManager = staticCacheManager;
        }

        /// <summary>
        /// Inserts a feedback
        /// </summary>
        /// <param name="feedback">feedback</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task InsertFeedbackAsync(ChatbotFeedback chatbotFeedback)
        {
            await _chatbotFeedbackRepository.InsertAsync(chatbotFeedback);
        }

        /// <summary>
        /// Checks customer is allowed to use chatbot or not
        /// </summary>
        /// <returns>A task returns true or false based on roles</returns>
        public async Task<bool> IsCustomerAllowedAsync()
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            // Prepare cache key 
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(
                AnniqueCustomizationDefaults.ChatbotCustomerAcesssCacheKey, customer.Id);

            // Retrieve from cache or execute the logic to determine chatbot access roles
            return await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var storeScope = await _storeContext.GetCurrentStoreAsync();
                var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(storeScope.Id);

                if (string.IsNullOrEmpty(settings.ChatbotAccessRoles))
                    return false;

                var customerIdsAllowedForChatbot = settings.ChatbotAccessRoles.Split(',').Select(int.Parse).ToList() ?? new List<int>();

                // Get current customer role IDs
                var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);

                // Check if any role ID matches
                return customerRoleIds.Intersect(customerIdsAllowedForChatbot).Any();
            });
        }

        /// <summary>
        /// Gets all feedbacks
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the chatbot feedback items
        /// </returns>
        public virtual async Task<IPagedList<ChatbotFeedback>> GetAllChatbotFeedbackAsync(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var feedback = await _chatbotFeedbackRepository.GetAllPagedAsync(query =>
            {
                return query.OrderByDescending(l => l.CreatedOnUtc);
            }, pageIndex, pageSize);

            return feedback;
        }

        /// <summary>
        /// Gets a feedback item
        /// </summary>
        /// <param name="feedbackId">Log item identifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the feedback item
        /// </returns>
        public virtual async Task<ChatbotFeedback> GetFeedbackByIdAsync(int feedbackId)
        {
            return await _chatbotFeedbackRepository.GetByIdAsync(feedbackId);
        }
    }
}
