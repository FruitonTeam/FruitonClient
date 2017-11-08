using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingSpinner : MonoBehaviour {

	void FixedUpdate ()
	{
	    transform.RotateAround(transform.position, Vector3.up, 7f);
	}
}
