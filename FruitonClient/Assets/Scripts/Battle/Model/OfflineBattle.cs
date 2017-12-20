using Cz.Cuni.Mff.Fruiton.Dto;
using fruiton.kernel;
using Google.Protobuf.Collections;
using haxe.root;
using System.Collections.Generic;
using UnityEngine;

public class OfflineBattle : Battle
{
    public OfflineBattle(BattleViewer battleViewer) : base(battleViewer)
    {
        Player kernelPlayer1 = new Player(0);
        Player kernelPlayer2 = new Player(1);
        string login = GameManager.Instance.UserName;
        Player1 = new LocalPlayer(battleViewer, kernelPlayer1, this, login);
        Player2 = new LocalPlayer(battleViewer, kernelPlayer2, this, login);

        IEnumerable<GameObject> currentTeam = ClientFruitonFactory.CreateClientFruitonTeam(gameManager.CurrentFruitonTeam.FruitonIDs, battleViewer.Board);
        IEnumerable<GameObject> opponentTeam = ClientFruitonFactory.CreateClientFruitonTeam(gameManager.CurrentFruitonTeam.FruitonIDs, battleViewer.Board);

        RepeatedField<Position> coords = gameManager.CurrentFruitonTeam.Positions;
        battleViewer.InitializeTeam(currentTeam, kernelPlayer1, coords);
        RepeatedField<Position> flippedCoords = BattleHelper.FlipCoordinates(coords, GameState.WIDTH, GameState.HEIGHT);
        battleViewer.InitializeTeam(opponentTeam, kernelPlayer2, flippedCoords);

        var fruitons = new Array<object>();
        foreach (var fruiton in currentTeam)
        {
            fruitons.push(fruiton.GetComponent<ClientFruiton>().KernelFruiton);
        }
        foreach (var fruiton in opponentTeam)
        {
            fruitons.push(fruiton.GetComponent<ClientFruiton>().KernelFruiton);
        }
        kernel = new Kernel(kernelPlayer1, kernelPlayer2, fruitons);
        BattleReady();
    }

}
