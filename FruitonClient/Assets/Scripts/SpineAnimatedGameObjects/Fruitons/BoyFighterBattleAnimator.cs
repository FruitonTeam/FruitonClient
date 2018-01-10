public class BoyFighterBattleAnimator : FruitonBattleAnimator
{

    public void Attack()
    {
        //skeletonAnim.AnimationState.SetAnimation(5, "startWalk", false);
        //skeletonAnim.AnimationState.AddAnimation(5, "walk", true, 0);
        SkeletonAnim.AnimationState.SetAnimation(5, "05_attack", false);
        SkeletonAnim.AnimationState.AddAnimation(5, "05_attack", false, 0);
    }
}
