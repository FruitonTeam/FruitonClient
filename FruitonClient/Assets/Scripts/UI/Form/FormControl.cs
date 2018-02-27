using UnityEngine.UI;

namespace UI.Form
{
    public class FormControl
    {
        public bool Touched;
        public string Name;
        public InputField InputField;
        public Selectable Selectable;
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