using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollController : MonoBehaviour {

    public HorizontalLayoutGroup content;

    public void OnScroll()
    {
        Debug.Log(content.transform.childCount * content.GetComponent<RectTransform>().sizeDelta.y);
        //this.GetComponent<ScrollRect>().StopMovement();
        //this.GetComponent<ScrollRect>().enabled = false;
    }
}
