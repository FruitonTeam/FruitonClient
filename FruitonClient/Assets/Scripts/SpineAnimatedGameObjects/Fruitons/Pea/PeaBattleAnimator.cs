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


public class PeaBattleAnimator : MonoBehaviour {
    public CharaterSide characterSide;

    private SkeletonAnimation skeletonAnim;

    // 0 - drawOrderBack/Side/(Front)                                           // decide which side to turn + change skins FrontQuarter/BackQuarter
    // 1 - blackEyeOff                                                          // no black eye
    // 2 - blackEyeRight/Left                                                   // turn on one of them
    // 3 - topOff/fallTop                                                       // toggle top of the body
    // 4 - starRingOff/starRingTwirl                                            // after taking damage star ring appears around its head
    // 5 - startWalk/walk/scratchMustache/wideStandGetGuns/fire/takeDamage      // usual animations


    public void startWalking()
    {
        //skeletonAnim.AnimationState.SetAnimation(5, "startWalk", false);
        //skeletonAnim.AnimationState.AddAnimation(5, "walk", true, 0);
        skeletonAnim.AnimationState.SetAnimation(5, "walk", true);
    }

    public void attack()
    {
        //skeletonAnim.AnimationState.SetAnimation(5, "startWalk", false);
        //skeletonAnim.AnimationState.AddAnimation(5, "walk", true, 0);
        skeletonAnim.AnimationState.SetAnimation(5, "wideStandGetGuns", false);
        skeletonAnim.AnimationState.AddAnimation(5, "fire", false, 0);
    }

    public void turnCharacter(CharaterSide side) {
        switch (side) {
            case CharaterSide.FRONT_L:
                skeletonAnim.AnimationState.ClearTrack(0);
                skeletonAnim.skeleton.SetSkin("FrontQuarter");
                skeletonAnim.skeleton.FlipX = false;
                skeletonAnim.skeleton.SetSlotsToSetupPose();
                skeletonAnim.AnimationState.Apply(skeletonAnim.Skeleton);
                break;
            case CharaterSide.FRONT_R:
                skeletonAnim.AnimationState.ClearTrack(0);
                skeletonAnim.skeleton.SetSkin("FrontQuarter");
                skeletonAnim.skeleton.FlipX = true;
                skeletonAnim.skeleton.SetSlotsToSetupPose();
                skeletonAnim.AnimationState.Apply(skeletonAnim.Skeleton);
                break;
            //case CharaterSide.SIDE_R:
            //    skeletonAnim.skeleton.FlipX = true;
            //    break;
            case CharaterSide.BACK_R:
                skeletonAnim.skeleton.FlipX = true;
                skeletonAnim.AnimationState.SetAnimation(0, "drawOrderBack", false);
                skeletonAnim.skeleton.SetSkin("BackQuarter");
                skeletonAnim.skeleton.SetSlotsToSetupPose();
                skeletonAnim.AnimationState.Apply(skeletonAnim.Skeleton);
                break;
            case CharaterSide.BACK_L:
                skeletonAnim.skeleton.FlipX = false;
                skeletonAnim.AnimationState.SetAnimation(0, "drawOrderBack", false);
                skeletonAnim.skeleton.SetSkin("BackQuarter");
                skeletonAnim.skeleton.SetSlotsToSetupPose();
                skeletonAnim.AnimationState.Apply(skeletonAnim.Skeleton);
                break;
            //case CharaterSide.SIDE_L:
            //    skeletonAnim.skeleton.FlipX = false;
            //    break;
        }
        characterSide = side;

    }
    public void resetCharacter()
    {
        skeletonAnim.AnimationState.SetAnimation(1, "blackEyeOff", true);
        skeletonAnim.AnimationState.ClearTrack(2);
        skeletonAnim.AnimationState.ClearTrack(3);
        skeletonAnim.AnimationState.SetAnimation(4, "starRingOff", true);
        skeletonAnim.AnimationState.ClearTrack(5);
    }

    void Awake()
    {
        skeletonAnim = GetComponent<SkeletonAnimation>();
    }

    void Start()
    {
        turnCharacter(characterSide);
        resetCharacter();
        //startWalking();
        //attack();
    }

    // Update is called once per frame
    void Update ()
    {
        transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
            Camera.main.transform.rotation * Vector3.up);
    }
}
