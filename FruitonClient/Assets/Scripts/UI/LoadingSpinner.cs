using UnityEngine;

namespace UI
{
    /// <summary>
    /// Rotates object it's attached to. Used for loading spinner animation.
    /// </summary>
    public class LoadingSpinner : MonoBehaviour {

        void Update ()
        {
            transform.RotateAround(transform.position, Vector3.up, 700f * Time.deltaTime);
        }
    }
}
