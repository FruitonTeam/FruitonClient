using System.Collections.Generic;
using Battle.View;
using Networking;
using UI.Fridge;
using UnityEngine;
using UnityEngine.UI;
using KFruiton = fruiton.kernel.Fruiton;

namespace TeamsManagement
{
    public abstract class FruitonVisualizerBase : MonoBehaviour
    {
        /// <summary> Template of a fruiton game object to be displayed in the scroll view. </summary>
        public GameObject FridgeFruitonTemplate;
        /// <summary> Game object containing all of the fridge fruiton game objects. </summary>
        public GameObject WrapperFruitons;
        public FridgeFilterManager FilterManager;
        /// <summary> Positions on which a tooltip can be displayed. </summary>
        public RectTransform[] TooltipPanelPositions;
        public GameObject PanelTooltip;
        public RectTransform ScrollContentRectTransform;
        public FridgeFruitonDetail FruitonDetail;

        /// <summary> Maps fruiton ids to corresponding fridge fruiton game objects </summary>
        protected Dictionary<int, FridgeFruiton> dbFridgeMapping;
        /// <summary> List of the fridge fruiton game objects </summary>
        protected List<FridgeFruiton> fridgeFruitons;

        protected virtual void Start()
        {
            dbFridgeMapping = new Dictionary<int, FridgeFruiton>();
        }

        /// <summary>
        /// Creates game object for every playable fruiton and adds it to the scene.
        /// </summary>
        protected virtual void InitializeAllFruitons()
        {
            GameManager gameManager = GameManager.Instance;
            IEnumerable<KFruiton> allFruitons = gameManager.AllPlayableFruitons;
            fridgeFruitons = new List<FridgeFruiton>();
            var i = 0;
            var templateRectTransform = FridgeFruitonTemplate.gameObject.GetComponent<RectTransform>();
            foreach (KFruiton fruiton in allFruitons)
            {
                var fridgeFruiton = Instantiate(FridgeFruitonTemplate);
                fridgeFruiton.transform.SetParent(WrapperFruitons.transform);
                fridgeFruiton.transform.localScale = templateRectTransform.localScale;
                fridgeFruiton.transform.localPosition = GetPositionOnScrollViewGrid(i);

                var kFruiton = fruiton;
                var fFruiton = fridgeFruiton.GetComponent<FridgeFruiton>();
                InitializeFridgeFruiton(fFruiton, kFruiton, i);
                dbFridgeMapping[kFruiton.dbId] = fFruiton;
                i++;
            }
            FridgeFruitonTemplate.SetActive(false);
            FilterManager.AllFruitons = fridgeFruitons;
            FilterManager.OnFilterUpdated.AddListener(ReindexFruitons);
            FilterManager.UpdateAvailableFruitons(gameManager.AvailableFruitons);
        
            if (gameManager.IsOnline)
            {
                UpdateAvailableFruitons();
            }
        }

        /// <summary>
        /// Initializes fridge fruiton game object data.
        /// </summary>
        /// <param name="fFruiton">game object to initialize</param>
        /// <param name="kFruiton">kernel fruiton to load data from</param>
        /// <param name="fridgeIndex">index of the fridge fruiton in the scene</param>
        protected virtual void InitializeFridgeFruiton(FridgeFruiton fFruiton, KFruiton kFruiton, int fridgeIndex)
        {
            fFruiton.SetKernelFruiton(kFruiton);
            fFruiton.FridgeIndex = fridgeIndex;
            fridgeFruitons.Add(fFruiton);
        }

        /// <summary>
        /// Loads available fruitons from the server then updates displayed information.
        /// </summary>
        protected virtual void UpdateAvailableFruitons()
        {
            PlayerHelper.GetAvailableFruitons(FilterManager.UpdateAvailableFruitons, Debug.Log);
        }
    
        /// <summary>
        /// Hides currently displayed fruiton tooltip.
        /// </summary>
        protected void HideTooltip()
        {
            PanelTooltip.SetActive(false);
        }

        /// <summary>
        /// Closes fruiton detail window.
        /// </summary>
        public void HideDetail()
        {
            FruitonDetail.gameObject.SetActive(false);
            FruitonDetail.Barrier.gameObject.SetActive(false);
            HideTooltip();
        }

        /// <summary>
        /// Opens fruiton detail window.
        /// </summary>
        /// <param name="fruiton">fruiton to show in the deail window</param>
        protected virtual void ShowDetail(FridgeFruiton fruiton)
        {
            FruitonDetail.TooltipText.text = TooltipUtil.GenerateTooltip(fruiton.KernelFruiton);
            FruitonDetail.gameObject.SetActive(true);
        }

        /// <summary>
        /// Calculates position of an object (fruiton or team) on the scroll view grid
        /// </summary>
        /// <param name="index">index of the object</param>
        /// <returns>position of an object on the scroll view grid</returns>
        protected static Vector3 GetPositionOnScrollViewGrid(int index)
        {
            return new Vector3(
                27.5f + (index / 2) * 240,
                -233 - (index % 2) * 231,
                0
            );
        }

        /// <summary>
        /// Shows fruiton tooltip on a given position.
        /// </summary>
        /// <param name="fruiton">fruiton to show tooltip of</param>
        /// <param name="positionIndex">index of the position in <see cref="TooltipPanelPositions"/></param>
        protected void ShowTooltip(KFruiton fruiton, int positionIndex = 0)
        {
            RectTransform targetTransform = TooltipPanelPositions[positionIndex];
            var tooltipTransform = PanelTooltip.GetComponent<RectTransform>();
            PanelTooltip.SetActive(true);
            PanelTooltip.transform.SetParent(targetTransform.parent);
            tooltipTransform.pivot = targetTransform.pivot;
            tooltipTransform.anchorMin = targetTransform.anchorMin;
            tooltipTransform.anchorMax = targetTransform.anchorMax;
            tooltipTransform.anchoredPosition = targetTransform.anchoredPosition;
            PanelTooltip.GetComponentInChildren<Text>().text = TooltipUtil.GenerateTooltip(fruiton);
        }

        /// <summary>
        /// Assigns indices to active fruitons and changes their positions according to them.
        /// </summary>
        protected virtual void ReindexFruitons()
        {
            int newIndex = 0;
            foreach (var fruiton in fridgeFruitons)
            {
                var oldIndex = fruiton.FridgeIndex;
                if (!fruiton.gameObject.activeSelf)
                {
                    fruiton.FridgeIndex = -1;
                    continue;
                }
                fruiton.FridgeIndex = newIndex;
                if (newIndex != oldIndex)
                {
                    if (oldIndex < 0)
                    {
                        fruiton.gameObject.transform.localPosition = GetPositionOnScrollViewGrid(newIndex);
                    }
                    else
                    {
                        //iTween.Stop(fruiton.gameObject);
                        //iTween.MoveTo(fruiton.gameObject, iTween.Hash(
                        //        "position", GetPositionOnScrollViewGrid(newIndex),
                        //        "islocal", true,
                        //        "time", 1,
                        //        "easetype", iTween.EaseType.easeOutExpo
                        //    )
                        //);
                        fruiton.gameObject.transform.localPosition = GetPositionOnScrollViewGrid(newIndex);     
                    }
                }
                newIndex++;
            }
            ResizeScrollContent(newIndex);
        }

        /// <summary>
        /// Resizes scroll view content to fit a number of objects.
        /// </summary>
        /// <param name="objectCount">number of objects to fit in</param>
        protected void ResizeScrollContent(int objectCount)
        {
            var contentSize = ScrollContentRectTransform.sizeDelta;
            var helperIndex = objectCount + objectCount % 2;
            var newWidth = GetPositionOnScrollViewGrid(helperIndex).x;
            ScrollContentRectTransform.sizeDelta = new Vector2(newWidth, contentSize.y);
            var scrollViewWidth = ScrollContentRectTransform.parent.parent.GetComponent<RectTransform>().rect.width;
            if (newWidth < scrollViewWidth)
            {
                ScrollContentRectTransform.localPosition = Vector3.zero;
                return;
            }
            var contentWidth = newWidth + ScrollContentRectTransform.localPosition.x;
            if (contentWidth < scrollViewWidth)
            {
                ScrollContentRectTransform.localPosition = new Vector3(scrollViewWidth - newWidth, 0, 0);
            }
        }

        public void LoadMainMenu()
        {
            Scenes.Load(Scenes.MAIN_MENU_SCENE);
        }
    }
}
