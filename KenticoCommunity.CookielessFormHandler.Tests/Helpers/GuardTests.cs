using KenticoCommunity.CookielessFormHandler.Helpers;
using NUnit.Framework;
using System;

namespace KenticoCommunity.CookielessFormHandler.Tests.Helpers
{
    [TestFixture]
    public class GuardTests
    {
        [TestCase("hi", "name"),
         TestCase("hi", null),
         TestCase("", "name")]
        public void ArgumentNotNull_ShouldSucceed(string value, string name)
        {
            Guard.ArgumentNotNull(value, name);
        }

        [Test]
        public void ArgumentNotNull_ObjectParameter()
        {
            Guard.ArgumentNotNull(new object(), "name");
        }

        [Test]
        public void ArgumentNotNull_ShouldThrow_ArgumentNullException()
        {
            Assert.That(() => Guard.ArgumentNotNull(null, "name"), Throws.TypeOf<ArgumentNullException>());
        }

        [Test]
        public void ArgumentGreaterThanZero_ShouldSucceed()
        {
            Assert.DoesNotThrow(() => Guard.ArgumentGreaterThanZero(15));
        }

        [Test]
        public void ArgumentGreaterThanZero_ShouldThrow_ArgumentException()
        {
            Assert.That(() => Guard.ArgumentGreaterThanZero(0), Throws.TypeOf<ArgumentException>());
        }

    }
}
