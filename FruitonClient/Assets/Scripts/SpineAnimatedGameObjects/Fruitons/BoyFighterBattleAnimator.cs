using System;
using System.Collections.Generic;

public class BoyFighterBattleAnimator : FruitonBattleAnimator
{
    private Queue<Tuple<string, Action>> actionsQueue;
    private Action nextCallback;

    protected override void Awake()
    {
        base.Awake();
        actionsQueue = new Queue<Tuple<string,Action>>();
        SkeletonAnim.AnimationState.Complete += delegate {
            CompleteAction();
        };
    }

    public void Attack(Action completeAction = null)
    {
        PlayAnimationOnce("05_attack", completeAction);
    }

    public void Cast(Action completeAction = null)
    {
        PlayAnimationOnce("06_cast", completeAction);
    }

    private void PlayAnimationOnce(string name, Action completeAction)
    {
        if (nextCallback == null)
        {
            SkeletonAnim.AnimationState.SetAnimation(5, name, false);
            nextCallback = completeAction;
        }
        else
        {
            actionsQueue.Enqueue(Tuple.New(name, completeAction));
        }
        
    }

    private void CompleteAction()
    {
        if (nextCallback == null) return;
        nextCallback();
        if (actionsQueue.Count != 0)
        {
            Tuple<string, Action> next = actionsQueue.Dequeue();
            SkeletonAnim.AnimationState.SetAnimation(5, next.First, false);
            nextCallback = next.Second;
        }
        else
        {
            nextCallback = null;
            SkeletonAnim.AnimationState.SetAnimation(5, "01_Idle", true);
        }
    }
}
