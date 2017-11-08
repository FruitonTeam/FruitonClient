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
        Touched = false;
        this.Name = name;
        this.InputField = inputField;
        this.Selectable = inputField;
        this.Validators = validators;
    }

    public FormControl(Selectable selectable)
    {
        this.Selectable = selectable;
    }
}