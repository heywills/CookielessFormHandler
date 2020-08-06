using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using CMS.Activities;
using CMS.Base;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Membership;
using CMS.Newsletters;
using CMS.OnlineForms;
using CMS.SiteProvider;
using CMS.Tests;
using KenticoCommunity.CookielessFormHandler.Interfaces;
using KenticoCommunity.CookielessFormHandler.Services;
using KenticoCommunity.CookielessFormHandler.Tests.Fakes;
using Moq;
using NUnit.Framework;

namespace KenticoCommunity.CookielessFormHandler.Tests.Services
{
    [TestFixture]
    public class CookielessFormServiceTests:UnitTests
    {
        private readonly Mock<ISiteService> _mockSiteServiceThatReturnsIsLiveSiteTrue = new Mock<ISiteService>();
        private readonly Mock<ISiteService> _mockSiteServiceThatReturnsIsLiveSiteFalse = new Mock<ISiteService>();
        private readonly Mock<IActivityLogService> _mockActivityLogService = new Mock<IActivityLogService>();

        private readonly Mock<ICurrentCookieLevelProvider> _mockCurrentCookieLevelProviderThatReturnsVisitor = 
            new Mock<ICurrentCookieLevelProvider>();

        private readonly Mock<ICurrentCookieLevelProvider> _mockCurrentCookieLevelProviderThatReturnsEssential = 
            new Mock<ICurrentCookieLevelProvider>();

        private readonly Mock<ISettingsService> _mockSettingsServiceThatReturnsMarketingEnabledTrue = 
            new Mock<ISettingsService>();

        private readonly Mock<ISettingsService> _mockSettingsServiceThatReturnsMarketingEnabledFalse = 
            new Mock<ISettingsService>();

        private readonly Mock<IEventLogService> _mockEventLogService = new Mock<IEventLogService>();
        private SiteInfo _fakeSiteInfo;
        private DataClassInfo _dataClassInfoWithContactFieldMapping;
        private DataClassInfo _dataClassInfoWithoutContactFieldMapping;
        private DataClassInfo _dataClassInfoWithFormLogActivityFalse;
        private BizFormItem _bizFormItemWithContactFieldMapping;
        private BizFormItem _bizFormItemWithoutContactFieldMapping;
        private BizFormItem _bizFormItemWithBlankEmail;
        private BizFormItem _bizFormItemWithFormLogActivityFalse;
        private ICookielessFormService _cookielessFormServiceThatWillLogActivity;


        [SetUp]
        public void SetupTests()
        {
            HttpContext.Current = new HttpContext(
                new HttpRequest("", "http://tempuri.org", ""),
                new HttpResponse(new StringWriter())
                );

            Fake<SiteInfo>();
            _fakeSiteInfo = new SiteInfo()
            {
                SiteName = "FakeSite",
                SiteID = 1
            };
            Fake<SiteInfo, SiteInfoProvider>().WithData(
                _fakeSiteInfo
            );


            _cookielessFormServiceThatWillLogActivity = new CookielessFormService(
                        _mockSiteServiceThatReturnsIsLiveSiteTrue.Object,
                        _mockActivityLogService.Object,
                        _mockCurrentCookieLevelProviderThatReturnsEssential.Object,
                        _mockSettingsServiceThatReturnsMarketingEnabledTrue.Object,
                        _mockEventLogService.Object);

            _mockCurrentCookieLevelProviderThatReturnsVisitor.Setup(
                x => x.GetCurrentCookieLevel())
                .Returns(CookieLevel.Visitor);

            _mockCurrentCookieLevelProviderThatReturnsEssential.Setup(
                x => x.GetCurrentCookieLevel())
                .Returns(CookieLevel.Essential);

            _mockSiteServiceThatReturnsIsLiveSiteTrue.SetupGet(x => x.IsLiveSite).Returns(true);
            _mockSiteServiceThatReturnsIsLiveSiteTrue.SetupGet(x => x.CurrentSite).Returns(_fakeSiteInfo);
            _mockSiteServiceThatReturnsIsLiveSiteFalse.SetupGet(x => x.IsLiveSite).Returns(false);
            _mockSiteServiceThatReturnsIsLiveSiteFalse.SetupGet(x => x.CurrentSite).Returns(_fakeSiteInfo);

            _mockSettingsServiceThatReturnsMarketingEnabledTrue.SetupGet(
                x => x["FakeSite.CMSEnableOnlineMarketing"])
                .Returns("True");

            _mockSettingsServiceThatReturnsMarketingEnabledFalse.SetupGet(
                x => x["FakeSite.CMSEnableOnlineMarketing"])
                .Returns("False");



            Fake<DataClassInfo>();
            _dataClassInfoWithoutContactFieldMapping = DataClassInfo.New("BizForm.FreeDogGrooming");
            _dataClassInfoWithoutContactFieldMapping.ClassID = 5576;
            _dataClassInfoWithoutContactFieldMapping.ClassDisplayName = "Free Dog Grooming";
            _dataClassInfoWithoutContactFieldMapping.ClassName = "BizForm.FreeDogGrooming";
            _dataClassInfoWithoutContactFieldMapping.ClassContactMapping = null;
            _dataClassInfoWithoutContactFieldMapping.ClassUsesVersioning = false;
            _dataClassInfoWithoutContactFieldMapping.ClassIsDocumentType = false;
            _dataClassInfoWithoutContactFieldMapping.ClassIsCoupledClass = true;
            _dataClassInfoWithoutContactFieldMapping.ClassGUID = new Guid("73A40AD9-9BF4-4D83-BFF4-8E06CAF8B747");
            _dataClassInfoWithoutContactFieldMapping.ClassIsCustomTable = false;
            _dataClassInfoWithoutContactFieldMapping.ClassIsForm = true;



            _dataClassInfoWithContactFieldMapping = DataClassInfo.New("BizForm.LoyaltyProgramRequest");
            _dataClassInfoWithContactFieldMapping.ClassID = 5594;
            _dataClassInfoWithContactFieldMapping.ClassDisplayName = "Loyalty Program Request";
            _dataClassInfoWithContactFieldMapping.ClassName = "BizForm.LoyaltyProgramRequest";
            _dataClassInfoWithContactFieldMapping.ClassIsForm = true;
            _dataClassInfoWithContactFieldMapping.ClassContactMapping =
                @"<form>
	                <field column=""ContactEmail"" mappedtofield=""Email""/>
	                <field column=""ContactFirstName"" mappedtofield=""FirstName""/>
	                <field column=""ContactLastName"" mappedtofield=""LastName""/>
                </form>";
            _dataClassInfoWithContactFieldMapping.ClassUsesVersioning = false;
            _dataClassInfoWithContactFieldMapping.ClassIsDocumentType = false;
            _dataClassInfoWithContactFieldMapping.ClassIsCoupledClass = true;
            _dataClassInfoWithContactFieldMapping.ClassGUID = new Guid("BF48A52E-A91D-4516-BF31-111472DA5C18");
            _dataClassInfoWithContactFieldMapping.ClassIsCustomTable = false;
            _dataClassInfoWithContactFieldMapping.ClassIsForm = true;
            _dataClassInfoWithContactFieldMapping.ClassContactOverwriteEnabled = true;

            _dataClassInfoWithFormLogActivityFalse = DataClassInfo.New("BizForm.LoyaltyProgramRequestFormLogActivityFalse");
            _dataClassInfoWithFormLogActivityFalse.ClassID = 5517;
            _dataClassInfoWithFormLogActivityFalse.ClassDisplayName = "Loyalty Program Request FormLogActivity False";
            _dataClassInfoWithFormLogActivityFalse.ClassName = "BizForm.LoyaltyProgramRequestFormLogActivityFalse";
            _dataClassInfoWithFormLogActivityFalse.ClassIsForm = true;
            _dataClassInfoWithFormLogActivityFalse.ClassContactMapping =
                @"<form>
	                <field column=""ContactEmail"" mappedtofield=""Email""/>
	                <field column=""ContactFirstName"" mappedtofield=""FirstName""/>
	                <field column=""ContactLastName"" mappedtofield=""LastName""/>
                </form>";
            _dataClassInfoWithFormLogActivityFalse.ClassUsesVersioning = false;
            _dataClassInfoWithFormLogActivityFalse.ClassIsDocumentType = false;
            _dataClassInfoWithFormLogActivityFalse.ClassIsCoupledClass = true;
            _dataClassInfoWithFormLogActivityFalse.ClassGUID = new Guid("BF48A52E-A91D-4516-BF31-111472DA5C02");
            _dataClassInfoWithFormLogActivityFalse.ClassIsCustomTable = false;
            _dataClassInfoWithFormLogActivityFalse.ClassIsForm = true;
            _dataClassInfoWithFormLogActivityFalse.ClassContactOverwriteEnabled = true;
            Fake<DataClassInfo, DataClassInfoProvider>().WithData(
                    _dataClassInfoWithContactFieldMapping,
                    _dataClassInfoWithoutContactFieldMapping,
                    _dataClassInfoWithFormLogActivityFalse
                );

            Fake<BizFormInfo, BizFormInfoProvider>().WithData(
                    new BizFormInfo()
                    {
                        FormID = 1,
                        FormGUID = new Guid("BF48A52E-A91D-4516-BF31-111472DA5C32"),
                        FormLogActivity = true,
                        FormClassID = 5594,
                        FormSiteID = 1
                    },
                    new BizFormInfo()
                    {
                        FormID = 2,
                        FormGUID = new Guid("BF48A52E-A91D-4516-BF31-111472DA5C33"),
                        FormLogActivity = true,
                        FormClassID = 5576,
                        FormSiteID = 1
                    },
                    new BizFormInfo()
                    {
                        FormID = 3,
                        FormGUID = new Guid("BF48A52E-A91D-4516-BF31-111472DA5C34"),
                        FormLogActivity = false,
                        FormClassID = 5517,
                        FormSiteID = 1
                    }
                );


            Fake<ContactInfo, ContactInfoProvider>().WithData(
                    new ContactInfo()
                    {
                        ContactEmail = "tiriansdoor@mailinator.com",
                        ContactFirstName = "Mike",
                        ContactLastName = "Wills"
                    }
                );

            Fake<UserInfo, UserInfoProvider>().WithData(
                    new UserInfo()
                    {
                        UserName = "public",
                        FullName = "Public Anonymous User"
                    }
                );

            Fake<SubscriberInfo, SubscriberInfoProvider>().WithData(
                );

            Fake<LicenseKeyInfo, LicenseKeyInfoProvider>().WithData(
                );


            _bizFormItemWithoutContactFieldMapping = new FakeBizFormItem(
                "BizForm.FreeDogGrooming",
                new Dictionary<string, object>()
                {
                    {"AppointmentDate", "6/30/2020" },
                    {"PetName", "Alan Bedillion Trehern"}
                });

            _bizFormItemWithContactFieldMapping = new FakeBizFormItem(
                "BizForm.LoyaltyProgramRequest",
                new Dictionary<string, object>()
                {
                    {"Email", "tiriansdoor@mailinator.com" },
                    {"FirstName", "Mike-Updated"},
                    {"LastName", "Wills"}
                });

            _bizFormItemWithBlankEmail = new FakeBizFormItem(
                "BizForm.LoyaltyProgramRequest",
                new Dictionary<string, object>()
                {
                    {"Email", "" },
                    {"FirstName", "Mike"},
                    {"LastName", "Wills"}
                });

            _bizFormItemWithFormLogActivityFalse = new FakeBizFormItem(
                "BizForm.LoyaltyProgramRequestFormLogActivityFalse",
                new Dictionary<string, object>()
                {
                    {"Email", "tiriansdoor@mailinator.com" },
                    {"FirstName", "Mike"},
                    {"LastName", "Wills"}
                });
        }

        [Test]
        public void InLiveSiteContext_Returns_True_If_IsLiveSite_And_HttpContext()
        {
            var result = _cookielessFormServiceThatWillLogActivity.InLiveSiteContext();
            Assert.IsTrue(result);
        }

        [Test]
        public void InLiveSiteContext_Returns_False_If_Not_IsLiveSite()
        {
            var cookielessFormService = new CookielessFormService(
                        _mockSiteServiceThatReturnsIsLiveSiteFalse.Object,
                        _mockActivityLogService.Object,
                        _mockCurrentCookieLevelProviderThatReturnsEssential.Object,
                        _mockSettingsServiceThatReturnsMarketingEnabledTrue.Object,
                        _mockEventLogService.Object);
            var result = cookielessFormService.InLiveSiteContext();
            Assert.IsFalse(result);
        }

        [Test]
        [NonParallelizable]
        public void InLiveSiteContext_Returns_False_If_Null_HttpContext()
        {
            var httpContext = HttpContext.Current;
            HttpContext.Current = null;
            var result = _cookielessFormServiceThatWillLogActivity.InLiveSiteContext();
            Assert.IsFalse(result);
            HttpContext.Current = httpContext;
        }



        [Test]
        public void IsCookieLevelAtLeastVisitor_Returns_False_If_CookieLevel_IsLessThan_Visitor()
        {
            var result = _cookielessFormServiceThatWillLogActivity.IsCookieLevelAtLeastVisitor();
            Assert.IsFalse(result);
        }

        [Test]
        public void IsCookieLevelAtLeastVisitor_Returns_True_If_CookieLevel_IsGreaterThanOrEqualTo_Visitor()
        {
            var cookielessFormService = new CookielessFormService(
                        _mockSiteServiceThatReturnsIsLiveSiteTrue.Object,
                        _mockActivityLogService.Object,
                        _mockCurrentCookieLevelProviderThatReturnsVisitor.Object,
                        _mockSettingsServiceThatReturnsMarketingEnabledTrue.Object,
                        _mockEventLogService.Object);
            var result = cookielessFormService.IsCookieLevelAtLeastVisitor();
            Assert.IsTrue(result);
        }

        [Test]
        public void GetEmailFromOnlineForm_Returns_Value_From_Form_Field_Mapped_To_ContactEmail()
        {
            var result = _cookielessFormServiceThatWillLogActivity
                .GetEmailFromOnlineForm(_bizFormItemWithContactFieldMapping,_dataClassInfoWithContactFieldMapping);
            Assert.AreEqual("tiriansdoor@mailinator.com", result);
        }

        [Test]
        public void GetEmailFromOnlineForm_Returns_Empty_If_Form_Field_Is_Not_Mapped()
        {
            var result = _cookielessFormServiceThatWillLogActivity
                .GetEmailFromOnlineForm(_bizFormItemWithoutContactFieldMapping, _dataClassInfoWithoutContactFieldMapping);
            Assert.AreEqual("", result);
        }

        [Test]
        public void GetEmailFromOnlineForm_Returns_Empty_If_Form_Field_Is_Empty()
        {
            var result = _cookielessFormServiceThatWillLogActivity
                .GetEmailFromOnlineForm(_bizFormItemWithBlankEmail, _dataClassInfoWithContactFieldMapping);
            Assert.AreEqual("", result);
        }
        [Test]
        public void GetEmailFromOnlineForm_Returns_Empty_If_BizFormItem_Is_Null()
        {
            var result = _cookielessFormServiceThatWillLogActivity
                .GetEmailFromOnlineForm(null, _dataClassInfoWithContactFieldMapping);
            Assert.AreEqual("", result);
        }
        [Test]
        public void GetEmailFromOnlineForm_Returns_Empty_If_DataClass_Is_Null()
        {
            var result = _cookielessFormServiceThatWillLogActivity.GetEmailFromOnlineForm(_bizFormItemWithBlankEmail, null);
            Assert.AreEqual("", result);
        }
        [Test]
        public void GetContactFromEmail_Returns_New_Contact_If_Email_Is_New()
        {
            var testEmail = "tiriansdoor+brandnew@mailinator.com";
            var contactCount = ContactInfoProvider.GetContacts().Count;
            var contactInfo = _cookielessFormServiceThatWillLogActivity.GetContactFormEmail(testEmail);
            Assert.NotNull(contactInfo);
            Assert.AreEqual(testEmail, contactInfo.ContactEmail);
            Assert.AreEqual(contactCount + 1, ContactInfoProvider.GetContacts().Count);
        }

        [Test]
        public void GetContactFromEmail_Throws_ArgumentException_If_Email_Is_Invalid()
        {
            var testEmail = "tiriansdoor+brandnewmailinator.com";
            Assert.That(() => _cookielessFormServiceThatWillLogActivity
                .GetContactFormEmail(testEmail), Throws.TypeOf<ArgumentException>());
        }

        [Test]
        [NonParallelizable]
        public void GetContactFromEmail_Returns_Existing_Contact_If_Email_Is_Existing()
        {
            var testEmail = "tiriansdoor@mailinator.com";
            var contactCount = ContactInfoProvider.GetContacts().Count;
            var contactInfo = _cookielessFormServiceThatWillLogActivity.GetContactFormEmail(testEmail);
            Assert.NotNull(contactInfo);
            Assert.AreEqual(testEmail, contactInfo.ContactEmail);
            Assert.AreEqual(contactCount, ContactInfoProvider.GetContacts().Count);
        }

        [Test]
        [NonParallelizable]
        public void GetContactFromEmail_Throws_ArgumentException_If_Email_Is_Empty()
        {
            var testEmail = "";

            Assert.That(() => _cookielessFormServiceThatWillLogActivity
                .GetContactFormEmail(testEmail), Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void GetContactFromEmail_Throws_ArgumentNullException_If_Email_Is_Null()
        {
            Assert.That(() => _cookielessFormServiceThatWillLogActivity
                .GetContactFormEmail(null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void IsOnlineMarketingEnabled_Returns_True_If_Setting_Is_True()
        {
            var result = _cookielessFormServiceThatWillLogActivity.IsOnlineMarketingEnabled();
            Assert.IsTrue(result);
        }
        [Test]
        public void IsOnlineMarketingEnabled_Returns_False_If_Setting_Is_False()
        {
            var cookielessFormService = new CookielessFormService(
                        _mockSiteServiceThatReturnsIsLiveSiteTrue.Object,
                        _mockActivityLogService.Object,
                        _mockCurrentCookieLevelProviderThatReturnsEssential.Object,
                        _mockSettingsServiceThatReturnsMarketingEnabledFalse.Object,
                        _mockEventLogService.Object);

            var result = cookielessFormService.IsOnlineMarketingEnabled();
            Assert.IsFalse(result);
        }

        [Test]
        public void EnsureFormSubmitActivityIsLogged_Should_Throw_ArgumentNullException_When_BizFormItem_Is_Null()
        {
            Assert.That(() => _cookielessFormServiceThatWillLogActivity
                .EnsureFormSubmitActivityIsLogged(null), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        [NonParallelizable]
        public void EnsureFormSubmitActivityIsLogged_Calls_Activity_Log_Service()
        {
            _mockActivityLogService.Reset();
            _mockEventLogService.Reset();

            _cookielessFormServiceThatWillLogActivity.EnsureFormSubmitActivityIsLogged(_bizFormItemWithContactFieldMapping);

            _mockActivityLogService.Verify(
                x => x.LogWithoutModifiersAndFilters(It.IsAny<IActivityInitializer>()),
                Times.Once);

            _mockEventLogService.Verify(
                x => x.LogEvent(EventType.INFORMATION,
                    nameof(CookielessFormService),
                    nameof(CookielessFormService.EnsureFormSubmitActivityIsLogged),
                    "Bizformsubmit activity logged for email, 'tiriansdoor@mailinator.com'."
                    ),
                Times.Once);
        }

        [Test]
        [NonParallelizable]
        public void EnsureFormSubmitActivityIsLogged_Does_Not_Call_Activity_Log_Service_When_OnlineMarketing_Is_Not_Enabled()
        {
            _mockActivityLogService.Reset();

            var cookielessFormService = new CookielessFormService(
                        _mockSiteServiceThatReturnsIsLiveSiteTrue.Object,
                        _mockActivityLogService.Object,
                        _mockCurrentCookieLevelProviderThatReturnsEssential.Object,
                        _mockSettingsServiceThatReturnsMarketingEnabledFalse.Object,
                        _mockEventLogService.Object);

            cookielessFormService.EnsureFormSubmitActivityIsLogged(_bizFormItemWithContactFieldMapping);

            _mockActivityLogService.Verify(
                x => x.LogWithoutModifiersAndFilters(It.IsAny<IActivityInitializer>()),
                Times.Never);
        }

        [Test]
        [NonParallelizable]
        public void EnsureFormSubmitActivityIsLogged_Calls_EventLogService_When_Exception_Is_Thrown()
        {
            var mockActivityLogService = new Mock<IActivityLogService>();
            var testException = new Exception("A fake exception occurred");

            mockActivityLogService.Setup(
                x => x.LogWithoutModifiersAndFilters(It.IsAny<IActivityInitializer>()))
                .Throws(testException);
            _mockEventLogService.Reset();

            var cookielessFormService = new CookielessFormService(
                        _mockSiteServiceThatReturnsIsLiveSiteTrue.Object,
                        mockActivityLogService.Object,
                        _mockCurrentCookieLevelProviderThatReturnsEssential.Object,
                        _mockSettingsServiceThatReturnsMarketingEnabledTrue.Object,
                        _mockEventLogService.Object);

            cookielessFormService.EnsureFormSubmitActivityIsLogged(_bizFormItemWithContactFieldMapping);

            mockActivityLogService.Verify(
                x => x.LogWithoutModifiersAndFilters(It.IsAny<IActivityInitializer>()),
                Times.Once);

            _mockEventLogService.Verify(
                x => x.LogException(nameof(CookielessFormService), 
                    nameof(CookielessFormService.EnsureFormSubmitActivityIsLogged), 
                    testException, 
                    null), 
                Times.Once);
        }

        [Test]
        [NonParallelizable]
        public void EnsureFormSubmitActivityIsLogged_Does_Not_Call_Activity_Log_Service_When_Not_In_Live_Site()
        {
            _mockActivityLogService.Reset();
            var cookielessFormService = new CookielessFormService(
                        _mockSiteServiceThatReturnsIsLiveSiteFalse.Object,
                        _mockActivityLogService.Object,
                        _mockCurrentCookieLevelProviderThatReturnsEssential.Object,
                        _mockSettingsServiceThatReturnsMarketingEnabledTrue.Object,
                        _mockEventLogService.Object);
            cookielessFormService.EnsureFormSubmitActivityIsLogged(_bizFormItemWithContactFieldMapping);
            _mockActivityLogService.Verify(x => x.LogWithoutModifiersAndFilters(It.IsAny<IActivityInitializer>()), Times.Never);
        }

        [Test]
        [NonParallelizable]
        public void EnsureFormSubmitActivityIsLogged_Does_Not_Call_Activity_Log_Service_When_Cookie_Level_Is_Visitor()
        {
            _mockActivityLogService.Reset();
            var cookielessFormService = new CookielessFormService(
                        _mockSiteServiceThatReturnsIsLiveSiteTrue.Object,
                        _mockActivityLogService.Object,
                        _mockCurrentCookieLevelProviderThatReturnsVisitor.Object,
                        _mockSettingsServiceThatReturnsMarketingEnabledTrue.Object,
                        _mockEventLogService.Object);
            cookielessFormService.EnsureFormSubmitActivityIsLogged(_bizFormItemWithContactFieldMapping);
            _mockActivityLogService.Verify(x => x.LogWithoutModifiersAndFilters(It.IsAny<IActivityInitializer>()), Times.Never);
        }
        [Test]
        [NonParallelizable]
        public void EnsureFormSubmitActivityIsLogged_Does_Not_Call_Activity_Log_Service_When_Form_Field_Is_Not_Mapped()
        {
            _mockActivityLogService.Reset();
            _cookielessFormServiceThatWillLogActivity.EnsureFormSubmitActivityIsLogged(_bizFormItemWithoutContactFieldMapping);
            _mockActivityLogService.Verify(x => x.LogWithoutModifiersAndFilters(It.IsAny<IActivityInitializer>()), Times.Never);
        }
        [Test]
        [NonParallelizable]
        public void EnsureFormSubmitActivityIsLogged_Does_Not_Call_Activity_Log_Service_When_Form_Field_Is_Empty()
        {
            _mockActivityLogService.Reset();
            _cookielessFormServiceThatWillLogActivity.EnsureFormSubmitActivityIsLogged(_bizFormItemWithBlankEmail);
            _mockActivityLogService.Verify(x => x.LogWithoutModifiersAndFilters(It.IsAny<IActivityInitializer>()), Times.Never);
        }

        [Test]
        [NonParallelizable]
        public void EnsureFormSubmitActivityIsLogged_Does_Not_Call_Activity_Log_Service_When_Email_Is_Invalid()
        {
            var bizFormItem = new FakeBizFormItem(
                "BizForm.LoyaltyProgramRequest",
                new Dictionary<string, object>()
                {
                    {"Email", "tiriansdoormailinator.com" },
                    {"FirstName", "Mike"},
                    {"LastName", "Wills"}
                });

            _mockActivityLogService.Reset();
            _cookielessFormServiceThatWillLogActivity.EnsureFormSubmitActivityIsLogged(bizFormItem);
            _mockActivityLogService.Verify(x => x.LogWithoutModifiersAndFilters(It.IsAny<IActivityInitializer>()), Times.Never);
        }

        [Test]
        [NonParallelizable]
        public void EnsureFormSubmitActivityIsLogged_Does_Not_Call_Activity_Log_Service_When_FormLogActivity_Is_False()
        {
            _mockActivityLogService.Reset();
            _cookielessFormServiceThatWillLogActivity.EnsureFormSubmitActivityIsLogged(_bizFormItemWithFormLogActivityFalse);
            _mockActivityLogService.Verify(x => x.LogWithoutModifiersAndFilters(It.IsAny<IActivityInitializer>()), Times.Never);
        }
    }
}
