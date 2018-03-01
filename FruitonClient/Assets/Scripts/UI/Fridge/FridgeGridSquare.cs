using fruiton.kernel;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Fridge
{
    /// <summary>
    /// Represents single square on the fridge team grid.
    /// </summary>
    public class FridgeGridSquare : MonoBehaviour, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Image ImageFruitonType;

        /// <summary> Type of the fruiton that can be placed on this square </summary>
        public int FruitonType { get; private set; }
        public UnityEvent OnBeginDrag { get; private set; }
        /// <summary> True if this square is empty. </summary>
        public bool IsEmpty { get; private set; }
        /// <summary> True if a suggested fruiton is shown on this square. </summary>
        public bool IsSuggestionShown { get; private set; }
        public RectTransform RectTransform { get; private set; }
        public Fruiton KernelFruiton { get; private set; }
        public UnityEvent OnMouseEnter { get; private set; }
        public UnityEvent OnMouseExit { get; private set; }

        /// <summary> Primary background color of the square. </summary>
        private Color primaryBgColor;
        /// <summary> Secondary background color of the square. </summary>
        public Color SecondaryBgColor { get; set; }

        private static Sprite[] typeIconSprites;

        private SkeletonGraphic spineSkeleton;
        private Image background;
        /// <summary> Current background color of the square. </summary>
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

        /// <summary>
        /// Toggles default background color of the square.
        /// </summary>
        public void SwitchDefaultBgColor()
        {
            if (defaultBackgroundColor == primaryBgColor)
                defaultBackgroundColor = SecondaryBgColor;
            else
                defaultBackgroundColor = primaryBgColor;
        }

        /// <summary>
        /// Sets secondary background color as default one.
        /// </summary>
        public void SetSecondaryBgColorAsDefault()
        {
            defaultBackgroundColor = SecondaryBgColor;
        }

        /// <summary>
        /// Sets primary background color as default one.
        /// </summary>
        public void ResetDefaultBgColor()
        {
            defaultBackgroundColor = primaryBgColor;
        }

        /// <summary>
        /// Sets a fruiton to the square.
        /// </summary>
        /// <param name="fruiton">fruiton to set</param>
        public void SetFruiton(Fruiton fruiton)
        {
            IsEmpty = false;
            KernelFruiton = fruiton;
            spineSkeleton.gameObject.SetActive(true);
            spineSkeleton.Skeleton.SetSkin(fruiton.model);
            spineSkeleton.color = Color.white;
        }

        /// <summary>
        /// Sets fruiton type of the square.
        /// </summary>
        /// <param name="type">fruiton type to set</param>
        public void SetFruitonType(int type)
        {
            FruitonType = type;
            ImageFruitonType.sprite = typeIconSprites[type];
            Color color;
            ColorUtility.TryParseHtmlString(FridgeFruiton.TypeColors[type] + "25", out color);
            ImageFruitonType.color = color;
        }

        /// <summary>
        /// Shows fruiton suggestion on the square.
        /// </summary>
        /// <param name="fruiton">fruiton to show suggestion of</param>
        public void SuggestFruiton(Fruiton fruiton)
        {
            IsSuggestionShown = true;
            spineSkeleton.gameObject.SetActive(true);
            spineSkeleton.Skeleton.SetSkin(fruiton.model);
            spineSkeleton.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        }

        /// <summary>
        /// Removes fruiton suggestion from the square.
        /// </summary>
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
        
        /// <summary>
        /// Removes fruiton from the square.
        /// </summary>
        public void ClearFruiton()
        {
            spineSkeleton.gameObject.SetActive(false);
            KernelFruiton = null;
            IsEmpty = true;
        }

        /// <summary>
        /// Temporarily highlights the square with a different color.
        /// </summary>
        /// <param name="color">color to highlight the square with</param>
        public void Highlight(Color color)
        {
            background.color = color;
        }

        /// <summary>
        /// Cancels highlight of the square.
        /// </summary>
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
}