using CMS.OnlineForms;
using System.Collections.Generic;

namespace KenticoCommunity.CookielessFormHandler.Tests.Fakes
{
    /// <summary>
    /// The BizFormItem cannot be created using Kentico's CMS.Tests.Fake framework.
    /// Create this derived type to override the constructor and needed virtual properties
    /// so that a fake object can be created without allowing Kentico to attempt a
    /// database connection.
    /// </summary>
    /// <remarks>This requires providing a custom constructor. Kentico's default constructor is
    /// marked as obsolete, because its i used for system purposes only.</remarks>
    public class FakeBizFormItem: BizFormItem
    {
        private readonly Dictionary<string, object> _fieldValues;
        private readonly string _className;

#pragma warning disable CS0618 
        public FakeBizFormItem(string className, Dictionary<string, object> fieldValues)
        {
            _className = className;
            _fieldValues = fieldValues;
        }
#pragma warning restore CS0618

        /// <summary>
        /// Override the GetStringValue method so that it can be used in
        /// unit tests.
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public override string GetStringValue(string fieldName, string defaultValue)
        {
            if(_fieldValues.TryGetValue(fieldName, out var returnValue))
            {
                return (string)returnValue;
            }
            else
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Override the TryGetValue method so that it can be used in
        /// unit tests.
        /// </summary>
        public override bool TryGetValue(string columnName, out object value)
        {
            return _fieldValues.TryGetValue(columnName, out value);
        }

        public override string BizFormClassName => _className;

        public override string ClassName => _className;

    }
}
