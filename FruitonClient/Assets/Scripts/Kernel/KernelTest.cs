using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KernelTest : MonoBehaviour {

    public Button yourButton;

    Random random;
    GameState gameState;

    void Start()
    {
        Button btn = yourButton.GetComponent<Button>();
        btn.onClick.AddListener(GenerateNewMove);

        random = new Random();

        gameState = Kernel.generateState();
        Debug.Log(gameState.pieces);
    }

    void GenerateNewMove()
    {
        var moveId = (int)Random.Range(0, 5);
        var moveX = (int)Random.Range(-5, 5);
        var moveY = (int)Random.Range(-5, 5);
        Move move = new Move(moveId, new Position(moveX, moveY));
        Debug.Log(move.ToString());
        gameState = Kernel.nextState(gameState, move);
        Debug.Log(gameState.pieces);
    }
}
