using System;
using System.Collections.Generic;
using Cz.Cuni.Mff.Fruiton.Dto;
using fruiton.kernel;
using Google.Protobuf.Collections;
using haxe.root;
using UnityEngine;
using Random = UnityEngine.Random;

enum AIType
{
    Random,
    AggroGreedy
}

class AIBattle : Battle
{
    private ClientPlayerBase humanPlayer
    {
        get { return Player1; }
        set { Player1 = value; }
    }
    private AIPlayerBase aiPlayer
    {
        get { return (AIPlayerBase)Player2; }
        set { Player2 = value; }
    }

    public AIBattle(BattleViewer battleViewer, AIType aiType)
        : base(battleViewer)
    {
        var kernelPlayer1 = new Player(0);
        var kernelPlayer2 = new Player(1);
        humanPlayer = new LocalPlayer(battleViewer, kernelPlayer1, this, GameManager.Instance.UserName);
        aiPlayer = CreateAI(aiType, battleViewer, kernelPlayer2, this);

        IEnumerable<GameObject> humanTeam = ClientFruitonFactory.CreateClientFruitonTeam(gameManager.CurrentFruitonTeam.FruitonIDs, battleViewer.Board);
        IEnumerable<GameObject> aiTeam = ClientFruitonFactory.CreateClientFruitonTeam(gameManager.CurrentFruitonTeam.FruitonIDs, battleViewer.Board);

        RepeatedField<Position> coords = gameManager.CurrentFruitonTeam.Positions;
        battleViewer.InitializeTeam(humanTeam, kernelPlayer1, coords);
        RepeatedField<Position> flippedCoords = BattleHelper.FlipCoordinates(coords, GameState.WIDTH, GameState.HEIGHT);
        battleViewer.InitializeTeam(aiTeam, kernelPlayer2, flippedCoords);

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

        kernel = new Kernel(kernelPlayer1, kernelPlayer2, fruitons, kernelSettings, false);
        BattleReady();
    }

    public override void Update()
    {
        base.Update();
        aiPlayer.Update();
    }

    private static AIPlayerBase CreateAI(AIType type, BattleViewer battleViewer, Player kernelPlayer, Battle battle)
    {
        switch (type)
        {
            case AIType.Random:
                return new RandomAIPlayer(battleViewer, kernelPlayer, battle);
            case AIType.AggroGreedy:
                return new AggroGreedyAIPlayer(battleViewer, kernelPlayer, battle);
            default:
                throw new ArgumentOutOfRangeException("type", type, null);
        }
    }
}
