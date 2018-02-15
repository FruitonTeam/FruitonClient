using System;
using System.Collections.Generic;
using Cz.Cuni.Mff.Fruiton.Dto;
using Networking;
using UnityEngine;

public class DraftHandler : IOnMessageListener
{
    private readonly DraftManager draftManager;

    public DraftHandler(DraftManager draftManager)
    {
        this.draftManager = draftManager;
    }

    public void StartListening()
    {
        ConnectionHandler.Instance.RegisterListener(WrapperMessage.MessageOneofCase.DraftRequest, this);
        ConnectionHandler.Instance.RegisterListener(WrapperMessage.MessageOneofCase.DraftResult, this);
        ConnectionHandler.Instance.RegisterListener(WrapperMessage.MessageOneofCase.DraftReady, this);
        ConnectionHandler.Instance.RegisterListener(WrapperMessage.MessageOneofCase.GameReady, this);
        ConnectionHandler.Instance.RegisterListener(WrapperMessage.MessageOneofCase.GameOver, this);
    }
    public void StopListening()
    {
        ConnectionHandler.Instance.UnregisterListener(WrapperMessage.MessageOneofCase.DraftRequest, this);
        ConnectionHandler.Instance.UnregisterListener(WrapperMessage.MessageOneofCase.DraftResult, this);
        ConnectionHandler.Instance.UnregisterListener(WrapperMessage.MessageOneofCase.DraftReady, this);
        ConnectionHandler.Instance.UnregisterListener(WrapperMessage.MessageOneofCase.GameReady, this);
        ConnectionHandler.Instance.UnregisterListener(WrapperMessage.MessageOneofCase.GameOver, this);
    }

    public void OnMessage(WrapperMessage message)
    {
        switch (message.MessageCase)
        {
            case WrapperMessage.MessageOneofCase.DraftRequest:
                ProcessMessage(message.DraftRequest);
                break;
            case WrapperMessage.MessageOneofCase.DraftResult:
                ProcessMessage(message.DraftResult);
                break;
            case WrapperMessage.MessageOneofCase.DraftReady:
                ProcessMessage(message.DraftReady);
                break;
            case WrapperMessage.MessageOneofCase.GameReady:
                ProcessMessage(message.GameReady);
                break;
            case WrapperMessage.MessageOneofCase.GameOver:
                ProcessMessage(message.GameOver);
                break;
        }
    }

    private void ProcessMessage(GameOver gameOver)
    {
        draftManager.GameOver(gameOver);
    }

    private void ProcessMessage(GameReady gameReady)
    {
        var param = new Dictionary<string, string>
        {
            {Scenes.BATTLE_TYPE, BattleType.OnlineBattle.ToString()},
            {Scenes.GAME_MODE, GameMode.Standard.ToString()}
        };
        var objParam = new Dictionary<string, object>
        {
            {Scenes.GAME_READY_MSG, gameReady}
        };
        Scenes.Load(Scenes.BATTLE_SCENE, param, objParam);
    }

    private void ProcessMessage(DraftReady draftReady)
    {
        draftManager.StartDraft(draftReady);
    }

    private void ProcessMessage(DraftRequest request)
    {
        draftManager.TurnOnDrafting(request);
    }

    private void ProcessMessage(DraftResult result)
    {
        if (result.Login == GameManager.Instance.UserName)
        {
            draftManager.UpdateFruitonInMyTeam(result);
        }
        else
        {
            draftManager.AddEnemyFruiton(result);
        }
        draftManager.TurnOffDrafting();
    }

    public void SendDraftResponse(int fruitonId)
    {
        var draftResponse = new DraftResponse
        {
            FruitonId = fruitonId
        };
        var wrapperMessage = new WrapperMessage
        {
            DraftResponse = draftResponse
        };
        ConnectionHandler.Instance.SendWebsocketMessage(wrapperMessage);
    }

    public void SendSurrender()
    {
        var draftSurrender = new DraftSurrenderMessage();
        var wrapperMessage = new WrapperMessage
        {
            DraftSurrenderMessage = draftSurrender
        };
        if (ConnectionHandler.Instance.IsLogged())
            ConnectionHandler.Instance.SendWebsocketMessage(wrapperMessage);
    }

    public void CancelSearch()
    {
        var cancelMessage = new CancelFindingGame();
        var wrapperMessage = new WrapperMessage
        {
            CancelFindingGame = cancelMessage
        };
        if (ConnectionHandler.Instance.IsLogged())
            ConnectionHandler.Instance.SendWebsocketMessage(wrapperMessage);
    }
}
