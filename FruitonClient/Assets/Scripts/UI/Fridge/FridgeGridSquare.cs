using fruiton.kernel;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FridgeGridSquare : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image ImageFruitonType;

    public int FruitonType { get; private set; }
    public UnityEvent OnBeginDrag { get; private set; }
    public bool IsEmpty { get; private set; }
    public bool IsSuggestionShown { get; private set; }
    public RectTransform RectTransform { get; private set; }
    public Fruiton KernelFruiton { get; private set; }
    public UnityEvent OnMouseEnter { get; private set; }
    public UnityEvent OnMouseExit { get; private set; }

    private Color primaryBgColor;
    public Color SecondaryBgColor { get; set; }

    private static Sprite[] typeIconSprites;

    private SkeletonGraphic spineSkeleton;
    private Image background;
    private Color defaultBackgroundColor;

    void Awake()
    {
        RectTransform = GetComponent<RectTransform>();
        spineSkeleton = GetComponentInChildren<SkeletonGraphic>();
        background = GetComponentInChildren<Image>();
        defaultBackgroundColor = GetComponentInChildren<Image>().color;
        primaryBgColor = defaultBackgroundColor;
        OnBeginDrag = new UnityEvent();
        OnMouseEnter = new UnityEvent();
        OnMouseExit = new UnityEvent();

        if (typeIconSprites == null)
        {
            typeIconSprites = new Sprite[4];
            for (int i = 1; i < 4; i++)
            {
                typeIconSprites[i] = Resources.Load<Sprite>("Images/UI/Icons/" + FridgeFruiton.TypeNames[i] + "_256");
            }
        }
    }

    public void SwitchDefaultBgColor()
    {
        if (defaultBackgroundColor == primaryBgColor)
            defaultBackgroundColor = SecondaryBgColor;
        else
            defaultBackgroundColor = primaryBgColor;
    }

    public void ResetDefaultBgColor()
    {
        defaultBackgroundColor = primaryBgColor;
    }

    public void SetFruiton(Fruiton fruiton)
    {
        IsEmpty = false;
        KernelFruiton = fruiton;
        spineSkeleton.gameObject.SetActive(true);
        spineSkeleton.Skeleton.SetSkin(fruiton.model);
        spineSkeleton.color = Color.white;
    }

    public void SetFruitonType(int type)
    {
        FruitonType = type;
        ImageFruitonType.sprite = typeIconSprites[type];
        Color color;
        ColorUtility.TryParseHtmlString(FridgeFruiton.TypeColors[type] + "25", out color);
        ImageFruitonType.color = color;
    }

    public void SuggestFruiton(Fruiton fruiton)
    {
        IsSuggestionShown = true;
        spineSkeleton.gameObject.SetActive(true);
        spineSkeleton.Skeleton.SetSkin(fruiton.model);
        spineSkeleton.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
    }

    public void ClearSuggestion()
    {
        if (!IsSuggestionShown)
        {
            return;
        }
        IsSuggestionShown = false;

        if (KernelFruiton == null)
        {
            ClearFruiton();
        }
        else
        {
            SetFruiton(KernelFruiton);
        }
    }

    public void ClearFruiton()
    {
        spineSkeleton.gameObject.SetActive(false);
        KernelFruiton = null;
        IsEmpty = true;
    }

    public void Highlight(Color color)
    {
        background.color = color;
    }

    public void CancelHighlight()
    {
        background.color = defaultBackgroundColor;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (KernelFruiton != null)
        {
            OnBeginDrag.Invoke();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnMouseEnter.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnMouseExit.Invoke();
    }
}