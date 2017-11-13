using Cz.Cuni.Mff.Fruiton.Dto;
using fruiton.kernel;
using fruiton.kernel.actions;
using Networking;
using UnityEngine.Assertions;
using Action = fruiton.kernel.actions.Action;
using ProtoAction = Cz.Cuni.Mff.Fruiton.Dto.Action;

public class OnlinePlayer : ClientPlayerBase, IOnMessageListener
{
    public OnlinePlayer(Player kernelPlayer, Battle battle, string name) 
        : base(kernelPlayer, battle, name)
    {
        if (ConnectionHandler.Instance.IsLogged())
            ConnectionHandler.Instance.RegisterListener(WrapperMessage.MessageOneofCase.Action, this);
    }


    public void OnMessage(WrapperMessage message)
    {
        // Online opponent should only receive Action messages.
        Assert.AreEqual(message.MessageCase, WrapperMessage.MessageOneofCase.Action);
        ProcessMessage(message.Action);
    }

    private void ProcessMessage(ProtoAction protoAction)
    {
        battle.PerformAction(protoAction.From.ToKernelPosition(), protoAction.To.ToKernelPosition(), protoAction.Id);
    }

    public override void ProcessOpponentAction(EndTurnAction action)
    {
        var position = new Position {X = -1, Y = -1};
        var actionMessage = new ProtoAction {From = position, To = position, Id = action.getId()};
        var wrapperMessage = new WrapperMessage {Action = actionMessage};
        ConnectionHandler.Instance.SendWebsocketMessage(wrapperMessage);
    }

    public override void ProcessOpponentAction(TargetableAction action)
    {
        var actionMessage = new ProtoAction
        {
            From = action.getContext().source.ToPosition(),
            To = action.getContext().target.ToPosition(),
            Id = ((Action) action).getId()
        };
        var wrapperMessage = new WrapperMessage {Action = actionMessage};
        ConnectionHandler.Instance.SendWebsocketMessage(wrapperMessage);
    }
}