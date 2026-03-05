using Nop.Core;
using Nop.Core.Domain.Forums;
using Nop.Core.Infrastructure;
using Nop.Services.Forums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Services.PrivateMessagePopUp
{
    public class CustomPrivateMessageService : ICustomPrivateMessageService
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public CustomPrivateMessageService(IStoreContext storeContext,
            IWorkContext workContext)
        {
            _storeContext = storeContext;
            _workContext = workContext;
        }

        #endregion

        /// <summary>
        /// Removes private message related to giftcard, award and discounts
        /// </summary>
        /// <param name="giftcardId">giftcard idenifier</param>
        /// <param name="awardId">award  idenifier</param>
        /// <param name="discountId">Discount idenifier</param>
        /// </param>
        /// <returns>
        /// A task that Removes private message related to giftcard, award and discounts when redeemed by user
        /// </returns>
        public async Task HandlePrivateMessageAsync(int? giftcardId = null, int? awardId = null, int? discountId = null)
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();

            //do not inject IForumService via constructor because it'll cause circular references
            var _forumService = EngineContext.Current.Resolve<IForumService>();

            // Retrieve all private messages
            var privateMessages = (await _forumService.GetAllPrivateMessagesAsync(store.Id,
               0, customer.Id, null, false, false, string.Empty)).ToList();

            // Filter private messages based on subject
            if (giftcardId.HasValue)
            {
                var giftcardSubject = $"Giftcard [{giftcardId}]";
                await UpdatePrivateMessagesBySubjectAsync(privateMessages, giftcardSubject);
            }

            if (awardId.HasValue)
            {
                var awardSubject = $"Award [{awardId}]";
                await UpdatePrivateMessagesBySubjectAsync(privateMessages, awardSubject);
            }

            if (discountId.HasValue)
            {
                var discountSubject = $"Voucher [{discountId}]";
                await UpdatePrivateMessagesBySubjectAsync(privateMessages, discountSubject);
            }
        }

        private async Task UpdatePrivateMessagesBySubjectAsync(IList<PrivateMessage> privateMessages, string subject)
        {
            //do not inject IForumService via constructor because it'll cause circular references
            var _forumService = EngineContext.Current.Resolve<IForumService>();

            // Filter private messages by subject
            var matchingMessages = privateMessages.Where(pm => pm.Subject.Trim().Equals(subject, StringComparison.InvariantCultureIgnoreCase));
           
            // delete matching messages
            foreach (var message in matchingMessages)
            {
                await _forumService.DeletePrivateMessageAsync(message);
            }
        }
    }
}
