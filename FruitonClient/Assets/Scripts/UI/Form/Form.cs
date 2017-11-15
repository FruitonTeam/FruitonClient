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
    private GameObject errorPanel;
    private Text errorTextComponent;
    private FormControl[] controls;
    private Button submitOverlay;
    private Button submitButton;
    private List<Validator.GlobalValidator> globalValidators;
    private bool valid;

    public Form SetInputs(Button submitButton, params FormControl[] formControls)
    {
        globalValidators = new List<Validator.GlobalValidator>();

        // create overlay over submit buttton to catch clicks
        this.submitButton = submitButton;
        this.submitButton.enabled = true;
        submitOverlay = GameObject.Instantiate(this.submitButton, this.submitButton.transform.parent);
        submitOverlay.onClick.RemoveAllListeners();
        // disables persistent event listeners - ones that are set in editor
        submitOverlay.onClick.SetPersistentListenerState(0, UnityEventCallState.Off);
        submitOverlay.image.color = new Color(0, 0, 1, 0.0f);
        submitOverlay.onClick.AddListener(SubmitForm);

        controls = formControls;

        foreach (var control in controls)
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

    public void ResetForm()
    {
        foreach (var control in controls)
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
        foreach (var control in controls)
        {
            if (control.Name == controlName)
            {
                control.InputField.text = value;
            }
        }
    }

    void SubmitForm()
    {
        foreach (var control in controls)
        {
            control.Touched = true;
        }

        valid = ValidateForm();
        submitButton.enabled = valid;
        if (valid)
        {
            submitButton.onClick.Invoke();
        }
    }

    bool ValidateForm(FormControl currentControl = null)
    {
        var values = new Dictionary<string, string>();
        var errors = new Dictionary<string, string>();

        foreach (var control in controls)
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

        foreach (var control in controls)
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
            errorPanel = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/FormErrorPanel"));
            errorTextComponent = errorPanel.GetComponentInChildren<Text>(true);
            errorPanel.SetActive(false);
        }

        // hide error panel at the start, show only when error is found
        errorPanel.SetActive(false);


        foreach (var control in controls)
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

                    var errorRt = errorPanel.GetComponent<RectTransform>();
                    var errorTextRt = errorTextComponent.GetComponent<RectTransform>();
                    var fieldRt = inputField.GetComponent<RectTransform>();

                    // use `setParent` with  `worldPositionStays` `false` to prevent problems with scaling
                    errorRt.SetParent(fieldRt.parent, false);
                    if (fieldRt.localPosition.x < 100)
                    {
                        errorRt.localPosition = fieldRt.localPosition + new Vector3(5 + fieldRt.sizeDelta.x, 0, 0);
                    }
                    else
                    {
                        errorRt.localPosition = fieldRt.localPosition + new Vector3(-5 - fieldRt.sizeDelta.x, 0, 0);
                    }
                    var newLines = error.Count(c => c == '\n');
                    errorRt.sizeDelta = new Vector2(fieldRt.sizeDelta.x, fieldRt.sizeDelta.y * (newLines + 1));
                    errorTextRt.sizeDelta = new Vector2(errorTextRt.sizeDelta.x, fieldRt.sizeDelta.y * (newLines + 1));
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
            foreach (var control in controls)
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

            if (tabIndex >= controls.Length)
            {
                tabIndex = 0;
            }
            else if (tabIndex < 0)
            {
                tabIndex = controls.Length - 1;
            }

            controls[tabIndex].Selectable.Select();
        }

        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
        {
            SubmitForm();
        }
    }
}