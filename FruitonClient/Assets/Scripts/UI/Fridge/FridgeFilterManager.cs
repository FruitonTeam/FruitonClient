using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class FridgeFilterManager : MonoBehaviour
{
    public InputField FilterInput;
    public Button ShowNotOwnedButton;
    public Button[] TypeButtons;
    public List<FridgeFruiton> AllFruitons;

    public UnityEvent OnFilterUpdated { get; private set; }

    private bool showNotOwned;
    private int typeFilter;
    private Color[] typeColors;

    void Awake()
    {
        OnFilterUpdated = new UnityEvent();
        FilterInput.onValueChanged.AddListener(text => { ApplyFilters(); });
        ShowNotOwnedButton.onClick.AddListener(ToggleShowNotOwned);
        typeColors = new Color[4];
        for (int i = 1; i < 4; i++)
        {
            int j = i;
            TypeButtons[i].onClick.AddListener(() => ToggleTypeFilter(j));
            typeColors[i] = TypeButtons[i].GetComponent<Image>().color;
        }
    }

    public void UpdateAvailableFruitons(List<int> availableIds)
    {
        var idSet = new HashSet<int>(availableIds);

        foreach (var fruiton in AllFruitons)
        {
            fruiton.IsOwned = idSet.Contains(fruiton.KernelFruiton.id);
        }
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var fireUpdate = false;
        var filterText = FilterInput.text.ToLower();
        foreach (var fruiton in AllFruitons)
        {
            var kFruiton = fruiton.KernelFruiton;
            var show = showNotOwned || fruiton.IsOwned;

            show = show && (filterText == "" || kFruiton.model.ToLower().Contains(filterText));

            show = show && (typeFilter == 0 || kFruiton.type == typeFilter);

            fireUpdate = fireUpdate || (show != fruiton.gameObject.activeSelf);

            fruiton.gameObject.SetActive(show);
        }
        if (fireUpdate)
        {
            OnFilterUpdated.Invoke();
        }
    }

    private void ToggleShowNotOwned()
    {
        showNotOwned = !showNotOwned;
        var buttonText = ShowNotOwnedButton.GetComponentInChildren<Text>(true);
        var buttonImage = ShowNotOwnedButton.GetComponent<Image>();
        if (showNotOwned)
        {
            buttonText.text = "HIDE MISSING";
            buttonText.color = Color.white;
            buttonImage.color = Color.black;
        }
        else
        {
            buttonText.text = "SHOW MISSING";
            buttonText.color = Color.black;
            buttonImage.color = Color.white;
        }
        ApplyFilters();
    }

    private void ToggleTypeFilter(int type)
    {
        var buttonImg = TypeButtons[type].GetComponent<Image>();

        if (typeFilter == type)
        {
            buttonImg.color = typeColors[type];
            typeFilter = 0;
        }
        else
        {
            if (typeFilter > 0)
            {
                TypeButtons[typeFilter].GetComponent<Image>().color = typeColors[typeFilter];
            }
            buttonImg.color = new Color(1, 1, 0.7f);
            typeFilter = type;
        }
        ApplyFilters();
    }
}