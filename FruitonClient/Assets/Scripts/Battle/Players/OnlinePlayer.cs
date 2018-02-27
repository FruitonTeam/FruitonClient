using Cz.Cuni.Mff.Fruiton.Dto;
using Extensions;
using fruiton.dataStructures;
using fruiton.kernel;
using fruiton.kernel.actions;
using Networking;
using UnityEngine.Assertions;
using Action = fruiton.kernel.actions.Action;
using ProtoAction = Cz.Cuni.Mff.Fruiton.Dto.Action;

namespace Battle.Players
{
    public class OnlinePlayer : ClientPlayerBase, IOnMessageListener
    {
        public OnlinePlayer(Player kernelPlayer, Model.Battle battle, string name) 
            : base(kernelPlayer, battle, name)
        {
        }

        public void OnMessage(WrapperMessage message)
        {
            // Online opponent should only receive Action messages.
            Assert.AreEqual(message.MessageCase, WrapperMessage.MessageOneofCase.Action);
            ProcessMessage(message.Action);
        }

        private void ProcessMessage(ProtoAction protoAction)
        {
            var from = new Point(-1, -1);
            var to = new Point(-1, -1);

            if (protoAction.From != null)
                from = protoAction.From.ToKernelPosition();
            if (protoAction.To != null)
                to = protoAction.To.ToKernelPosition();

            battle.PerformAction(from, to, protoAction.Id);
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

        public void OnEnable()
        {
            if (ConnectionHandler.Instance.IsLogged())
            {
                ConnectionHandler.Instance.RegisterListener(WrapperMessage.MessageOneofCase.Action, this);
            }
        }

        public void OnDisable()
        {
            if (ConnectionHandler.Instance.IsLogged())
            {
                ConnectionHandler.Instance.UnregisterListener(WrapperMessage.MessageOneofCase.Action, this);
            }
        }
    }
}