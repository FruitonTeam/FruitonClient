using System;
using Enums;
using fruiton.kernel;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace UI.Fridge
{
    public class FridgeFruitonDetail : MonoBehaviour
    {

        public SkeletonGraphic SpineSkeleton;
        public Button CloseButton;
        public Text TooltipText;
        public Text TipText;
        public Text TypeText;
        public Text NameText;
        public Button Barrier;
        public Button AddToTeamButton;
        public Image TypeImage;
        public Fruiton CurrentFruiton { get; private set; }

        private static Sprite[] typeIconSprites;

        private static readonly Color ERROR_TIP_COLOR = new Color(0.55f, 0, 0);
        private static readonly string TIP_FRUITON_NOT_OWNED = "You do not own this fruiton";
        private static readonly string TIP_FRUITON_ALREADY_USED = "You are already using every {0} you own in the team";
        private static readonly string TIP_FRUITON_NO_SQUARES_LEFT = "This fruiton can't be added to the team right now because there are no empty squares left for its type";
#if UNITY_ANDROID
        private static readonly string TIP_ANDROID_DND = "<b>TIP</b>: Tap and hold fruiton in the fridge to add it to the team";
#endif

        void Update()
        {
            // we need to check Skeleton for null because sometimes it doesn't get initialized in time (more investigation required)
            if (SpineSkeleton.Skeleton != null && SpineSkeleton.Skeleton.skin.Name != CurrentFruiton.model)
            {
                SpineSkeleton.Skeleton.SetSkin(CurrentFruiton.model);
                SpineSkeleton.AnimationState.SetEmptyAnimation(0, 0);
            }

            var animState = SpineSkeleton.AnimationState;
            if (animState.GetCurrent(0).IsComplete)
            {
                var animations = SpineSkeleton.SkeletonData.Animations.Items;
                animState.SetAnimation(0, animations[Random.Range(0, animations.Length)], false);
            }
            Barrier.gameObject.SetActive(true);
        }

        public void SetFruiton(FridgeFruiton fruiton, bool isFreeSquareInTeam)
        {
            var kFruiton = fruiton.KernelFruiton;
            CurrentFruiton = kFruiton;

            if (typeIconSprites == null)
            {
                LoadIconSprites();
            }

            TypeImage.sprite = typeIconSprites[kFruiton.type];
            Color color;
            ColorUtility.TryParseHtmlString(FridgeFruiton.TypeColors[kFruiton.type] + "55", out color);
            TypeImage.color = color;
            ColorUtility.TryParseHtmlString(FridgeFruiton.TypeColors[kFruiton.type] + "88", out color);
            TypeText.color = color;
            TypeText.text = ((FruitonType)FruitonType.ToObject(typeof(FruitonType), kFruiton.type)).ToString();
            NameText.text = kFruiton.model;

            AddToTeamButton.gameObject.SetActive(fruiton.IsOwned && isFreeSquareInTeam && fruiton.Count > 0);

            if (!fruiton.IsOwned)
            {
                TipText.text =TIP_FRUITON_NOT_OWNED;
                TipText.color = ERROR_TIP_COLOR;
                return;
            }

            if (fruiton.Count == 0)
            {
                TipText.text = String.Format(TIP_FRUITON_ALREADY_USED, fruiton.KernelFruiton.name);
                TipText.color = ERROR_TIP_COLOR;
                return;
            }

            if (isFreeSquareInTeam)
            {
#if UNITY_ANDROID
                TipText.text = TIP_ANDROID_DND;
                TipText.color = Color.black;
#else
            TipText.text = "";
#endif
            }
            else
            {
                TipText.text = TIP_FRUITON_NO_SQUARES_LEFT;
                TipText.color = ERROR_TIP_COLOR;
            }

        }

        private void LoadIconSprites()
        {
            typeIconSprites = new Sprite[4];
            for (int i = 1; i < 4; i++)
            {
                typeIconSprites[i] = Resources.Load<Sprite>("Images/UI/Icons/" + FridgeFruiton.TypeNames[i] + "_256");
            }
        }
    }
}
