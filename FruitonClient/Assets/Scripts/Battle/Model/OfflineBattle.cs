using Cz.Cuni.Mff.Fruiton.Dto;
using fruiton.kernel;
using Google.Protobuf.Collections;
using haxe.root;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OfflineBattle : Battle
{
    public OfflineBattle(BattleViewer battleViewer) : base(battleViewer)
    {
        var kernelPlayer1 = new Player(0);
        var kernelPlayer2 = new Player(1);
        string login = GameManager.Instance.UserName;
        Player1 = new LocalPlayer(battleViewer, kernelPlayer1, this, "Player 1");
        Player2 = new LocalPlayer(battleViewer, kernelPlayer2, this, "Player 2");

        IEnumerable<GameObject> currentTeam = ClientFruitonFactory.CreateClientFruitonTeam(gameManager.CurrentFruitonTeam.FruitonIDs, battleViewer.Board);
        IEnumerable<GameObject> opponentTeam = ClientFruitonFactory.CreateClientFruitonTeam(gameManager.OfflineOpponentTeam.FruitonIDs, battleViewer.Board);

        RepeatedField<Position> coords = gameManager.CurrentFruitonTeam.Positions;
        battleViewer.InitializeTeam(currentTeam, kernelPlayer1, coords.ToArray());
        RepeatedField<Position> flippedCoords = BattleHelper.FlipCoordinates(coords, GameState.WIDTH, GameState.HEIGHT);
        battleViewer.InitializeTeam(opponentTeam, kernelPlayer2, flippedCoords.ToArray());

        var fruitons = new Array<object>();
        foreach (var fruiton in currentTeam)
        {
            fruitons.push(fruiton.GetComponent<ClientFruiton>().KernelFruiton);
        }
        foreach (var fruiton in opponentTeam)
        {
            fruitons.push(fruiton.GetComponent<ClientFruiton>().KernelFruiton);
        }
        
        Array<int> maps = GameManager.Instance.FruitonDatabase.getMapsIds();
        int rndMapId = maps[Random.Range(0, maps.length)];
        GameSettings kernelSettings = GameSettingsFactory.CreateGameSettings(rndMapId, battleViewer.GameMode);

        kernel = new Kernel(kernelPlayer1, kernelPlayer2, fruitons, kernelSettings, false, false);
        BattleReady();
    }

}
