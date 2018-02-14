using Cz.Cuni.Mff.Fruiton.Dto;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KFruiton = fruiton.kernel.Fruiton;
using System;
using System.Linq;
using fruiton.kernel;
using fruiton.kernel.fruitonTeam;
using haxe.root;
using Networking;

public class FruitonTeamsManager : MonoBehaviour
{
    private class Option<TEnum>
    {
        public string Name { get; private set; }
        public TEnum Type { get; private set; }

        public Option(string name, TEnum type)
        {
            Name = name;
            Type = type;
        }
    }

    enum ViewMode
    {
        TeamSelect,
        TeamEdit
    }

    public ScrollRect ScrollRect;
    public FridgeTeamGrid TeamGrid;
    public Image DragAndDropBarrier;
    public GameObject FridgeFruitonTemplate;
    public FridgeDndFruiton DragAndDropFruiton;
    public GameObject FridgeTeamTemplate;
    public GameObject WrapperFruitons;
    public GameObject WrapperTeams;
    public GameObject PanelTooltip;
    public GameObject Filters;
    public FridgeFruitonDetail FruitonDetail;
    public RectTransform ScrollContentRectTransform;
    public RectTransform[] TooltipPanelPositions;
    public Button ButtonPlay;
    public Button ButtonNewTeam;
    public Button ButtonDelete;
    public Button ButtonEdit;
    public Button ButtonBack;
    public Button ButtonDone;
    public InputField InputTeamName;
    public Text WarningText;
    public FridgeFilterManager FilterManager;
    public GameObject DropdownPanel;


    private ViewMode viewMode;
    private List<FridgeFruitonTeam> teams;
    private int selectedTeamIndex;
    private Color defaultTeamIconColor;
    private bool isDragging;
    private bool isDraggingFromTeam;
    private bool isAddingFromDetail;
    private Position teamDragGridPosition;
    private KFruiton draggedFruiton;
    private List<FridgeFruiton> fridgeFruitons;

    private readonly List<Option<GameMode>> gameModes = new List<Option<GameMode>>
    {
        new Option<GameMode>("Standard", GameMode.Standard),
        new Option<GameMode>("Last man standing", GameMode.LastManStanding)
    };

    private readonly List<Option<AIType>> aiModes = new List<Option<AIType>>
    {
        new Option<AIType>("Fruiton Bowl", AIType.SportsMen),
        new Option<AIType>("North Pole", AIType.Santas),
        new Option<AIType>("Circus", AIType.Clowns)
    };

    /// <summary> true if player is actually editing teams, false if only viewing/picking </summary>
    private bool isInTeamManagement;

    /// <summary> Name of scene param, true if player is actually editing teams, false if only viewing/picking</summary>
    public static readonly string TEAM_MANAGEMENT_STATE = "teamManagementState";

    public static readonly int MAX_TEAM_COUNT = 16;

    // Use this for initialization
    void Start()
    {
        isInTeamManagement = bool.Parse(Scenes.GetParam(TEAM_MANAGEMENT_STATE));
        InitializeTeams(isInTeamManagement);
        SwitchViewMode(viewMode);
        if (isInTeamManagement)
        {
            ButtonPlay.gameObject.SetActive(false);
            InitializeAllFruitons();
            DropdownPanel.SetActive(false);
        }
        else
        {
            ButtonNewTeam.gameObject.SetActive(false);
            ButtonEdit.gameObject.SetActive(false);
            ButtonDelete.gameObject.SetActive(false);

            PlayerOptions playerOptions = GameManager.Instance.PlayerOptions;
            var battleType = (BattleType)Enum.Parse(typeof(BattleType), Scenes.GetParam(Scenes.BATTLE_TYPE));
            if (battleType == BattleType.AIBattle)
            {
                SetupModeDropdown(aiModes, playerOptions.LastSelectedAIMode);
            }
            else
            {
                SetupModeDropdown(gameModes, playerOptions.LastSelectedGameMode);
            }
        }
        InitializeTeamGridListeners();
        InitializeFruitonDetailListeners();
        if (viewMode == ViewMode.TeamSelect)
        {
            if (GameManager.Instance.CurrentFruitonTeam != null)
            {
                SelectTeam(
                    GameManager.Instance.FruitonTeamList.FruitonTeams.IndexOf(GameManager.Instance.CurrentFruitonTeam)
                );
            }
            else
            {
                SelectTeam(0);
            }
        }
        DragAndDropFruiton.gameObject.SetActive(false);

        gameObject.AddComponent<Form>().SetInputs(
            ButtonDone,
            new FormControl("team_name", InputTeamName,
                Validator.Required("Please enter team name"),
                name =>
                {
                    for (int i = 0; i < teams.Count; i++)
                    {
                        if (teams[i].KernelTeam.Name == name && i != selectedTeamIndex)
                        {
                            return "Another team with this name already exists!";
                        }
                    }
                    return null;
                })
        ).SetErrorFontSize(24);
    }

    private void SetupModeDropdown<TEnum>(IList<Option<TEnum>> options, int selectedIdx)
    {
        DropdownPanel.SetActive(true);
        var dropdown = DropdownPanel.GetComponentInChildren<Dropdown>();
        dropdown.options.Clear();
        foreach (Option<TEnum> option in options)
        {
            dropdown.options.Add(new Dropdown.OptionData(option.Name));
        }

        dropdown.value = selectedIdx;
        dropdown.captionText.text = options[dropdown.value].Name;
    }

    void Update()
    {
        // drag and drop (or adding from fruiton detail window) logic
        if (isDragging)
        {
            Vector3 dndPosition = Vector3.left * 1000;
            Vector3 pointerPosition = Vector3.left * 1000;
#if UNITY_ANDROID
            if (Input.touchCount > 0)
            {
                var touchPos = Input.GetTouch(0).position;
                pointerPosition = new Vector3(touchPos.x, touchPos.y, 0) + new Vector3(-1, 1, 0) * 50;
                dndPosition = pointerPosition + new Vector3(-1, 1, 0) * 100;
            }
#endif
#if UNITY_STANDALONE || UNITY_EDITOR
            pointerPosition = Input.mousePosition;
            dndPosition = Input.mousePosition + Vector3.down * 50;
#endif
            DragAndDropFruiton.transform.position = dndPosition;

            var dropGridPosition =
                TeamGrid.SuggestFruitonAtMousePosition(pointerPosition, draggedFruiton, teamDragGridPosition);

            if (dropGridPosition == null)
            {
                DragAndDropFruiton.SetDropStatus(
                    isDraggingFromTeam
                        ? FridgeDndFruiton.DropStatus.Delete
                        : FridgeDndFruiton.DropStatus.Nothing
                );
            }
            else
            {
                DragAndDropFruiton.SetDropStatus(
                    isDraggingFromTeam
                        ? FridgeDndFruiton.DropStatus.Swap
                        : FridgeDndFruiton.DropStatus.Ok
                );
            }

            // stop drag and drop
            if (
                (!isAddingFromDetail && !Input.GetMouseButton(0))
                || (isAddingFromDetail && Input.GetMouseButton(0)))
            {
                HideTooltip();
                isDragging = false;
                isAddingFromDetail = false;
                DragAndDropFruiton.gameObject.SetActive(false);
                DragAndDropBarrier.gameObject.SetActive(false);
                TeamGrid.CancelHighlights();
                ScrollRect.horizontal = true;

                if (isDraggingFromTeam)
                {
                    if (dropGridPosition == null)
                    {
                        RemoveTeamMember(teamDragGridPosition);
                    }
                    else
                    {
                        SwapTeamMembers(teamDragGridPosition, dropGridPosition);
                    }
                }
                else if (dropGridPosition != null)
                {
                    AddFruitonToTeam(draggedFruiton, dropGridPosition);
                }
                TeamGrid.LoadTeam(teams[selectedTeamIndex].KernelTeam);
                WarningText.transform.parent.gameObject.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReturnToMenu();
        }
    }

    public void CreateNewTeam()
    {
        var newFruitonTeam = new FruitonTeam {Name = GetNextAvailableTeamName()};
        GameManager.Instance.FruitonTeamList.FruitonTeams.Add(newFruitonTeam);
        AddTeamToScene(newFruitonTeam);
        SelectTeam(teams.Count - 1);
        SwitchViewMode(ViewMode.TeamEdit);        
        ButtonNewTeam.interactable = teams.Count < MAX_TEAM_COUNT;
    }

    public void DeleteTeam()
    {
        var deleteIndex = selectedTeamIndex;
        var team = teams[deleteIndex];
        SelectTeam(deleteIndex - 1);
        Destroy(team.gameObject);
        teams.RemoveAt(deleteIndex);
        ReindexTeams();
        ResizeScrollContent(teams.Count);
        GameManager.Instance.FruitonTeamList.FruitonTeams.Remove(team.KernelTeam);
        Serializer.SerializeFruitonTeams();
        PlayerHelper.RemoveFruitonTeam(team.KernelTeam.Name, Debug.Log, Debug.Log);
        ButtonNewTeam.interactable = teams.Count < MAX_TEAM_COUNT;
    }

    public void StartTeamEdit()
    {
        SwitchViewMode(ViewMode.TeamEdit);
    }

    public void EndTeamEdit()
    {
        FridgeFruitonTeam team = teams[selectedTeamIndex];
        FruitonTeam kTeam = teams[selectedTeamIndex].KernelTeam;
        var newName = InputTeamName.text;
        if (kTeam.Name != newName)
        {
            string oldName = kTeam.Name;
            kTeam.Name = newName;
            team.gameObject.GetComponentInChildren<Text>().text = GetTeamDescription(kTeam);
            PlayerHelper.RemoveFruitonTeam(oldName, (r) =>
                {
                    PlayerHelper.UploadFruitonTeam(team.KernelTeam, Debug.Log, Debug.Log);
                },
                Debug.Log);
        }
        else
        {
            team.gameObject.GetComponentInChildren<Text>().text = GetTeamDescription(kTeam);
            PlayerHelper.UploadFruitonTeam(team.KernelTeam, Debug.Log, Debug.Log);
        }
        Serializer.SerializeFruitonTeams();
        SwitchViewMode(ViewMode.TeamSelect);
    }

    public void HideDetail()
    {
        FruitonDetail.gameObject.SetActive(false);
        FruitonDetail.Barrier.gameObject.SetActive(false);
        HideTooltip();
    }

    public void LoadBattle()
    {
        if (GameManager.Instance.CurrentFruitonTeam != null)
        {
            var param = new Dictionary<string, string>
            {
                {Scenes.BATTLE_TYPE, Scenes.GetParam(Scenes.BATTLE_TYPE)}
            };

            var battleType = (BattleType) Enum.Parse(typeof(BattleType), Scenes.GetParam(Scenes.BATTLE_TYPE));
            if (battleType == BattleType.AIBattle)
            {
                var aiModeDropdown = DropdownPanel.GetComponentInChildren<Dropdown>();
                AIType aiMode = aiModes[aiModeDropdown.value].Type;
                GameManager.Instance.PlayerOptions.LastSelectedAIMode = aiModeDropdown.value;
                GameManager.Instance.SavePlayerSettings();
                param.Add(Scenes.AI_TYPE, aiMode.ToString());
                param.Add(Scenes.GAME_MODE, GameMode.Standard.ToString());
            }
            else
            {
                var gameModeDropdown = DropdownPanel.GetComponentInChildren<Dropdown>();
                GameMode gameMode = gameModes[gameModeDropdown.value].Type;
                GameManager.Instance.PlayerOptions.LastSelectedGameMode = gameModeDropdown.value;
                GameManager.Instance.SavePlayerSettings();
                param.Add(Scenes.GAME_MODE, gameMode.ToString());
            }

            Scenes.Load(Scenes.BATTLE_SCENE, param);
        }
    }

    public void ReturnToMenu()
    {
        if (isInTeamManagement)
        {
            GameManager.Instance.FruitonTeamList = new FruitonTeamList();
            foreach (var team in teams)
            {
                GameManager.Instance.FruitonTeamList.FruitonTeams.Add(team.KernelTeam);
            }
        }
        Scenes.Load(Scenes.MAIN_MENU_SCENE);
    }

    private void AddTeamToScene(FruitonTeam team)
    {
        GameObject fruitonTeamObject = Instantiate(FridgeTeamTemplate);
        fruitonTeamObject.transform.SetParent(WrapperTeams.transform);

        var teamIndex = teams.Count;
        var fridgeFruitonTeam = fruitonTeamObject.GetComponent<FridgeFruitonTeam>();
        fridgeFruitonTeam.FridgeIndex = teamIndex;
        fridgeFruitonTeam.KernelTeam = team;
        teams.Add(fridgeFruitonTeam);

        fruitonTeamObject.name = team.Name;
        fruitonTeamObject.transform.localScale = FridgeFruitonTemplate.gameObject.GetComponent<RectTransform>()
            .localScale;
        fruitonTeamObject.transform.localPosition = GetPositionOnScrollViewGrid(teamIndex);
        fruitonTeamObject.GetComponentInChildren<Text>().text = GetTeamDescription(team);
        fruitonTeamObject.GetComponent<Button>().onClick.AddListener(() => SelectTeam(fridgeFruitonTeam.FridgeIndex));
        fruitonTeamObject.SetActive(true);
    }

    private void InitializeFruitonDetailListeners()
    {
        FruitonDetail.CloseButton.onClick.AddListener(HideDetail);
        FruitonDetail.Barrier.onClick.AddListener(HideDetail);
        FruitonDetail.AddToTeamButton.onClick.AddListener(() =>
        {
            HideDetail();
            var availablePositions = TeamGrid.GetAvailableSquares(FruitonDetail.CurrentFruiton);
            if (availablePositions.Count == 1)
            {
                AddFruitonToTeam(FruitonDetail.CurrentFruiton, availablePositions[0]);
            }
            else
            {
                isAddingFromDetail = true;
                BeginFruitonDrag(FruitonDetail.CurrentFruiton);
            }
        });
    }

    private void InitializeTeamGridListeners()
    {
        TeamGrid.OnBeginDragFromTeam.AddListener(BeginFruitonDragFromTeam);
        TeamGrid.OnMouseEnterSquare.AddListener(square =>
        {
            if (!isDragging)
            {
                square.Highlight(new Color(1, 1, 0.8f));
                if (square.KernelFruiton != null)
                {
                    ShowTooltip(square.KernelFruiton, 1);
                }
            }
        });
        TeamGrid.OnMouseExitSquare.AddListener(square =>
        {
            if (!isDragging)
            {
                square.CancelHighlight();
                HideTooltip();
            }
        });
    }

    private void InitializeTeams(bool includeIncomplete)
    {
        GameManager gameManager = GameManager.Instance;
        teams = new List<FridgeFruitonTeam>();
        FridgeTeamTemplate.SetActive(true);
        foreach (FruitonTeam fruitonTeam in gameManager.FruitonTeamList.FruitonTeams)
        {
            if (includeIncomplete || IsTeamComplete(fruitonTeam))
            {
                AddTeamToScene(fruitonTeam);
            }
        }
        defaultTeamIconColor = FridgeTeamTemplate.GetComponent<Image>().color;
        FridgeTeamTemplate.SetActive(false);
    }

    private bool IsTeamComplete(FruitonTeam team)
    {
        int[] fruitonIDsArray = new int[team.FruitonIDs.Count];
        team.FruitonIDs.CopyTo(fruitonIDsArray, 0);
        return FruitonTeamValidator
            .validateFruitonTeam(new Array<int>(fruitonIDsArray), GameManager.Instance.FruitonDatabase).complete;
    }

    private void InitializeAllFruitons()
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
            fFruiton.OnBeginDrag.AddListener(() => BeginFruitonDrag(fFruiton));
#if UNITY_STANDALONE || UNITY_EDITOR
            fFruiton.OnMouseEnter.AddListener(() => ShowTooltip(kFruiton));
            fFruiton.OnMouseExit.AddListener(HideTooltip);
            fFruiton.OnRightClick.AddListener(() => ShowDetail(fFruiton));
#endif
            fFruiton.OnTap.AddListener(() => ShowDetail(fFruiton));
            fFruiton.SetKernelFruiton(kFruiton);
            fFruiton.FridgeIndex = i;
            fridgeFruitons.Add(fFruiton);
            i++;
        }
        FridgeFruitonTemplate.SetActive(false);
        FilterManager.AllFruitons = fridgeFruitons;
        FilterManager.OnFilterUpdated.AddListener(ReindexFruitons);
        FilterManager.UpdateAvailableFruitons(gameManager.AvailableFruitons);
        PlayerHelper.GetAvailableFruitons(FilterManager.UpdateAvailableFruitons, Debug.Log);
    }

    private void AddFruitonToTeam(KFruiton fruiton, Position position)
    {
        var team = teams[selectedTeamIndex].KernelTeam;
        team.FruitonIDs.Add(fruiton.id);
        team.Positions.Add(position);
        TeamGrid.AddFruitonAt(fruiton, position);
    }

    private void SwapTeamMembers(Position pos1, Position pos2)
    {
        var team = teams[selectedTeamIndex].KernelTeam;
        var i1 = team.Positions.IndexOf(pos1);
        var i2 = team.Positions.IndexOf(pos2);
        if (i1 >= 0)
        {
            team.Positions[i1] = pos2;
        }
        if (i2 >= 0)
        {
            team.Positions[i2] = pos1;
        }
    }

    private void RemoveTeamMember(Position position)
    {
        var team = teams[selectedTeamIndex].KernelTeam;
        var index = team.Positions.IndexOf(position);
        team.Positions.RemoveAt(index);
        team.FruitonIDs.RemoveAt(index);
    }

    private void ReindexTeams()
    {
        int newIndex = 0;
        foreach (var team in teams)
        {
            var oldIndex = team.FridgeIndex;
            team.FridgeIndex = newIndex;
            if (newIndex != oldIndex)
            {
                iTween.MoveTo(team.gameObject, iTween.Hash(
                        "position", GetPositionOnScrollViewGrid(newIndex),
                        "islocal", true,
                        "time", 1,
                        "easetype", iTween.EaseType.easeOutExpo
                    )
                );
            }
            newIndex++;
        }
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

    /// <summary>
    /// Finds the next name for the fruiton team in the following way:
    /// "New Team N" where N is the smallest available positive integer,
    /// whilst by available is meant that no other fruiton team has the same name.
    /// </summary>
    /// <returns></returns>
    private string GetNextAvailableTeamName()
    {
        for (int i = 1;; i++)
        {
            string potentialName = "New Team " + i;
            if (teams.All(ft => ft.KernelTeam.Name != potentialName))
            {
                return potentialName;
            }
        }
    }

    /// <summary>
    /// Calculates position of an object (fruiton or team) on the scroll view grid
    /// </summary>
    /// <param name="index">index of the object</param>
    /// <returns>position of an object on the scroll view grid</returns>
    private Vector3 GetPositionOnScrollViewGrid(int index)
    {
        return new Vector3(
            27.5f + (index / 2) * 240,
            -233 - (index % 2) * 231,
            0
        );
    }

    private string GetTeamDescription(FruitonTeam team)
    {
        return String.Format(
            "{0}\n\n({1}/10)",
            team.Name,
            team.FruitonIDs.Count
        );
    }

    private void BeginFruitonDrag(FridgeFruiton fruiton)
    {
        if (fruiton.IsOwned)
        {
            BeginFruitonDrag(fruiton.KernelFruiton);
        }
    }

    private void BeginFruitonDrag(KFruiton fruiton, Position teamPosition = null)
    {
        draggedFruiton = fruiton;
        DragAndDropBarrier.gameObject.SetActive(true);
        DragAndDropFruiton.gameObject.SetActive(true);
        DragAndDropFruiton.SetSkin(fruiton.model);
        ScrollRect.horizontal = false;
        teamDragGridPosition = teamPosition;
        isDragging = true;
        isDraggingFromTeam = teamPosition != null;
#if UNITY_ANDROID && !UNITY_EDITOR
        ShowTooltip(fruiton, 2);
# else
        HideTooltip();
#endif
        var isAnySquareAvailable = TeamGrid.HighlightAvailableSquares(fruiton.type, isDraggingFromTeam);
        if (isAnySquareAvailable)
        {
            if (isDraggingFromTeam)
            {
                WarningText.text =
                    "<color=#5555ff>Move</color> fruiton to other <color=#00ff00>available square</color> or <color=#ff0000>remove</color> it from the team";
            }
            else if (isAddingFromDetail)
            {
                WarningText.text =
                    "Choose an <color=#00ff00>available square</color> or click anywhere else to cancel";
            }
            else
            {
                WarningText.text =
                    "Drag the fruiton to any of the <color=#00ff00>available squares</color> to add it to the team";
            }
            WarningText.color = Color.white;
        }
        else
        {
            WarningText.text = "You don't have any empty square for fruiton of this type!";
            WarningText.color = Color.red;
        }
        WarningText.transform.parent.gameObject.SetActive(true);
    }

    private void BeginFruitonDragFromTeam(KFruiton fruiton, Position position)
    {
        if (viewMode == ViewMode.TeamEdit)
        {
            BeginFruitonDrag(fruiton, position);
        }
    }

    private void SelectTeam(int index)
    {
        var isInvalidIndex = index < 0 || index >= teams.Count;

        ButtonPlay.interactable = !isInvalidIndex;
        ButtonEdit.interactable = !isInvalidIndex;
        ButtonDelete.interactable = !isInvalidIndex;

        if (isInvalidIndex)
        {
            selectedTeamIndex = -1;
            GameManager.Instance.CurrentFruitonTeam = null;
            TeamGrid.ResetTeam();
            return;
        }

        if (selectedTeamIndex >= 0)
        {
            teams[selectedTeamIndex].gameObject.GetComponent<Image>().color = defaultTeamIconColor;
        }

        teams[index].gameObject.GetComponent<Image>().color = new Color(0.55f, 0.85f, 1);
        selectedTeamIndex = index;
        var newTeam = teams[selectedTeamIndex].KernelTeam;
        InputTeamName.text = newTeam.Name;
        GameManager.Instance.CurrentFruitonTeam = newTeam;
        TeamGrid.LoadTeam(newTeam);
    }

    private void ShowDetail(FridgeFruiton fruiton)
    {
        FruitonDetail.SetFruiton(fruiton, TeamGrid.GetAvailableSquares(fruiton.KernelFruiton).Count != 0);
        FruitonDetail.TooltipText.text = TooltipUtil.GenerateTooltip(fruiton.KernelFruiton);
        FruitonDetail.gameObject.SetActive(true);
    }

    private void ShowTooltip(KFruiton fruiton, int positionIndex = 0)
    {
        var targetTransform = TooltipPanelPositions[positionIndex];
        var tooltipTransform = PanelTooltip.GetComponent<RectTransform>();
        PanelTooltip.SetActive(true);
        PanelTooltip.transform.SetParent(targetTransform.parent);
        tooltipTransform.pivot = targetTransform.pivot;
        tooltipTransform.anchorMin = targetTransform.anchorMin;
        tooltipTransform.anchorMax = targetTransform.anchorMax;
        tooltipTransform.anchoredPosition = targetTransform.anchoredPosition;
        PanelTooltip.GetComponentInChildren<Text>().text = TooltipUtil.GenerateTooltip(fruiton);
    }

    private void HideTooltip()
    {
        PanelTooltip.SetActive(false);
    }

    private void SwitchViewMode(ViewMode viewMode)
    {
        this.viewMode = viewMode;

        var isEditing = viewMode == ViewMode.TeamEdit;

        WrapperFruitons.SetActive(isEditing);
        Filters.SetActive(isEditing);
        TeamGrid.AllowEdit = isEditing;
        ButtonDone.gameObject.SetActive(isEditing);

        WrapperTeams.SetActive(!isEditing);
        ButtonNewTeam.gameObject.SetActive(!isEditing);
        ButtonEdit.gameObject.SetActive(!isEditing);
        ButtonDelete.gameObject.SetActive(!isEditing);
        ButtonBack.gameObject.SetActive(!isEditing);

        switch (viewMode)
        {
            case ViewMode.TeamSelect:
                ResizeScrollContent(teams.Count);
                break;
            case ViewMode.TeamEdit:
                ResizeScrollContent(GameManager.Instance.AllPlayableFruitons.Count());
                break;
        }
    }

    private void ResizeScrollContent(int objectCount)
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

    public static IEnumerable<Position> CreatePositionsForArtificialTeam(IEnumerable<KFruiton> fruitons)
    {
        var result = new List<Position>();
        int i, j;
        int majorRow = 0;
        int minorRow = 1;
        int majorCounter = 2;
        int minorCounter = 2;
        foreach (KFruiton kernelFruiton in fruitons)
        {
            switch ((FruitonType)kernelFruiton.type)
            {
                case FruitonType.KING:
                {
                    i = GameState.WIDTH / 2;
                    j = majorRow;
                }
                    break;
                case FruitonType.MAJOR:
                {
                    i = GameState.WIDTH / 2 - majorCounter;
                    j = majorRow;
                    if (--majorCounter == 0) --majorCounter;
                }
                    break;
                case FruitonType.MINOR:
                {
                    i = GameState.WIDTH / 2 - minorCounter;
                    j = minorRow;
                    --minorCounter;
                }
                    break;
                default:
                {
                    throw new UndefinedFruitonTypeException();
                }
            }
            result.Add(new Position { X = i, Y = j });
        }
        return result;
    }
}