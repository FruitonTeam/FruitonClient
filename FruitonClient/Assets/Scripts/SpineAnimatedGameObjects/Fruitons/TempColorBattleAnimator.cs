using Spine.Unity;
using UnityEngine;

// TODO remove when proper sprites are used
namespace SpineAnimatedGameObjects.Fruitons
{
    public class TempColorBattleAnimator : FruitonBattleAnimator
    {
        public Color FruitonColor;

        protected void Start()
        {
            SkeletonAnim.skeleton.SetColor(FruitonColor);
        }

        protected override void ResetCharacter()
        {
            base.ResetCharacter();
            SkeletonAnim.AnimationState.SetAnimation(1, "blackEyeOff", true);
            SkeletonAnim.AnimationState.SetAnimation(4, "starRingOff", true);
        }
    }
}
