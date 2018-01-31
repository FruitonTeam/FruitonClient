using fruiton.kernel;
using fruiton.kernel.actions;
using UnityEngine;
using KAction = fruiton.kernel.actions.Action;

abstract class AIPlayerBase : ClientPlayerBase
{
    protected readonly BattleViewer battleViewer;

    protected AIPlayerBase(BattleViewer battleViewer, Player kernelPlayer, Battle battle, string name) 
        : base(kernelPlayer, battle, name)
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
            PerformNextAction();
        }
    }

    protected abstract void PerformNextAction();

    protected void PerformAction(KAction action)
    {
        Debug.Log(Name + ": " + action.toString());

        var targetableAction = action as TargetableAction;
        if (targetableAction != null)
        {
            battle.PerformAction(targetableAction.getContext().source, targetableAction.getContext().target, action.getId());
        }
        else
        {
            battle.PerformAction(null, null, action.getId());
        }
    }
}
