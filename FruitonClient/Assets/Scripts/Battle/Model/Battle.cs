using System;
using System.Collections.Generic;
using System.Linq;
using Cz.Cuni.Mff.Fruiton.Dto;
using fruiton.kernel;
using fruiton.kernel.actions;
using fruiton.kernel.events;
using haxe.root;
using UnityEngine;
using KEvent = fruiton.kernel.events.Event;
using KVector2 = fruiton.dataStructures.Point;
using KAction = fruiton.kernel.actions.Action;
using KFruiton = fruiton.kernel.Fruiton;

public enum BattleType
{
    OnlineBattle,
    OfflineBattle,
    AIBattle,
    TutorialBattle
}

public abstract class Battle
{
    protected GameManager gameManager;
    protected Kernel kernel;
    protected BattleViewer battleViewer;
    protected Dictionary<int, ClientFruiton> clientFruitons;

    public ClientPlayerBase Player1 { get; protected set; }
    public ClientPlayerBase Player2 { get; protected set; }

    public ClientPlayerBase ActivePlayer
    {
        get
        {
            return IsPlayerActive(Player1) ? Player1 : Player2;
        }
    }

    public ClientPlayerBase WaitingPlayer
    {
        get
        {
            return IsPlayerActive(Player1) ? Player2 : Player1;
        }
    }

    protected Battle(BattleViewer battleViewer)
    {
        gameManager = GameManager.Instance;
        this.battleViewer = battleViewer;
    }

    /// <summary>
    /// Call this whenever kernel is initialized.
    /// </summary>
    protected void BattleReady()
    {
        battleViewer.InitializeMap(kernel.currentState.field.field.CastToList2D<Tile>());

        clientFruitons = new Dictionary<int, ClientFruiton>();
        foreach (GameObject fruitonObject in battleViewer.Grid)
        {
            ClientFruiton clientFruiton;
            if (fruitonObject != null &&
                (clientFruiton = fruitonObject.GetComponent<ClientFruiton>()) != null)
            {
                clientFruitons[clientFruiton.KernelFruiton.id] = clientFruiton;
            }
        }
        battleViewer.HighlightNameTags(IsPlayerActive(Player1));
    }

    private void PerformAction(KAction performedAction)
    {
        List<KEvent> events = kernel.performAction(performedAction).CastToList<KEvent>();

        for (int i = 0; i < kernel.currentState.fruitons.length; i++)
        {
            KFruiton fruiton = kernel.currentState.fruitons[i] as KFruiton;
            var clientFruiton = clientFruitons[fruiton.id];
            clientFruiton.KernelFruiton = fruiton;
        }

        foreach (var item in events)
        {
            battleViewer.ProcessEvent(item);
        }
            
    }

    public void PerformAction(KVector2 from, KVector2 to, int actionId)
    {
        if (actionId == EndTurnAction.ID)
        {
            var endTurnAction = EndTurnAction.createNew();
            WaitingPlayer.ProcessOpponentAction(endTurnAction);
            PerformAction(endTurnAction);
        }
        else if (actionId == MoveAction.ID)
        {
            var moveAction = GetTargetableAction<MoveAction>(from, to);
            WaitingPlayer.ProcessOpponentAction(moveAction);
            PerformAction(moveAction);
        }
        else if (actionId == AttackAction.ID)
        {
            var attackAction = GetTargetableAction<AttackAction>(from, to);
            WaitingPlayer.ProcessOpponentAction(attackAction);
            PerformAction(attackAction);
        }
        else if (actionId == HealAction.ID)
        {
            var healAction = GetTargetableAction<HealAction>(from, to);
            WaitingPlayer.ProcessOpponentAction(healAction);
            PerformAction(healAction);
        }
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
            if (ActivePlayer is LocalPlayer && battleViewer.battleType != BattleType.TutorialBattle)
                battleViewer.EndTurn();
            // Else wait for server to send us this event
            return 0;
        }
        return timeLeft;
    }

    public void LeftButtonUpEvent(RaycastHit[] hit)
    {
        TryPassLeftButtonEvent(Player1, hit);
        TryPassLeftButtonEvent(Player2, hit);
    }

    public void EndTurnEvent()
    {
        if (IsPlayerActive(Player1) && Player1 is LocalPlayer)
            ((LocalPlayer) Player1).EndTurn();
        else if (IsPlayerActive(Player2) && Player2 is LocalPlayer)
            ((LocalPlayer) Player2).EndTurn();
    }

    public virtual void SurrenderEvent()
    {
    }

    public virtual void CancelSearchEvent()
    {
    }

    private bool TryPassLeftButtonEvent(ClientPlayerBase player, RaycastHit[] hit)
    {
        var localPlayer = player as LocalPlayer;
        var pass = localPlayer != null && IsPlayerActive(player);
        if (pass)
            localPlayer.LeftButtonUpLogic(hit);
        return pass;
    }

    public bool IsPlayerActive(ClientPlayerBase player)
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
            var attacks = attackGenerator.getAttacks(potentialPosition, kernelFruiton.currentAttributes.damage)
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

    public virtual void OnEnable()
    {
    }

    public virtual void OnDisable()
    {
    }

    public virtual void Update()
    {
    }

    public Kernel GetKernelClone()
    {
        return kernel.clone();
    }
}