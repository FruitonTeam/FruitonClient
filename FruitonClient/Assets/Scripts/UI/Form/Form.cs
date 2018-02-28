using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Form
{
    /// <summary>
    /// Handles form behaviour in game.
    /// </summary>
    public class Form : MonoBehaviour
    {
        private Button submitButton;
        private GameObject errorPanel;
        private Text errorTextComponent;
        private FormControl[] formControls;
        private Button submitOverlay;
        private List<Validator.GlobalValidator> globalValidators;
        private bool valid;
        private int errorFontSize;

        /// <summary>
        /// Sets form's inputs and submit button.
        /// </summary>
        /// <param name="submitButton">button to be used for submitting</param>
        /// <param name="formControls">list of form's controls</param>
        public Form SetInputs(Button submitButton, params FormControl[] formControls)
        {
            this.submitButton = submitButton;
            // disable persistent event listeners - ones that are set in editor
            // to block button's original funcionality while the form is not in valid state
            for (int i = 0; i < this.submitButton.onClick.GetPersistentEventCount(); i++)
            {
                this.submitButton.onClick.SetPersistentListenerState(i, UnityEventCallState.Off);
            }
            // add our own onClick listener - it triggers validation
            // and then invokes all persistent listeners if the form is valid
            this.submitButton.onClick.AddListener(SubmitForm);

            globalValidators = new List<Validator.GlobalValidator>();
            this.formControls = formControls;

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

        /// <summary>
        /// Adds global validator to the form.
        /// </summary>
        /// <param name="globalValidator">global validator to add</param>
        public Form AddGlobalValidator(Validator.GlobalValidator globalValidator)
        {
            globalValidators.Add(globalValidator);
            return this;
        }

        /// <summary>
        /// Sets font size of form errors.
        /// </summary>
        /// <param name="size">size to use for form errors</param>
        public Form SetErrorFontSize(int size)
        {
            errorFontSize = size;
            return this;
        }

        /// <summary>
        /// Clears all input fields and removes errors.
        /// </summary>
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

        /// <summary>
        /// Sets value of a form control.
        /// </summary>
        /// <param name="controlName">name of the control</param>
        /// <param name="value">value to use</param>
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

        /// <summary>
        /// Validates inputs and submits the form.
        /// </summary>
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
                for (int i = 0; i < submitButton.onClick.GetPersistentEventCount(); i++)
                {
                    SendMessage(
                        submitButton.onClick.GetPersistentMethodName(i),
                        submitButton.onClick.GetPersistentTarget(i)
                    );
                }
            }
        }

        /// <summary>
        /// Validates form inputs.
        /// </summary>
        /// <param name="focusedControl">currently focused form control</param>
        /// <returns>true if the form is valid</returns>
        bool ValidateForm(FormControl focusedControl = null)
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
                        if (focusedControl == control
                            || (focusedControl == null && errors[control.Name] == null))
                        {
                            errors[control.Name] = errorMessage;
                        }
                        else if (focusedControl != null)
                        {
                            errors[control.Name] = "";
                        }
                        break;
                    }
                }
            }

            UpdateErrorPanel(errors, focusedControl);

            foreach (var rec in errors)
            {
                if (rec.Value != null)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Shows errors in form.
        /// </summary>
        /// <param name="errors">mapping of control names to errors</param>
        /// <param name="focusedControl">currently focused form control</param>
        private void UpdateErrorPanel(Dictionary<string, string> errors, FormControl focusedControl = null)
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
                    // error panel is shown only only for one error - error from currently focused control
                    // takes priority, if there's no such error we show panel for first error with text

                    if (error != ""
                        && (!errorPanel.activeInHierarchy
                            || control == focusedControl)
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

        /// <summary>
        /// Listens for enter, tab and shift + tab key presses, updates form focus according to them.
        /// </summary>
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

                if (tabIndex >= formControls.Length)
                {
                    tabIndex = 0;
                }
                else if (tabIndex < 0)
                {
                    tabIndex = formControls.Length - 1;
                }

                formControls[tabIndex].Selectable.Select();
            }

            if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
            {
                SubmitForm();
            }
        }
    }
}