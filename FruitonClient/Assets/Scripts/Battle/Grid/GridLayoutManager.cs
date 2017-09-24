using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GridCellType { None, New, Scratched, Damaged }

public class GridLayoutManager : MonoBehaviour {

    public GameObject GridCellBase;
    public int WidthCount;
    public int HeighCount;

    private float CELL_SIZE = 1f;

    public static GridLayoutManager Instance { get; private set; }

    private GameObject[,] SpawnedGrid;
    private GridCellType[,] SpawnedGridType;

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
        SpawnedGridType = new GridCellType[WidthCount, HeighCount];

        for (int i = 0; i < WidthCount; i++) {
            for (int j = 0; j < HeighCount; j++) {
                SpawnedGrid[i, j] = (GameObject) Instantiate(GridCellBase, new Vector3(originX + (i * CELL_SIZE), transform.position.y, originZ + (j * CELL_SIZE)), GridCellBase.transform.rotation, transform);
                SpawnedGridType[i, j] = GridCellType.New;
            }
        }

    }
	
	// Destroy one cell of the grid
	 public bool destroyCell (int x, int y) {
        if (x >= 0 && x < WidthCount && y >= 0 && y < HeighCount && SpawnedGrid[x,y] != null)
        {
            Debug.Log("Grid Cell [" + x + ", " + y + "] is type " + SpawnedGridType[x, y]);
            Destroy(SpawnedGrid[x, y]);
            SpawnedGrid[x, y] = null;
            SpawnedGridType[x, y] = GridCellType.None;
            return true;
        }
        return false;
	}

    // Return type of grid cell
    public GridCellType gridCellType(int x, int y)
    {
        if (x >= 0 && x < WidthCount && y >= 0 && y < HeighCount)
        {
            return SpawnedGridType[x, y];
        }
        return GridCellType.None;
    }

    public Vector3 GetCellPosition(int x, int y)
    {
        return SpawnedGrid[x, y].transform.position;
    }

    public void HighlightCell(int x, int y)
    {
        SpawnedGrid[x, y].GetComponent<Renderer>().material.color = Color.blue;
    }

    public void ResetHighlights()
    {
        foreach (var cell in SpawnedGrid)
        {
            cell.GetComponent<Renderer>().material.color = Color.white;
        }
    }
}
