using System;

public class BoyFighterBattleAnimator : FruitonBattleAnimator
{
    private Action NextCompleteAction;

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
        NextCompleteAction = completeAction;
        SkeletonAnim.AnimationState.SetAnimation(5, name, false);
        SkeletonAnim.AnimationState.Complete += delegate {
            CompleteAction();
        };
    }

    private void CompleteAction()
    {
        if (NextCompleteAction != null)
        {
            SkeletonAnim.AnimationState.SetAnimation(5, "01_Idle", true);
            NextCompleteAction();
            NextCompleteAction = null;
        }
    }
}
