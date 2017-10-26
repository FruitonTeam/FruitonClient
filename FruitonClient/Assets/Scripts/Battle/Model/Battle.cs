using System;
using System.Collections.Generic;
using System.Linq;
using fruiton.kernel;
using fruiton.kernel.actions;
using UnityEngine;
using KEvent = fruiton.kernel.events.Event;
using KVector2 = fruiton.dataStructures.Point;
using KAction = fruiton.kernel.actions.Action;
using KFruiton = fruiton.kernel.Fruiton;

public class Battle
{
    protected GameManager gameManager;
    protected Kernel kernel;
    protected ClientPlayerBase player1;
    protected ClientPlayerBase player2;
    protected BattleViewer battleViewer;

    public Battle(BattleViewer battleViewer)
    {
        gameManager = GameManager.Instance;
        this.battleViewer = battleViewer;
    }

    private void PerformAction(KAction performedAction)
    {
        var events = kernel.performAction(performedAction).CastToList<KEvent>();
        foreach (var item in events)
            battleViewer.ProcessEvent(item);
    }

    public void PerformAction(KVector2 from, KVector2 to, int actionId)
    {
        if (actionId == EndTurnAction.ID)
        {
            var endTurnAction = EndTurnAction.createNew();
            GetWaitingPlayer().ProcessOpponentAction(endTurnAction);
            PerformAction(endTurnAction);
        }
        else if (actionId == MoveAction.ID)
        {
            var moveAction = GetTargetableAction<MoveAction>(from, to);
            GetWaitingPlayer().ProcessOpponentAction(moveAction);
            PerformAction(moveAction);
        }
        else if (actionId == AttackAction.ID)
        {
            var attackAction = GetTargetableAction<AttackAction>(from, to);
            GetWaitingPlayer().ProcessOpponentAction(attackAction);
            PerformAction(attackAction);
        }
    }

    private ClientPlayerBase GetWaitingPlayer()
    {
        if (IsPlayerActive(player1))
            return player2;
        return player1;
    }

    private TTargetableAction GetTargetableAction<TTargetableAction>(KVector2 from, KVector2 to)
        where TTargetableAction : KAction, TargetableAction
    {
        var allValidActionsFrom = kernel.getAllValidActionsFrom(from).CastToList<KAction>().OfType<TTargetableAction>();
        var performedAction = allValidActionsFrom.SingleOrDefault(x => x.getContext().target.equalsTo(to));
        return performedAction;
    }

    public int ComputeRemainingTime()
    {
        var currentEpochTime = (int) (DateTime.UtcNow - Constants.EPOCH_START).TotalSeconds;
        var timeLeft = (int) (kernel.currentState.turnState.endTime - currentEpochTime);
        if (timeLeft <= 0)
        {
            PerformAction(EndTurnAction.createNew());
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
            ((LocalPlayer) player1).EndTurn();
        else if (IsPlayerActive(player2) && player2 is LocalPlayer)
            ((LocalPlayer) player2).EndTurn();
    }

    private bool TryPassLeftButtonEvent(ClientPlayerBase player, RaycastHit hit)
    {
        var localPlayer = player as LocalPlayer;
        var pass = localPlayer != null && IsPlayerActive(player);
        if (pass)
            localPlayer.LeftButtonUpLogic(hit);
        return pass;
    }

    private bool IsPlayerActive(ClientPlayerBase player)
    {
        return player.ID == kernel.currentState.get_activePlayer().id;
    }

    public List<KAction> GetAllValidActionFrom(KVector2 position)
    {
        return kernel.getAllValidActionsFrom(position).CastToList<KAction>();
    }

    public List<KVector2> ComputePossibleAttacks(KVector2 potentialPosition, KFruiton kernelFruiton)
    {
        var possibleAttacks = new List<KVector2>();
        var attackGenerators = kernelFruiton.attackGenerators.CastToList<AttackGenerator>();
        foreach (var attackGenerator in attackGenerators)
        {
            var attacks = attackGenerator.getAttacks(potentialPosition, kernelFruiton.damage)
                .CastToList<AttackAction>();
            foreach (var attack in attacks)
            {
                var target = ((AttackActionContext) attack.actionContext).target;
                if (kernel.currentState.field.exists(target))
                {
                    var potentialTarget = kernel.currentState.field.get(target).fruiton;
                    if (potentialTarget != null &&
                        potentialTarget.owner.id != kernel.currentState.get_activePlayer().id)
                        possibleAttacks.Add(potentialTarget.position);
                }
            }
        }
        return possibleAttacks;
    }

    public KFruiton GetFruiton(KVector2 position)
    {
        return kernel.currentState.field.get(position).fruiton;
    }
}