using Cz.Cuni.Mff.Fruiton.Dto;
using fruiton.fruitDb;
using fruiton.fruitDb.factories;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KFruiton = fruiton.kernel.Fruiton;
using System;
using System.Linq;
using fruiton.kernel;
using fruiton.kernel.fruitonTeam;
using Google.Protobuf.Collections;
using haxe.root;
using UnityEngine.SceneManagement;
using Networking;
using Spine.Unity;

public class FruitonTeamsManager : MonoBehaviour
{
    enum ViewMode
    {
        TeamSelect,
        TeamEdit
    }

    private const int MaxTeamCount = 16;

    public ScrollRect ScrollRect;
    public FridgeTeamGrid TeamGrid;
    public Image DragAndDropBarrier;
    public GameObject FridgeFruitonTemplate;
    public FridgeDndFruiton DragAndDropFruiton;
    public GameObject FridgeTeamTemplate;
    public GameObject WrapperFruitons;
    public GameObject WrapperTeams;
    public RectTransform ScrollContentRectTransform;
    public Button ButtonPlay;
    public Button ButtonNewTeam;
    public Button ButtonDelete;
    public Button ButtonEdit;
    public Button ButtonBack;
    public Button ButtonDone;
    public InputField InputTeamName;
    public Text WarningText;


    private ViewMode viewMode;
    private List<FridgeFruitonTeam> teams;
    private int selectedTeamIndex = 0;
    private Color defaultTeamIconColor;
    private bool isDragging = false;
    private bool isDraggingFromTeam = false;
    private Position teamDragGridPosition;
    private KFruiton draggedFruiton;

    /// <summary> true if player is actually editing teams, false if only viewing/picking </summary>
    private bool teamManagementState;

    /// <summary> Name of scene param, true if player is actually editing teams, false if only viewing/picking</summary>
    public static readonly string TEAM_MANAGEMENT_STATE = "teamManagementState";

    // Use this for initialization
    void Start()
    {
        teamManagementState = bool.Parse(Scenes.GetParam(TEAM_MANAGEMENT_STATE));
        InitializeTeams(teamManagementState);
        SwitchViewMode(viewMode);
        if (teamManagementState)
        {
            ButtonPlay.gameObject.SetActive(false);
            InitializeAllFruitons();
        }
        else
        {
            ButtonNewTeam.gameObject.SetActive(false);
            ButtonEdit.gameObject.SetActive(false);
            ButtonDelete.gameObject.SetActive(false);
        }
        TeamGrid.OnBeginDragFromTeam.AddListener(BeginFruitonDragFromTeam);
        if (viewMode == ViewMode.TeamSelect)
        {
            SelectTeam(0);
        }
        DragAndDropFruiton.gameObject.SetActive(false);
    }

    void Update()
    {
        // drag and drop logic
        if (isDragging)
        {
            Vector3 dndPosition = Vector3.zero;
            Vector3 pointerPosition = Vector3.zero;
#if UNITY_ANDROID
            if (Input.touchCount > 0)
            {
                var touchPos = Input.GetTouch(0).position;
                pointerPosition = new Vector3(touchPos.x, touchPos.y, 0) + new Vector3(-1,1,0) * 50;
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
                if (isDraggingFromTeam)
                {
                    DragAndDropFruiton.SetDropStatus(FridgeDndFruiton.DropStatus.Delete);
                }
                else
                {
                    DragAndDropFruiton.SetDropStatus(FridgeDndFruiton.DropStatus.Nothing);
                }
            }
            else
            {
                if (isDraggingFromTeam)
                {
                    DragAndDropFruiton.SetDropStatus(FridgeDndFruiton.DropStatus.Swap);
                }
                else
                {
                    DragAndDropFruiton.SetDropStatus(FridgeDndFruiton.DropStatus.Ok);
                }
            }

            // stop drag and drop
            if (!Input.GetMouseButton(0))
            {
                isDragging = false;
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
    }

    public void CreateNewTeam()
    {
        var newFruitonTeam = new FruitonTeam {Name = GetNextAvailableTeamName()};
        AddTeamToScene(newFruitonTeam);
        SelectTeam(teams.Count - 1);
        SwitchViewMode(ViewMode.TeamEdit);
        ButtonNewTeam.interactable = teams.Count < MaxTeamCount;
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
        PlayerHelper.RemoveFruitonTeam(team.KernelTeam, Debug.Log, Debug.Log);
        ButtonNewTeam.interactable = teams.Count < MaxTeamCount;
    }

    public void StartTeamEdit()
    {
        SwitchViewMode(ViewMode.TeamEdit);
    }

    public void EndTeamEdit()
    {
        var team = teams[selectedTeamIndex];
        var kTeam = teams[selectedTeamIndex].KernelTeam;
        var newName = InputTeamName.text;
        if (kTeam.Name != newName)
        {
            PlayerHelper.RemoveFruitonTeam(team.KernelTeam, (r) =>
                {
                    kTeam.Name = newName;
                    team.gameObject.GetComponentInChildren<Text>().text = GetTeamDescription(kTeam);
                    PlayerHelper.UploadFruitonTeam(team.KernelTeam, Debug.Log, Debug.Log);
                },
                Debug.Log);
        }
        else
        {
            PlayerHelper.UploadFruitonTeam(team.KernelTeam, Debug.Log, Debug.Log);
        }
        SwitchViewMode(ViewMode.TeamSelect);
    }

    public void LoadBattle()
    {
        if (GameManager.Instance.CurrentFruitonTeam != null)
        {
            Scenes.Load(Scenes.BATTLE_SCENE, Scenes.ONLINE, Scenes.GetParam(Scenes.ONLINE));
        }
    }

    public void ReturnToMenu()
    {
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
        fruitonTeamObject.transform.localPosition = getPositionOnScrollViewGrid(teamIndex);
        fruitonTeamObject.GetComponentInChildren<Text>().text = GetTeamDescription(team);
        fruitonTeamObject.GetComponent<Button>().onClick.AddListener(() => SelectTeam(fridgeFruitonTeam.FridgeIndex));
        fruitonTeamObject.SetActive(true);
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
        SelectTeam(selectedTeamIndex);
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
        // TODO: FILTER UNAVAILABLE FRUITONS 
        // PlayerHelper.GetAvailableFruitons(UpdateAvailableFruitons, Debug.Log);
        List<GameObject> fruitons = new List<GameObject>();
        GameManager gameManager = GameManager.Instance;
        IEnumerable<KFruiton> allFruitons = gameManager.AllFruitons;
        var i = 0;
        var prefabRectTransform = FridgeFruitonTemplate.gameObject.GetComponent<RectTransform>();
        foreach (KFruiton fruiton in allFruitons)
        {
            var fridgeFruiton = Instantiate(FridgeFruitonTemplate);
            fridgeFruiton.transform.SetParent(WrapperFruitons.transform);
            fridgeFruiton.transform.localScale = prefabRectTransform.localScale;
            fridgeFruiton.transform.localPosition = getPositionOnScrollViewGrid(i);

            var spineSkeleton = fridgeFruiton.GetComponentInChildren<SkeletonGraphic>();
            spineSkeleton.Skeleton.SetSkin(fruiton.model);
            var kFruiton = fruiton;
            fridgeFruiton.GetComponent<FridgeFruiton>()
                .OnBeginDrag.AddListener(() => BeginFruitonDrag(kFruiton));
            i++;
        }
        FridgeFruitonTemplate.SetActive(false);
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
                        "position", getPositionOnScrollViewGrid(newIndex),
                        "islocal", true,
                        "time", 1,
                        "easetype", iTween.EaseType.easeOutExpo
                    )
                );
            }
            newIndex++;
        }
    }

    /// <summary>
    /// Finds the next name for the fruiton team in the following way:
    /// "New Team N" where N is the smallest available positive integer,
    /// whilst by available is meant that no other fruiton team has the same name.
    /// </summary>
    /// <returns></returns>
    private string GetNextAvailableTeamName()
    {
        for (int i = 1; ; i++)
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
    private Vector3 getPositionOnScrollViewGrid(int index)
    {
        return new Vector3(
            27.5f + (index / 2) * 240,
            -225 - (index % 2) * 230,
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

        var isAnySquareAvailable = TeamGrid.HighlightAvailableSquares(fruiton.type, isDraggingFromTeam);
        if (!isAnySquareAvailable)
        {
            WarningText.text = "You don't have any empty square for fruiton of this type!";
            WarningText.transform.parent.gameObject.SetActive(true);
        }
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

        teams[index].gameObject.GetComponent<Image>().color = new Color(0.55f,0.85f,1);
        selectedTeamIndex = index;
        var newTeam = teams[selectedTeamIndex].KernelTeam;
        InputTeamName.text = newTeam.Name;
        GameManager.Instance.CurrentFruitonTeam = newTeam;
        TeamGrid.LoadTeam(newTeam);
    }

    private void SwitchViewMode(ViewMode viewMode)
    {
        this.viewMode = viewMode;

        var isEditing = viewMode == ViewMode.TeamEdit;

        WrapperFruitons.SetActive(isEditing);
        TeamGrid.AllowEdit = isEditing;
        ButtonDone.gameObject.SetActive(isEditing);
        InputTeamName.gameObject.SetActive(isEditing);

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
                ResizeScrollContent(GameManager.Instance.AllFruitons.Count());
                break;
        }
    }

    private void ResizeScrollContent(int objectCount)
    {
        var contentSize = ScrollContentRectTransform.sizeDelta;
        var helperIndex = objectCount + objectCount % 2;
        var newWidth = getPositionOnScrollViewGrid(helperIndex).x;
        ScrollContentRectTransform.sizeDelta = new Vector2(newWidth, contentSize.y);
    }
}