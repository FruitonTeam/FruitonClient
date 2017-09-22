using System;
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
        grid = new ClientFruiton[gridLayoutManager.WidthCount, gridLayoutManager.HeighCount];
        IEnumerable<ClientFruiton> currentTeam = ClientFruitonFactory.CreateClientFruitonTeam(gameManager.CurrentFruitonTeam.FruitonIDs);
        // TODO: This is just temporary offline solution. It is needed to obtain opponent team from server.
        IEnumerable<ClientFruiton> opponentTeam = ClientFruitonFactory.CreateClientFruitonTeam(gameManager.CurrentFruitonTeam.FruitonIDs);
        InitializeTeam(currentTeam, true);
        InitializeTeam(opponentTeam, false);
    }

    private void InitializeTeam(IEnumerable<ClientFruiton> currentTeam, bool down)
    {
        int majorRow = down ? 0 : gridLayoutManager.HeighCount - 1;
        int minorRow = down ? 1 : majorRow - 1;
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
                        j = majorRow;
                    }
                    break;
                case 2:
                    {
                        i = gridLayoutManager.WidthCount / 2 - majorCounter;
                        j = majorRow;
                        if (--majorCounter == 0) --majorCounter;
                    }
                    break;
                case 3:
                    {
                        i = gridLayoutManager.WidthCount / 2 - minorCounter;
                        j = minorRow;
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
