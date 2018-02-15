using System;

public class BoyFighterBattleAnimator : FruitonBattleAnimator
{
    private Action nextCompleteAction;

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
        nextCompleteAction = completeAction;
        SkeletonAnim.AnimationState.SetAnimation(5, name, false);
        SkeletonAnim.AnimationState.Complete += delegate {
            CompleteAction();
        };
    }

    private void CompleteAction()
    {
        if (nextCompleteAction != null)
        {
            SkeletonAnim.AnimationState.SetAnimation(5, "01_Idle", true);
            nextCompleteAction();
            nextCompleteAction = null;
        }
    }
}
