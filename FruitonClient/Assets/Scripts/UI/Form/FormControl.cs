using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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