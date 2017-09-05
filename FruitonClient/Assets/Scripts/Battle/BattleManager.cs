using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour {

    private ClientFruiton[,] grid;
    private GridLayoutManager gridLayoutManager;
    private GameManager gameManager;


    private void Start()
    {
        gridLayoutManager = GridLayoutManager.Instance;
        gameManager = GameManager.Instance;
        grid = new ClientFruiton[gridLayoutManager.HeighCount, gridLayoutManager.WidthCount];
        IEnumerable<ClientFruiton> currentTeam = ClientFruitonFactory.CreateClientFruitonTeam(gameManager.CurrentFruitonTeam.FruitonIDs);

        int majorCounter = 2;
        int minorCounter = 2;
        int i = 0, j = 0;
        foreach (ClientFruiton clientFruiton in currentTeam)
        {
            clientFruiton.FruitonObject.AddComponent<BoxCollider>();
            
            switch (clientFruiton.KernelFruiton.type)
            {
                case 1:
                {
                        i = gridLayoutManager.WidthCount / 2;
                        j = 0;
                } break;
                case 2:
                {
                        i = gridLayoutManager.WidthCount / 2 - majorCounter;
                        j = 0;
                        if (--majorCounter == 0) --majorCounter;
                }
                break;
                case 3:
                {
                        i = gridLayoutManager.WidthCount / 2 - minorCounter;
                        j = 1;
                        --minorCounter;
                }
                break;
            }
            grid[i, j] = clientFruiton;
            Vector3 cellPosition = gridLayoutManager.GetCellPosition(i, j);
            clientFruiton.FruitonObject.transform.position = cellPosition + new Vector3(0, clientFruiton.FruitonObject.transform.lossyScale.y, 0);

        }
    }
}
