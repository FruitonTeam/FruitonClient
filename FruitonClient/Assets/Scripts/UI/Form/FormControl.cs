using UnityEngine.UI;

namespace UI.Form
{
    /// <summary>
    /// Represents form control in unity
    /// </summary>
    public class FormControl
    {
        /// <summary>
        /// true if user changed value of the control
        /// </summary>
        public bool Touched;
        /// <summary>
        /// name of the control
        /// </summary>
        public string Name;
        /// <summary>
        /// input field attached to the control
        /// </summary>
        public InputField InputField;
        /// <summary>
        /// selectable game object attached to the control
        /// </summary>
        public Selectable Selectable;
        /// <summary>
        /// List of validators attached to the control
        /// </summary>
        public Validator.ValidatorFunc[] Validators;


        public FormControl(string name, InputField inputField, params Validator.ValidatorFunc[] validators)
        {
            Name = name;
            InputField = inputField;
            Selectable = inputField;
            Validators = validators;
        }

        public FormControl(Selectable selectable)
        {
            Selectable = selectable;
        }
    }
}