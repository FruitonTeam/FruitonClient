using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UI.Form
{
    public static class Validator {

        public delegate void GlobalValidator(Dictionary<string, string> values, Dictionary<string, string> errors);
        public delegate string ValidatorFunc(string value);

        public static ValidatorFunc Required(string message = "")
        {
            return value => string.IsNullOrEmpty(value) ? message : null;
        }

        public static ValidatorFunc MinLength(int length, string message = "")
        {
            return value => value.Length < length ? message : null;
        }

        public static ValidatorFunc MaxLength(int length, string message = "")
        {
            return value => value.Length > length ? message : null;
        }

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
