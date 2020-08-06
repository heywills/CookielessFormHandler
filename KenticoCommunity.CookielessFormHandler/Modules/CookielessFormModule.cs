using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.OnlineForms;
using KenticoCommunity.CookielessFormHandler.Interfaces;
using KenticoCommunity.CookielessFormHandler.Modules;
using KenticoCommunity.CookielessFormHandler.Services;

[assembly: RegisterModule(typeof(CookielessFormModule))]

namespace KenticoCommunity.CookielessFormHandler.Modules
{
    /// <summary>
    /// Custom Kentico Module to setup the event listeners and configurations
    /// for the cookieless form solution.  The module listens to the
    /// BizFormItemEvents.Insert.After event, so that it can ensure
    /// new BizFormItem's provide a form submission activity log.
    /// </summary>
    public class CookielessFormModule: Module
    {
        ICookielessFormService _cookielessFormService;

        public CookielessFormModule() :base(nameof(CookielessFormModule))
        { }


        /// <summary>
        /// Register the CookielessFormService using Kentico's built-in
        /// DI framework, so that this solution will run in any Kentico
        /// project, MVC or CMS project.
        /// </summary>
        protected override void OnPreInit()
        {
            base.OnPreInit();
            Service.Use<ICookielessFormService, CookielessFormService>();
            
        }

        /// <summary>
        /// Setup the event listener for BizFormItemEvents, and create the
        /// CookielessFormService using Kentico's built-in DI framework, so that
        /// Kentico will supply all the required dependencies.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();
            _cookielessFormService = Service.Resolve<ICookielessFormService>();
            BizFormItemEvents.Insert.After += BizFormItemInsertAfter;
        }

        /// <summary>
        /// Invoked when a new BizFormItem is created.  Call the CookielessFormService
        /// to ensure a form submission activity is logged.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void BizFormItemInsertAfter(object sender, BizFormItemEventArgs args)
        {
            _cookielessFormService.EnsureFormSubmitActivityIsLogged(args.Item);
        }
    }
}
