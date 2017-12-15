using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cz.Cuni.Mff.Fruiton.Dto;
using fruiton.dataStructures;
using fruiton.kernel;
using fruiton.kernel.actions;
using Google.Protobuf.WellKnownTypes;
using UnityEngine;
using Action = fruiton.kernel.actions.Action;
using KAction = fruiton.kernel.actions.Action;
using Type = System.Type;

public class LocalPlayer : ClientPlayerBase
{
    // Key = Action type, Value = List of currently available action of that type to perform.
    private Dictionary<Type, List<TargetableAction>> availableActions;
    private readonly BattleViewer battleViewer;
    private readonly GridLayoutManager gridLayoutManager;

    public LocalPlayer(BattleViewer battleViewer, Player kernelPlayer, Battle battle, string login) 
        : base(kernelPlayer, battle, login)
    {
        this.battleViewer = battleViewer;
        gridLayoutManager = GridLayoutManager.Instance;
        availableActions = new Dictionary<Type, List<TargetableAction>>();
        
        var targetableActionType = typeof(TargetableAction);
        Assembly assembly = Assembly.GetExecutingAssembly();
        // Find all types that implement TargetableAction interface. !p.ContainsGenericParameters is ther to avoid TargetableAction<TContext> type.
        IEnumerable<Type> types = assembly.GetTypes().Where(p => p.GetInterfaces().Contains(targetableActionType) && !p.ContainsGenericParameters);
        foreach (Type type in types)
        {
            availableActions[type] = new List<TargetableAction>();
        }
    }

    public void LeftButtonUpLogic(RaycastHit[] hits)
    {
        if (hits.Length == 0)
        {
            ClearAllAvailableActions();
        }
        gridLayoutManager.ResetHighlights();
        Func<RaycastHit, bool> isHitGridTile =
            hit => battleViewer.GridLayoutManager.ContainsTile(hit.transform.gameObject);
        // We are only interrested in clicks on the tiles.
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
            performedAnyAction |= TryFindAndPerformAction(typeof(AttackAction), tilePosition);
            performedAnyAction |= TryFindAndPerformAction(typeof(HealAction), tilePosition);
            // Check if I clicked on my fruiton in order to take action on him or only to select him.
            if (!performedAnyAction && hitFruiton.GetComponent<ClientFruiton>().KernelFruiton.owner.id == ID)
            {
                battleViewer.GridLayoutManager.HighlightCell(tilePosition.x, tilePosition.y, Color.magenta);
            }
            for (int i = 0; i < availableActions.Keys.Count; i++)
            {
                Type currentType = availableActions.Keys.ElementAt(i);
                availableActions[currentType] = battleViewer.VisualizeActionsOfType(tilePosition, currentType);
            }
        }
        // A tile without fruiton was clicked.
        else
        {
            if (!TryFindAndPerformAction(typeof(MoveAction), tilePosition))
            {
                ClearAllAvailableActions();
            }
        }
    }

    private bool TryFindAndPerformAction(Type type, Point tilePosition)
    {
        List<TargetableAction> performedActions = availableActions[type].FindAll(x =>
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
        ClearAllAvailableActions();
        gridLayoutManager.ResetHighlights();
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