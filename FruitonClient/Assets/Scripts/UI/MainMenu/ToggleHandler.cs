using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToggleHandler : MonoBehaviour
{
    public Button loginButton;

    public void OnValueChanged(bool on)
    {
        loginButton.Select();
    }
}
