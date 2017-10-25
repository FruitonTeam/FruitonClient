using fruiton.kernel;
using fruiton.kernel.actions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using KEvent = fruiton.kernel.events.Event;
using KVector2 = fruiton.dataStructures.Point;
using KAction = fruiton.kernel.actions.Action;
using KFruiton = fruiton.kernel.Fruiton;

public class Battle
{
    protected ClientPlayerBase player1;
    protected ClientPlayerBase player2;
    protected Kernel kernel;
    protected BattleViewer BattleViewer { get; set; }
    protected GameManager gameManager;

    private readonly DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public Battle(BattleViewer battleViewer)
    {
        gameManager = GameManager.Instance;
        BattleViewer = battleViewer;
    }

    private void PerformAction(KAction performedAction)
    {
        var events = kernel.performAction(performedAction).CastToList<KEvent>();
        foreach (var item in events)
        {
            BattleViewer.ProcessEvent(item);
        }
    }

    public void PerformAction(KVector2 from, KVector2 to, int actionId)
    {
        if (actionId == EndTurnAction.ID)
        {
            var endTurnAction = new EndTurnAction(new EndTurnActionContext());
            GetWaitingPlayer().ProcessOpponentAction(endTurnAction);
            PerformAction(endTurnAction);
        }
        else if (actionId == MoveAction.ID)
        {
            MoveAction moveAction =  GetTargetableAction<MoveAction>(from, to);
            PerformAction(moveAction);
            GetWaitingPlayer().ProcessOpponentAction(moveAction);
        } 
        else if (actionId == AttackAction.ID)
        {
            AttackAction attackAction = GetTargetableAction<AttackAction>(from, to);
            PerformAction(attackAction);
            GetWaitingPlayer().ProcessOpponentAction(attackAction);
        }
        
    }

    private ClientPlayerBase GetWaitingPlayer()
    {
        if (IsPlayerActive(player1))
        {
            return player2;
        }
        return player1;
    }

    private TTargetableAction GetTargetableAction<TTargetableAction>(KVector2 from, KVector2 to) where TTargetableAction : KAction, TargetableAction
    {
        IEnumerable<TTargetableAction> allValidActionsFrom = kernel.getAllValidActionsFrom(from).CastToList<fruiton.kernel.actions.Action>().OfType<TTargetableAction>();
        TTargetableAction performedAction = allValidActionsFrom.SingleOrDefault(x => (x.getContext()).target.equalsTo(to));
        return performedAction;
    }

    public int ComputeRemainingTime()
    {
        int currentEpochTime = (int)(DateTime.UtcNow - epochStart).TotalSeconds;
        int timeLeft = (int)(kernel.currentState.turnState.endTime - currentEpochTime);
        if (timeLeft <= 0)
        {
            PerformAction(new EndTurnAction(new EndTurnActionContext()));
            return 0;
        }
        return timeLeft;
    }

    public void LeftButtonUpEvent(RaycastHit hit)
    {
        TryPassLeftButtonEvent(player1, hit);
        TryPassLeftButtonEvent(player2, hit);
    }

    public void EndTurnEvent()
    {
        if (IsPlayerActive(player1) && player1 is LocalPlayer)
        {
            ((LocalPlayer)player1).EndTurn();
        } 
        else if (IsPlayerActive(player2) && player2 is LocalPlayer)
        {
            ((LocalPlayer)player2).EndTurn();
        }
    }

    private void TryPassLeftButtonEvent(ClientPlayerBase player, RaycastHit hit)
    {
        if (IsPlayerActive(player) && player is LocalPlayer)
        {
            ((LocalPlayer)player).LeftButtonUpLogic(hit);
        }
    }

    private bool IsPlayerActive(ClientPlayerBase player)
    {
        return (player.KernelPlayer.id == kernel.currentState.get_activePlayer().id);
    }

    public List<KAction> GetAllValidActionFrom(KVector2 indices)
    {
        return kernel.getAllValidActionsFrom(indices).CastToList<KAction>();
    }

    public List<KVector2> ComputePossibleAttacks(KVector2 potentialPosition, KFruiton kernelFruiton)
    {
        List<KVector2> possibleAttacks = new List<KVector2>();
        var attackGenerators = kernelFruiton.attackGenerators.CastToList<AttackGenerator>();
        foreach (var attackGenerator in attackGenerators)
        {
            var attacks = attackGenerator.getAttacks(potentialPosition, kernelFruiton.damage).CastToList<AttackAction>();
            foreach (var attack in attacks)
            {
                var target = ((AttackActionContext)attack.actionContext).target;
                if (kernel.currentState.field.exists(target))
                {
                    var potentialTarget = kernel.currentState.field.get(target).fruiton;
                    if (potentialTarget != null && potentialTarget.owner.id != kernel.currentState.get_activePlayer().id)
                    {
                        possibleAttacks.Add(potentialTarget.position);
                    }
                }
            }
        }
        return possibleAttacks;
    }

    public KFruiton GetFruiton(KVector2 indices)
    {
        return kernel.currentState.field.get(indices).fruiton; 
    }
}
