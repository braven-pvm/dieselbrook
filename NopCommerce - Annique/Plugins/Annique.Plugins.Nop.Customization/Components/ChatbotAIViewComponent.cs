using Annique.Plugins.Nop.Customization.Services.ChatbotAnnalie;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Configuration;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Components
{
    /// <summary>
    /// Represents a view component that displays chatbot nav link and view
    /// </summary>
    [ViewComponent(Name = "ChatbotAIView")]
    public class ChatbotAIViewComponent : ViewComponent
    {
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IChatbotService _chatbotService;

        #region Ctor

        public ChatbotAIViewComponent(ISettingService settingService,
            IStoreContext storeContext,
            IChatbotService chatbotService)
        {
            _settingService = settingService;
            _storeContext = storeContext;   
            _chatbotService = chatbotService;
        }

        #endregion

        #region Method

        /// <summary>
        /// Invoke view component
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the view component result
        /// </returns>
        public async Task<IViewComponentResult> InvokeAsync()
        {
            var storeScope = await _storeContext.GetCurrentStoreAsync();
            var settings = await _settingService.LoadSettingAsync<AnniqueCustomizationSettings>(storeScope.Id);

            if(!settings.IsChatbotEnable) 
                return Content(string.Empty);

            var hasAccess = await _chatbotService.IsCustomerAllowedAsync();

            return hasAccess ? View() : Content(string.Empty);
        }

        #endregion
    }
}
