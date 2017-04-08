using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridLayoutManager : MonoBehaviour {

    public GameObject GridCellBase;
    public int WidthCount;
    public int HeighCount;

    private float CELL_SIZE = 1f;

    public static GridLayoutManager Instance { get; private set; }

    private GameObject[,] SpawnedGrid;

    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Use this for initialization
    void Start() {
        float originX = transform.position.x;
        float originZ = transform.position.z;

        CELL_SIZE = GridCellBase.GetComponent<Renderer>().bounds.size.x;

        originX -= ((WidthCount / 2f) - .5f) * CELL_SIZE;
        originZ -= ((HeighCount / 2f) - .5f) * CELL_SIZE;

        SpawnedGrid = new GameObject[WidthCount, HeighCount];

        for (int i = 0; i < WidthCount; i++) {
            for (int j = 0; j < HeighCount; j++) {

                SpawnedGrid[i,j] = (GameObject) Instantiate(GridCellBase, new Vector3(originX + (i * CELL_SIZE), transform.position.y, originZ + (j * CELL_SIZE)), GridCellBase.transform.rotation, transform);
            }
        }

    }
	
	// Destroy one cell of the grid
	 public bool destroyCell (int x, int y) {
        if (x >= 0 && x < WidthCount && y >= 0 && y < HeighCount && SpawnedGrid[x,y] != null) {
            Destroy(SpawnedGrid[x, y]);
            SpawnedGrid[x, y] = null;
            return true;
        }
        return false;
	}
}
