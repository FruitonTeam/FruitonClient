using System.Collections;
using System.Linq;
using fruiton.kernel;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Fridge
{
    /// <summary>
    /// Represents fruiton game object in scroll view in fridge scenes.
    /// </summary>
    public class FridgeFruiton : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Text TextAttack;
        public Text TextHealth;
        public Text TextCount;
        public Image PanelName;
        public SkeletonGraphic SpineSkeleton;
        public Image[] TypeIcons;
        public int FridgeIndex;

        public Fruiton KernelFruiton { get; private set; }

        /// <summary>
        /// True if logged player owns this fruiton.
        /// </summary>
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

        private int count;
        /// <summary>
        /// Number of fruitons that are available to be added to a team.
        /// </summary>
        public int Count
        {
            get
            {
                return count;
            }
            set
            {
                count = value;
                TextCount.text = count + "x";
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

        /// <summary> Time (in seconds) for which user has to keep finger on the game object to trigger drag and drop. </summary>
        private static readonly float TOUCH_DRAG_DELAY = 0.25f;
        /// <summary> Maximum distance player can move their finger while touching the game object to still trigger drag and drop. </summary>
        private static readonly int TOUCH_DRAG_MAX_OFFSET = 100;
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

        /// <summary>
        /// Loads data from kernel fruiton and displays them on the game object.
        /// </summary>
        /// <param name="kFruiton">kernel fruiton to load</param>
        public void SetKernelFruiton(Fruiton kFruiton)
        {
            KernelFruiton = kFruiton;
            TextAttack.text = kFruiton.currentAttributes.damage.ToString();
            TextHealth.text = kFruiton.currentAttributes.hp.ToString();
            Count = GameManager.Instance.AvailableFruitons.Count(id => id == kFruiton.dbId);
            textName.text = kFruiton.name;
            Color color;
            ColorUtility.TryParseHtmlString(TypeColors[kFruiton.type], out color);
            PanelName.color = color;
            foreach (var icon in TypeIcons)
            {
                icon.sprite = typeIconSprites[kFruiton.type];
            }
            SpineSkeleton.Skeleton.SetSkin(kFruiton.model);
        }

#if UNITY_ANDROID && UNITY_EDITOR
        /// <summary>
        /// Checks if player touched the game object check for shorter than <see cref="TOUCH_DRAG_DELAY"/> to trigger tap event,
        /// if user was touching the object for that amount of time and didn't move their finger for more than <see cref="TOUCH_DRAG_MAX_OFFSET"/> triggers drag and drop.
        /// </summary>
        void Update()
        {
            if (pointerDownCoroutine != null)
            {
                if (Input.touchCount > 0 && Vector2.Distance(Input.GetTouch(0).position, dragBeginPosition) > TOUCH_DRAG_MAX_OFFSET)
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
            yield return new WaitForSecondsRealtime(TOUCH_DRAG_DELAY);
            pointerDownCoroutine = null;
            OnBeginDrag.Invoke();
        }
#endif
    }
}