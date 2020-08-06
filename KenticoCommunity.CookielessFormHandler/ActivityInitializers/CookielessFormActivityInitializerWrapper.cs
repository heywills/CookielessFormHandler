using CMS.Activities;
using KenticoCommunity.CookielessFormHandler.Helpers;

namespace KenticoCommunity.CookielessFormHandler.ActivityInitializers
{
    /// <summary>
    /// Wrap an activity initializer and add the ActivityContactID and ActivitySiteID
    /// to the activity created by the original initializer.
    /// </summary>
    /// <seealso cref="http://devnet.kentico.com/docs/12_0/api/html/T_CMS_Activities_ActivityInitializerWrapperBase.htm"/>
    public class CookielessFormActivityInitializerWrapper: ActivityInitializerWrapperBase
    {
        private readonly int _contactId;
        private readonly int _siteId;

        /// <summary>
        /// Create the initializer, with the original initializer and the
        /// ContactID and SiteID values.
        /// </summary>
        /// <param name="originalInitializer"></param>
        /// <param name="contactId"></param>
        /// <param name="siteId"></param>
        public CookielessFormActivityInitializerWrapper(
            IActivityInitializer originalInitializer, 
            int contactId, 
            int siteId)
            : base(originalInitializer)
        {
            Guard.ArgumentNotNull(originalInitializer, nameof(originalInitializer));
            Guard.ArgumentGreaterThanZero(contactId, nameof(contactId));
            Guard.ArgumentGreaterThanZero(siteId, nameof(siteId));
            _contactId = contactId;
            _siteId = siteId;
        }

        /// <summary>
        /// Call the original initializer and then set the ActivityContactID
        /// and ActivitySiteID properties.
        /// </summary>
        /// <param name="activity"></param>
        public override void Initialize(IActivityInfo activity)
        {
            Guard.ArgumentNotNull(activity, nameof(activity));
            OriginalInitializer.Initialize(activity);
            activity.ActivityContactID = _contactId;
            activity.ActivitySiteID = _siteId;
        }
    }
}
