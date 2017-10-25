using Cz.Cuni.Mff.Fruiton.Dto;
using fruiton.kernel;
using Google.Protobuf.Collections;
using haxe.root;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OfflineBattle : Battle
{
    public OfflineBattle(BattleViewer battleViewer) : base(battleViewer)
    {
        player1 = new LocalPlayer(battleViewer, new Player(0), this);
        player2 = new LocalPlayer(battleViewer, new Player(1), this);

        IEnumerable<GameObject> currentTeam = ClientFruitonFactory.CreateClientFruitonTeam(gameManager.CurrentFruitonTeam.FruitonIDs);
        IEnumerable<GameObject> opponentTeam = ClientFruitonFactory.CreateClientFruitonTeam(gameManager.CurrentFruitonTeam.FruitonIDs);

        RepeatedField<Position> coords = gameManager.CurrentFruitonTeam.Positions;
        battleViewer.InitializeTeam(currentTeam, player1, coords);
        RepeatedField<Position> flippedCoords = BattleHelper.FlipCoordinates(coords, GameState.WIDTH, GameState.HEIGHT);
        battleViewer.InitializeTeam(opponentTeam, player2, flippedCoords);

        var fruitons = new Array<object>();
        foreach (var fruiton in currentTeam)
        {
            fruitons.push(fruiton.GetComponent<ClientFruiton>().KernelFruiton);
        }
        foreach (var fruiton in opponentTeam)
        {
            fruitons.push(fruiton.GetComponent<ClientFruiton>().KernelFruiton);
        }
        kernel = new Kernel(player1.KernelPlayer, player2.KernelPlayer, fruitons);
    }
}
