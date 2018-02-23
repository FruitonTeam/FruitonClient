using Networking;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KFruiton = fruiton.kernel.Fruiton;

public abstract class FruitonVisualizerBase : MonoBehaviour
{
    public GameObject FridgeFruitonTemplate;
    public GameObject WrapperFruitons;
    public FridgeFilterManager FilterManager;
    public RectTransform[] TooltipPanelPositions;
    public GameObject PanelTooltip;
    public RectTransform ScrollContentRectTransform;
    public FridgeFruitonDetail FruitonDetail;

    protected List<FridgeFruiton> fridgeFruitons;

    protected virtual void Start()
    {
        InitializeAllFruitons();
    }

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

    protected virtual void InitializeFridgeFruiton(FridgeFruiton fFruiton, KFruiton kFruiton, int fridgeIndex)
    {
        fFruiton.SetKernelFruiton(kFruiton);
        fFruiton.FridgeIndex = fridgeIndex;
        fridgeFruitons.Add(fFruiton);
    }

    protected virtual void UpdateAvailableFruitons()
    {
        PlayerHelper.GetAvailableFruitons(FilterManager.UpdateAvailableFruitons, Debug.Log);
    }
    
    protected void HideTooltip()
    {
        PanelTooltip.SetActive(false);
    }

    public void HideDetail()
    {
        FruitonDetail.gameObject.SetActive(false);
        FruitonDetail.Barrier.gameObject.SetActive(false);
        HideTooltip();
    }

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

    private void ReindexFruitons()
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
                    iTween.Stop(fruiton.gameObject);
                    iTween.MoveTo(fruiton.gameObject, iTween.Hash(
                            "position", GetPositionOnScrollViewGrid(newIndex),
                            "islocal", true,
                            "time", 1,
                            "easetype", iTween.EaseType.easeOutExpo
                        )
                    );
                }
            }
            newIndex++;
        }
        ResizeScrollContent(newIndex + 1);
    }

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
