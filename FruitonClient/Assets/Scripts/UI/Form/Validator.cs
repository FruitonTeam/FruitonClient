using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UI.Form
{
    /// <summary>
    /// Helper class for form validators.
    /// </summary>
    public static class Validator {

        /// <summary>
        /// Delegate for global validators.
        /// </summary>
        /// <param name="values">mapping of form control names to respective values</param>
        /// <param name="errors">mapping of form control names to respective errors</param>
        public delegate void GlobalValidator(Dictionary<string, string> values, Dictionary<string, string> errors);

        /// <summary>
        /// Delegate for single form control validator.
        /// </summary>
        /// <param name="value">value of the form control</param>
        /// <returns>error message, null if value is valid</returns>
        public delegate string ValidatorFunc(string value);
        
        /// <summary>
        /// Contructs validator that requires control to have non-empty value.
        /// </summary>
        /// <param name="message">error message to display when form control's value is empty</param>
        /// <returns></returns>
        public static ValidatorFunc Required(string message = "")
        {
            return value => string.IsNullOrEmpty(value) ? message : null;
        }

        /// <summary>
        /// Constructs validator that requires form control value to have a value of a minimum length.
        /// </summary>
        /// <param name="length">minimum required length of the value</param>
        /// <param name="message">error message to display when form control's value is shorter than required length</param>
        /// <returns></returns>
        public static ValidatorFunc MinLength(int length, string message = "")
        {
            return value => value.Length < length ? message : null;
        }

        /// <summary>
        /// Constructs validator that requires form control value to have a value of a maximum length.
        /// </summary>
        /// <param name="length">maximum allowed length of the value</param>
        /// <param name="message">error message to display when form control's value is longer than allowed length</param>
        /// <returns></returns>
        public static ValidatorFunc MaxLength(int length, string message = "")
        {
            return value => value.Length > length ? message : null;
        }

        /// <summary>
        /// Constructs validator that requires form control value to match a regular expression.
        /// </summary>
        /// <param name="pattern">regular expression that the value should match</param>
        /// <param name="message">error message to display when form control's value doesn't match the regular expression</param>
        /// <returns></returns>
        public static ValidatorFunc Regex(string pattern, string message = "")
        {
            return value =>
            {
                var x = new Regex(pattern).Match(value);
                return x.Success ? null : message;
            };
        }
    }
}
