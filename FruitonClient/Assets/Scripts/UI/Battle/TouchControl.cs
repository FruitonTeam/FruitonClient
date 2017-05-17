using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchControl : MonoBehaviour {

    public GameObject board;
    /// <summary> Y axis of the plane on which camera movec dring translation. </summary>
    Vector3 translateDirection;
    /// <summary> X axis of the plane on which camera movec dring translation. </summary>
    Vector3 translateNormal;

    private void Start()
    {
        ComputeTranslateVectors();
    }

	// Update is called once per frame
	void Update () {
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
        // Rotations of the screen
        if (touch.phase == TouchPhase.Moved)
        {
            // the center of the board
            Vector2 center = Camera.main.WorldToScreenPoint(board.transform.position);
            // angle between previous and current finger position
            float angle = FruitMath.GetAngleBetweenTwoPoints(touch.position - center, touch.position - touch.deltaPosition - center);
            transform.RotateAround(new Vector3(0, 0, 0), Vector3.down, 2 * angle);
        }

    }

    /// <summary>
    /// Updates the values of translateDirection and translateNormal.
    /// </summary>
    private void ComputeTranslateVectors()
    {
        Plane planeXZ = new Plane(Vector3.up, Vector3.zero);
        Vector3 center;
        FruitMath.LinePlaneIntersection(out center, Camera.main.transform.position, Camera.main.transform.forward, Vector3.up, Vector3.zero); 
        center.y = 0;
        Vector3 camera = Camera.main.transform.position;
        // Project the camera location to XZ plane.
        camera.y = 0; 
        translateDirection = (center - camera).normalized;
        translateNormal = new Vector3(translateDirection.z, 0, -translateDirection.x);
    }

    /// <summary>
    /// User performed an action using two touches.
    /// </summary>
    private void DoubleTouchLogic(Touch touch1, Touch touch2)
    {
        if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
        {
            // Comptute the translation vectors in case translation will occur.
            ComputeTranslateVectors();
        }
        // Translation
        else if ((touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Stationary) ||
            (touch2.phase == TouchPhase.Moved && touch1.phase == TouchPhase.Stationary))
        {
            // Move the camera on a plane that is parallerl to the plane given by board and intrsects with the camera coords.
            Vector3 delta = new Vector3(touch1.deltaPosition.x, 0, touch1.deltaPosition.y) + new Vector3(touch2.deltaPosition.x, 0, touch2.deltaPosition.y);
            Camera.main.transform.position -= 0.01f * (delta.x * translateNormal + delta.z * translateDirection);
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
            float fieldOfView = Camera.main.fieldOfView;
        }
    }
}
