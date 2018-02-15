using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Cz.Cuni.Mff.Fruiton.Dto;
using fruiton.kernel;
using fruiton.kernel.gameModes;
using Google.Protobuf.Collections;
using haxe.root;
using UnityEngine;
using Random = UnityEngine.Random;

enum AIType
{
    Santas,
    SportsMen,
    Clowns,
    Tutorial
}

class AIBattle : Battle
{
    public ClientPlayerBase HumanPlayer
    {
        get { return Player1; }
        set { Player1 = value; }
    }
    public AIPlayerBase AiPlayer
    {
        get { return (AIPlayerBase)Player2; }
        set { Player2 = value; }
    }

    public AIBattle(BattleViewer battleViewer, AIType aiType)
        : base(battleViewer)
    {
        int[] aiTeamIDs;

        var kernelPlayer1 = new Player(0);
        var kernelPlayer2 = new Player(1);
        HumanPlayer = new LocalPlayer(battleViewer, kernelPlayer1, this, GameManager.Instance.UserName);
        AiPlayer = CreateAI(aiType, battleViewer, kernelPlayer2, this, out aiTeamIDs);

        IEnumerable<GameObject> humanTeam;
        IEnumerable<Position> coords;
        IEnumerable<Position> flippedCoords;
        IEnumerable<GameObject> aiTeam = ClientFruitonFactory.CreateClientFruitonTeam(aiTeamIDs, battleViewer.Board);

        if (aiType == AIType.Tutorial)
        {
            int[] humanTeamIDs = {2, 5, 15, 14, 12, 17, 21, 21, 30, 25};
            humanTeam = ClientFruitonFactory.CreateClientFruitonTeam(humanTeamIDs, battleViewer.Board);
            coords = FruitonTeamsManager.CreatePositionsForArtificialTeam(humanTeam.Select(gameObject =>
            gameObject.GetComponent<ClientFruiton>().KernelFruiton));
        }
        else
        {
            humanTeam = ClientFruitonFactory.CreateClientFruitonTeam(gameManager.CurrentFruitonTeam.FruitonIDs, battleViewer.Board);
            coords = gameManager.CurrentFruitonTeam.Positions;
        }
        battleViewer.InitializeTeam(humanTeam, kernelPlayer1, coords.ToArray());
        flippedCoords = BattleHelper.FlipCoordinates(coords, GameState.WIDTH, GameState.HEIGHT);
        battleViewer.InitializeTeam(aiTeam, kernelPlayer2, flippedCoords.ToArray());

        var fruitons = new Array<object>();
        foreach (var fruiton in humanTeam)
        {
            fruitons.push(fruiton.GetComponent<ClientFruiton>().KernelFruiton);
        }
        foreach (var fruiton in aiTeam)
        {
            fruitons.push(fruiton.GetComponent<ClientFruiton>().KernelFruiton);
        }

        Array<int> maps = GameManager.Instance.FruitonDatabase.getMapsIds();
        int rndMapId = maps[Random.Range(0, maps.length)];
        GameSettings kernelSettings = GameSettingsFactory.CreateGameSettings(rndMapId, battleViewer.GameMode);

        bool infiniteTurnTime = battleViewer.battleType == BattleType.TutorialBattle;
        kernel = new Kernel(kernelPlayer1, kernelPlayer2, fruitons, kernelSettings, false, infiniteTurnTime);
        BattleReady();
    }

    public override void Update()
    {
        base.Update();
        AiPlayer.Update();
    }

    private static AIPlayerBase CreateAI(AIType type, BattleViewer battleViewer, Player kernelPlayer, Battle battle, out int[] teamIds)
    {
        switch (type)
        {
            case AIType.Clowns:
                teamIds = AITeams.Clowns;
                return new AggroGreedyAIPlayer(battleViewer, kernelPlayer, battle);
            case AIType.Santas:
                teamIds = AITeams.Santas;
                return new AggroGreedyAIPlayer(battleViewer, kernelPlayer, battle);
            case AIType.SportsMen:
                teamIds = AITeams.SportsMen;
                return new AggroGreedyAIPlayer(battleViewer, kernelPlayer, battle);
            case AIType.Tutorial:
                teamIds = AITeams.Tutorial;
                return new TutorialPlayer(battleViewer, kernelPlayer, battle);
            default:
                throw new ArgumentOutOfRangeException("type", type, null);
        }
    }
}
