using System.Collections;
using System.Collections.Generic;
using Spine.Unity;
using UnityEngine;

public class FruitonBattleAnimator : MonoBehaviour
{
    protected static readonly int USUAL_TRACK = 5;

    public SkeletonAnimation SkeletonAnim { get; private set; }

    public virtual void StartWalking()
    {
        SkeletonAnim.AnimationState.SetAnimation(USUAL_TRACK, "walk", true);
    }

    public virtual void StopWalking()
    {
        SkeletonAnim.AnimationState.SetEmptyAnimation(USUAL_TRACK, 0.0f);
    }

    protected virtual void ResetCharacter()
    {
        SkeletonAnim.AnimationState.ClearTrack(2);
        SkeletonAnim.AnimationState.ClearTrack(3);
        SkeletonAnim.AnimationState.ClearTrack(USUAL_TRACK);
    }

    public virtual void Initialize()
    {
        ResetCharacter();
        transform.Rotate(0, -90, 0);
    }

    protected virtual void Awake()
    {
        SkeletonAnim = GetComponent<SkeletonAnimation>();
    }
}
