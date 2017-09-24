using fruiton.kernel;
using fruiton.kernel.actions;
using fruiton.kernel.targetPatterns;
using System.Collections.Generic;
using UnityEngine;

using Action = fruiton.kernel.actions.Action;
using Event = fruiton.kernel.events.Event;
using KEvent = fruiton.kernel.events.Event;
using Field = fruiton.kernel.Field;
using KFruiton = fruiton.kernel.Fruiton;
using KAction = fruiton.kernel.actions.Action;
using KVector2 = fruiton.dataStructures.Point;


public class BattleManager : MonoBehaviour {

    private GameObject[,] grid;
    private GridLayoutManager gridLayoutManager;
    private GameManager gameManager;
    private Player me, opponent;
    private GameState gameState;
    private Kernel kernel;

    private void Start()
    {
      
        me = new Player(0);
        opponent = new Player(1);
        gridLayoutManager = GridLayoutManager.Instance;
        gameManager = GameManager.Instance;
        grid = new GameObject[gridLayoutManager.WidthCount, gridLayoutManager.HeighCount];
        IEnumerable<GameObject> currentTeam = ClientFruitonFactory.CreateClientFruitonTeam(gameManager.CurrentFruitonTeam.FruitonIDs);
        // TODO: This is just temporary offline solution. It is needed to obtain opponent team from server.
        IEnumerable<GameObject> opponentTeam = ClientFruitonFactory.CreateClientFruitonTeam(gameManager.CurrentFruitonTeam.FruitonIDs);
        var fruitons = new Array<object>();
        InitializeTeam(currentTeam, me);
        InitializeTeam(opponentTeam, opponent);

        foreach (var fruiton in currentTeam)
        {
            fruitons.push(fruiton.GetComponent<ClientFruiton>().KernelFruiton);
        }
        foreach (var fruiton in opponentTeam)
        {
            fruitons.push(fruiton.GetComponent<ClientFruiton>().KernelFruiton);
        }



        //var kingMovement = new Array<object>();
        //kingMovement.push(new MoveGenerator(new RangeTargetPattern(null, 0, 1)));
        //var rookMovement = new Array<object>();
        //rookMovement.push(new MoveGenerator(new LineTargetPattern(new KVector2(0, 1), 0, 16)));
        //rookMovement.push(new MoveGenerator(new LineTargetPattern(new KVector2(0, -1), 0, 16)));
        //rookMovement.push(new MoveGenerator(new LineTargetPattern(new KVector2(1, 0), 0, 16)));
        //rookMovement.push(new MoveGenerator(new LineTargetPattern(new KVector2(-1, 0), 0, 16)));
        //var attacks = new Array<object>();
        //attacks.push(new AttackGenerator(new RangeTargetPattern(new KVector2(0, 0), 0, 1), 6));

        //var fruitons = new Array<object>();
        //// P1
        //fruitons.push(new KFruiton(1, new KVector2(0, 1), me, 10, "", kingMovement, attacks, 1));
        //fruitons.push(new KFruiton(2, new KVector2(1, 1), me, 10, "", kingMovement, attacks, 1));
        //fruitons.push(new KFruiton(3, new KVector2(0, 2), me, 10, "", kingMovement, attacks, 1));
        //fruitons.push(new KFruiton(4, new KVector2(3, 5), me, 10, "", kingMovement, attacks, 1));
        //fruitons.push(new KFruiton(5, new KVector2(0, 5), me, 10, "", rookMovement, attacks, 1));
        //// P2
        //fruitons.push(new KFruiton(6, new KVector2(0, 7), opponent, 10, "", rookMovement, attacks, 1));
        //fruitons.push(new KFruiton(7, new KVector2(2, 3), opponent, 10, "", kingMovement, attacks, 1));
        kernel = new Kernel(me, opponent, fruitons);
    }

    private void InitializeTeam(IEnumerable<GameObject> currentTeam, Player player)
    {
        int majorRow = player.id == 0 ? 0 : gridLayoutManager.HeighCount - 1;
        int minorRow = player.id == 0 ? 1 : majorRow - 1;
        int majorCounter = 2;
        int minorCounter = 2;
        int i = 0, j = 0;
        foreach (GameObject clientFruiton in currentTeam)
        {
            var kernelFruiton = clientFruiton.GetComponent<ClientFruiton>().KernelFruiton;
            kernelFruiton.owner = player;
            
            clientFruiton.gameObject.AddComponent<BoxCollider>();

            switch (kernelFruiton.type)
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
            kernelFruiton.position = new fruiton.dataStructures.Point(j, i);
            Vector3 cellPosition = gridLayoutManager.GetCellPosition(i, j);
            clientFruiton.transform.position = cellPosition + new Vector3(0, clientFruiton.transform.lossyScale.y, 0);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            LeftButtonUpLogic();
        }
    }

    private void LeftButtonUpLogic()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject HitObject = hit.transform.gameObject;
            var indices = grid.GetIndices(HitObject);
            var kernelFruiton = HitObject.GetComponent<ClientFruiton>().KernelFruiton;
            var actions = kernel.getAllValidActionsFrom(kernelFruiton.position);
            gridLayoutManager.ResetHighlights();
            foreach (Action action in actions.ToList())
            {
                if (action != null)
                {
                    VisualizeAction(action);
                }
                    
            }
        }
    }

    private void VisualizeAction(Action action)
    {
        if (action is MoveAction)
        {
            var moveAction = (MoveAction)action;
            var target = ((MoveActionContext)(moveAction.actionContext)).target;
            Debug.Log("X: " + target.x + " Y: " + target.y);
            gridLayoutManager.HighlightCell(target.y, target.x);
        }
    }
}
