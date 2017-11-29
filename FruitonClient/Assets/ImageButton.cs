using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ImageButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public UnityEvent OnClickEvent;
    public Color HoverColor;

    private Image image;
    private Color baseColor;

    void Awake()
    {
        if (OnClickEvent == null)
            OnClickEvent = new UnityEvent();
        image = GetComponent<Image>();
        baseColor = image.color;
    }

    void OnMouseDown()
    {
        OnClickEvent.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        image.color = HoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        image.color = baseColor;
    }
}
