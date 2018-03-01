using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FpsDisplay : MonoBehaviour
{
    public float RefreshTime = 0.5f;

    private Text text;
    private int frameCounter = 0;
    private float timeCounter = 0.0f;
    private float lastFramerate = 0.0f;

    void Start () {
		text = GetComponent<Text>();
	}
	
	void Update () {
	    if (timeCounter < RefreshTime)
	    {
	        timeCounter += Time.deltaTime;
	        frameCounter++;
	    }
	    else
	    {
	        lastFramerate = (float)frameCounter / timeCounter;
	        frameCounter = 0;
	        timeCounter = 0.0f;
	    }

	    text.text = (Mathf.Round(lastFramerate * 10) / 10f).ToString();
	}
}
