using Cz.Cuni.Mff.Fruiton.Dto;
using Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using fruiton.kernel;
using haxe.root;

public class OnlineBattle : Battle, IOnMessageListener
{
    private bool isLocalPlayerFirst;

    public OnlineBattle(BattleViewer battleViewer) : base(battleViewer)
    {
        if (ConnectionHandler.Instance.IsLogged())
        {
            ConnectionHandler.Instance.RegisterListener(WrapperMessage.MessageOneofCase.GameReady, this);
            ConnectionHandler.Instance.RegisterListener(WrapperMessage.MessageOneofCase.GameStarts, this);
        }
        FindGame();
    }

    private void FindGame()
    {
        var connectionHandler = ConnectionHandler.Instance;
        FindGame findGameMessage = new FindGame
        {
            Team = GameManager.Instance.CurrentFruitonTeam
        };
        var wrapperMessage = new WrapperMessage
        {
            FindGame = findGameMessage
        };
        Debug.Log(GameManager.Instance.CurrentFruitonTeam);
        connectionHandler.SendWebsocketMessage(wrapperMessage);
    }

    public void OnMessage(WrapperMessage message)
    {
        switch (message.MessageCase)
        {
            case WrapperMessage.MessageOneofCase.GameReady:
                {
                    ProcessMessage(message.GameReady);
                }
                break;
            case WrapperMessage.MessageOneofCase.GameStarts:
                {
                    ProcessMessage(message.GameStarts);
                }
                break;
        }
    }

    private void ProcessMessage(GameReady gameReadyMessage)
    {
        player1 = new LocalPlayer(BattleViewer, new Player(0), this);
        player2 = new OnlineOpponent(new Player(1), this);
        IEnumerable<GameObject> opponentTeam = ClientFruitonFactory.CreateClientFruitonTeam(gameReadyMessage.OpponentTeam.FruitonIDs);
        IEnumerable<GameObject> currentTeam = ClientFruitonFactory.CreateClientFruitonTeam(gameManager.CurrentFruitonTeam.FruitonIDs);
        BattleViewer.InitializeTeam(opponentTeam, player2, gameReadyMessage.OpponentTeam.Positions);

        var fruitons = new Array<object>();
        foreach (var fruiton in currentTeam)
        {
            fruitons.push(fruiton.GetComponent<ClientFruiton>().KernelFruiton);
        }
        foreach (var fruiton in opponentTeam)
        {
            fruitons.push(fruiton.GetComponent<ClientFruiton>().KernelFruiton);
        }
        isLocalPlayerFirst = gameReadyMessage.StartsFirst;
        if (isLocalPlayerFirst)
        {
            BattleViewer.InitializeTeam(currentTeam, player1, GameManager.Instance.CurrentFruitonTeam.Positions);
            kernel = new Kernel(player1.KernelPlayer, player2.KernelPlayer, fruitons);
        }
        else
        {
            var width = GameState.WIDTH;
            var height = GameState.HEIGHT;
            var flippedPositions = BattleHelper.FlipCoordinates(GameManager.Instance.CurrentFruitonTeam.Positions, width, height);
            BattleViewer.InitializeTeam(currentTeam, player1, flippedPositions);
            kernel = new Kernel(player2.KernelPlayer, player1.KernelPlayer, fruitons);
            BattleViewer.DisableEndTurnButton();
        }
        
        SendReadyMessage();
    }

    private void SendReadyMessage()
    {
        var connectionHandler = ConnectionHandler.Instance;
        var playerReadyMessage = new PlayerReady();
        var wrapperMessage = new WrapperMessage
        {
            PlayerReady = playerReadyMessage
        };
        connectionHandler.SendWebsocketMessage(wrapperMessage);
    }

    private void ProcessMessage(GameStarts gameStartsMessage)
    {
        kernel.startGame();
        BattleViewer.StartGame(isLocalPlayerFirst);
    }
}
