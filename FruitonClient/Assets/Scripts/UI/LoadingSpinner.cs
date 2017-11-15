using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingSpinner : MonoBehaviour {

	void Update ()
	{
	    transform.RotateAround(transform.position, Vector3.up, 700f * Time.deltaTime);
	}
}
