using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeactivateOnClick : MonoBehaviour {

    void OnMouseOver()
    {
        Debug.Log("bbb");
        gameObject.SetActive(false);
    }

    void OnMouseDown()
    {
        Debug.Log("aaa");
        gameObject.SetActive(false);
    }
}
