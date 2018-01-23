using System.Collections;
using System.Collections.Generic;
using fruiton.kernel;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class FridgeFruiton : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Text TextAttack;
    public Text TextHealth;
    public Image PanelName;
    public SkeletonGraphic SpineSkeleton;
    public Image[] TypeIcons;
    public int FridgeIndex;

    public Fruiton KernelFruiton { get; private set; }

    public bool IsOwned
    {
        get { return isOwned; }
        set
        {
            isOwned = value;
            backgroud.color = isOwned ? Color.white : Color.gray;
            SpineSkeleton.color = isOwned ? Color.white : Color.gray;
        }
    }

    public static string[] TypeColors = {"#ffffff", "#002366", "#8D2626", "#4b5320"};
    public static string[] TypeNames = {"", "king", "major", "pawn"};

    private static Sprite[] typeIconSprites;

    private bool isOwned;
    private Text textName;
    private Image backgroud;

#if UNITY_ANDROID
    private Coroutine pointerDownCoroutine;
    private Vector2 dragBeginPosition;
#endif

    public UnityEvent OnBeginDrag { get; private set; }
    public UnityEvent OnMouseEnter { get; private set; }
    public UnityEvent OnMouseExit { get; private set; }
    public UnityEvent OnTap { get; private set; }
    public UnityEvent OnRightClick { get; private set; }

    void Awake()
    {
        OnBeginDrag = new UnityEvent();
        OnMouseEnter = new UnityEvent();
        OnMouseExit = new UnityEvent();
        OnTap = new UnityEvent();
        OnRightClick = new UnityEvent();
        backgroud = GetComponent<Image>();
        textName = PanelName.GetComponentInChildren<Text>();
        if (typeIconSprites == null)
        {
            typeIconSprites = new Sprite[4];
            for (int i = 1; i < 4; i++)
            {
                typeIconSprites[i] = Resources.Load<Sprite>("Images/UI/Icons/" + TypeNames[i] + "_32");
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        dragBeginPosition = eventData.position;
        pointerDownCoroutine = StartCoroutine(PointerDownTimer());
#endif
#if UNITY_STANDALONE || UNITY_EDITOR
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            OnBeginDrag.Invoke();
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            OnRightClick.Invoke();
        }
#endif
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnMouseEnter.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        OnMouseExit.Invoke();
    }

    public void SetKernelFruiton(Fruiton kFruiton)
    {
        KernelFruiton = kFruiton;
        TextAttack.text = kFruiton.currentAttributes.damage.ToString();
        TextHealth.text = kFruiton.currentAttributes.hp.ToString();
        textName.text = kFruiton.model;
        Color color;
        ColorUtility.TryParseHtmlString(TypeColors[kFruiton.type], out color);
        PanelName.color = color;
        foreach (var icon in TypeIcons)
        {
            icon.sprite = typeIconSprites[kFruiton.type];
        }
        SpineSkeleton.Skeleton.SetSkin(kFruiton.model);
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    void Update()
    {
        if (pointerDownCoroutine != null)
        {
            if (Input.touchCount > 0 && Vector2.Distance(Input.GetTouch(0).position, dragBeginPosition) > 100)
            {
                StopCoroutine(pointerDownCoroutine);
                pointerDownCoroutine = null;
            } else if (Input.touchCount == 0)
            {
                StopCoroutine(pointerDownCoroutine);
                pointerDownCoroutine = null;
                OnTap.Invoke();
            }
        }
    }

    IEnumerator PointerDownTimer()
    {
        yield return new WaitForSecondsRealtime(0.25f);
        pointerDownCoroutine = null;
        OnBeginDrag.Invoke();
    }
#endif
}