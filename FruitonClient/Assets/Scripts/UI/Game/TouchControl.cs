using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchControl : MonoBehaviour {
    public GameObject board;

    Dictionary<int, Touch> touches;
    Vector3 translateDirection, translateNormal;

    private void Start()
    {
        touches = new Dictionary<int, Touch>();
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

    private void SingleTouchLogic(Touch touch)
    {
        // TODO: collisions with fruitons
        //Ray ray = Camera.main.ScreenPointToRay(touch.position);
        //RaycastHit hit = new RaycastHit();
        //if (Physics.Raycast(ray.origin, ray.direction, out hit, 1000))

        Vector2 center = Camera.main.WorldToScreenPoint(board.transform.position);
        float angle = FruitMath.GetAngleBetweenTwoPoints(touch.position - center, touch.position - touch.deltaPosition - center);
        // Rotations of the screen
        if (touch.phase == TouchPhase.Moved)
        {
            transform.RotateAround(new Vector3(0, 0, 0), Vector3.down, 2 * angle);
        }

    }

    private void ComputeTranslateVectors()
    {
        Plane planeXZ = new Plane(Vector3.up, Vector3.zero);
        Vector3 center;
        FruitMath.LinePlaneIntersection(out center, Camera.main.transform.position, Camera.main.transform.forward, Vector3.up, Vector3.zero);
        
        center.y = 0;
        Vector3 camera = Camera.main.transform.position;
        camera.y = 0;
        translateDirection = (center - camera).normalized;
        translateNormal = new Vector3(translateDirection.z, 0, -translateDirection.x);
    }

    private void DoubleTouchLogic(Touch touch1, Touch touch2)
    {
        if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
        {
            ComputeTranslateVectors();

        }
        else if ((touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Stationary) ||
            (touch2.phase == TouchPhase.Moved && touch1.phase == TouchPhase.Stationary))
        {
            Vector3 delta = (new Vector3(touch1.deltaPosition.x, 0, touch1.deltaPosition.y) + new Vector3(touch2.deltaPosition.x, 0, touch2.deltaPosition.y)) / 2;
            Camera.main.transform.position -= delta.x * translateNormal + delta.z * translateDirection;
        }
        else if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
        {

            //if (touch1.phase == TouchPhase.Moved && touch2.phase == TouchPhase.Moved && FruitMath.GetAngleBetweenTwoPoints(touch1.deltaPosition, touch2.deltaPosition) < 45)
            //{

            //}
            //else
            {
                float distance = Vector2.Distance(touch1.position, touch2.position);
                float lastDistance = Vector2.Distance(touch1.position - touch1.deltaPosition, touch2.position - touch2.deltaPosition);
                float zoom;
                zoom = 0.1f * (lastDistance - distance);
                Camera.main.fieldOfView += zoom;
                float fieldOfView = Camera.main.fieldOfView;
                //if (fieldOfView < 15)
                //{
                //    Camera.main.fieldOfView = 15;
                //}
                //if (fieldOfView > 150)
                //{
                //    Camera.main.fieldOfView = 150;
                //}
            }
        }
    }
}
