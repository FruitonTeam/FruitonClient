﻿using System.Collections;
using System.Collections.Generic;
using fruiton.dataStructures;
using fruiton.kernel;
using fruiton.kernel.actions;
using UnityEngine;

public class TutorialPlayer : AIPlayerBase
{

    private bool active = false;

    public TutorialPlayer(BattleViewer battleViewer, Player kernelPlayer, Battle battle) : base(battleViewer, kernelPlayer, battle, "Dummy player")
    {
    }

    protected override void PerformNextAction()
    {
        Debug.Log("TUTORIAL PLAYER: Perform next action");
        if (active)
        {
            ActiveLogic();
        }
        else
        {
            PassiveLogic();
        }
    }

    private void ActiveLogic()
    {
        List<Action> allValidActionFrom = battle.GetAllValidActionFrom(new Point(5, 8));
        PerformAction(allValidActionFrom.Find(action => action.getId() == AttackAction.ID));
        active = false;
    }

    private void PassiveLogic()
    {
        PerformAction(EndTurnAction.createNew());
    }

    public void MakeMove()
    {
        active = true;
    }
}
