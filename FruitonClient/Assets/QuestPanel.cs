using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestPanel : MonoBehaviour
{
    public GameObject QuestsButton;
    public Text Quest1Text;
    public Text Quest2Text;
    public Text Quest3Text;
    public Color PanelColor = new Color(250, 241, 181);

    private GameObject questsButtonText;
    private GameObject[] childrenGameObjects;
    private Image image;
    private Vector3 questsButtonPosition;
    private Vector2 questsButtonSize;
    private RectTransform questsButtonRectTransform;

    public void Start()
    {
        int childCount = transform.childCount;
        childrenGameObjects = new GameObject[childCount];
        for (int i = 0; i < childCount; i++)
        {
            var child = transform.GetChild(i).gameObject;
            childrenGameObjects[i] = child;
            child.SetActive(false);
        }

        image = gameObject.GetComponent<Image>();

        questsButtonRectTransform = QuestsButton.GetComponent<RectTransform>();
        questsButtonSize = questsButtonRectTransform.sizeDelta;
        questsButtonPosition = QuestsButton.transform.position;
    }

    public void OpenPanel()
    {
        if (questsButtonText == null)
        {
            questsButtonText = QuestsButton.transform.GetChild(0).gameObject;
        }

        questsButtonText.SetActive(false);

        iTween.MoveTo(QuestsButton, iTween.Hash(
                "position", gameObject.transform.position,
                "time", 1,
                "easetype", iTween.EaseType.easeOutExpo
            )
        );
        iTween.ValueTo(gameObject, iTween.Hash(
                "from", questsButtonRectTransform.sizeDelta,
                "to", gameObject.GetComponent<RectTransform>().sizeDelta,
                "time", 1,
                "onupdate", "OnOpenUpdate",
                "oncomplete", "OnOpenComplete"
            )
        );
        iTween.ValueTo(gameObject, iTween.Hash(
                "from", new Color(PanelColor.r, PanelColor.g, PanelColor.b, 0),
                "to", PanelColor,
                "delay", 1,
                "time", 0.2f,
                "onupdate", "OnOpenUpdateColor",
                "oncomplete", "OnOpenCompleteColor"
            )
        );
    }

    public void ClosePanel()
    {
        image.color = new Color(0, 0, 0, 0);
        image.raycastTarget = false;
        foreach (var child in childrenGameObjects)
        {
            child.SetActive(false);
        }
        QuestsButton.SetActive(true);
        iTween.MoveTo(QuestsButton, iTween.Hash(
                "position", questsButtonPosition,
                "time", 0.5f,
                "easetype", iTween.EaseType.easeInExpo
            )
        );
        iTween.ValueTo(gameObject, iTween.Hash(
                "from", questsButtonRectTransform.sizeDelta,
                "to", questsButtonSize,
                "time", 0.5f,
                "onupdate", "OnCloseUpdate",
                "oncomplete", "OnCloseComplete"
            )
        );
    }

    void OnOpenUpdate(Vector2 sizeDelta)
    {
        questsButtonRectTransform.sizeDelta = sizeDelta;
    }

    void OnOpenUpdateColor(Color color)
    {
        image.color = color;
    }

    void OnOpenComplete()
    {
        image.raycastTarget = true;

        foreach (var child in childrenGameObjects)
        {
            child.SetActive(true);
        }
    }

    void OnOpenCompleteColor()
    {
        QuestsButton.SetActive(false);
    }

    void OnCloseUpdate(Vector2 sizeDelta)
    {
        questsButtonRectTransform.sizeDelta = sizeDelta;
    }

    void OnCloseComplete()
    {
        questsButtonText.SetActive(true);
    }

}