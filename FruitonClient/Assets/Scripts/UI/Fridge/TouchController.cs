using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchController : MonoBehaviour {

    public GameObject AllFruitons;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Touch[] myTouches = Input.touches;
        if (myTouches.Length == 1)
        {
            Touch touch = myTouches[0];
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray.origin, ray.direction, out hit, 100) && hit.collider == AllFruitons.GetComponent<BoxCollider>())
            {
                if (touch.phase == TouchPhase.Moved)
                {
                    transform.position -= new Vector3(touch.deltaPosition.x, 0, 0);
                }
                else if (touch.phase == TouchPhase.Ended)
                {

                }
                
            }
                
        }
        
    }
}
