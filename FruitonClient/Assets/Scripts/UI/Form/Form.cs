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
    private GameObject _errorPanel;
    private Text _errorTextComponent;
    private FormControl[] _controls;
    private Button _submitOverlay;
    private Button _submitButton;
    private List<Validator.GlobalValidator> _globalValidators;
    private int _currentTabIndex;
    private bool _valid;

    // Use this for initialization
    void Start()
    {
        _errorPanel = (GameObject) GameObject.Instantiate(Resources.Load("Prefabs/FormErrorPanel"));
        _errorTextComponent = _errorPanel.GetComponentInChildren<Text>();
        _errorPanel.SetActive(false);
    }

    public Form SetInputs(Button submitButton, params FormControl[] formControls)
    {
        _currentTabIndex = 0;
        _globalValidators = new List<Validator.GlobalValidator>();

        // create overlay over submit buttton to catch clicks
        _submitButton = submitButton;
        _submitButton.enabled = true;
        _submitOverlay = GameObject.Instantiate(_submitButton, _submitButton.transform.parent);
        _submitOverlay.onClick.RemoveAllListeners();
        // disables persistent event listeners - ones that are set in editor
        _submitOverlay.onClick.SetPersistentListenerState(0, UnityEventCallState.Off);
        _submitOverlay.image.color = new Color(0, 0, 1, 0.0f);
        _submitOverlay.onClick.AddListener(SubmitForm);

        _controls = formControls;

        foreach (var control in _controls)
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
        _globalValidators.Add(globalValidator);
        return this;
    }

    public void Clear()
    {
        foreach (var control in _controls)
        {
            if (control.InputField != null)
            {
                control.Touched = false;
                control.InputField.text = "";
            }
        }
    }

    public void SetValue(string controlName, string value)
    {
        foreach (var control in _controls)
        {
            if (control.Name == controlName)
            {
                control.InputField.text = value;
            }
        }
    }

    void SubmitForm()
    {
        foreach (var control in _controls)
        {
            control.Touched = true;
        }

        _valid = ValidateForm();
        _submitButton.enabled = _valid;
        if (_valid)
        {
            _submitButton.onClick.Invoke();
        }
    }

    bool ValidateForm(FormControl currentControl = null)
    {
        var values = new Dictionary<string, string>();
        var errors = new Dictionary<string, string>();

        foreach (var control in _controls)
        {
            if (control.InputField != null)
            {
                values.Add(control.Name, control.InputField.text);
                errors.Add(control.Name, null);
            }
        }

        foreach (var globalValidator in _globalValidators)
        {
            globalValidator(values, errors);
        }

        foreach (var control in _controls)
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
        // hide error panel at the start, show only when error is found
        _errorPanel.SetActive(false);


        foreach (var control in _controls)
        {
            if (control.InputField == null)
            {
                continue;
            }

            string error = errors[control.Name];
            var inputField = control.InputField;

            if (error == null)
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
                    && (!_errorPanel.activeInHierarchy
                        || control == currentControl)
                )
                {
                    _errorPanel.SetActive(true);
                    _errorTextComponent.text = error;

                    var errorRt = _errorPanel.GetComponent<RectTransform>();
                    var errorTextRt = _errorTextComponent.GetComponent<RectTransform>();
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

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            var tabIndex = 0;
            foreach (var control in _controls)
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

            if (tabIndex >= _controls.Length)
            {
                tabIndex = 0;
            }
            else if (tabIndex < 0)
            {
                tabIndex = _controls.Length - 1;
            }

            _controls[tabIndex].Selectable.Select();
        }

        if (Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.Return))
        {
            SubmitForm();
        }
    }
}