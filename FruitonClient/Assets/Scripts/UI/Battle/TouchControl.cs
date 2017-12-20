﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchControl : MonoBehaviour {

    #region All platforms fields

    public GameObject Board;

    #endregion

    #region Windows fields

    Vector3? startPointRight;

    #endregion


    private void Start()
    {
    }

	// Update is called once per frame
	private void Update () {
    #if UNITY_ANDROID && !UNITY_EDITOR
            UpdateAndroid();
    #elif UNITY_EDITOR || UNITY_STANDALONE
        UpdateStandalone();
        #endif
    }  

    #region All platforms methods

        // Move the camera on a plane that is parallel to the plane given by board and intrsects with the camera coords.
        private void TranslateBoard(Vector3 delta)
        {
            Board.transform.position += delta;
        }

    #endregion

    #region Android methods

    private void UpdateAndroid()
    {
        Touch[] myTouches = Input.touches;
        switch (Input.touchCount)
        {
            case 1:
                {
                    SingleTouchLogic(myTouches[0]);
                }
                break;
            case 2:
                {
                    DoubleTouchLogic(myTouches[0], myTouches[1]);
                }
                break;
        }
    }

    /// <summary>
    /// User performed an action using single <paramref name="touch"/>
    /// </summary>
    /// <param name="touch"></param>
    private void SingleTouchLogic(Touch touch)
    {

    }

    /// <summary>
    /// User performed an action using two touches.
    /// </summary>
    private void DoubleTouchLogic(Touch touch1, Touch touch2)
    {
        // Translation
        if ((touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Stationary) ||
            (touch2.phase == TouchPhase.Moved && touch1.phase == TouchPhase.Stationary))
        {
            Vector3 delta = new Vector3(touch1.deltaPosition.x, 0, touch1.deltaPosition.y) + new Vector3(touch2.deltaPosition.x, 0, touch2.deltaPosition.y);
            TranslateBoard(delta);
        }
        // Scale (pinch scale)
        else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
        {
            float distance = Vector2.Distance(touch1.position, touch2.position);
            float lastDistance = Vector2.Distance(touch1.position - touch1.deltaPosition, touch2.position - touch2.deltaPosition);
            float zoom;
            // Scale according to the delta of finger distances. 
            zoom = 0.1f * (lastDistance - distance);
            Camera.main.fieldOfView += zoom;
        }
    }

    #endregion

    #region Windows methods

    private void UpdateStandalone()
    {
        // ZOOM
        float mouseWheel = Input.GetAxis("Mouse ScrollWheel");
        if (mouseWheel != 0)
        {
            float potentialSize = Camera.main.orthographicSize - 10 * mouseWheel;
            if (potentialSize >= 1 && potentialSize <= 20)
            {
                Camera.main.orthographicSize = potentialSize;
            }
            
        }

        // The moment of pressing both mouse buttons. Tranlslate vectors have to be computed
        // in order to translate the board in the correct direction. 
        if (Input.GetMouseButtonDown(1))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            startPointRight = ray.origin + (ray.direction * Vector3.Distance(Camera.main.transform.position, Board.transform.position));
        }
        else
        {
            if (Input.GetMouseButton(1))
            {
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                Vector3 endPoint = ray.origin + (ray.direction * Vector3.Distance(Camera.main.transform.position, Board.transform.position));
                if (startPointRight.HasValue)
                {
                    // Check whether the user wants to translate (2 buttons) or to rotate (1 button)
                    if (Input.GetMouseButton(1))
                    {
                        Vector3 delta = endPoint - startPointRight.Value;
                        TranslateBoard(delta);
                    }
                }
                startPointRight = endPoint;
            }
            else if (Input.GetMouseButtonUp(1))
            {
                startPointRight = null;
            }
        }
    }

    #endregion
}
