using UnityEngine;

namespace UI
{
    public class LoadingSpinner : MonoBehaviour {

        void Update ()
        {
            transform.RotateAround(transform.position, Vector3.up, 700f * Time.deltaTime);
        }
    }
}
