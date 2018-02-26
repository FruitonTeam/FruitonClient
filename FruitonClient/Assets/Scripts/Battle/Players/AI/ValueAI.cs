using fruiton.dataStructures;
using fruiton.kernel;
using fruiton.kernel.actions;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using UnityEngine;

public class ValueAI : AIPlayerBase
{
    private bool startedComputation;

    public ValueAI(BattleViewer battleViewer, Player kernelPlayer, Battle battle, string name)
        : base(battleViewer, kernelPlayer, battle, name)
    {
    }

    protected override void PerformNextAction()
    {
        if (!startedComputation)
        {
            startedComputation = true;
            Thread aiThread = new Thread(PerformNextActionImpl);
            aiThread.Start();
        }
    }

    private void PerformNextActionImpl()
    {
        Tuple<int, Action> bestAction = ShallowMinimax(battle.Kernel.clone(), null);
        TaskManager.Instance.RunOnMainThread(() => { startedComputation = false; PerformAction(bestAction.Second); });
    }

    private Tuple<int, Action> ShallowMinimax(Kernel kernel, Action lastAction)
    {
        UnityEngine.Debug.Log("action = " + lastAction);
        if (kernel.currentState.get_activePlayer().id != ID)
        {
            int rating = ComputeGameStateValue(kernel.currentState);
            UnityEngine.Debug.Log("action: " + lastAction + " rating: " + rating);
            return new Tuple<int, Action>(rating, null);
        }

        List<Action> availableActions;
        if (lastAction != null)
        {
            if (lastAction.getId() == MoveAction.ID)
            {
                availableActions = kernel.getAllValidActionsFrom(((MoveAction)lastAction).getContext().target).CastToList<Action>();
            }
            else
            {
                int rating = ComputeGameStateValue(kernel.currentState);
                UnityEngine.Debug.Log("action: " + lastAction + " rating: " + rating);
                return new Tuple<int, Action>(rating, EndTurnAction.createNew());
            }
        }
        else
        {
            availableActions = kernel.getAllValidActions().CastToList<Action>();
        }

        SortedDictionary<int, Action> actionsRatings = new SortedDictionary<int, Action>();
        foreach (Action availableAction in availableActions)
        {
            Kernel kernelCopy = kernel.clone();
            kernelCopy.performAction(availableAction);
            Tuple<int, Action> lowerLevelResult = ShallowMinimax(kernelCopy, availableAction);
            actionsRatings[lowerLevelResult.First] = availableAction;
        }
        var result = new Tuple<int, Action>(actionsRatings.Keys.Last(), actionsRatings.Values.Last());
        return result;
    }

    private int ComputeGameStateValue(GameState state)
    {
        if (state.losers.ToList().Contains(ID))
        {
            return int.MinValue;
        }
        else if (state.losers.ToList().Contains(battle.WaitingPlayer.ID))
        {
            return int.MaxValue;
        }
        int rating = 0;
        List<Fruiton> fruitons = state.fruitons.CastToList<Fruiton>();
        IEnumerable<Fruiton> kings = fruitons.Where(fruiton => fruiton.type == (int)FruitonType.KING);
        Fruiton aiKing = kings.First(fruiton => fruiton.owner.id == ID);
        Fruiton playerKing = kings.First(fruiton => fruiton.owner.id != ID);
        foreach (Fruiton fruiton in fruitons)
        {
            
            bool isFruitonAi = fruiton.owner.id == ID;
            int sign = isFruitonAi ? 1 : -1;
            Point enemyKingPosition = isFruitonAi ? playerKing.position : aiKing.position;
            int xDiff = fruiton.position.x - enemyKingPosition.x;
            int yDiff = fruiton.position.y - enemyKingPosition.y;
            int distanceToEnemyKingSquared = xDiff * xDiff + yDiff * yDiff;
            rating += sign * (10000 * (fruiton.currentAttributes.hp + fruiton.currentAttributes.damage) - distanceToEnemyKingSquared);
        }
        return rating;
    }
}
