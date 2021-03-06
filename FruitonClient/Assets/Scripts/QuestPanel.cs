﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles displaying information about player's quests.
/// </summary>
public class QuestPanel : MonoBehaviour
{   
    public GameObject QuestsButton;
    public Text[] QuestTexts;
    public Color PanelColor = new Color(250, 241, 181);
    public Text QuestCountText;

    private GameObject[] questsButtonChildren;
    private GameObject[] childrenGameObjects;
    private Image image;
    private Vector3 questsButtonPosition;
    private Vector2 questsButtonSize;
    private RectTransform questsButtonRectTransform;

    /// <summary>
    /// Loads player's quests data.
    /// </summary>
    public void Start()
    {
        UpdateQuestCount();

        int childCount = transform.childCount;
        childrenGameObjects = new GameObject[childCount];
        for (int i = 0; i < childCount; i++)
        {
            var child = transform.GetChild(i).gameObject;
            childrenGameObjects[i] = child;
            child.SetActive(false);
        }

        childCount = QuestsButton.transform.childCount;
        questsButtonChildren = new GameObject[childCount];
        for (int i = 0; i < childCount; i++)
        {
            questsButtonChildren[i] = QuestsButton.transform.GetChild(i).gameObject;
        }

        image = gameObject.GetComponent<Image>();

        questsButtonRectTransform = QuestsButton.GetComponent<RectTransform>();
        questsButtonSize = questsButtonRectTransform.sizeDelta;
        questsButtonPosition = QuestsButton.transform.position;
    }

    void OnEnable()
    {
        UpdateQuestCount();
    }

    /// <summary>
    /// Counts number of player's quests and displays it.
    /// </summary>
    /// <returns>player's quests count</returns>
    public int UpdateQuestCount()
    {
        var questCount = GameManager.Instance.Quests.Count;
        if (questCount <= 0)
        {
            QuestCountText.transform.parent.gameObject.SetActive(false);
        }
        else
        {
            QuestCountText.transform.parent.gameObject.SetActive(true);
            QuestCountText.text = questCount.ToString();
        }

        return questCount;
    }

    /// <summary>
    /// Opens quests panel and displays quests data.
    /// </summary>
    public void OpenPanel()
    {
        var quests = GameManager.Instance.Quests;
        for (int i = 0; i < QuestTexts.Length; i++)
        {
            if (i < quests.Count)
            {
                var quest = quests[i];
                QuestTexts[i].text = String.Format(
                    "<b>- {0}</b>: {1} ({2}/{3})",
                    quest.Name,
                    quest.Description,
                    quest.Progress,
                    quest.Goal
                    );
            }
            else
            {
                QuestTexts[i].text = "";
            }
        }
        if (UpdateQuestCount() <= 0)
        {
            QuestTexts[0].text = "No quests available right now!";
        }


        foreach (var child in questsButtonChildren)
        {
            child.SetActive(false);
        }

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

    /// <summary>
    /// Closes quest panel.
    /// </summary>
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

    #region iTween callbacks

    private void OnOpenUpdate(Vector2 sizeDelta)
    {
        questsButtonRectTransform.sizeDelta = sizeDelta;
    }

    private void OnOpenUpdateColor(Color color)
    {
        image.color = color;
    }

    private void OnOpenComplete()
    {
        image.raycastTarget = true;

        foreach (var child in childrenGameObjects)
        {
            child.SetActive(true);
        }
    }

    private void OnOpenCompleteColor()
    {
        QuestsButton.SetActive(false);
    }

    private void OnCloseUpdate(Vector2 sizeDelta)
    {
        questsButtonRectTransform.sizeDelta = sizeDelta;
    }

    private void OnCloseComplete()
    {
        foreach (var child in questsButtonChildren)
        {
            child.SetActive(true);
        }
        UpdateQuestCount();
    }

    #endregion
}