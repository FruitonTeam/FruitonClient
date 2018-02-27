using Battle.Grid;
using UnityEngine;

namespace DebugUtils
{
    public class GridDebug : MonoBehaviour {

        public int x;
        public int y;
        public bool destroyCell = false;

        void Start()
        {
            destroyCell = false;
        }

        // Update is called once per frame
        void Update () {
            if (destroyCell) {
                if (GridLayoutManager.Instance.destroyCell(x, y))
                {
                    Debug.Log("Grid Cell [" + x + ", " + y + "] destroyed!");
                }
                else
                {
                    Debug.Log("Grid Cell [" + x + ", " + y + "] doesn't exist!");
                }
                destroyCell = false;
            }
        }
    }
}
