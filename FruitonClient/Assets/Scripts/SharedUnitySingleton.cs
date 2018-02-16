using Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SharedUnitySingleton : MonoBehaviour
{
    public static SharedUnitySingleton Instance { get; private set; }
    private static readonly float CONNECTION_CHECK_INTERVAL = 10;
    private float connectionCheckTimeLeft = CONNECTION_CHECK_INTERVAL;


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

    private void Update()
    {
        if (ConnectionHandler.Instance == null ||
            GameManager.Instance == null ||
            !GameManager.Instance.IsOnline ||
            SceneManager.GetActiveScene().name == Scenes.LOGIN_SCENE) return;
        connectionCheckTimeLeft -= Time.deltaTime;
        
        if (connectionCheckTimeLeft < 0)
        {
            connectionCheckTimeLeft = CONNECTION_CHECK_INTERVAL;
            ConnectionHandler.Instance.CheckConnection();
        }
        
    }
}
