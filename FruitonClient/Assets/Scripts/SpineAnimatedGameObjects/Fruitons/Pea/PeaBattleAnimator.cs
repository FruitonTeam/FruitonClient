namespace SpineAnimatedGameObjects.Fruitons.Pea
{
    public class PeaBattleAnimator : FruitonBattleAnimator
    {
        // 0 - drawOrderBack/Side/(Front)                                           // decide which side to turn + change skins FrontQuarter/BackQuarter
        // 1 - blackEyeOff                                                          // no black eye
        // 2 - blackEyeRight/Left                                                   // turn on one of them
        // 3 - topOff/fallTop                                                       // toggle top of the body
        // 4 - starRingOff/starRingTwirl                                            // after taking damage star ring appears around its head
        // 5 - startWalk/walk/scratchMustache/wideStandGetGuns/fire/takeDamage      // usual animations

        public void Attack()
        {
            //skeletonAnim.AnimationState.SetAnimation(5, "startWalk", false);
            //skeletonAnim.AnimationState.AddAnimation(5, "walk", true, 0);
            SkeletonAnim.AnimationState.SetAnimation(5, "wideStandGetGuns", false);
            SkeletonAnim.AnimationState.AddAnimation(5, "fire", false, 0);
        }

        protected override void ResetCharacter()
        {
            base.ResetCharacter();
            SkeletonAnim.AnimationState.SetAnimation(1, "blackEyeOff", true);
            SkeletonAnim.AnimationState.SetAnimation(4, "starRingOff", true);
        }
    }
}
