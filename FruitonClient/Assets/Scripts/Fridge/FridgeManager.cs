using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FridgeManager : MonoBehaviour {

    public Camera FruitonCamera;
    public GameObject Fruitons;
    public GameObject AddSalad;
    public GameObject Salads;

	// Use this for initialization
	void Start () {
        InitializeAllFruitons();
        InitializeSalads();
    }

    private void InitializeSalads()
    {

    }

    private void InitializeAllFruitons()
    {
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
            position.x += 30;
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

        if (Input.GetMouseButtonUp(0))
        {
            LeftButtonUpLogic();
        }
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            ScrollLogic(scroll);
        }
    }

    private void ScrollLogic(float scroll)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            string hitName = hit.transform.name;
            if (hitName == AddSalad.name || hitName == "Salad" || hitName == "Panel_Salads")
            {
                Salads.transform.position += new Vector3(25 * scroll, 0, 0);
            }
            else if (hitName == "Panel_AllFruitons" || HitsChildOf(Fruitons, hitName))
            {
                Fruitons.transform.position += new Vector3(25 * scroll, 0, 0);
            }
                
        }
    }

    private bool HitsChildOf(GameObject gameObject, string hitName)
    {
        foreach (Transform transform in gameObject.transform)
        {
            if (transform.name == hitName)
            {
                return true;
            }
        }
        return false;
    }

    private void LeftButtonUpLogic()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.name == AddSalad.name)
            {
                GameObject saladObject = Instantiate(Resources.Load("Models/Salad", typeof(GameObject))) as GameObject;
                saladObject.transform.position = AddSalad.transform.position;
                saladObject.transform.parent = Salads.transform;
                saladObject.name = "Salad";
                AddSalad.transform.position += new Vector3(50, 0, 0);
            }
        }
    }

}
