using CMS.ContactManagement;
using CMS.DataEngine;
using CMS.OnlineForms;

namespace KenticoCommunity.CookielessFormHandler.Interfaces
{
    /// <summary>
    /// Provide functionality to ensure a FormSubmission activity is logged
    /// when a form is submitted, even if the user has not consented to
    /// visitor level cookies.
    /// </summary>
    public interface ICookielessFormService
    {
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
        void EnsureFormSubmitActivityIsLogged(BizFormItem bizFormItem);

        /// <summary>
        /// Return true if the code is running in the context of the live site.
        /// </summary>
        /// <returns></returns>
        bool InLiveSiteContext();

        /// <summary>
        /// Return true if the current user has consented to at least Visitor (200)
        /// level cookies. If true, this custom solution isn't needed.
        /// </summary>
        /// <returns></returns>
        bool IsCookieLevelAtLeastVisitor();

        /// <summary>
        /// Check the form's contact mapping configuration. If a field is mapped to
        /// the ContactEmail field, get the field value and return it.
        /// </summary>
        /// <param name="bizFormItem"></param>
        /// <param name="dataClass"></param>
        /// <returns></returns>
        string GetEmailFromOnlineForm(BizFormItem bizFormItem, DataClassInfo dataClass);

        /// <summary>
        /// Get a contact for the provided email address, by returning an existing one
        /// if available, or creating a new one if needed.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        ContactInfo GetContactFormEmail(string email);

        /// <summary>
        /// Return true if Online Marketing features are enabled for the site.
        /// </summary>
        /// <returns></returns>
        bool IsOnlineMarketingEnabled();

    }
}
