using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FridgeManager : MonoBehaviour {

    public Camera FruitonCamera;
    public GameObject Fruitons;

	// Use this for initialization
	void Start () {
        GameManager gameManager = GameManager.Instance;
        while (!gameManager.IsInitialized)
        {
            Debug.Log("Waiting for game manager to initialize");
        }
        Fruitons allFruitons = gameManager.AllFruitons;
        Vector3 position = Fruitons.transform.position;
        foreach (Fruiton fruiton in allFruitons.FruitonList)
        {
            fruiton.gameobject = Instantiate(Resources.Load("Models/" + fruiton.Model, typeof(GameObject))) as GameObject;
            GameObject fruitonObject = fruiton.gameobject;
            fruitonObject.transform.position = position;
            position.x += 300;
            fruitonObject.transform.parent = Fruitons.transform;
            fruitonObject.ChangeLayerRecursively("3DUI");
        }

    }

    void Update()
    {
        foreach (Fruiton fruiton in GameManager.Instance.AllFruitons.FruitonList)
        {
            fruiton.gameobject.transform.Rotate(new Vector3(0, 50 * Time.deltaTime, 0));
        }
    }

}
