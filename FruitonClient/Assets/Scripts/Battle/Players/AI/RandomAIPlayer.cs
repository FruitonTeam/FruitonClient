using System;
using System.Diagnostics;
using System.Linq;
using System.Collections.Generic;
using fruiton.kernel;
using fruiton.kernel.actions;
using KAction = fruiton.kernel.actions.Action;
using UDebug = UnityEngine.Debug;

class RandomAIPlayer : ClientPlayerBase
{
    private readonly Random rnd = new Random();
    private readonly BattleViewer battleViewer;

    public RandomAIPlayer(BattleViewer battleViewer, Player kernelPlayer, Battle battle, string login) 
        : base(kernelPlayer, battle, login)
    {
        this.battleViewer = battleViewer;
    }

    public override void ProcessOpponentAction(EndTurnAction action)
    {
    }

    public override void ProcessOpponentAction(TargetableAction action)
    {
    }

    public void Update()
    {
        if (battle.ActivePlayer.ID == ID && battleViewer.IsInputEnabled)
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
                UDebug.Log("RandomAI action: " + randomAction.toString());
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
