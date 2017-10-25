using Networking;
using System;
using Cz.Cuni.Mff.Fruiton.Dto;
using ProtoAction = Cz.Cuni.Mff.Fruiton.Dto.Action;
using KVector2 = fruiton.dataStructures.Point;
using Action = fruiton.kernel.actions.Action;
using fruiton.kernel.actions;
using fruiton.kernel;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System.Linq;

public class OnlineOpponent : ClientPlayerBase, IOnMessageListener
{
    private Action nextAction;

    public OnlineOpponent(Battle battle) : base(battle)
    {
    }

    void OnEnable()
    {
        if (ConnectionHandler.Instance.IsLogged())
        {
            ConnectionHandler.Instance.RegisterListener(WrapperMessage.MessageOneofCase.Action, this);
        }
    }

    void OnDisable()
    {
        if (ConnectionHandler.Instance.IsLogged())
        {
            ConnectionHandler.Instance.UnregisterListener(WrapperMessage.MessageOneofCase.Action, this);
        }
    }

    public void OnMessage(WrapperMessage message)
    {
        // Online opponent should only receive Action messages.
        Assert.AreEqual(message.MessageCase, WrapperMessage.MessageOneofCase.Action);
        ProcessMessage(message.Action);
    }

    private void ProcessMessage(ProtoAction protoAction)
    {
        if (protoAction.Id == EndTurnAction.ID)
        {
            battle.PerformAction(new EndTurnAction(new EndTurnActionContext()));
        }
        else if (protoAction.Id == MoveAction.ID || protoAction.Id == AttackAction.ID)
        {
            battle.PerformAction(GetTargetableAction<MoveAction>(protoAction));

        }
    }

    private TTargetableAction GetTargetableAction<TTargetableAction>(ProtoAction protoAction) where TTargetableAction : Action, TargetableAction
    {
        KVector2 from = protoAction.From.ToKernelPosition();
        KVector2 to = protoAction.To.ToKernelPosition();
        IEnumerable<TTargetableAction> allValidActionsFrom = kernel.getAllValidActionsFrom(from).CastToList<fruiton.kernel.actions.Action>().OfType<TTargetableAction>();
        TTargetableAction performedAction = allValidActionsFrom.SingleOrDefault(x => (x.getContext()).target.equalsTo(to));
        return performedAction;
    }

    public override void ProcessOpponentAction(EndTurnAction action)
    {
        var actionMessage = new ProtoAction { From = null, To = null, Id = action.getId() };
        var wrapperMessage = new WrapperMessage { Action = actionMessage };
        ConnectionHandler.Instance.SendWebsocketMessage(wrapperMessage);
    }
}
