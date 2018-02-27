using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Battle.View;
using Extensions;
using fruiton.kernel;
using fruiton.kernel.actions;
using KAction = fruiton.kernel.actions.Action;
using UDebug = UnityEngine.Debug;

namespace Battle.Players.AI
{
    class RandomAIPlayer : AIPlayerBase
    {
        private readonly Random rnd = new Random();

        public RandomAIPlayer(BattleViewer battleViewer, Player kernelPlayer, Model.Battle battle)
            : base(battleViewer, kernelPlayer, battle, "Random AI")
        {
        }

        protected override void PerformNextAction()
        {
            Kernel kernel = battle.GetKernelClone();
            List<KAction> actions = kernel.getAllValidActions().CastToList<KAction>();
            Debug.Assert(actions.Count > 0);
            if (actions.Count == 1)
            {
                Debug.Assert(actions[0].GetType() == typeof(EndTurnAction));
                battle.PerformAction(null, null, EndTurnAction.ID);
            }
            else
            {
                var nonEndTurnActions = actions.Where(x => x.GetType() != typeof(EndTurnAction)).ToList();
                int rndIdx = rnd.Next(nonEndTurnActions.Count);
                KAction randomAction = nonEndTurnActions[rndIdx];
                UDebug.Log(Name + ": " + randomAction.toString());
                var randomTargetable = randomAction as TargetableAction;
                if (randomTargetable != null)
                {
                    battle.PerformAction(randomTargetable.getContext().source, randomTargetable.getContext().target, randomAction.getId());
                }
                else
                {
                    battle.PerformAction(null, null, randomAction.getId());
                }
            }
        }
    }
}
