using Cz.Cuni.Mff.Fruiton.Dto;
using fruiton.kernel;
using fruiton.kernel.actions;
using fruiton.kernel.events;
using Google.Protobuf.Collections;
using Networking;
using Spine.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Util;
using Action = fruiton.kernel.actions.Action;
using Fruiton = fruiton.kernel.Fruiton;
using KAction = fruiton.kernel.actions.Action;
using KEvent = fruiton.kernel.events.Event;
using KFruiton = fruiton.kernel.Fruiton;
using KVector2 = fruiton.dataStructures.Point;
using UObject = UnityEngine.Object;

public class BattleViewer : MonoBehaviour
{
    public Battle battle;
    public BattleType battleType;
    private Tutorial tutorial;
    private bool isGameStarted;
    public bool IsInputEnabled { get; private set; }

    /// <summary> For handling grid tiles. </summary>
    public GridLayoutManager GridLayoutManager;

    public Button EndTurnButton;
    public Button SurrendButton;
    public Button TutorialContinueButton;
    public Button InfoAndroidButton;
    public Button CancelFindBattleButton;
    public GameObject PanelLoadingGame;
    public Text TimeCounter;
    public Text MyLoginText;
    public Text OpponentLoginText;
    public Image MyAvatar;
    public Image OpponentAvatar;
    public GameObject Board;
    public GameObject FruitonInfoPanel;
    public GameObject TutorialPanel;
    public GameObject MyPanel;
    public GameObject OpponentPanel;
    public GameResultsPanel GameResultsPanel;

    private static readonly Color FOREST_DARK = new Color(25 / 255f, 39 / 255f, 13 / 255f);
    private static readonly Color FOREST_GREEN = new Color(37 / 255f, 89 / 255f, 31 / 255f);
    private Coroutine moveCoroutine;


    /// <summary> Client fruitons stored at their position. </summary>
    public GameObject[,] FruitonsGrid { get; set; }

    public GameMode GameMode { get; private set; }

    public bool IsGameOffline
    {
        get { return !(battleType == BattleType.ChallengeBattle || battleType == BattleType.OnlineBattle); }
    }

    public BattleViewer()
    {
        IsInputEnabled = true;
    }

    public void DisableCancelFindButton()
    {
        CancelFindBattleButton.interactable = false;
    }

    private void Start()
    {
#if UNITY_ANDROID
        InfoAndroidButton.gameObject.SetActive(true);
#endif
        GameResultsPanel.gameObject.SetActive(false);
        GridLayoutManager = GridLayoutManager.Instance;
        FruitonsGrid = GridLayoutManager.MakeNewGrid();

        battleType = (BattleType) Enum.Parse(typeof(BattleType), Scenes.GetParam(Scenes.BATTLE_TYPE));
        GameMode = (GameMode) Enum.Parse(typeof(GameMode), Scenes.GetParam(Scenes.GAME_MODE));

        Debug.Log("playing battle: " + battleType + " in mode: " + GameMode);

        // We come from draft
        object gameReady;
        bool comeFromDraft = Scenes.TryGetObjParam(Scenes.GAME_READY_MSG, out gameReady);

        switch (battleType)
        {
            case BattleType.OnlineBattle:
                battle = new OnlineBattle(this, !comeFromDraft);
                PanelLoadingGame.SetActive(true);
                break;
            case BattleType.LocalDuel:
                battle = new OfflineBattle(this);
                InitializeOfflineGame();
                break;
            case BattleType.AIBattle:
                var aiType = (AIType) Enum.Parse(typeof(AIType), Scenes.GetParam(Scenes.AI_TYPE));
                Debug.Log("Battle vs AI: " + aiType);
                battle = new AIBattle(this, aiType);
                InitializeOfflineGame();
                break;
            case BattleType.ChallengeBattle:
                battle = new OnlineBattle(this, false);
                PanelLoadingGame.SetActive(true);
                break;
            case BattleType.TutorialBattle:
                battle = new AIBattle(this, AIType.Tutorial);
                TutorialPanel.SetActive(true);
                InitializeOfflineGame();
                tutorial = new Tutorial(this);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        battle.OnEnable();

        if (comeFromDraft)
        {
            Debug.Assert(battleType == BattleType.OnlineBattle);
            ((OnlineBattle)battle).ProcessMessage((GameReady)gameReady);
        }
    }

    public void HighlightNameTags(bool firstsTurn)
    {
        if (firstsTurn)
        {
            MyPanel.GetComponent<Image>().color = FOREST_GREEN;
            OpponentPanel.GetComponent<Image>().color = FOREST_DARK;
        }
        else
        {
            MyPanel.GetComponent<Image>().color = FOREST_DARK;
            OpponentPanel.GetComponent<Image>().color = FOREST_GREEN;
        }
    }

    public void HighlightEndTurnButton(bool highlight)
    {
        string prefabName;
        if (highlight)
        {
            prefabName = "Circle"; 
        }
        else
        {
            prefabName = "CircleYellow";
        }
        EndTurnButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/UI/Battle/" + prefabName);
    }

    private void Update()
    {
        if (!isGameStarted)
            return;
        UpdateTimer();
        battle.Update();
        if (battleType == BattleType.TutorialBattle)
        {
            tutorial.Update();
        }
        else if (!GameManager.Instance.IsInputBlocked)
        {
            DefaultUpdate();
        }
    }

    public void DefaultUpdate()
    {
        if (Input.GetMouseButtonUp(0) && IsInputEnabled)
        {
            HandleLeftButtonUp();
        }
        else
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            HoverLogic();
#endif
        }
    }

    public void HandleLeftButtonUp()
    {
#if UNITY_ANDROID
        if (InfoAndroidButton.GetComponent<Image>().color == Color.white)
        {
            LeftButtonUpLogic();
        }
        else
        {
            HoverLogic();
        }
#else

        LeftButtonUpLogic();
#endif
    }

    private void OnDisable()
    {
        if (battle != null)
        {
            battle.OnDisable();
        }
    }

    private void InitializeOfflineGame()
    {
        isGameStarted = true;
        InitializePlayersInfo(false);
        SetupSurrenderButton();
    }

    public void SetupSurrenderButton()
    {
        SurrendButton.onClick.RemoveAllListeners();
        SurrendButton.onClick.AddListener(Surrender);
    }

    public void InitializePlayersInfo(bool loadImages = true)
    {
        string login1 = battle.Player1.Name;
        string login2 = battle.Player2.Name;
        MyLoginText.text = login1;
        OpponentLoginText.text = login2;
        
        if (loadImages)
        {
            MyAvatar.sprite = SpriteUtils.TextureToSprite(GameManager.Instance.Avatar);
            PlayerHelper.GetAvatar(login2,
                texture => OpponentAvatar.sprite = SpriteUtils.TextureToSprite(texture),
                Debug.Log);
        }
    }

    public void LeftButtonUpLogic(RaycastHit[] raycastHits = null)
    {
        if (raycastHits == null)
        {
            FruitonInfoPanel.SetActive(false);
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            raycastHits = Physics.RaycastAll(ray);
        }
        battle.LeftButtonUpEvent(raycastHits);
    }

    public void HoverLogic()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit[] raycastHits = Physics.RaycastAll(ray);
        RaycastHit tileHit = raycastHits.FirstOrDefault(hit => GridLayoutManager.ContainsTile(hit.transform.gameObject));
        if (!tileHit.Equals(default(RaycastHit)))
        {
            KVector2 tilePosition = GridLayoutManager.GetIndicesOfTile(tileHit.transform.gameObject);
            GameObject hitFruiton = FruitonsGrid[tilePosition.x, tilePosition.y];
            if (hitFruiton != null)
            {
                UpdateAndShowTooltip(hitFruiton);
                return;
            }
        }
        FruitonInfoPanel.SetActive(false);
    }

    private void UpdateAndShowTooltip(GameObject fruitonObject)
    {
        var clientFruiton = fruitonObject.GetComponent<ClientFruiton>();
        Fruiton kernelFruiton = clientFruiton.KernelFruiton;
        var fruitonInfo = TooltipUtil.GenerateTooltip(kernelFruiton);
        FruitonInfoPanel.SetActive(true);
        FruitonInfoPanel.GetComponentInChildren<Text>().text = fruitonInfo;
    }

    /// <summary>
    ///     Starts the online game. In case the local player is the second one, rotates the view so that he could see his
    ///     fruitons
    ///     at the bottom side of his screen.
    /// </summary>
    public void StartOnlineGame(bool isLocalPlayerFirst)
    {
        if (!isLocalPlayerFirst)
        {
            FlipFruitons();
            Vector3 oldPosition = Camera.main.transform.position;
            Camera.main.transform.position = new Vector3(-oldPosition.x, oldPosition.y, oldPosition.z);
            Vector3 oldEulerAngles = Camera.main.transform.eulerAngles;
            Camera.main.transform.eulerAngles = new Vector3(oldEulerAngles.x, oldEulerAngles.y + 180, oldEulerAngles.z);
        }
        PanelLoadingGame.SetActive(false);
        isGameStarted = true;
        SetupSurrenderButton();
    }

    public void FlipFruitons()
    {
        foreach (GameObject fruiton in FruitonsGrid)
        {
            if (fruiton != null)
            {
                var clientFruiton = fruiton.GetComponent<ClientFruiton>();
                clientFruiton.FlipAround();
            }
        }
    }

    public void InitializeTeam(
        IEnumerable<GameObject> currentTeam, 
        Player player, 
        bool isPlayerLeft,
        Position[] fruitonsPositions = null
    )
    {
        var counter = 0;
        int i = 0, j = 0;
        foreach (var clientFruiton in currentTeam)
        {
            var kernelFruiton = clientFruiton.GetComponent<ClientFruiton>().KernelFruiton;
            var anim = clientFruiton.GetComponentInChildren<SkeletonAnimation>();
            if (isPlayerLeft)
            {
                anim.Skeleton.FlipX = true;
            }

            kernelFruiton.owner = player;
            clientFruiton.gameObject.AddComponent<BoxCollider>();
            if (fruitonsPositions != null)
            {
                var currentPosition = fruitonsPositions[counter];
                i = currentPosition.X;
                j = currentPosition.Y;
                counter++;
            }
            FruitonsGrid[i, j] = clientFruiton;
            kernelFruiton.position = new KVector2(i, j);
            var cellPosition = GridLayoutManager.GetCellPosition(i, j);
            clientFruiton.transform.position = cellPosition;
        }
    }

    public void InitializeMap(List<List<Tile>> field)
    {
        UObject obstacleResource = Resources.Load("Prefabs/Obstacle");
        foreach (List<Tile> tiles in field)
        {
            foreach (Tile tile in tiles)
            {
                if (tile.type == TileType.impassable)
                {
                    var obstacle = (GameObject)Instantiate(obstacleResource);
                    Vector3 pos = GridLayoutManager.GetCellPosition(tile.position.x, tile.position.y);
                    GridLayoutManager.MarkAsObstacle(tile.position.x, tile.position.y);
                    obstacle.transform.position = pos;
                    obstacle.transform.parent = Board.transform;
                }
            }
        }
    }

    public void UpdateTimer()
    {
        var timeLeft = battle.ComputeRemainingTime();
        TimeCounter.text = timeLeft.ToString();
    }

    public LazyDictionary<int, List<TargetableAction>> VisualizeAvailableTargetableActions(KVector2 indices)
    {
        var result = new LazyDictionary<int, List<TargetableAction>>();
        List<Action> allActions = battle.GetAllValidActionFrom(indices);
        Fruiton kernelFruiton = battle.GetFruiton(indices);
        foreach (Action action in allActions)
        {
            VisualizeAction(action, kernelFruiton);
            TargetableAction castAction = action as TargetableAction;
            if (castAction != null)
                result[action.getId()].Add(castAction);
        }
        return result;
    }

    public void ProcessEvent(KEvent kEvent)
    {
        var eventType = kEvent.GetType();
        if (eventType == typeof(MoveEvent))
            ProcessMoveEvent((MoveEvent)kEvent);
        else if (eventType == typeof(AttackEvent))
            ProcessAttackEvent((AttackEvent)kEvent);
        else if (eventType == typeof(DeathEvent))
            ProcessDeathEvent((DeathEvent)kEvent);
        else if (eventType == typeof(ModifyAttackEvent))
            ProcessModifyAttackEvent((ModifyAttackEvent)kEvent);
        else if (eventType == typeof(HealEvent))
            ProcessHealEvent((HealEvent)kEvent);
        else if (eventType == typeof(ModifyHealthEvent))
            ProcessModifyHealthEvent((ModifyHealthEvent)kEvent);
        else if (eventType == typeof(GameOverEvent))
            ProcessGameOverEvent((GameOverEvent)kEvent);
        else if (eventType == typeof(TimeExpiredEvent))
            ProcessTimeExpiredEvent((TimeExpiredEvent)kEvent);
        else if (eventType == typeof(EndTurnEvent))
            ProcessEndTurnEvent((EndTurnEvent)kEvent);
    }

    private void ProcessEndTurnEvent(EndTurnEvent kEvent)
    {
        var endTurnObject = EndTurnButton.gameObject;
        float zEulerAngles = endTurnObject.transform.eulerAngles.z;
        float rotateToZ = zEulerAngles == 0 ? -180 : 0;
        iTween.RotateTo(endTurnObject, rotateToZ * Vector3.forward, 1);
        HighlightNameTags(battle.IsPlayerActive(battle.Player1));
        HighlightEndTurnButton(false);
    }

    private void ProcessTimeExpiredEvent(TimeExpiredEvent kEvent)
    {
        Debug.Log("Time expired.");
    }

    private void ProcessModifyHealthEvent(ModifyHealthEvent kEvent)
    {
        KVector2 kEventPosition = kEvent.position;
        var clientFruiton = FruitonsGrid[kEventPosition.x, kEventPosition.y].GetComponent<ClientFruiton>();
        clientFruiton.ModifyHealth(kEvent.newHealth);
    }

    private void ProcessHealEvent(HealEvent kEvent)
    {
        KVector2 healerPosition = kEvent.source;
        GameObject attacker = FruitonsGrid[healerPosition.x, healerPosition.y];
        attacker.GetComponentInChildren<BoyFighterBattleAnimator>().Cast(() => AfterHealAnimation(kEvent));
    }

    private void AfterHealAnimation(HealEvent kEvent)
    {
        KVector2 kEventPosition = kEvent.target;
        var clientFruiton = FruitonsGrid[kEventPosition.x, kEventPosition.y].GetComponent<ClientFruiton>();
        clientFruiton.ReceiveHeal(kEvent.heal);
        ShowFloatingText(clientFruiton.transform.position, kEvent.heal);
    }

    private void ProcessModifyAttackEvent(ModifyAttackEvent kEvent)
    {
        KVector2 kEventPosition = kEvent.position;
        var clientFruiton = FruitonsGrid[kEventPosition.x, kEventPosition.y].GetComponent<ClientFruiton>();
        clientFruiton.ModifyAttack(kEvent.newAttack);
    }

    private void ProcessDeathEvent(DeathEvent kEvent)
    {
        var killedPos = kEvent.fruiton.position;
        var killed = FruitonsGrid[killedPos.x, killedPos.y];
        Destroy(killed);
        FruitonsGrid[killedPos.x, killedPos.y] = null;
    }

    private void ProcessAttackEvent(AttackEvent kEvent)
    {
        KVector2 attackerPosition = kEvent.source;
        GameObject attacker = FruitonsGrid[attackerPosition.x, attackerPosition.y];
        attacker.GetComponentInChildren<BoyFighterBattleAnimator>().Attack(() => AfterAttackAnimation(kEvent));
    }

    private void AfterAttackAnimation(AttackEvent kEvent)
    {
        KVector2 damagedPosition = kEvent.target;
        GameObject damaged = FruitonsGrid[damagedPosition.x, damagedPosition.y];
        Vector3 damagedWorldPosition = GridLayoutManager.GetCellPosition(damagedPosition.x, damagedPosition.y);
        if (damaged != null)
        {
            damaged.GetComponent<ClientFruiton>().TakeDamage(kEvent.damage);
        }
        
        ShowFloatingText(damagedWorldPosition, -kEvent.damage);
    }

    private void ProcessMoveEvent(MoveEvent moveEvent)
    {
        IsInputEnabled = false;
        KVector2 from = moveEvent.from;
        KVector2 to = moveEvent.to;
        GameObject movedObject = FruitonsGrid[from.x, from.y];
        FruitonsGrid[to.x, to.y] = movedObject;
        FruitonsGrid[from.x, from.y] = null;
        Vector3 toPosition = GridLayoutManager.GetCellPosition(to.x, to.y);
        moveCoroutine = StartCoroutine(MoveCoroutine(movedObject.transform.position, toPosition, movedObject));
        GridLayoutManager.ResetHighlights();
    }

    private void ProcessGameOverEvent(GameOverEvent gameOverEvent)
    {
        if (!IsGameOffline)
        {
            return;
        }

        string winnerName = null;
        var losers = gameOverEvent.losers.ToList();
        if (!losers.Contains(battle.Player1.ID))
        {
            winnerName = battle.Player1.Name;
        }
        else if (!losers.Contains(battle.Player2.ID))
        {
            winnerName = battle.Player2.Name;
        }

        var message = new GameOver
        {
            Reason = Cz.Cuni.Mff.Fruiton.Dto.GameOver.Types.Reason.Standard,
            WinnerLogin = winnerName,
            GameRewards = new GameRewards()
        };
        GameOver(message);
    }

    private void ShowFloatingText(Vector3 position, int amount)
    {
        GameObject floatingText = Instantiate(Resources.Load<GameObject>("Models/Battle/TextChange"));
        if ((battleType == BattleType.OnlineBattle || battleType == BattleType.ChallengeBattle) && !((OnlineBattle)battle).IsLocalPlayerFirst)
        {
            Vector3 eulerAngles = floatingText.transform.eulerAngles;
            floatingText.transform.eulerAngles = new Vector3(eulerAngles.x, -eulerAngles.y, eulerAngles.z);
        }
        floatingText.transform.position = position;
        floatingText.transform.parent = GridLayoutManager.transform;
        bool heal = amount > 0;
        string sign = heal ? "+" : "";
        var textMesh = floatingText.GetComponent<TextMesh>();
        textMesh.text = sign + amount;
        textMesh.color = heal ? Color.green : Color.red;
    }

    private IEnumerator MoveCoroutine(Vector3 from, Vector3 to, GameObject movedObject)
    {
        var anim = movedObject.GetComponentInChildren<FruitonBattleAnimator>();

        bool isFlipped = false;
        if ((anim.SkeletonAnim.Skeleton.FlipX && from.z > to.z ||
             !anim.SkeletonAnim.Skeleton.FlipX && from.z < to.z))
        {
            isFlipped = true;
            anim.SkeletonAnim.Skeleton.FlipX = !anim.SkeletonAnim.Skeleton.FlipX;
        }

        float currentTime = 0.0f;
        Vector3 direction = to - from;
        anim.StartWalking();

        float distance, previousDistance = float.MaxValue;
        while ((distance = Vector3.Distance(movedObject.transform.position, to)) > 0.05f &&
            distance <= previousDistance) // Are we still going closer?
        {
            previousDistance = distance;
            currentTime += Time.deltaTime;
            Vector3 moveVector = direction * currentTime;
            movedObject.transform.position = from + moveVector;
            yield return null;
        }

        movedObject.transform.position = to; // Always make sure we made it exactly there
        anim.StopWalking();
        if (isFlipped)
            anim.SkeletonAnim.Skeleton.FlipX = !anim.SkeletonAnim.Skeleton.FlipX;

        IsInputEnabled = true;
    }

    private void VisualizeAction(KAction action, KFruiton kernelFruiton)
    {
        var type = action.GetType();
        if (type == typeof(MoveAction))
        {
            var moveAction = (MoveAction) action;
            var target = ((MoveActionContext) moveAction.actionContext).target;
            //Debug.Log("Highlight x=" + target.x + " y=" + target.y);
            GridLayoutManager.HighlightCell(target.x, target.y, Color.blue);
            VisualizePossibleAttacks(target, kernelFruiton);
        }
        else if (type == typeof(AttackAction))
        {
            var attackAction = (AttackAction) action;
            var target = ((AttackActionContext) attackAction.actionContext).target;
            GridLayoutManager.HighlightCell(target.x, target.y, Color.red);
        }
        else if (type == typeof(HealAction))
        {
            var healAction = (HealAction) action;
            var target = ((HealActionContext)healAction.actionContext).target;
            GridLayoutManager.HighlightCell(target.x, target.y, Color.green);
        }
    }

    private void VisualizePossibleAttacks(KVector2 potentialPosition, KFruiton kernelFruiton)
    {
        var potentialTargets = battle.ComputePossibleAttacks(potentialPosition, kernelFruiton);
        foreach (var potentialTarget in potentialTargets)
        {
            if (!GridLayoutManager.IsTileAttack(potentialTarget.x, potentialTarget.y))
            {
                GridLayoutManager.HighlightCell(potentialTarget.x, potentialTarget.y, Color.yellow);
            }
        }  
    }

    public void EndTurn()
    {
        if (battleType == BattleType.TutorialBattle)
        {
            tutorial.EndAndNextStage();
        }
        DisableEndTurnButton();
        GridLayoutManager.ResetHighlights();
        battle.EndTurnEvent();
    }

    public void Surrender()
    {
        battle.SurrenderEvent();
        GameOver(new GameOver
        {
            WinnerLogin = battleType == BattleType.LocalDuel ? battle.WaitingPlayer.Name : battle.Player2.Name,
            GameRewards = new GameRewards()
        });
    }

    public void CancelSearch()
    {
        battle.CancelSearchEvent();
        Scenes.Load(Scenes.MAIN_MENU_SCENE);
    }

    public void GameOver(GameOver gameOverMessage)
    {
        GameResultsPanel.ShowResult(gameOverMessage, battleType == BattleType.LocalDuel);

        var rewards = gameOverMessage.GameRewards;
        GameManager.Instance.CompleteQuests(rewards.Quests.Select(quest => quest.Name));
        Debug.Log("Game over, reason: " + gameOverMessage.Reason + ", "
                  + gameOverMessage.WinnerLogin + " won, rewards: " + rewards);
    }

    public void EnableEndTurnButton()
    {
        EndTurnButton.interactable = true;
    }

    public void DisableEndTurnButton()
    {
        EndTurnButton.interactable = false;
    }

    public void InfoAndroidButtonClick()
    {
        if (battleType == BattleType.TutorialBattle)
        {
            tutorial.EndAndNextStage();
        }
        Color currentColor = InfoAndroidButton.GetComponent<Image>().color;
        InfoAndroidButton.GetComponent<Image>().color = currentColor == Color.white ? Color.red : Color.white;
    }

    public void CorrectView()
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);
        IsInputEnabled = true;

        foreach (GameObject o in FruitonsGrid)
        {
            if (o != null)
                Destroy(o);
        }
        FruitonsGrid = GridLayoutManager.MakeNewGrid();
        GridLayoutManager.ResetHighlights();
    }
}