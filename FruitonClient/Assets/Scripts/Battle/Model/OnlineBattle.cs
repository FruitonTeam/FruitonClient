using Cz.Cuni.Mff.Fruiton.Dto;
using Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using fruiton.kernel;
using haxe.root;

public class OnlineBattle : Battle, IOnMessageListener
{
    public bool IsLocalPlayerFirst;

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

    public OnlineBattle(BattleViewer battleViewer, bool shouldFindGame) : base(battleViewer)
    {
        if (shouldFindGame)
            FindGame();
    }

    private void FindGame()
    {
        var connectionHandler = ConnectionHandler.Instance;
        var findGameMessage = new FindGame
        {
            Team = GameManager.Instance.CurrentFruitonTeam,
            GameMode = battleViewer.GameMode
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
            case WrapperMessage.MessageOneofCase.GameOver:
                {
                    ProcessMessage(message.GameOver);
                }
                break;
        }
    }

    public void ProcessMessage(GameReady gameReadyMessage)
    {
        var kernelPlayer1 = new Player(0);
        var kernelPlayer2 = new Player(1);
        LocalPlayer = new LocalPlayer(battleViewer, kernelPlayer1, this, gameManager.UserName);
        OnlinePlayer = new OnlinePlayer(kernelPlayer2, this, gameReadyMessage.Opponent.Login);
        ((OnlinePlayer)OnlinePlayer).OnEnable();
        IEnumerable<GameObject> opponentTeam = ClientFruitonFactory.CreateClientFruitonTeam(gameReadyMessage.OpponentTeam.FruitonIDs, battleViewer.Board);
        IEnumerable<GameObject> currentTeam = ClientFruitonFactory.CreateClientFruitonTeam(gameManager.CurrentFruitonTeam.FruitonIDs, battleViewer.Board);
        // The opponent team is obtained from the server with the correctly set positions.
        battleViewer.InitializeTeam(opponentTeam, kernelPlayer2, gameReadyMessage.OpponentTeam.Positions.ToArray());

        GameSettings kernelSettings = GameSettingsFactory.CreateGameSettings(gameReadyMessage.MapId, battleViewer.GameMode);

        var fruitons = new Array<object>();
        foreach (var fruiton in currentTeam)
        {
            fruitons.push(fruiton.GetComponent<ClientFruiton>().KernelFruiton);
        }
        foreach (var fruiton in opponentTeam)
        {
            fruitons.push(fruiton.GetComponent<ClientFruiton>().KernelFruiton);
        }
        IsLocalPlayerFirst = gameReadyMessage.StartsFirst;
        Player player1;
        Player player2;

        // If the local player begins, the game will be started with kernelPlayer1 as first argument.
        if (IsLocalPlayerFirst)
        {
            battleViewer.InitializeTeam(currentTeam, kernelPlayer1, GameManager.Instance.CurrentFruitonTeam.Positions.ToArray());
            player1 = kernelPlayer1;
            player2 = kernelPlayer2;
        }
        // If the online opponent begins, we need to flip the positions to the opposite side because we do not receive 
        // the new positions from the server. The first argument has to be the online opponent = kernelPlayer2.
        else
        {
            var width = GameState.WIDTH;
            var height = GameState.HEIGHT;
            var flippedPositions = BattleHelper.FlipCoordinates(GameManager.Instance.CurrentFruitonTeam.Positions, width, height);
            battleViewer.InitializeTeam(currentTeam, kernelPlayer1, flippedPositions.ToArray());
            player1 = kernelPlayer2;
            player2 = kernelPlayer1;
            battleViewer.DisableEndTurnButton();
        }
        kernel = new Kernel(player1, player2, fruitons, kernelSettings, false, false);

        SendReadyMessage();
        battleViewer.InitializePlayersInfo();
        BattleReady();
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
        battleViewer.StartOnlineGame(IsLocalPlayerFirst);
    }

    private void ProcessMessage(GameOver gameOverMessage)
    {
        battleViewer.GameOver(gameOverMessage);
    }

    public override void SurrenderEvent()
    {
        var surrenderMessage = new Surrender();
        var wrapperMessage = new WrapperMessage { Surrender = surrenderMessage };
        ConnectionHandler.Instance.SendWebsocketMessage(wrapperMessage);
    }

    public override void CancelSearchEvent()
    {
        var cancelMessage = new CancelFindingGame();
        var wrapperMessage = new WrapperMessage {CancelFindingGame = cancelMessage};
        ConnectionHandler.Instance.SendWebsocketMessage(wrapperMessage);
        Debug.Log("Cancel search");
    }

    public override void OnEnable()
    {
        base.OnEnable();
        if (ConnectionHandler.Instance.IsLogged())
        {
            ConnectionHandler.Instance.RegisterListener(WrapperMessage.MessageOneofCase.GameReady, this);
            ConnectionHandler.Instance.RegisterListener(WrapperMessage.MessageOneofCase.GameStarts, this);
            ConnectionHandler.Instance.RegisterListener(WrapperMessage.MessageOneofCase.GameOver, this);
        }
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if (ConnectionHandler.Instance.IsLogged())
        {
            ConnectionHandler.Instance.UnregisterListener(WrapperMessage.MessageOneofCase.GameReady, this);
            ConnectionHandler.Instance.UnregisterListener(WrapperMessage.MessageOneofCase.GameStarts, this);
            ConnectionHandler.Instance.UnregisterListener(WrapperMessage.MessageOneofCase.GameOver, this);
        }
        if (OnlinePlayer != null)
        {
            ((OnlinePlayer) OnlinePlayer).OnDisable();
        }
    }
}
