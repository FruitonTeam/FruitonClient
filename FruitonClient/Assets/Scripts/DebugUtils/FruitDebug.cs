using MathUtils;
using UnityEngine;

namespace DebugUtils
{
    public class FruitDebug : MonoBehaviour {

        // Use this for initialization
        void Start () {
            Vector2 vector = new Vector2(1, -1);
            float angle = FruitMath.GetAngleFromXAxis(vector);
            Debug.Log("angle from x of " + vector + " is " + angle);
            Debug.Log(System.Environment.Version);
        }
	
    }
}
