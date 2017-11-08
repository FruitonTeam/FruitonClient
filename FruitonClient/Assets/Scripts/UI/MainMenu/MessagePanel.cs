using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessagePanel : MonoBehaviour
{

    public GameObject SuccessImage;
    public GameObject ErrorImage;

    private Text _textComponent;
    private Button _button;

    void Start()
    {
        _textComponent = GetComponentInChildren<Text>();
        _button = GetComponentInChildren<Button>();
        gameObject.SetActive(false);
    }

    public void ShowInfoMessage(string text)
    {
        gameObject.SetActive(true);
        SuccessImage.SetActive(true);
        ErrorImage.SetActive(false);
        _textComponent.text = text;
        _button.Select();
    }

    public void ShowErrorMessage(string text)
    {
        gameObject.SetActive(true);
        SuccessImage.SetActive(false);
        ErrorImage.SetActive(true);
        _textComponent.text = text;
        _button.Select();
    }

    public void HideMessage()
    {
        gameObject.SetActive(false);
    }
}
