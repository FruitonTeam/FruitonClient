using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridLayoutManager : MonoBehaviour {

    public GameObject GridCellBase;
    public int WidthCount;
    public int HeighCount;

    private float CELL_SIZE = 1f;

    //private ArrayList<GameObject> GridCells;

	// Use this for initialization
	void Start () {
        float originX = transform.position.x;
        float originZ = transform.position.z;

        CELL_SIZE = GridCellBase.GetComponent<Renderer>().bounds.size.x;

        originX -= (WidthCount / 2) * CELL_SIZE;
        originZ -= (HeighCount / 2) * CELL_SIZE;

        for (int i = 0; i < WidthCount; i++) {
            for (int j = 0; j < HeighCount; j++) {

                Instantiate(GridCellBase, new Vector3(originX + (i * CELL_SIZE), transform.position.y, originZ + (j * CELL_SIZE)), GridCellBase.transform.rotation, transform);
            }
        }

    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
