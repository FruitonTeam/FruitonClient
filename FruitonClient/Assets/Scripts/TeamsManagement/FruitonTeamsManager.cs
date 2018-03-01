using System;
using System.Collections.Generic;
using System.Linq;
using Battle.Model;
using Cz.Cuni.Mff.Fruiton.Dto;
using Exceptions;
using fruiton.kernel;
using fruiton.kernel.fruitonTeam;
using Networking;
using Serialization;
using UI.Form;
using UI.Fridge;
using UnityEngine;
using UnityEngine.UI;
using Fruiton = fruiton.kernel.Fruiton;
using FruitonType = Enums.FruitonType;

namespace TeamsManagement
{
    public enum TeamManagementState
    {
        TEAM_MANAGEMENT,
        ONLINE_CHOOSE,
        LOCAL_CHOOSE_FIRST,
        LOCAL_CHOOSE_SECOND,
        AI_CHOOSE,
        CHALLENGE_CHOOSE
    }

    /// <summary>
    /// Handles team management logic.
    /// </summary>
    public class FruitonTeamsManager : TeamManagerBase
    {
        private class Option<TEnum>
        {
            public string Name { get; private set; }
            public TEnum Type { get; private set; }
            public PickMode PickMode { get; private set; }
        
            public Option(
                string name,
                TEnum type,
                PickMode pickMode = PickMode.StandardPick
            )
            {
                Name = name;
                Type = type;
                PickMode = pickMode;
            }
        }

        private enum ViewMode
        {
            TeamSelect,
            TeamEdit
        }

        public GameObject FridgeTeamTemplate;
        public GameObject WrapperTeams;
        public Button ButtonPlay;
        public Button ButtonNewTeam;
        public Button ButtonDelete;
        public Button ButtonEdit;
        public Button ButtonBack;
        public Button ButtonDone;
        public InputField InputTeamName;
        public GameObject DropdownPanel;
        public Text LocalDuelHeadline;

        private ViewMode viewMode;
        /// <summary> List of  <summary>
        private List<FridgeFruitonTeam> teams;
        /// <summary> Index of currently selected team, -1 if no team is selected. </summary>
        private int selectedTeamIndex;
        /// <summary> True if player can start the game without selecting a team (e.g. draft) </summary>
        private bool canPlayWithoutTeamSelected;

        /// <summary> List of game modes for online play </summary>
        private readonly List<Option<GameMode>> gameModes = new List<Option<GameMode>>
        {
            new Option<GameMode>("Standard", GameMode.Standard),
            new Option<GameMode>("Last man standing", GameMode.LastManStanding),
            new Option<GameMode>("Draft", GameMode.Standard, PickMode.Draft)
        };

        /// <summary> List of game modes for local play </summary>
        private readonly List<Option<GameMode>> localGameModes = new List<Option<GameMode>>
        {
            new Option<GameMode>("Standard", GameMode.Standard),
            new Option<GameMode>("Last man standing", GameMode.LastManStanding)
        };

        /// <summary> List of game modes for playing vs AI. </summary>
        private readonly List<Option<AIType>> aiModes = new List<Option<AIType>>
        {
            new Option<AIType>("Fruiton Bowl", AIType.SportsMen),
            new Option<AIType>("North Pole", AIType.Santas),
            new Option<AIType>("Circus", AIType.Clowns)
        };

        private TeamManagementState state;

        public static readonly int MAX_TEAM_COUNT = 16;
        private static readonly string CHOOSE_OFFLINE_TEAM_TITLE = "Team for Player {0}.";
        private static readonly string CHOOSE_CHALLENGE_TEAM_TITLE = "Select a team to use in challenge";


        /// <returns>true if player is actually editing teams, false if only viewing/picking</returns>
        private bool isInTeamManagement
        {
            get { return state == TeamManagementState.TEAM_MANAGEMENT; }
        }

        private FruitonTeam CurrentFruitonTeam
        {
            get
            {
                if (state == TeamManagementState.LOCAL_CHOOSE_SECOND)
                {
                    return GameManager.Instance.OfflineOpponentTeam;
                }
                return GameManager.Instance.CurrentFruitonTeam;
            }
            set
            {
                if (state == TeamManagementState.LOCAL_CHOOSE_SECOND)
                {
                    GameManager.Instance.OfflineOpponentTeam = value;
                }
                else
                {
                    GameManager.Instance.CurrentFruitonTeam = value;
                }
            
            }
        }

        /// <summary>
        /// Initializes scene based on game mode.
        /// </summary>
        protected override void Start()
        {
            base.Start();
            state = (TeamManagementState) Enum.Parse(typeof(TeamManagementState), Scenes.GetParam(Scenes.TEAM_MANAGEMENT_STATE));
            InitializeTeams(isInTeamManagement);

            SwitchViewMode(viewMode);
            switch (state)
            {
                case TeamManagementState.ONLINE_CHOOSE:
                    OnlineChooseStart();
                    break;
                case TeamManagementState.TEAM_MANAGEMENT:
                    TeamManagementStart();
                    break;
                case TeamManagementState.LOCAL_CHOOSE_FIRST:
                    LocalChooseStart();
                    break;
                case TeamManagementState.LOCAL_CHOOSE_SECOND:
                    LocalChooseSecondStart();
                    break;
                case TeamManagementState.AI_CHOOSE:
                    AIChooseStart();
                    break;
                case TeamManagementState.CHALLENGE_CHOOSE:
                    ChallengeChooseStart();
                    break;
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

        /// <summary>
        /// Initializes scene for game vs AI.
        /// </summary>
        private void AIChooseStart()
        {
            CommonChooseStart();
            PlayerOptions playerOptions = GameManager.Instance.PlayerOptions;
            SetupModeDropdown(aiModes, playerOptions.LastSelectedAIMode);
        }

        /// <summary>
        /// Initializes scene for online play.
        /// </summary>
        private void OnlineChooseStart()
        {
            PlayerOptions playerOptions = GameManager.Instance.PlayerOptions;
            SetupModeDropdown(gameModes, playerOptions.LastSelectedGameMode);
            var dropdown = DropdownPanel.GetComponentInChildren<Dropdown>();
            dropdown.onValueChanged.AddListener(ModeDropDownChanged);
            var gameMode = gameModes[dropdown.value];
            canPlayWithoutTeamSelected = gameMode.PickMode == PickMode.Draft;
            CommonChooseStart();
        }

        /// <summary>
        /// Sets up scene for second player in local duel.
        /// </summary>
        private void LocalChooseSecondStart()
        {
            ButtonPlay.GetComponentInChildren<Text>().text = "Play";
            CommonChooseStart();
            PlayerOptions playerOptions = GameManager.Instance.PlayerOptions;
            SetupModeDropdown(localGameModes, playerOptions.LastSelectedLocalGameMode);
            LocalDuelHeadline.text = String.Format(CHOOSE_OFFLINE_TEAM_TITLE, 2);
        }

        /// <summary>
        /// Initializes scene for local duel.
        /// </summary>
        private void LocalChooseStart()
        {
            ButtonPlay.GetComponentInChildren<Text>().text = "Next";
            PlayerOptions playerOptions = GameManager.Instance.PlayerOptions;
            SetupModeDropdown(localGameModes, playerOptions.LastSelectedLocalGameMode);
            CommonChooseStart();
            LocalDuelHeadline.text = String.Format(CHOOSE_OFFLINE_TEAM_TITLE, 1);
        }

        /// <summary>
        /// Initializes scene for challenge battle.
        /// </summary>
        private void ChallengeChooseStart()
        {
            ButtonPlay.GetComponentInChildren<Text>().text = "Select";
            CommonChooseStart();
            DropdownPanel.SetActive(false);
            LocalDuelHeadline.text = CHOOSE_CHALLENGE_TEAM_TITLE;
        }

        /// <summary>
        /// Initializes scene for team management.
        /// </summary>
        private void TeamManagementStart()
        {
            LocalDuelHeadline.gameObject.SetActive(false);
            ButtonPlay.gameObject.SetActive(false);
            InitializeAllFruitons();
            DropdownPanel.SetActive(false);
            ButtonBack.GetComponentInChildren<Text>().text = "Back";
        }

        /// <summary>
        /// Deactivates team management buttons.
        /// </summary>
        private void CommonChooseStart()
        {
            ButtonNewTeam.gameObject.SetActive(false);
            ButtonEdit.gameObject.SetActive(false);
            ButtonDelete.gameObject.SetActive(false);
        }

        /// <summary>
        /// Activates dropdown and sets dropdown options.
        /// </summary>
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

        /// <summary>
        /// Creates new team, switches to team edit mode and selects it.
        /// </summary>
        public void CreateNewTeam()
        {
            var newFruitonTeam = new FruitonTeam {Name = GetNextAvailableTeamName()};
            GameManager.Instance.FruitonTeamList.FruitonTeams.Add(newFruitonTeam);
            AddTeamToScene(newFruitonTeam, true);
            SelectTeam(teams.Count - 1);
            SwitchViewMode(ViewMode.TeamEdit);        
            ButtonNewTeam.interactable = teams.Count < MAX_TEAM_COUNT;
        }

        /// <summary>
        /// Deletes currently selected team.
        /// </summary>
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

        /// <summary>
        /// Switches view mode to team edit.
        /// </summary>
        public void StartTeamEdit()
        {
            SwitchViewMode(ViewMode.TeamEdit);
        }

        /// <summary>
        /// Saves team that is currently being edited and switches to team select mode.
        /// </summary>
        public void EndTeamEdit()
        {
            FridgeFruitonTeam team = teams[selectedTeamIndex];
            FruitonTeam kTeam = teams[selectedTeamIndex].KernelTeam;
            team.Valid = !TeamContainsMissingFruitons(kTeam);
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

        /// <summary>
        /// Starts battle based on selected game mode.
        /// </summary>
        public void LoadBattle()
        {
            switch (state)
            {
                case TeamManagementState.LOCAL_CHOOSE_SECOND:
                case TeamManagementState.ONLINE_CHOOSE:
                case TeamManagementState.CHALLENGE_CHOOSE:
                    PlayDefault();
                    break;
                case TeamManagementState.TEAM_MANAGEMENT:
                    break;
                case TeamManagementState.LOCAL_CHOOSE_FIRST:
                    ReloadForSecondTeam();
                    break;
                case TeamManagementState.AI_CHOOSE:
                    PlayAI();
                    break;
            }
        }

        /// <summary>
        /// Reloads scene to allow second player pick their team for local duel.
        /// </summary>
        private void ReloadForSecondTeam()
        {
            var battleType = (BattleType)Enum.Parse(typeof (BattleType), Scenes.GetParam(Scenes.BATTLE_TYPE));
            var param = new Dictionary<string, string>
            {
                {Scenes.BATTLE_TYPE, Scenes.GetParam(Scenes.BATTLE_TYPE)},
                {Scenes.TEAM_MANAGEMENT_STATE, TeamManagementState.LOCAL_CHOOSE_SECOND.ToString()},
                {Scenes.GAME_MODE, GetAndSaveGameMode(battleType).ToString()}
            };
            Scenes.Load(Scenes.TEAMS_MANAGEMENT_SCENE, param);
        }

        private void PlayAI()
        {
            var param = new Dictionary<string, string>
            {
                {Scenes.BATTLE_TYPE, Scenes.GetParam(Scenes.BATTLE_TYPE)}
            };
            var aiModeDropdown = DropdownPanel.GetComponentInChildren<Dropdown>();
            AIType aiMode = aiModes[aiModeDropdown.value].Type;
            GameManager.Instance.PlayerOptions.LastSelectedAIMode = aiModeDropdown.value;
            GameManager.Instance.SavePlayerSettings();
            param.Add(Scenes.AI_TYPE, aiMode.ToString());
            param.Add(Scenes.GAME_MODE, GameMode.Standard.ToString());
            Scenes.Load(Scenes.BATTLE_SCENE, param);
        }

        private void PlayDefault()
        {
            var battleType = (BattleType)Enum.Parse(typeof(BattleType), Scenes.GetParam(Scenes.BATTLE_TYPE));
            var param = new Dictionary<string, string>
            {
                {Scenes.BATTLE_TYPE, Scenes.GetParam(Scenes.BATTLE_TYPE)}
            };
            var gameModeDropdown = DropdownPanel.GetComponentInChildren<Dropdown>();
            Option<GameMode> gameMode = gameModes[gameModeDropdown.value];
            param.Add(Scenes.GAME_MODE, GetAndSaveGameMode(battleType).ToString());
            param.Add(Scenes.PICK_MODE, gameMode.PickMode.ToString());

            if (gameMode.PickMode == PickMode.Draft)
            {
                Scenes.Load(Scenes.DRAFT_SCENE, param);
                return;
            }
            Scenes.Load(Scenes.BATTLE_SCENE, param);
        }

        private GameMode GetAndSaveGameMode(BattleType battleType)
        {
            var gameModeDropdown = DropdownPanel.GetComponentInChildren<Dropdown>();
            GameMode gameMode = gameModes[gameModeDropdown.value].Type;

            if (battleType == BattleType.LocalDuel)
                GameManager.Instance.PlayerOptions.LastSelectedLocalGameMode = gameModeDropdown.value;
            else if (battleType == BattleType.OnlineBattle)
                GameManager.Instance.PlayerOptions.LastSelectedGameMode = gameModeDropdown.value;

            GameManager.Instance.SavePlayerSettings();
            return gameMode;
        }

        /// <summary>
        /// Stores all teams and returns to menu.
        /// </summary>
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

        /// <summary>
        /// Creates team game object in a scene.
        /// </summary>
        /// <param name="team">team object</param>
        /// <param name="valid">true if team is valid</param>
        private void AddTeamToScene(FruitonTeam team, bool valid)
        {
            GameObject fruitonTeamObject = Instantiate(FridgeTeamTemplate);
            fruitonTeamObject.transform.SetParent(WrapperTeams.transform);

            var teamIndex = teams.Count;
            var fridgeFruitonTeam = fruitonTeamObject.GetComponent<FridgeFruitonTeam>();
            fridgeFruitonTeam.Valid = valid;
            fridgeFruitonTeam.FridgeIndex = teamIndex;
            fridgeFruitonTeam.KernelTeam = team;
            teams.Add(fridgeFruitonTeam);

            fruitonTeamObject.name = team.Name;
            fruitonTeamObject.transform.localScale = FridgeFruitonTemplate.gameObject.GetComponent<RectTransform>()
                .localScale;
            fruitonTeamObject.transform.localPosition = GetPositionOnScrollViewGrid(teamIndex);
            fruitonTeamObject.GetComponentInChildren<Text>().text = GetTeamDescription(team);
            fruitonTeamObject.GetComponent<Button>().onClick.AddListener(() => SelectTeam(fridgeFruitonTeam.FridgeIndex));
            fruitonTeamObject.GetComponent<Image>().color = valid ? FridgeFruitonTeam.COLOR_DEFAULT : FridgeFruitonTeam.COLOR_INVALID;
            fruitonTeamObject.SetActive(true);
        }

        /// <summary>
        /// Evaluates whether fruiton can be drag and dropped.
        /// </summary>
        /// <param name="fruiton">fruiton to drag</param>
        /// <returns>true if given fruiton can be dragged</returns>
        protected override bool ShouldBeginDrag(FridgeFruiton fruiton)
        {
            return fruiton.IsOwned && fruiton.Count > 0;
        }

        /// <summary>
        /// Hides fruiton detail, adds selected fruiton to the team.
        /// </summary>
        protected override void AddToTeamButtonListener()
        {
            HideDetail();
            var availablePositions = MyTeamGrid.GetAvailableSquares(FruitonDetail.CurrentFruiton);
            if (availablePositions.Count == 1)
            {
                AddFruitonToTeam(FruitonDetail.CurrentFruiton, availablePositions[0]);
            }
            else
            {
                isAddingFromDetail = true;
                BeginFruitonDrag(FruitonDetail.CurrentFruiton);
            }
        }

        /// <summary>
        /// Initializes fruiton drag from the team grid if player is in team edit view mode.
        /// </summary>
        /// <param name="fruiton">fruiton to be draged</param>
        /// <param name="position">position in team from which fruiton is dragged</param>
        protected override void OnBeginDragFromTeamListener(Fruiton fruiton, Position position)
        {
            if (viewMode == ViewMode.TeamEdit)
            {
                BeginFruitonDrag(fruiton, position);
            }
        }

        /// <summary>
        /// Cancels drag and drop, moves fruiton to gived position or removes it from the team.
        /// </summary>
        /// <param name="dropGridPosition">position in team grid where fruiton was dropped,
        /// null if fruiton was dropped outside of the team grid</param>
        protected override void ProcessStopDrag(Position dropGridPosition)
        {
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
            MyTeamGrid.LoadTeam(teams[selectedTeamIndex].KernelTeam, dbFridgeMapping);
        }

        /// <summary>
        /// Processes player's teams and adds them to the scene.
        /// </summary>
        /// <param name="includeInvalid">if true, even teams that aren't valid will be added</param>
        private void InitializeTeams(bool includeInvalid)
        {
            GameManager gameManager = GameManager.Instance;
            teams = new List<FridgeFruitonTeam>();
            FridgeTeamTemplate.SetActive(true);
            foreach (FruitonTeam fruitonTeam in gameManager.FruitonTeamList.FruitonTeams)
            {
                bool containsMissingFruitons = TeamContainsMissingFruitons(fruitonTeam);
                if (includeInvalid || (IsTeamComplete(fruitonTeam) && !containsMissingFruitons))
                {
                    AddTeamToScene(fruitonTeam, !containsMissingFruitons);
                }
            }
            FridgeTeamTemplate.SetActive(false);
        }

        /// <summary>
        /// Checks if the team is missing any fruiton.
        /// </summary>
        /// <param name="team">team to check</param>
        /// <returns>true if the team is complete</returns>
        private bool IsTeamComplete(FruitonTeam team)
        {
            int[] fruitonIDsArray = new int[team.FruitonIDs.Count];
            team.FruitonIDs.CopyTo(fruitonIDsArray, 0);
            return FruitonTeamValidator
                .validateFruitonTeam(new haxe.root.Array<int>(fruitonIDsArray), GameManager.Instance.FruitonDatabase).complete;
        }

        /// <summary>
        /// Checks if the team contains any fruitons that are no longer owned by the player.
        /// </summary>
        /// <param name="team">team to check</param>
        /// <returns>true if the team contains fruitons that are no longer owned by the player</returns>
        private bool TeamContainsMissingFruitons(FruitonTeam team)
        {
            Dictionary<int, int> teamCounts = new Dictionary<int, int>();
            foreach (var id in team.FruitonIDs)
            {
                if (!teamCounts.ContainsKey(id))
                {
                    teamCounts[id] = 0;
                }
                teamCounts[id]++;
            }
            foreach (var r in teamCounts)
            {
                var teamFruitonId = r.Key;
                if(r.Value > GameManager.Instance.AvailableFruitons.Count(id => id == teamFruitonId))
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Add a fruiton to currently selected team.
        /// </summary>
        /// <param name="fruiton">fruiton to add</param>
        /// <param name="position">position in team where the fruiton should be added</param>
        private void AddFruitonToTeam(Fruiton fruiton, Position position)
        {
            FridgeFruiton fridgeFruiton = dbFridgeMapping[fruiton.dbId];
            fridgeFruiton.Count--;
            var team = teams[selectedTeamIndex].KernelTeam;
            team.FruitonIDs.Add(fruiton.dbId);
            team.Positions.Add(position);
            MyTeamGrid.AddFruitonAt(fruiton, position);
        }

        /// <summary>
        /// Swaps two positions in currently selected team.
        /// </summary>
        /// <param name="pos1">1st position to be swapped</param>
        /// <param name="pos2">2nd position to be swapped</param>
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

        /// <summary>
        /// Removes fruiton from currently selected team.
        /// </summary>
        /// <param name="position">position in team from which fruiton should be removed</param>
        private void RemoveTeamMember(Position position)
        {
            FridgeFruiton removedFruiton = dbFridgeMapping[draggedFruiton.dbId];
            removedFruiton.Count++;
            var team = teams[selectedTeamIndex].KernelTeam;
            var index = team.Positions.IndexOf(position);
            team.Positions.RemoveAt(index);
            team.FruitonIDs.RemoveAt(index);
        }

        /// <summary>
        /// Calculates index of each team, set them to correct position by index and resizes the scroll view.
        /// </summary>
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

        /// <summary>
        /// Reindexes fruitons if view mode is team edit.
        /// </summary>
        protected override void ReindexFruitons()
        {
            if (viewMode == ViewMode.TeamEdit)
            {
                base.ReindexFruitons();
            }
        }

        /// <summary>
        /// Finds the next name for the fruiton team in the following way:
        /// "New Team N" where N is the smallest available positive integer,
        /// whilst by available is meant that no other fruiton team has the same name.
        /// </summary>
        /// <returns>next available team name</returns>
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

        /// <param name="team">team to get description of</param>
        /// <returns>text to be displayed on team game object</returns>
        private string GetTeamDescription(FruitonTeam team)
        {
            if (TeamContainsMissingFruitons(team))
            {
                return String.Format(
                    "{0}\n\n(INVALID)",
                    team.Name
                );
            }

            return String.Format(
                "{0}\n\n({1}/10)",
                team.Name,
                team.FruitonIDs.Count
            );
        }

        /// <summary>
        /// Selects and loads fruiton team to team grid.
        /// </summary>
        /// <param name="index">index of selected team</param>
        private void SelectTeam(int index)
        {
            var isValidTeamIndex = IsValidTeamIndex(index);

            ButtonPlay.interactable = isValidTeamIndex || canPlayWithoutTeamSelected;
            ButtonEdit.interactable = isValidTeamIndex;
            ButtonDelete.interactable = isValidTeamIndex;

            if (!isValidTeamIndex)
            {
                selectedTeamIndex = -1;
                CurrentFruitonTeam = null;
                MyTeamGrid.ResetTeam();
                return;
            }

            if (selectedTeamIndex >= 0)
            {
                var lastSelectedTeam = teams[selectedTeamIndex];
                lastSelectedTeam.gameObject.GetComponent<Image>().color = lastSelectedTeam.Valid ? FridgeFruitonTeam.COLOR_DEFAULT : FridgeFruitonTeam.COLOR_INVALID;
            }

            teams[index].gameObject.GetComponent<Image>().color = FridgeFruitonTeam.COLOR_SELECTED;
            selectedTeamIndex = index;
            var newTeam = teams[selectedTeamIndex].KernelTeam;
            InputTeamName.text = newTeam.Name;
            CurrentFruitonTeam = newTeam;
            Dictionary<int, FridgeFruiton> passedDictionary = dbFridgeMapping;
            if (state != TeamManagementState.TEAM_MANAGEMENT)
            {
                passedDictionary = null;
            } 
            MyTeamGrid.LoadTeam(newTeam, passedDictionary);
        }

        private bool IsValidTeamIndex(int index)
        {
            return index >= 0 && index < teams.Count;
        }

        /// <summary>
        /// Switches view mode, disabled and enables corresponding buttons.
        /// </summary>
        /// <param name="viewMode">view mode to switch to</param>
        private void SwitchViewMode(ViewMode viewMode)
        {
            this.viewMode = viewMode;

            var isEditing = viewMode == ViewMode.TeamEdit;

            WrapperFruitons.SetActive(isEditing);
            Filters.SetActive(isEditing);
            MyTeamGrid.AllowEdit = isEditing;
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
                    ReindexFruitons();
                    break;
            }
        }

        /// <summary>
        /// Enables or disables play button based on selected mode in dropdown.
        /// </summary>
        /// <param name="newSelection">index of selected dropdown mode</param>
        private void ModeDropDownChanged(int newSelection)
        {
            var dropdown = DropdownPanel.GetComponentInChildren<Dropdown>();
            var gameMode = gameModes[dropdown.value];
            canPlayWithoutTeamSelected = gameMode.PickMode == PickMode.Draft;
            ButtonPlay.interactable = canPlayWithoutTeamSelected
                                      || IsValidTeamIndex(selectedTeamIndex);
        }
    }
}