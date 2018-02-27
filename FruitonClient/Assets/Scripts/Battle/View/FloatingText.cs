using UnityEngine;

namespace Battle.View
{
    public class FloatingText : MonoBehaviour
    {
        private float timeToDestroy = 5;
        private static readonly float SPEED = 5;

        void Update ()
        {
            float deltaTime = Time.deltaTime;
            timeToDestroy -= deltaTime;
            transform.position += SPEED * deltaTime * Vector3.up;
            if (timeToDestroy < 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
