using fruiton.kernel;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

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

    void Update()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        // explanation below
        if (SpineSkeleton.Skeleton != null && SpineSkeleton.Skeleton.skin.Name != CurrentFruiton.model)
        {
            SpineSkeleton.Skeleton.SetSkin(CurrentFruiton.model);
            SpineSkeleton.AnimationState.SetEmptyAnimation(0, 0);
        }
#endif

        var animState = SpineSkeleton.AnimationState;
        if (animState.GetCurrent(0).IsComplete)
        {
            var animations = SpineSkeleton.SkeletonData.Animations.Items;
            animState.SetAnimation(0, animations[Random.Range(0, animations.Length)], false);
        }
    }

    public void SetFruiton(Fruiton fruiton, bool canAddToTeam)
    {
        CurrentFruiton = fruiton;

        if (typeIconSprites == null)
        {
            LoadIconSprites();
        }

#if UNITY_STANDALONE || UNITY_EDITOR
        // TODO: figure out why SpineSkeleton.Skeleton is null on android when this method is first called
        SpineSkeleton.Skeleton.SetSkin(fruiton.model);
        SpineSkeleton.AnimationState.SetEmptyAnimation(0,0);
#endif

        TypeImage.sprite = typeIconSprites[fruiton.type];
        Color color;
        ColorUtility.TryParseHtmlString(FridgeFruiton.TypeColors[fruiton.type] + "55", out color);
        TypeImage.color = color;
        ColorUtility.TryParseHtmlString(FridgeFruiton.TypeColors[fruiton.type] + "88", out color);
        TypeText.color = color;
        TypeText.text = ((FruitonType)FruitonType.ToObject(typeof(FruitonType), fruiton.type)).ToString();
        NameText.text = fruiton.model;

        AddToTeamButton.gameObject.SetActive(canAddToTeam);
        if (canAddToTeam)
        {
#if UNITY_ANDROID
            TipText.text =
                "<b>TIP</b>: Tap and hold fruiton in the fridge to add it to the team";
            TipText.color = Color.black;
#else
            TipText.text = "";
#endif
        }
        else
        {
            TipText.text =
                "This fruiton can't be added to the team right now because there are no empty squares left for its type";
            TipText.color = new Color(0.6f, 0, 0);
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
