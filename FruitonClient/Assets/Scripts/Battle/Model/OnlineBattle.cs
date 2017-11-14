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

    private ClientPlayerBase LocalPlayer
    {
        get { return Player1; }
        set { Player1 = value; }
    }

    private ClientPlayerBase OnlinePlayer
    {
        get { return Player2; }
        set { Player2 = value; }
    }

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
        Player kernelPlayer1 = new Player(0);
        Player kernelPlayer2 = new Player(1);
        LocalPlayer = new LocalPlayer(battleViewer, kernelPlayer1, this, gameManager.UserName);
        OnlinePlayer = new OnlinePlayer(kernelPlayer2, this, gameReadyMessage.Opponent.Login);
        IEnumerable<GameObject> opponentTeam = ClientFruitonFactory.CreateClientFruitonTeam(gameReadyMessage.OpponentTeam.FruitonIDs);
        IEnumerable<GameObject> currentTeam = ClientFruitonFactory.CreateClientFruitonTeam(gameManager.CurrentFruitonTeam.FruitonIDs);
        // The opponent team is obtained from the server with the correctly set positions.
        battleViewer.InitializeTeam(opponentTeam, kernelPlayer2, gameReadyMessage.OpponentTeam.Positions);

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
        // If the local player begins, the game will be started with kernelPlayer1 as first argument.
        if (isLocalPlayerFirst)
        {
            battleViewer.InitializeTeam(currentTeam, kernelPlayer1, GameManager.Instance.CurrentFruitonTeam.Positions);
            kernel = new Kernel(kernelPlayer1, kernelPlayer2, fruitons);
        }
        // If the online opponent begins, we need to flip the positions to the opposite side because we do not receive 
        // the new positions from the server. The first argument has to be the online opponent = kernelPlayer2.
        else
        {
            var width = GameState.WIDTH;
            var height = GameState.HEIGHT;
            var flippedPositions = BattleHelper.FlipCoordinates(GameManager.Instance.CurrentFruitonTeam.Positions, width, height);
            battleViewer.InitializeTeam(currentTeam, kernelPlayer1, flippedPositions);
            kernel = new Kernel(kernelPlayer2, kernelPlayer1, fruitons);
            battleViewer.DisableEndTurnButton();
        }
        
        SendReadyMessage();
        battleViewer.InitializePlayersInfo();
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
        battleViewer.StartOnlineGame(isLocalPlayerFirst);
    }
}
