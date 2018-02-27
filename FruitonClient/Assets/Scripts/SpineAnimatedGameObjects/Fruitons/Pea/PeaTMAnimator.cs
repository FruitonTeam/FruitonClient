using System;
using System.Linq;
using Spine.Unity;
using UnityEngine;

namespace SpineAnimatedGameObjects.Fruitons.Pea
{
    public class PeaTMAnimator : MonoBehaviour
    {
        private SkeletonAnimation skeletonAnim;

        public float AnimSwitchTime;
        private float currentAnimTime;

        // 0 - drawOrderBack/Side/(Front)                                           // decide which side to turn + change skins FrontQuarter/BackQuarter
        // 1 - blackEyeOff                                                          // no black eye
        // 2 - blackEyeRight/Left                                                   // turn on one of them
        // 3 - topOff/fallTop                                                       // toggle top of the body
        // 4 - starRingOff/starRingTwirl                                            // after taking damage star ring appears around its head
        // 5 - startWalk/walk/scratchMustache/wideStandGetGuns/fire/takeDamage      // usual animations

        private static readonly int USUAL_TRACK = 5;

        private static readonly System.Random rnd = new System.Random();

        private enum IdleAnimations
        {
            Walk,
            Stand,
            Fire,
            TakeDamage
        }

        private IdleAnimations currentIdleAnimation;

        private void FinishCurrentAnim()
        {
            skeletonAnim.AnimationState.SetEmptyAnimation(USUAL_TRACK, 0);
        }

        private void AddNextRandomAnim()
        {
            var values = Enum
                .GetValues(typeof(IdleAnimations))
                .Cast<IdleAnimations>()
                .Where(x => x != currentIdleAnimation)
                .ToList();

            currentIdleAnimation = values[rnd.Next(values.Count)];

            switch (currentIdleAnimation)
            {
                case IdleAnimations.Fire:
                    Attack();
                    break;
                case IdleAnimations.Stand:
                    StandIdle();
                    break;
                case IdleAnimations.TakeDamage:
                    TakeDamageIdle();
                    break;
                case IdleAnimations.Walk:
                    StartWalking();
                    break;
            }
        }

        private void StartWalking()
        {
            skeletonAnim.AnimationState.AddAnimation(USUAL_TRACK, "startWalk", false, 0);
            skeletonAnim.AnimationState.AddAnimation(USUAL_TRACK, "walk", true, 0);
        }

        private void TakeDamageIdle()
        {
            skeletonAnim.AnimationState.AddAnimation(USUAL_TRACK, "takeDamage", false, 0);
            skeletonAnim.AnimationState.AddAnimation(USUAL_TRACK, "takeDamage", false, 3);
        }

        private void StandIdle()
        {
            skeletonAnim.AnimationState.AddAnimation(USUAL_TRACK, "scratchMustache", false, 0);
            skeletonAnim.AnimationState.AddAnimation(USUAL_TRACK, "scratchMustache", false, 4);
        }

        private void Attack()
        {
            skeletonAnim.AnimationState.AddAnimation(USUAL_TRACK, "wideStandGetGuns", false, 0);
            skeletonAnim.AnimationState.AddAnimation(USUAL_TRACK, "fire", false, 0);
            //skeletonAnim.AnimationState.AddEmptyAnimation(USUAL_TRACK, 0, 0);
            // TODO Move weapons back so they do not float
        }

        private void ResetCharacter()
        {
            skeletonAnim.AnimationState.SetAnimation(1, "blackEyeOff", true);
            skeletonAnim.AnimationState.ClearTrack(2);
            skeletonAnim.AnimationState.ClearTrack(3);
            skeletonAnim.AnimationState.SetAnimation(4, "starRingOff", true);
            skeletonAnim.AnimationState.ClearTrack(USUAL_TRACK);
        }

        private void StartTeamManagementIdle()
        {
            StartWalking();
            currentIdleAnimation = IdleAnimations.Walk;
        }

        private void Awake()
        {
            skeletonAnim = GetComponent<SkeletonAnimation>();
        }

        private void Start()
        {
            ResetCharacter();
            StartTeamManagementIdle();
        }

        private void Update()
        {
            currentAnimTime += Time.deltaTime;
            if (currentAnimTime > AnimSwitchTime)
            {
                currentAnimTime -= AnimSwitchTime;
                FinishCurrentAnim();
                AddNextRandomAnim();
            }
        }
    }
}