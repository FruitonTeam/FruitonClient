using System;
using System.Collections.Generic;
using System.Linq;
using fruiton.dataStructures;
using fruiton.dataStructures._Vector2;
using fruiton.kernel;
using fruiton.kernel.actions;
using Util;
using Debug = System.Diagnostics.Debug;
using KAction = fruiton.kernel.actions.Action;
using Random = System.Random;

class AggroGreedyAIPlayer : AIPlayerBase
{
    private readonly Random rnd = new Random();
    private readonly double PERFORM_ATTACK_PROBA = 0.95;
    private readonly double PERFORM_HEAL_PROBA = 0.8;

    private readonly double RANDOM_ATTACK_PROBA = 0.05;
    private readonly double RANDOM_HEAL_PROBA = 0.05;
    private readonly double RANDOM_MOVE_PROBA = 0.20;

    private delegate bool IsBetterAction<in TAction>(TAction newAction, TAction previousAction, Kernel kernel);

    public AggroGreedyAIPlayer(BattleViewer battleViewer, Player kernelPlayer, Battle battle) 
        : base(battleViewer, kernelPlayer, battle, "Aggro greedy")
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
            PerformAction(actions[0]);
        }
        else
        {
            List<KAction> nonEndTurnActions = actions.Where(x => x.GetType() != typeof(EndTurnAction)).ToList();
            PlayBestAction(nonEndTurnActions, kernel);
        }
    }

    private void PlayBestAction(IList<KAction> nonEndTurnActions, Kernel kernel)
    {
        List<AttackAction> attacks = nonEndTurnActions.OfType<AttackAction>().ToList();
        List<MoveAction> moves = nonEndTurnActions.OfType<MoveAction>().ToList();
        List<HealAction> heals = nonEndTurnActions.OfType<HealAction>().ToList();
        bool performAttack = rnd.NextDouble() < PERFORM_ATTACK_PROBA;
        bool performHeal = rnd.NextDouble() < PERFORM_HEAL_PROBA;
        
        // Skip attack if there are no attacks or rnd says so and there are other actions to do
        if (attacks.Count > 0 && (performAttack || (moves.Count == 0 && heals.Count == 0)))
        {
            PlayBest(attacks, RANDOM_ATTACK_PROBA, IsBetterAttack, kernel);
        }
        // Skip heal if there are no heals or rnd says so and there are moves to do
        else if (heals.Count > 0 && (performHeal || moves.Count == 0))
        {
            PlayBest(heals, RANDOM_HEAL_PROBA, IsBetterHeal, kernel);
        }
        else if (moves.Count > 0)
        {
            PlayBestMove(moves, kernel);
        }
        else
        {
            PlayRandomAction(nonEndTurnActions);
        }
    }

    private void PlayBest<TAction>(
        IList<TAction> actions, 
        double randomProba,
        IsBetterAction<TAction> isBetterAction,
        Kernel kernel
    )
        where TAction : KAction
    {
        if (!TryPlayRandom(actions, randomProba))
        {
            TAction bestAction = null;
            foreach (TAction action in actions)
            {
                if (bestAction == null
                    || isBetterAction(action, bestAction, kernel))
                {
                    bestAction = action;
                }
            }

            PerformAction(bestAction);
        }
    }
    
    private bool IsBetterAttack(AttackAction newAction, AttackAction prevAction, Kernel kernel)
    {
        var newContext = (AttackActionContext) newAction.getContext();
        var prevContext = (AttackActionContext) prevAction.getContext();
        Fruiton newTarget = KernelUtils.GetFruitonAt(kernel, newContext.target);
        Fruiton prevTarget = KernelUtils.GetFruitonAt(kernel, prevContext.target);

        bool isNewKill = newTarget.currentAttributes.hp <= newContext.damage;
        bool isPrevKill = prevTarget.currentAttributes.hp <= prevContext.damage;

        if (prevTarget.get_isKing())
            return false;

        return newTarget.get_isKing()
            || isNewKill && !isPrevKill
            || newContext.damage > prevContext.damage
            || newTarget.type == Fruiton.MAJOR_TYPE && prevTarget.type == Fruiton.MINOR_TYPE;
    }

    private bool IsBetterHeal(HealAction newAction, HealAction prevAction, Kernel kernel)
    {
        var newContext = (HealActionContext) newAction.getContext();
        var prevContext = (HealActionContext) prevAction.getContext();
        Fruiton newTarget = kernel.currentState.field.get(newContext.target).fruiton;
        Fruiton prevTarget = kernel.currentState.field.get(prevContext.target).fruiton;

        int newHealAmount = Math.Min(newContext.heal, newTarget.originalAttributes.hp - newTarget.currentAttributes.hp);
        int prevHealAmount = Math.Min(prevContext.heal, prevTarget.originalAttributes.hp - prevTarget.currentAttributes.hp);

        return newTarget.get_isKing()
            || newTarget.type == Fruiton.MAJOR_TYPE && prevTarget.type == Fruiton.MINOR_TYPE
            || newTarget.type == prevTarget.type && newHealAmount > prevHealAmount;
    }

    private void PlayBestMove(IList<MoveAction> actions, Kernel kernel)
    {
        if (!TryPlayRandom(actions, RANDOM_MOVE_PROBA))
        {
            MoveAction bestAction = null;
            float minDistance = float.MaxValue;
            foreach (MoveAction moveAction in actions)
            {
                var currentContext = (MoveActionContext) moveAction.actionContext;
                float currentDist = ClosestEnemyDistance(currentContext.target, kernel);
                if (bestAction == null
                    || IsBetterMove(moveAction, currentDist, bestAction, minDistance, kernel))
                {
                    bestAction = moveAction;
                    minDistance = currentDist;
                }
            }

            PerformAction(bestAction);
        }
    }

    private bool IsBetterMove(MoveAction newAction, float currentDistance, MoveAction prevAction, float bestDistance, Kernel kernel)
    {
        var newContext = (MoveActionContext) newAction.getContext();
        var prevContext = (MoveActionContext) prevAction.getContext();
        Fruiton newSource = kernel.currentState.field.get(newContext.source).fruiton;
        Fruiton prevSource = kernel.currentState.field.get(prevContext.source).fruiton;

        float newDistModifier = newSource.type == Fruiton.MAJOR_TYPE ? -1 : newSource.get_isKing() ? +1 : 0;
        float prevDistModifier = prevSource.type == Fruiton.MAJOR_TYPE ? -1 : prevSource.get_isKing() ? +1 : 0;

        return currentDistance + newDistModifier < bestDistance + prevDistModifier;
    }

    private float ClosestEnemyDistance(Point start, Kernel kernel)
    {
        var fruitons = kernel.currentState.fruitons.CastToList<Fruiton>();
        float minDistance = float.MaxValue;
        foreach (Fruiton fruiton in fruitons)
        {
            if (fruiton.owner.id == kernel.currentState.get_otherPlayer().id)
            {
                float currentDistance = (float)Vector2_Impl_.distance(start, fruiton.position);
                if (currentDistance < minDistance)
                {
                    minDistance = currentDistance + rnd.Next(3) - 1;
                }
            }
        }

        return minDistance;
    }

    private bool TryPlayRandom<TAction>(IList<TAction> actions, double shouldPlayRandomProba)
        where TAction : KAction
    {
        bool randomMove = rnd.NextDouble() < shouldPlayRandomProba;
        if (randomMove)
        {
            PlayRandomAction(actions);
        }
        return randomMove;
    }

    private void PlayRandomAction<TAction>(IList<TAction> nonEndTurnActions) 
        where TAction : KAction
    {
        KAction randomAction = nonEndTurnActions.GetRandomElement(rnd);
        PerformAction(randomAction);
    }
}
