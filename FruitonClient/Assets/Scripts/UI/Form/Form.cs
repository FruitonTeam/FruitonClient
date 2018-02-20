using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Form : MonoBehaviour
{
    public Button SubmitButton;

    private GameObject errorPanel;
    private Text errorTextComponent;
    private List<FormControl> formControls;
    private Button submitOverlay;
    private List<Validator.GlobalValidator> globalValidators;
    private bool valid;
    private int errorFontSize;


    public Form SetInputs(Button submitButton, params FormControl[] formControls)
    {
        SubmitButton = submitButton;
        // disable persistent event listeners - ones that are set in editor
        // to block button's original funcionality while the form is not in valid state
        for (int i = 0; i < SubmitButton.onClick.GetPersistentEventCount(); i++)
        {
            SubmitButton.onClick.SetPersistentListenerState(i, UnityEventCallState.Off);
        }
        // add our own onClick listener - it triggers validation
        // and then invokes all persistent listeners if the form is valid
        SubmitButton.onClick.AddListener(SubmitForm);

        globalValidators = new List<Validator.GlobalValidator>();
        this.formControls = new List<FormControl>();
        this.formControls.AddRange(formControls);

        foreach (var control in formControls)
        {
            if (control.InputField != null)
            {
                // create new variable to use in closure below
                // since the one from foreach may behave differently with different compilers
                var c = control;
                control.InputField.onValueChanged.AddListener(
                    delegate
                    {
                        c.Touched = true;
                        ValidateForm(c);
                    });
            }
        }

        return this;
    }

    public Form AddGlobalValidator(Validator.GlobalValidator globalValidator)
    {
        globalValidators.Add(globalValidator);
        return this;
    }

    public Form SetErrorFontSize(int size)
    {
        errorFontSize = size;
        return this;
    }

    public void ResetForm()
    {
        foreach (var control in formControls)
        {
            if (control.InputField != null)
            {
                control.InputField.text = "";
                control.Touched = false;
            }
        }

        ValidateForm();
    }

    public void SetValue(string controlName, string value)
    {
        foreach (var control in formControls)
        {
            if (control.Name == controlName)
            {
                control.InputField.text = value;
            }
        }
    }

    void SubmitForm()
    {
        foreach (var control in formControls)
        {
            control.Touched = true;
        }

        valid = ValidateForm();

        if (valid)
        {
            // invoke all persistent listeners
            for (int i = 0; i < SubmitButton.onClick.GetPersistentEventCount(); i++)
            {
                SendMessage(
                    SubmitButton.onClick.GetPersistentMethodName(i),
                    SubmitButton.onClick.GetPersistentTarget(i)
                );
            }
        }
    }

    bool ValidateForm(FormControl currentControl = null)
    {
        var values = new Dictionary<string, string>();
        var errors = new Dictionary<string, string>();

        foreach (var control in formControls)
        {
            if (control.InputField != null)
            {
                values.Add(control.Name, control.InputField.text);
                errors.Add(control.Name, null);
            }
        }

        foreach (var globalValidator in globalValidators)
        {
            globalValidator(values, errors);
        }

        foreach (var control in formControls)
        {
            if (control.InputField == null)
            {
                continue;
            }
            foreach (var validator in control.Validators)
            {
                string errorMessage = validator(values[control.Name]);
                if (errorMessage != null)
                {
                    if (currentControl == control
                        || (currentControl == null && errors[control.Name] == null))
                    {
                        errors[control.Name] = errorMessage;
                    }
                    else if (currentControl != null)
                    {
                        errors[control.Name] = "";
                    }
                    break;
                }
            }
        }

        UpdateErrorPanel(errors, currentControl);

        foreach (var rec in errors)
        {
            if (rec.Value != null)
            {
                return false;
            }
        }
        return true;
    }

    private void UpdateErrorPanel(Dictionary<string, string> errors, FormControl currentControl = null)
    {
        if (errorPanel == null)
        {
            errorPanel = (GameObject) GameObject.Instantiate(Resources.Load("Prefabs/FormErrorPanel"));
            errorTextComponent = errorPanel.GetComponentInChildren<Text>(true);
            errorPanel.SetActive(false);
        }

        // hide error panel at the start, show only when error is found
        errorPanel.SetActive(false);


        foreach (var control in formControls)
        {
            if (control.InputField == null)
            {
                continue;
            }

            string error = errors[control.Name];
            var inputField = control.InputField;

            if (error == null || !control.Touched)
            {
                inputField.image.color = Color.white;
            }
            // do not show errors on controls that haven't been touched yet
            else if (control.Touched)
            {
                inputField.image.color = Color.red;

                // show error panel if there's an error with text
                // error panel is shown only only for one error - error from currently selected control
                // takes priority, if there's no such error we show panel for first error with text

                if (error != ""
                    && (!errorPanel.activeInHierarchy
                        || control == currentControl)
                )
                {
                    errorPanel.SetActive(true);
                    errorTextComponent.text = error;
                    errorTextComponent.fontSize = errorFontSize > 0
                        ? errorFontSize
                        : inputField.GetComponentInChildren<Text>().fontSize;

                    var errorRt = errorPanel.GetComponent<RectTransform>();
                    var fieldRt = inputField.GetComponent<RectTransform>();


                    // copy rect transform
                    errorRt.SetParent(fieldRt.parent);
                    errorRt.anchorMin = fieldRt.anchorMin;
                    errorRt.anchorMax = fieldRt.anchorMax;
                    errorRt.pivot = fieldRt.pivot;
                    errorRt.localScale = fieldRt.localScale;

                    if (fieldRt.localPosition.x < 100)
                    {
                        errorRt.anchoredPosition = fieldRt.anchoredPosition + new Vector2(5 + fieldRt.rect.width, 0);
                    }
                    else
                    {
                        errorRt.anchoredPosition = fieldRt.anchoredPosition + new Vector2(-5 - fieldRt.rect.width, 0);
                    }
                    var newLines = error.Count(c => c == '\n');
                    errorRt.sizeDelta = new Vector2(fieldRt.sizeDelta.x, fieldRt.sizeDelta.y * (newLines + 1));
                }
            }
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            var tabIndex = 0;
            foreach (var control in formControls)
            {
                if (control.Selectable.gameObject == selected)
                {
                    break;
                }
                tabIndex++;
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                tabIndex--;
            }
            else
            {
                tabIndex++;
            }

            if (tabIndex >= formControls.Count)
            {
                tabIndex = 0;
            }
            else if (tabIndex < 0)
            {
                tabIndex = formControls.Count - 1;
            }

            formControls[tabIndex].Selectable.Select();
        }

        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
        {
            SubmitForm();
        }
    }
}