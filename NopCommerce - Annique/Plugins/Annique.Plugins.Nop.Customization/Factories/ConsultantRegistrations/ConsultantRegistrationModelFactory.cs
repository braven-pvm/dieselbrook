using Annique.Plugins.Nop.Customization.Domain.ConsultantRegistrations;
using Annique.Plugins.Nop.Customization.Models.ConsultantRegistrations.Admin;
using Annique.Plugins.Nop.Customization.Models.ConsultantRegistrations.Public;
using Annique.Plugins.Nop.Customization.Services.ConsultantRegistrations;
using Annique.Plugins.Nop.Customization.Services.UserProfile;
using Nop.Core;
using Nop.Core.Domain.Security;
using Nop.Services.Helpers;
using Nop.Web.Framework.Models.Extensions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Annique.Plugins.Nop.Customization.Factories.ConsultantRegistrations
{
    public class ConsultantRegistrationModelFactory : IConsultantRegistrationModelFactory
    {
        #region Fields

        private readonly IConsultantNewRegistrationService _consultantNewRegistrationService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly IUserProfileAdditionalInfoService _userProfileAdditionalInfoService;
        private readonly IStoreContext _storeContext;
        private readonly CaptchaSettings _captchaSettings;

        #endregion

        #region Ctor

        public ConsultantRegistrationModelFactory(IConsultantNewRegistrationService consultantNewRegistrationService,
            IDateTimeHelper dateTimeHelper,
            IUserProfileAdditionalInfoService userProfileAdditionalInfoService,
            IStoreContext storeContext,
            CaptchaSettings captchaSettings)
        {
            _consultantNewRegistrationService = consultantNewRegistrationService;
            _dateTimeHelper = dateTimeHelper;
            _userProfileAdditionalInfoService = userProfileAdditionalInfoService;
            _storeContext = storeContext;
            _captchaSettings = captchaSettings;
        }

        #endregion

        #region Admin methods

        /// <summary>
        /// Prepare registration search model
        /// </summary>
        /// <param name="searchModel">ConsultantRegistration search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ConsultantRegistration search model
        /// </returns>
        public async Task<ConsultantRegistrationSearchModel> PrepareConsultantRegistrationSearchModelAsync(ConsultantRegistrationSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //prepare page parameters
            searchModel.SetGridPageSize();

            return await Task.FromResult(searchModel);
        }

        /// <summary>
        /// Prepare paged Consultant Registration list model
        /// </summary>
        /// <param name="searchModel">Consultant Registration search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the Consultant Registration list model
        /// </returns>
        public async Task<ConsultantRegistrationListModel> PrepareConsultantRegistrationListModelAsync(ConsultantRegistrationSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var registrationList = await _consultantNewRegistrationService.GetAllRegistrationsAsync(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = await new ConsultantRegistrationListModel().PrepareToGridAsync(searchModel, registrationList, () =>
            {
                //fill in model values from the entity
                return registrationList.SelectAwait(async registrationItem =>
                {
                    //fill in model values from the entity
                    var overviewModel = new ConsultantRegistrationOverviewModel()
                    {
                        Id = registrationItem.Id,
                        Fullname = registrationItem.cFname + " " + registrationItem.cLname,
                        Sponsor = registrationItem.csponsor,
                        Status = registrationItem.Status
                    };

                    return overviewModel;
                });
            });

            return model;
        }

        /// <summary>
        /// Prepare Consultant Registration model
        /// </summary>
        /// <param name="model">Log model</param>
        /// <param name="newRegistrations">new Consultant Registration</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ConsultantRegistration overview model
        /// </returns>
        public async Task<ConsultantRegistrationOverviewModel> PrepareConsultantRegistrationOverviewModelAsync(ConsultantRegistrationOverviewModel model, NewRegistrations newRegistrations)
        {
            if (newRegistrations != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = new ConsultantRegistrationOverviewModel()
                    {
                        Id = newRegistrations.Id,
                        Sponsor = newRegistrations.csponsor,
                        FirstName = newRegistrations.cFname,
                        LastName = newRegistrations.cLname,
                        Email = newRegistrations.cEmail,
                        Cell = newRegistrations.cPhone1,
                        Whatsapp = newRegistrations.cPhone2,
                        Postcode = newRegistrations.cZip,
                        Country = newRegistrations.ccountry,
                        SelectedLanguage = newRegistrations.cLanguage,
                        SelectedCallTime = newRegistrations.besttocall,
                        IPAddress = newRegistrations.IPAddress,
                        Browser = newRegistrations.Browser,
                        Status = newRegistrations.Status
                    };

                    model.CreatedOnUtc = await _dateTimeHelper.ConvertToUserTimeAsync(newRegistrations.CreatedOnUtc, DateTimeKind.Utc);
                }
            }
            return model;
        }

        #endregion

        #region Public methods
        /// <summary>
        /// Prepare Consultant Registration model
        /// </summary>
        /// <param name="model">Log model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the ConsultantRegistration model
        /// </returns>
        public async Task<ConsultantRegistrationModel> PrepareConsultantRegistrationModelAsync(ConsultantRegistrationModel model = null)
        {
            model ??= new ConsultantRegistrationModel();

            var store = await _storeContext.GetCurrentStoreAsync();

            model.Languages = await _userProfileAdditionalInfoService.GetSelectListAsync(AnniqueCustomizationDefaults.LanguageLookup, store.Id);
            model.CallTimes = await _userProfileAdditionalInfoService.GetSelectListAsync("CALLTIME", store.Id);
            model.DisplayCaptcha = _captchaSettings.Enabled;

            var settings = await _consultantNewRegistrationService.GetPageSettings();
            model.Css = settings.CustomCSS;
            model.Js = settings.CustomJS;

            if (settings.TopSectionPublished)
                model.TopSection = settings.TopSectionBody;

            if (settings.LeftSectionPublished)
                model.LeftSection = settings.LeftSectionBody;

            if (settings.BottomSectionPublished)
                model.BottomSection = settings.BottomSectionBody;

            return model;
        }

        #endregion

    }
}
