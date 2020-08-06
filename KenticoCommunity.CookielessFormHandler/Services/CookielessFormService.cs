using CMS.Activities;
using CMS.Base;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.Newsletters;
using CMS.OnlineForms;
using KenticoCommunity.CookielessFormHandler.ActivityInitializers;
using KenticoCommunity.CookielessFormHandler.Helpers;
using KenticoCommunity.CookielessFormHandler.Interfaces;
using System;
using System.Linq;

namespace KenticoCommunity.CookielessFormHandler.Services
{
    /// <summary>
    /// Provide functionality to ensure a FormSubmission activity is logged
    /// when a form is submitted, even if the user has not consented to
    /// visitor level cookies.
    /// </summary>
    public class CookielessFormService: ICookielessFormService
    {
        private const string CmsEnableOnlineMarketingSettingsKey = "CMSEnableOnlineMarketing";
        private const string ContactEmailFieldName = "ContactEmail";

        private readonly ISiteService _siteService;
        private readonly IActivityLogService _activityLogService;
        private readonly ICurrentCookieLevelProvider _currentCookieLevelProvider;
        private readonly ISettingsService _settingsService;
        private readonly IEventLogService _eventLogService;

        public CookielessFormService(ISiteService siteService, 
            IActivityLogService activityLogService,
            ICurrentCookieLevelProvider currentCookieLevelProvider, 
            ISettingsService settingsService,
            IEventLogService eventLogService)
        {
            _siteService = siteService;
            _activityLogService = activityLogService;
            _currentCookieLevelProvider = currentCookieLevelProvider;
            _settingsService = settingsService;
            _eventLogService = eventLogService;
        }


        /// <summary>
        /// With the provided BizFormItem, check if the related BizForm is
        /// configured to update contact data and log activities.
        /// If the submitting user has consented to cookies, let Kentico
        /// provide its normal processing, and simply abort this custom code.
        /// If the user has not consented, get the related BizFormInfo and
        /// DataClassInfo. If the form has an email field that is mapped to
        /// ContactEmail, get a related contact. Create one if necessary.
        /// Update the mapped contact fields.  If the BizForm is configured to
        /// log activities, log the form submission activity.
        /// </summary>
        /// <param name="bizFormItem"></param>
        public void EnsureFormSubmitActivityIsLogged(BizFormItem bizFormItem)
        {
            Guard.ArgumentNotNull(bizFormItem, nameof(bizFormItem));

            try
            {
                if (!IsOnlineMarketingEnabled())
                {
                    return;
                }

                if (!InLiveSiteContext())
                {
                    return;
                }

                if (IsCookieLevelAtLeastVisitor())
                {
                    return;
                }
                var dataClass = DataClassInfoProvider.GetDataClassInfo(bizFormItem.ClassName);

                // Is the form field mapped to contact email and provide a valid email value?
                var email = GetEmailFromOnlineForm(bizFormItem, dataClass);
                if (string.IsNullOrWhiteSpace(email) || (!ValidationHelper.IsEmail(email)))
                {
                    return;
                }

                // Get a contact for the email, whether new or matching
                var contact = GetContactFormEmail(email);

                // Update contact with form data
                ContactInfoProvider.UpdateContactFromExternalData(bizFormItem, dataClass.ClassContactOverwriteEnabled, contact);

                // Log form submission activity
                if (bizFormItem.BizFormInfo.FormLogActivity)
                {
                    FormSubmitActivityInitializer activityInitializer = 
                        SystemContext.IsCMSRunningAsMainApplication ? 
                            new FormSubmitActivityInitializer(bizFormItem, DocumentContext.CurrentDocument) : 
                            new FormSubmitActivityInitializer(bizFormItem);

                    var cookielessFormActivityInitializerWrapper = 
                        new CookielessFormActivityInitializerWrapper(activityInitializer, 
                            contact.ContactID, 
                            _siteService.CurrentSite.SiteID);

                    _activityLogService.LogWithoutModifiersAndFilters(cookielessFormActivityInitializerWrapper);
                    _eventLogService.LogEvent(EventType.INFORMATION,
                        nameof(CookielessFormService),
                        nameof(EnsureFormSubmitActivityIsLogged),
                        $"Bizformsubmit activity logged for email, '{email}'.");
                }
            }
            catch(Exception exception)
            {
                _eventLogService.LogException(nameof(CookielessFormService), nameof(EnsureFormSubmitActivityIsLogged), exception);
            }
        }

        /// <summary>
        /// Return true if the code is running in the context of the live site.
        /// </summary>
        /// <returns></returns>
        public bool InLiveSiteContext()
        {
            return (_siteService.IsLiveSite) && (CMSHttpContext.Current != null);
        }

        /// <summary>
        /// Return true if the current user has consented to at least Visitor (200)
        /// level cookies. If true, this custom solution isn't needed.
        /// </summary>
        /// <returns></returns>
        public bool IsCookieLevelAtLeastVisitor()
        {
            return _currentCookieLevelProvider.GetCurrentCookieLevel() >= CookieLevel.Visitor;
        }

        /// <summary>
        /// Check the form's contact mapping configuration. If a field is mapped to
        /// the ContactEmail field, get the field value and return it.
        /// </summary>
        /// <param name="bizFormItem"></param>
        /// <param name="dataClass"></param>
        /// <returns></returns>
        public string GetEmailFromOnlineForm(BizFormItem bizFormItem, DataClassInfo dataClass)
        {
            if((bizFormItem == null) || (dataClass == null))
            {
                return string.Empty;
            }
            FormFieldInfo emailFormField = new FormInfo(dataClass.ClassContactMapping)
                .ItemsList.OfType<FormFieldInfo>()
                .FirstOrDefault(item => item.Name == ContactEmailFieldName);
            if (emailFormField != null)
            {
                string email = bizFormItem.GetStringValue(emailFormField.MappedToField, string.Empty).Trim();
                return email;
            }
            return string.Empty;
        }

        /// <summary>
        /// Get a contact for the provided email address, by returning an existing one
        /// if available, or creating a new one if needed.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        /// <remarks>GetContactForSubscribing is perfect. It checks if there's a current contact,
        /// but only uses it if the ContactEmail matches or is blank. If the current contact
        /// isn't appropriate, it looks up a contact by email address. If it still doesn't find one
        /// it creates an anonymous cookie. In the end, we'll have a contact with the ContactEmail set.</remarks>
        public ContactInfo GetContactFormEmail(string email)
        {
            var contactProvider = Service.Resolve<IContactProvider>();
            var contactInfo = contactProvider.GetContactForSubscribing(email);
            return contactInfo;
        }

        /// <summary>
        /// Return true if Online Marketing features are enabled for the site.
        /// </summary>
        /// <returns></returns>
        public bool IsOnlineMarketingEnabled()
        {
            return _settingsService[_siteService.CurrentSite?.SiteName + "." + CmsEnableOnlineMarketingSettingsKey].ToBoolean(defaultValue: false);
        }


    }
}
