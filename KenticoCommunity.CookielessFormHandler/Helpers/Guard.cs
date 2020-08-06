using System;

namespace KenticoCommunity.CookielessFormHandler.Helpers
{
    public static class Guard
    {
        /// <summary>
        /// Throws an exception if an argument is null
        /// </summary>
        /// <param name="value">The value to be tested</param>
        /// <param name="name">The name of the argument</param>
        public static void ArgumentNotNull(object value, string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = nameof(value);
            }

            if (value == null)
            {
                throw new ArgumentNullException(name, "Argument " + name + " must not be null");
            }
        }

        /// <summary>
        /// Throws an exception if a string argument is not greater than zero.
        /// </summary>
        /// <param name="value">The value to be tested</param>
        /// <param name="name">The name of the argument</param>
        public static void ArgumentGreaterThanZero(int value, string name = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = nameof(value);
            }


            if (value <= 0)
            {
                throw new ArgumentException($"Argument { name } must not be an greater than zero", name);
            }
        }
    }
}
