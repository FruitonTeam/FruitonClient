using System;
using System.Collections;
using System.Collections.Generic;
using Extensions;
using fruiton.dataStructures;
using UnityEngine;

namespace Battle.Grid
{
    public enum GridCellType { None, New, Scratched, Damaged }

    public class GridLayoutManager : MonoBehaviour {

        public GameObject GridCellBase;
        public int WidthCount;
        public int HeighCount;
        public List<GameObject> Obstacles;

        private float CELL_SIZE = 1f;
        private float transparencyLevel = 235f / 255f;

        public static GridLayoutManager Instance { get; private set; }

        private GameObject[,] SpawnedGrid;
        private GridCellType[,] SpawnedGridType;

        public static GameObject[,] MakeNewGrid()
        {
            return new GameObject[Instance.WidthCount, Instance.HeighCount];
        }

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
            Obstacles = new List<GameObject>();

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

        public void HighlightCell(int x, int y, Color color)
        {
            color.a = transparencyLevel;
            SpawnedGrid[x, y].GetComponent<Renderer>().material.color = color;
        }

        public void ResetHighlights()
        {
            foreach (var cell in SpawnedGrid)
            {
                cell.GetComponent<Renderer>().material.color = new Color(1,1,1,1-transparencyLevel);
            }
        }

        public bool ContainsTile(GameObject gameObject)
        {
            return SpawnedGrid.Contains(gameObject);
        }

        public Point GetIndicesOfTile(GameObject tile)
        {
            return SpawnedGrid.GetIndices(tile);
        }

        public bool IsTileAttack(int x, int y)
        {
            return IsTileAttack(SpawnedGrid[x, y]);
        }

        private bool IsTileAttack(GameObject tile)
        {
            Color red = Color.red;
            red.a = transparencyLevel;
            return tile.GetComponent<Renderer>().material.color == red;
        }

        private bool IsTileMovement(GameObject tile)
        {
            Color blue = Color.blue;
            blue.a = transparencyLevel;
            return tile.GetComponent<Renderer>().material.color == blue;
        }

        private bool IsTentativeAttack(GameObject tile)
        {
            Color yellow = Color.yellow;
            yellow.a = transparencyLevel;
            return tile.GetComponent<Renderer>().material.color == yellow;
        }

        public List<GameObject> GetMovementTiles()
        {
            return FilterTiles(IsTileMovement);
        }

        private List<GameObject> FilterTiles(Predicate<GameObject> condition)
        {
            List<GameObject> result = new List<GameObject>();
            foreach (GameObject gameObject in SpawnedGrid)
            {
                if (condition(gameObject))
                {
                    result.Add(gameObject);
                }
            }
            return result;
        }

        public void MarkAsObstacle(int x, int y)
        {
            Obstacles.Add(SpawnedGrid[x, y]);
        }

        public List<GameObject> GetTentativeAttacks()
        {
            return FilterTiles(IsTentativeAttack);
        }


        public GameObject GetTile(int x, int y)
        {
            return SpawnedGrid[x, y];
        }
    }
}