using System;
using System.Collections;
using System.Collections.Generic;
using fruiton.kernel;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FridgeGridSquare : MonoBehaviour, IPointerDownHandler
{
    public int FruitonType;
    public UnityEvent OnBeginDrag { get; private set; }
    public bool IsEmpty { get; private set; }
    public bool IsSuggestionShown { get; private set; }
    public RectTransform RectTransform { get; private set; }
    public Fruiton KernelFruiton { get; private set; }

    private SkeletonGraphic spineSkeleton;
    private Image background;
    private Color defaultBackgroundColor;

    // Use this for initialization
    void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
        spineSkeleton = GetComponentInChildren<SkeletonGraphic>();
        background = GetComponentInChildren<Image>();
        defaultBackgroundColor = GetComponentInChildren<Image>().color;
        OnBeginDrag = new UnityEvent();
    }

    public void SetFruiton(Fruiton fruiton)
    {
        IsEmpty = false;
        KernelFruiton = fruiton;
        spineSkeleton.gameObject.SetActive(true);
        spineSkeleton.Skeleton.SetSkin(fruiton.model);
        spineSkeleton.color = Color.white;
    }

    public void SuggestFruiton(Fruiton fruiton)
    {
        IsSuggestionShown = true;
        spineSkeleton.gameObject.SetActive(true);
        spineSkeleton.Skeleton.SetSkin(fruiton.model);
        spineSkeleton.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    }

    public void ClearSuggestion()
    {
        if (!IsSuggestionShown)
        {
            return;
        }
        IsSuggestionShown = false;

        if (KernelFruiton == null)
        {
            ClearFruiton();
        }
        else
        {
            SetFruiton(KernelFruiton);
        }
    }

    public void ClearFruiton()
    {
        spineSkeleton.gameObject.SetActive(false);
        KernelFruiton = null;
        IsEmpty = true;
    }

    public void Highlight(Color color)
    {
        background.color = color;
    }

    public void CancelHighlight()
    {
        background.color = defaultBackgroundColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (KernelFruiton != null)
        {
            OnBeginDrag.Invoke();
        }
    }
}