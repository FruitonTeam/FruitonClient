using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FridgeManager : MonoBehaviour {

    public Camera FruitonCamera;
    public GameObject Fruitons;
    public GameObject AddSalad;
    public GameObject Salads;
    public GameObject Highlight;
    public GameObject CurrentSaladObject;
    private Salad currentSalad;
    Dictionary<GameObject, Salad> saladDictionary;
    Vector3 currentSaladTranslation;
    Vector3 defaultCurrentSaladPosition;
	// Use this for initialization
	void Start () {
        InitializeAllFruitons();
        InitializeSalads();
        saladDictionary = new Dictionary<GameObject, Salad>();
        currentSaladTranslation = Vector3.zero;
        currentSalad = null;
        defaultCurrentSaladPosition = CurrentSaladObject.transform.position;
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
            fruitonObject.name = fruiton.Model;
            if (fruitonObject.GetComponent<Collider>() == null)
            {
                SphereCollider collider = fruitonObject.AddComponent<SphereCollider>();
                collider.radius = 15 * fruitonObject.transform.localScale.x;
            }

            fruitonObject.transform.position = position;
            position.x += 50;
            fruitonObject.transform.parent = Fruitons.transform;
            //fruitonObject.ChangeLayerRecursively("3DUI");
            
        }
    }

    void Update()
    {
        foreach (Fruiton fruiton in GameManager.Instance.AllFruitons.FruitonList)
        {
            fruiton.gameobject.transform.Rotate(new Vector3(0, 50 * Time.deltaTime, 0));
        }

        foreach (Transform child in CurrentSaladObject.transform)
        {
            child.Rotate(new Vector3(0, 50 * Time.deltaTime, 0));
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
            if (hitName == AddSalad.name || hitName == "Salad" || hitName == "Panel_Salads" || hitName == "Highlight" || IsInSaladPanel(hitName))
            {
                Salads.transform.position += new Vector3(25 * scroll, 0, 0);
            }
            else if (hitName == "Panel_AllFruitons" || HitsChildOf(Fruitons, hitName))
            {
                Fruitons.transform.position += new Vector3(25 * scroll, 0, 0);
            }
            else if (hitName == "Panel_CurrentSalad" || IsInCurrentSalad(hitName))
            {
                CurrentSaladObject.transform.position += new Vector3(25 * scroll, 0, 0);
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
                GameManager gameManager = GameManager.Instance;
                GameObject saladObject = Instantiate(Resources.Load("Models/Salad", typeof(GameObject))) as GameObject;
                saladObject.transform.position = AddSalad.transform.position;
                saladObject.transform.parent = Salads.transform;
                saladObject.name = "Salad";
                AddSalad.transform.position += new Vector3(50, 0, 0);

                GameObject signCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                saladObject.AddComponent<SphereCollider>();
                SphereCollider collider = saladObject.GetComponent<SphereCollider>();
                collider.radius = 1;
                signCube.transform.position = saladObject.transform.position - new Vector3(0,15,20);
                signCube.transform.localScale = new Vector3(30,10,0.01f);
                signCube.GetComponent<Renderer>().material.color = new Color(0.2f, 0.3f, 0.1f);
                signCube.transform.parent = saladObject.transform;
                signCube.name = "Salad_Cube";
                

                GameObject text3D = new GameObject("Sign");
                TextMesh mesh = text3D.AddComponent<TextMesh>();
                mesh.text = "New Salad" + gameManager.Salads.Count;
                mesh.anchor = TextAnchor.MiddleCenter;
                mesh.fontSize = 300;
                mesh.characterSize = 0.18f;
                text3D.transform.position = signCube.transform.position;
                text3D.transform.parent = signCube.transform;
                text3D.name = "Salad_3DText";
                Salad newSalad = new Salad(mesh.text);
                gameManager.Salads.Add(newSalad);

                saladDictionary.Add(saladObject, newSalad);

            }
            // Switch to another Salad
            else if (hit.transform.name == "Salad")
            {
                currentSaladTranslation = Vector3.zero;
                CurrentSaladObject.transform.position = defaultCurrentSaladPosition;                
                ClearCurrentSalad();
                Vector3 hitPosition = hit.transform.position;
                Highlight.transform.position = new Vector3(hitPosition.x, hitPosition.y - 1, 120);
                Highlight.transform.parent = hit.transform;
                currentSalad = saladDictionary[hit.collider.gameObject];
                foreach (string fruitonID in currentSalad.FruitonIDs)
                {
                    GameObject saladMember = Instantiate(Resources.Load("Models/" + fruitonID, typeof(GameObject))) as GameObject;
                    saladMember.name = "CurrentSalad_" + fruitonID;
                    saladMember.transform.position = defaultCurrentSaladPosition + currentSaladTranslation;
                    currentSaladTranslation.x += 50;
                    saladMember.transform.parent = CurrentSaladObject.transform;
                }                
            }
            // Pick a new Fruiton in Salad.
            else if (currentSalad != null && HitsChildOf(Fruitons, hit.transform.name))
            {
                GameObject saladMember = Instantiate(hit.collider.gameObject) as GameObject;
                saladMember.name = "CurrentSalad_" + saladMember.name;
                saladMember.transform.position = CurrentSaladObject.transform.position + currentSaladTranslation;
                currentSaladTranslation.x += 50;
                saladMember.transform.parent = CurrentSaladObject.transform;
                currentSalad.Add(hit.transform.name);
            }
            else if (IsInCurrentSalad(hit.transform.name))
            {
                GameObject toBeDestroyed = hit.transform.gameObject;
                Vector3 destroyedPosition = toBeDestroyed.transform.position;
                Destroy(toBeDestroyed);
                foreach (Transform child in CurrentSaladObject.transform)
                {
                    if (child.transform.position.x > destroyedPosition.x)
                    {
                        child.transform.position -= new Vector3(50, 0, 0);
                    }
                }
                currentSaladTranslation -= new Vector3(50, 0, 0);
            }
        }
    }
    
    private bool IsInCurrentSalad(string name)
    {
        string[] splitName = name.Split('_');
        return splitName[0] == "CurrentSalad";
    }

    private bool IsInSaladPanel(string name)
    {
        string[] splitName = name.Split('_');
        return splitName[0] == "Salad";
    }

    private void ClearCurrentSalad()
    {
        foreach (Transform child in CurrentSaladObject.transform)
        {
            Destroy(child.gameObject);
        }
    }
}
