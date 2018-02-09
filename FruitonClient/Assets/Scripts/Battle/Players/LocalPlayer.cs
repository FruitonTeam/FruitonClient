using fruiton.kernel.actions;
using System;
using System.Collections.Generic;
using System.Linq;
using fruiton.dataStructures;
using fruiton.kernel;
using UnityEngine;
using Action = fruiton.kernel.actions.Action;

public class LocalPlayer : ClientPlayerBase
{
    // Key = Action type, Value = List of currently available action of that type to perform.
    private LazyDictionary<int, List<TargetableAction>> availableActions;
    private readonly BattleViewer battleViewer;
    private readonly GridLayoutManager gridLayoutManager;
    private bool movedThisTurn;

    public LocalPlayer(BattleViewer battleViewer, Player kernelPlayer, Battle battle, string name) 
        : base(kernelPlayer, battle, name)
    {
        this.battleViewer = battleViewer;
        gridLayoutManager = GridLayoutManager.Instance;
        availableActions = new LazyDictionary<int, List<TargetableAction>>();
    }

    public void LeftButtonUpLogic(RaycastHit[] hits)
    {
        if (!movedThisTurn)
        {
            if (hits.Length == 0)
            {
                ClearAllAvailableActions();
            }
            gridLayoutManager.ResetHighlights();
        }
            
        Func<RaycastHit, bool> isHitGridTile =
            hit => battleViewer.GridLayoutManager.ContainsTile(hit.transform.gameObject);
        // We are only interested in clicks on the tiles.
        if (!hits.Any(isHitGridTile)) return;
        
        GameObject hitTile = hits.FirstOrDefault(isHitGridTile).transform.gameObject;
        if (hitTile.Equals(default(GameObject))) return;
        Point tilePosition = battleViewer.GridLayoutManager.GetIndicesOfTile(hitTile);
        GameObject hitFruiton = battleViewer.Grid[tilePosition.x, tilePosition.y];
        
        // A tile with a fruiton was clicked.
        if(hitFruiton != null)
        {
            bool performedAnyAction = false;
            // Find the action where the target is the clicked fruiton and perform it (if such action exists).
            performedAnyAction |= TryFindAndPerformAction(AttackAction.ID, tilePosition);
            performedAnyAction |= TryFindAndPerformAction(HealAction.ID, tilePosition);
            if (performedAnyAction)
            {
                movedThisTurn = false;
                gridLayoutManager.ResetHighlights();
            }
            if (!movedThisTurn)
            {
                // Check if I clicked on my fruiton in order to take action on him or only to select him.
                if (!performedAnyAction && hitFruiton.GetComponent<ClientFruiton>().KernelFruiton.owner.id == ID)
                {
                    battleViewer.GridLayoutManager.HighlightCell(tilePosition.x, tilePosition.y, Color.magenta);
                }
                availableActions = battleViewer.VisualizeAvailableTargetableActions(tilePosition);
            }

        }
        // A tile without fruiton was clicked.
        else
        {
            if (!TryFindAndPerformAction(MoveAction.ID, tilePosition))
            {
                if (!movedThisTurn) ClearAllAvailableActions();
            }
            else
            {
                availableActions = battleViewer.VisualizeAvailableTargetableActions(tilePosition);
                battleViewer.GridLayoutManager.HighlightCell(tilePosition.x, tilePosition.y, Color.magenta);
                movedThisTurn = true;
            }
        }
    }

    private bool TryFindAndPerformAction(int actionType, Point tilePosition)
    {
        List<TargetableAction> performedActions = availableActions[actionType].FindAll(x =>
            x.getContext().target.equalsTo(tilePosition));
        bool success = performedActions.Count != 0;
        if (success)
        {
            var performedAction = performedActions[0];
            battle.PerformAction(performedAction.getContext().source, performedAction.getContext().target,
                ((Action)performedAction).getId());
        }
        return success;
    }

    private void ClearAllAvailableActions()
    {
        foreach (List<TargetableAction> actions in availableActions.Values)
        {
            actions.Clear();
        }
    }

    public void EndTurn()
    {
        movedThisTurn = false;
        ClearAllAvailableActions();
        battle.PerformAction(null, null, EndTurnAction.ID);
    }

    public override void ProcessOpponentAction(EndTurnAction action)
    {
        battleViewer.EnableEndTurnButton();
    }

    public override void ProcessOpponentAction(TargetableAction action)
    {
    }
}