using System.Collections;
using System.Collections.Generic;
using fruiton.kernel;
using fruiton.kernel.actions;
using UnityEngine;

public class TutorialPlayer : AIPlayerBase
{

    public enum State  { Passive, };

    public State CurrentState;

    public TutorialPlayer(BattleViewer battleViewer, Player kernelPlayer, Battle battle) : base(battleViewer, kernelPlayer, battle, "Dummy player")
    {
    }

    protected override void PerformNextAction()
    {
        switch (CurrentState)
        {
            case State.Passive: PassiveLogic();
                break;
        }
    }

    private void PassiveLogic()
    {
        PerformAction(EndTurnAction.createNew());
    }
}
