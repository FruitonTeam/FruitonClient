using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleTypeHandler : MonoBehaviour
{
    public Button EndTurnButton; 

	void Start ()
    {
        bool online = Scenes.GetParam(Scenes.ONLINE) == bool.TrueString;
        if (online)
        {
            Camera.main.GetComponent<BattleManager>().enabled = true;
            EndTurnButton.onClick.AddListener(() => Camera.main.GetComponent<BattleManager>().EndTurn());
        }
        else
        {
            Camera.main.GetComponent<OfflineBattleManager>().enabled = true;
            EndTurnButton.onClick.AddListener(() => Camera.main.GetComponent<OfflineBattleManager>().EndTurn()); 
        }
	}
}
