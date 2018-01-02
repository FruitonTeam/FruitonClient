using System;
using UnityEngine;
using UnityEngine.UI;

public class MessagePanel : MonoBehaviour
{
    public GameObject SuccessImage;
    public GameObject ErrorImage;

    private Text textComponent;
    private Button button;

    private Action onCloseAction;

    public void ShowInfoMessage(string text)
    {
        Activate();
        SuccessImage.SetActive(true);
        ErrorImage.SetActive(false);
        textComponent.text = text;
        button.Select();
    }

    public void ShowErrorMessage(string text)
    {
        Activate();
        SuccessImage.SetActive(false);
        ErrorImage.SetActive(true);
        textComponent.text = text;
        button.Select();
    }

    public void HideMessage()
    {
        gameObject.SetActive(false);
    }

    private void Activate()
    {
        if (textComponent == null)
        {
            textComponent = GetComponentInChildren<Text>(true);
        }
        if (button == null)
        {
            button = GetComponentInChildren<Button>(true);
        }
        gameObject.SetActive(true);
    }

    public void OnClose(Action a)
    {
        onCloseAction = a;
        if (button == null)
        {
            button = GetComponentInChildren<Button>(true);
        }
        button.onClick.AddListener(PerformOnCloseAction);
    }

    private void PerformOnCloseAction()
    {
        if (onCloseAction != null)
        {
            onCloseAction();
        }
        onCloseAction = null;
    }

}