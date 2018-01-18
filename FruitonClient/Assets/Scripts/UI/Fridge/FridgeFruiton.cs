using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


public class FridgeFruiton : MonoBehaviour, IPointerDownHandler
{
#if UNITY_ANDROID
    private Coroutine pointerDownCoroutine;
    private Vector2 dragBeginPosition;
#endif

    public UnityEvent OnBeginDrag { get; private set; }

    void Awake()
    {
        OnBeginDrag = new UnityEvent();
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
#if UNITY_ANDROID
        dragBeginPosition = eventData.position;
        pointerDownCoroutine = StartCoroutine(PointerDownTimer());
#endif
#if UNITY_STANDALONE || UNITY_EDITOR
        OnBeginDrag.Invoke();
#endif
    }

#if UNITY_ANDROID
    void Update()
    {
        if (pointerDownCoroutine != null
            && (
                Input.touchCount == 0
                ||
                Vector2.Distance(Input.GetTouch(0).position, dragBeginPosition) > 100
            ))
        {
            StopCoroutine(pointerDownCoroutine);
            pointerDownCoroutine = null;
        }
    }

    IEnumerator PointerDownTimer()
    {
        yield return new WaitForSecondsRealtime(0.25f);
        OnBeginDrag.Invoke();
    }
#endif
}