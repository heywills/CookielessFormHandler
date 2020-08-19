# Cookieless Form Handler Module for Kentico Xperience
This module enables using Kentico Xperience online forms without requiring site visitors to consent to cookies.  
## Background
Compliance regulations like Europe’s GDPR and California’s CCPA require web sites to request a visitor’s consent before using cookies. However, Xperience requires cookies for its contact management and activity tracking to work. Therefore, if a Kentico site is set up to use a default cookie level less than 200 (i.e. Visitor), it will not track unique visitors using contact management and activity tracking until they provide consent. 

However, the technique for using Kentico’s email marketing features to respond to online form submissions requires triggering marketing automation with a form submit activity (see [How to Customize Kentico to Provide Form Response Emails with Rich Marketing Content](https://bluemodus.com/articles/how-to-customize-kentico-to-provide-form-response-emails-with-rich-marketing-content)). In short, if a site visitor submits a form with their email address, but does not consent to cookies, a contact will not be created, a form submit activity will not be logged, and marketing automation will not be triggered. Without cookies, marketing automation cannot be relied upon to send emails.

## Solution
This custom module handles the `BizFormItemEvents.Insert.After` global event. When the event fires, it checks to see if the visitor's cookie level is too low for out-of-the-box activity tracking. If it is, the module checks the form configuration to see if it is mapped to contact fields and if it is configured to log activities. If so, this module ensures there is a contact for the provided email address and logs a form submission activity.

With this solution in place, Kentico reliably runs marketing automation processes, triggered by form submission activities, even if the visitor has not consented to cookies.  Customers can use Kentico’s marketing automation feature to send emails created in the marketing email module. 

## Compatibility

* .NET 4.6.1 or higher
* Kentico Xperience 12.0.39 or higher
* Compatible with both Portal Engine and MVC development models

## Personal data and consents
Tracking consent is different than consent to process personal data. Therefore, you can never assume that visitors have consented to processing personal data when they click **allow** in a cookie consent prompt. Additionally, Kentico allows visitors to submit online forms even if they didn't consent to cookies. This is appropriate, because privacy laws typically require separate consents for different purposes. Therefore, it's important to request consent on your online forms. In MVC you can do this by adding the **Consent agreement** form component, and in Portal Engine the **Consent agreement** form control.

The **Cookieless Form Handler** does not add any PII to the system that is not already stored in the Kentico form, so it does not add any compliance challenges. However, it supports copying form data fields to contact fields, and it creates a form submission activity. This information is already in the form data record, including a record of the visitor submitting the form. Additionally, this is what Kentico does automatically if a user has consented to tracking cookies.

Privacy laws do not care whether the PII is stored in a Kentico form data record or in a Kentico contact record, but they do care that the system receives the appropriate consent to use the data and that there is a way to erase the personal data. It is your responsibility to comply with the appropriate regulations whether the data is stored in a form data record or in a contact record.

### Disclaimer
This readme is provided for informational purposes only and should not be relied upon as legal advice.   Instead, use the information to understand the flow of PII in your system and take appropriate steps to remain compliant with privacy regulations.

### For more information:

[GDPR – Building Consents and Privacy Notices](https://xperience.io/discover/blog/2018-03/gdpr-building-consents-and-privacy-notices)

[Personal data in Kentico](https://docs.kentico.com/k12sp/configuring-kentico/data-protection/gdpr-compliance/personal-data-in-kentico)



## Installation
If you are using the Kentico Xperience MVC development model, install the NuGet package, `KenticoCommunity.CookielessFormHandler` to your MVC project.

If using the Portal Engine model, install the NuGet package to your CMS project.

## Usage
The only step required to use this module is to add it to your project. You can verify its working, by creating an online form, with an email field mapped to the ContactEmail property, and with activity logging turned on.  Visit the site as a user that only allows essential cookies are less (you can set the default cookie level in site settings).  Fill in the form's email field and submit.  You should see a contact for the submitted email address and a form submission activity.  Additionally, the event code 'EnsureFormSubmitActivityIsLogged' will be logged to Kentico Xperience's event log.

## License

This project uses a standard MIT license which can be found [here](https://github.com/heywills/CookielessFormHandler/blob/master/license).

## Contribution

Contributions are welcome. Feel free to submit pull requests to the repo.

## Support

Please report bugs as issues in this GitHub repo.  We'll respond as soon as possible.



