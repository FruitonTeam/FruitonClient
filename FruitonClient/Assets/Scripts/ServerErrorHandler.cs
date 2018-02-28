using System;
using Cz.Cuni.Mff.Fruiton.Dto;
using Networking;
using UI.Notification;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles errors from server and displays them to player.
/// </summary>
public class ServerErrorHandler : MonoBehaviour, IOnMessageListener
{
    public static ServerErrorHandler Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void OnMessage(WrapperMessage message)
    {
        var errorMsg = message.ErrorMessage;
        Debug.LogError("SERVER ERROR: " + errorMsg.Message);
        switch (errorMsg.ErrorId)
        {
            case ErrorMessage.Types.ErrorId.UseOfNotUnlockedFruiton:
                SceneManager.LoadScene(Scenes.MAIN_MENU_SCENE);
                NotificationManager.Instance.ShowError("Invalid fruitons",
                    "You were removed from the queue because you don't own all of the fruitons you were trying to use in your team.");
                return;
            case ErrorMessage.Types.ErrorId.InvalidTeam:
                SceneManager.LoadScene(Scenes.MAIN_MENU_SCENE);
                NotificationManager.Instance.ShowError("Invalid team",
                    "You were removed from the queue because the team you were trying to use isn't valid.");
                return;
        }
    }
}