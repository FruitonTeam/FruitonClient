using UnityEngine;
using Spine.Unity;

public enum CharaterSide
{
    FRONT_L,
    FRONT_R,
    //SIDE_R,
    BACK_R,
    BACK_L,
    //SIDE_L,
}


public class PeaBattleAnimator : FruitonBattleAnimator {
    public CharaterSide characterSide;

    // 0 - drawOrderBack/Side/(Front)                                           // decide which side to turn + change skins FrontQuarter/BackQuarter
    // 1 - blackEyeOff                                                          // no black eye
    // 2 - blackEyeRight/Left                                                   // turn on one of them
    // 3 - topOff/fallTop                                                       // toggle top of the body
    // 4 - starRingOff/starRingTwirl                                            // after taking damage star ring appears around its head
    // 5 - startWalk/walk/scratchMustache/wideStandGetGuns/fire/takeDamage      // usual animations


    public void attack()
    {
        //skeletonAnim.AnimationState.SetAnimation(5, "startWalk", false);
        //skeletonAnim.AnimationState.AddAnimation(5, "walk", true, 0);
        SkeletonAnim.AnimationState.SetAnimation(5, "wideStandGetGuns", false);
        SkeletonAnim.AnimationState.AddAnimation(5, "fire", false, 0);
    }

    public void turnCharacter(CharaterSide side) {
        switch (side) {
            case CharaterSide.FRONT_L:
                SkeletonAnim.AnimationState.ClearTrack(0);
                SkeletonAnim.skeleton.SetSkin("FrontQuarter");
                SkeletonAnim.skeleton.FlipX = false;
                SkeletonAnim.skeleton.SetSlotsToSetupPose();
                SkeletonAnim.AnimationState.Apply(SkeletonAnim.Skeleton);
                break;
            case CharaterSide.FRONT_R:
                SkeletonAnim.AnimationState.ClearTrack(0);
                SkeletonAnim.skeleton.SetSkin("FrontQuarter");
                SkeletonAnim.skeleton.FlipX = true;
                SkeletonAnim.skeleton.SetSlotsToSetupPose();
                SkeletonAnim.AnimationState.Apply(SkeletonAnim.Skeleton);
                break;
            //case CharaterSide.SIDE_R:
            //    skeletonAnim.skeleton.FlipX = true;
            //    break;
            case CharaterSide.BACK_R:
                SkeletonAnim.skeleton.FlipX = true;
                SkeletonAnim.AnimationState.SetAnimation(0, "drawOrderBack", false);
                SkeletonAnim.skeleton.SetSkin("BackQuarter");
                SkeletonAnim.skeleton.SetSlotsToSetupPose();
                SkeletonAnim.AnimationState.Apply(SkeletonAnim.Skeleton);
                break;
            case CharaterSide.BACK_L:
                SkeletonAnim.skeleton.FlipX = false;
                SkeletonAnim.AnimationState.SetAnimation(0, "drawOrderBack", false);
                SkeletonAnim.skeleton.SetSkin("BackQuarter");
                SkeletonAnim.skeleton.SetSlotsToSetupPose();
                SkeletonAnim.AnimationState.Apply(SkeletonAnim.Skeleton);
                break;
            //case CharaterSide.SIDE_L:
            //    skeletonAnim.skeleton.FlipX = false;
            //    break;
        }
        characterSide = side;

    }

    protected override void ResetCharacter()
    {
        base.ResetCharacter();
        SkeletonAnim.AnimationState.SetAnimation(1, "blackEyeOff", true);
        SkeletonAnim.AnimationState.SetAnimation(4, "starRingOff", true);
    }
}
