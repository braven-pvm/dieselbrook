using Annique.Plugins.Nop.Customization.Domain;
using Annique.Plugins.Nop.Customization.Models.UserProfile;
using Nop.Core.Domain.Common;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Factories.UserProfile
{
    public interface IUserProfileAdditionalInfoModelFactory
    {
        /// <summary>
        /// Prepare UserProfileAdditionalInfoModel
        /// </summary>
        /// <param name="customerId">Customer Indetifier</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the UserProfileAdditionalInfoModel
        /// </returns>
        Task<UserProfileAdditionalInfoModel> PrepareUserProfileAdditionalInfoModelAsync(int customerId);

        /// <summary>
        /// Prepare UserProfileAdditionalInfo fields
        /// </summary>
        /// <param name="model">UserProfileAdditionalInfo Model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the UserProfileAdditionalInfo
        /// </returns>
        UserProfileAdditionalInfo PrepareUserProfileAdditionalInfoFields(UserProfileAdditionalInfoModel model);

        /// <summary>
        /// Prepare Old address copy 
        /// </summary>
        /// <param name="oldAddress">Old address</param>
        /// <returns>
        /// The task result contains the old Address copy
        /// </returns>
        Address PrepareOldAddressCopy(Address oldAddress);
    }
}
