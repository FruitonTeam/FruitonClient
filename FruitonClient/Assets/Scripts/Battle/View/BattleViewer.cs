using Cz.Cuni.Mff.Fruiton.Dto;
using fruiton.kernel;
using fruiton.kernel.actions;
using fruiton.kernel.events;
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
    public GameObject PanelLoadingGame;
    public Text TimeCounter;
    public Text MyLoginText;
    public Text OpponentLoginText;
    public Image MyAvatar;
    public Image OpponentAvatar;
    public GameObject Board;
    public MessagePanel GameResultsPanel;
    public GameObject FruitonInfoPanel;
    public GameObject TutorialPanel;


    /// <summary> Client fruitons stored at their position. </summary>
    public GameObject[,] Grid { get; set; }

    public GameMode GameMode { get; private set; }

    public BattleViewer()
    {
        IsInputEnabled = true;
    }

    private void Start()
    {
#if UNITY_ANDROID
        InfoAndroidButton.gameObject.SetActive(true);
#endif
        GridLayoutManager = GridLayoutManager.Instance;
        Grid = new GameObject[GridLayoutManager.WidthCount, GridLayoutManager.HeighCount];

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
            case BattleType.OfflineBattle:
                battle = new OfflineBattle(this);
                InitializeOfflineGame();
                break;
            case BattleType.AIBattle:
                var aiType = (AIType) Enum.Parse(typeof(AIType), Scenes.GetParam(Scenes.AI_TYPE));
                Debug.Log("Battle vs AI: " + aiType);
                battle = new AIBattle(this, aiType);
                InitializeOfflineGame();
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
        else
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
        InitializePlayersInfo();
        SetupSurrenderButton();
    }

    public void SetupSurrenderButton()
    {
        SurrendButton.onClick.RemoveAllListeners();
        SurrendButton.onClick.AddListener(Surrender);
    }

    public void InitializePlayersInfo()
    {
        string login1 = battle.Player1.Name;
        string login2 = battle.Player2.Name;
        MyLoginText.text = login1;
        OpponentLoginText.text = login2;

        MyAvatar.sprite = SpriteUtils.TextureToSprite(GameManager.Instance.Avatar);
        PlayerHelper.GetAvatar(login2,
            texture => OpponentAvatar.sprite = SpriteUtils.TextureToSprite(texture),
            Debug.Log);
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
            GameObject hitFruiton = Grid[tilePosition.x, tilePosition.y];
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
            foreach (var fruiton in Grid)
                if (fruiton != null)
                {
                    var clientFruiton = fruiton.GetComponent<ClientFruiton>();
                    clientFruiton.FlipAround();
                }
            Vector3 oldPosition = Camera.main.transform.position;
            Camera.main.transform.position = new Vector3(-oldPosition.x, oldPosition.y, oldPosition.z);
            Vector3 oldEulerAngles = Camera.main.transform.eulerAngles;
            Camera.main.transform.eulerAngles = new Vector3(oldEulerAngles.x, oldEulerAngles.y + 180, oldEulerAngles.z);
        }
        PanelLoadingGame.SetActive(false);
        isGameStarted = true;
        SetupSurrenderButton();
    }

    public void InitializeTeam(IEnumerable<GameObject> currentTeam, Player player,
        Position[] fruitonsPositions = null)
    {
        var counter = 0;
        int i = 0, j = 0;
        foreach (var clientFruiton in currentTeam)
        {
            var kernelFruiton = clientFruiton.GetComponent<ClientFruiton>().KernelFruiton;
            var anim = clientFruiton.GetComponentInChildren<SkeletonAnimation>();
            if (player.id == 0)
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
            Grid[i, j] = clientFruiton;
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
            ProcessMoveEvent((MoveEvent) kEvent);
        else if (eventType == typeof(AttackEvent))
            ProcessAttackEvent((AttackEvent) kEvent);
        else if (eventType == typeof(DeathEvent))
            ProcessDeathEvent((DeathEvent) kEvent);
        else if (eventType == typeof(ModifyAttackEvent))
            ProcessModifyAttackEvent((ModifyAttackEvent) kEvent);
        else if (eventType == typeof(HealEvent))
            ProcessHealEvent((HealEvent) kEvent);
        else if (eventType == typeof(ModifyHealthEvent))
            ProcessModifyHealthEvent((ModifyHealthEvent) kEvent);
        else if (eventType == typeof(GameOverEvent))
            ProcessGameOverEvent((GameOverEvent) kEvent);
        else if (eventType == typeof(TimeExpiredEvent))
            ProcessTimeExpiredEvent((TimeExpiredEvent) kEvent);
    }

    private void ProcessTimeExpiredEvent(TimeExpiredEvent kEvent)
    {
        Debug.Log("Time expired.");
    }

    private void ProcessModifyHealthEvent(ModifyHealthEvent kEvent)
    {
        KVector2 kEventPosition = kEvent.position;
        var clientFruiton = Grid[kEventPosition.x, kEventPosition.y].GetComponent<ClientFruiton>();
        clientFruiton.ModifyHealth(kEvent.newHealth);
    }

    private void ProcessHealEvent(HealEvent kEvent)
    {
        KVector2 kEventPosition = kEvent.target;
        var clientFruiton = Grid[kEventPosition.x, kEventPosition.y].GetComponent<ClientFruiton>();
        clientFruiton.ReceiveHeal(kEvent.heal);
    }

    private void ProcessModifyAttackEvent(ModifyAttackEvent kEvent)
    {
        KVector2 kEventPosition = kEvent.position;
        var clientFruiton = Grid[kEventPosition.x, kEventPosition.y].GetComponent<ClientFruiton>();
        clientFruiton.ModifyAttack(kEvent.newAttack);
    }

    private void ProcessDeathEvent(DeathEvent kEvent)
    {
        var killedPos = kEvent.fruiton.position;
        var killed = Grid[killedPos.x, killedPos.y];
        Destroy(killed);
        Grid[killedPos.x, killedPos.y] = null;
    }

    private void ProcessAttackEvent(AttackEvent kEvent)
    {
        var damagedPosition = kEvent.target;
        var damaged = Grid[damagedPosition.x, damagedPosition.y];
        damaged.GetComponent<ClientFruiton>().TakeDamage(kEvent.damage);
    }

    private void ProcessMoveEvent(MoveEvent moveEvent)
    {
        IsInputEnabled = false;
        KVector2 from = moveEvent.from;
        KVector2 to = moveEvent.to;
        GameObject movedObject = Grid[from.x, from.y];
        Grid[to.x, to.y] = movedObject;
        Grid[from.x, from.y] = null;
        Vector3 toPosition = GridLayoutManager.GetCellPosition(to.x, to.y);
        StartCoroutine(MoveCoroutine(movedObject.transform.position, toPosition, movedObject));
        GridLayoutManager.ResetHighlights();
    }

    private void ProcessGameOverEvent(GameOverEvent gameOverEvent)
    {
        var message = new GameOver
        {
            Reason = Cz.Cuni.Mff.Fruiton.Dto.GameOver.Types.Reason.Standard,
            Results = new GameResults
            {
                Money = 0,
                Quests = {},
                UnlockedFruitons = {}
            }
        };
        GameOver(message);
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
        Scenes.Load(Scenes.MAIN_MENU_SCENE);
    }

    public void CancelSearch()
    {
        battle.CancelSearchEvent();
        Scenes.Load(Scenes.MAIN_MENU_SCENE);
    }

    public void GameOver(GameOver gameOverMessage)
    {
        GameResultsPanel.OnClose(() => Scenes.Load(Scenes.MAIN_MENU_SCENE));
        GameResultsPanel.ShowInfoMessage("Game over: " + gameOverMessage.Reason + Environment.NewLine +
                                         "Money gain: " + gameOverMessage.Results.Money + Environment.NewLine +
                                         "Unlocked fruitons: " + gameOverMessage.Results.UnlockedFruitons + Environment.NewLine +
                                         "Unlocked quests: " + string.Join(",", 
                                             gameOverMessage.Results.Quests.Select(q => q.Name).ToArray()));
        
        GameManager.Instance.AddMoney(gameOverMessage.Results.Money);
        GameManager.Instance.UnlockFruitons(gameOverMessage.Results.UnlockedFruitons);

        if (gameOverMessage.Results.Quests != null)
        {
            foreach (Quest q in gameOverMessage.Results.Quests)
            {
                GameManager.Instance.AddMoney(q.Reward.Money);
            }
        }
        
        Debug.Log("Game over, reason: " + gameOverMessage.Reason + ", result: " + gameOverMessage.Results);
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
}