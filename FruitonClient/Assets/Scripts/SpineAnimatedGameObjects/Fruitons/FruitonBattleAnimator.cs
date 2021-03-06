﻿using Spine.Unity;
using UnityEngine;

namespace SpineAnimatedGameObjects.Fruitons
{
    public class FruitonBattleAnimator : MonoBehaviour
    {
        protected static readonly int USUAL_TRACK = 5;

        public SkeletonAnimation SkeletonAnim { get; private set; }

        public virtual void StartWalking()
        {
            SkeletonAnim.AnimationState.SetAnimation(USUAL_TRACK, "02_walk", true);
        }

        public virtual void StopWalking()
        {
            SkeletonAnim.AnimationState.SetEmptyAnimation(USUAL_TRACK, 0.0f);
        }

        protected virtual void ResetCharacter()
        {
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
}
