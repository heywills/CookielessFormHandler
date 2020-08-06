using CMS.Activities;
using KenticoCommunity.CookielessFormHandler.ActivityInitializers;
using Moq;
using NUnit.Framework;
using System;

namespace KenticoCommunity.CookielessFormHandler.Tests.ActivityInitializers
{
    [TestFixture]
    public class CookielessFormActivityInitializerWrapperTests
    {
        private readonly Mock<IActivityInitializer> _mockActivityInitializer = new Mock<IActivityInitializer>();

        [SetUp]
        public void SetupTests()
        {
        }

        [Test]
        public void Constructor_Should_Throw_ArgumentNullException_When_OriginalInitializer_Is_Null()
        {
            Assert.That(() => new CookielessFormActivityInitializerWrapper(null, 12, 6), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void Constructor_Should_Throw_ArgumentException_When_ContactId_Is_LessThan_One()
        {
            Assert.That(() => new CookielessFormActivityInitializerWrapper(_mockActivityInitializer.Object, 0, 6), Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Constructor_Should_Throw_ArgumentException_When_SiteId_Is_LessThan_One()
        {
            Assert.That(() => new CookielessFormActivityInitializerWrapper(_mockActivityInitializer.Object, 1, -1), Throws.TypeOf<ArgumentException>());
        }

        [Test]
        public void Initialize_Sets_ContactID_And_SiteID()
        {
            var cookielessFormActivityInitializerWrapper = new CookielessFormActivityInitializerWrapper(_mockActivityInitializer.Object, 12, 6);
            var activity = new Mock<IActivityInfo>();
            activity.SetupAllProperties();
            cookielessFormActivityInitializerWrapper.Initialize(activity.Object);
            Assert.AreEqual(12, activity.Object.ActivityContactID);
            Assert.AreEqual(6, activity.Object.ActivitySiteID);
        }

        [Test]
        public void Initialize_Calls_OriginalInitializer_Initialize()
        {
            var cookielessFormActivityInitializerWrapper = new CookielessFormActivityInitializerWrapper(_mockActivityInitializer.Object, 12, 6);
            var activity = new Mock<IActivityInfo>();
            cookielessFormActivityInitializerWrapper.Initialize(activity.Object);
            _mockActivityInitializer.Verify(x => x.Initialize(activity.Object), Times.Once);
        }

        [Test]
        public void Initialize_Should_Throw_ArgumentNullException_If_Activity_Is_Null()
        {
            var cookielessFormActivityInitializerWrapper = new CookielessFormActivityInitializerWrapper(_mockActivityInitializer.Object, 12, 6);
            Assert.That(() => cookielessFormActivityInitializerWrapper.Initialize(null), Throws.TypeOf<ArgumentNullException>());
        }

    }
}
