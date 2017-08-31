
using Cz.Cuni.Mff.Fruiton.Dto;
using fruiton.fruitDb;
using fruiton.fruitDb.factories;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KFruiton = fruiton.kernel.Fruiton;
using System;
using fruiton.kernel.fruitonTeam;
using Google.Protobuf.Collections;

public class FridgeManager : MonoBehaviour
{
    public Camera FruitonCamera;
    public GameObject FruitonsWrapper;
    public GameObject CurrentSaladWrapper;
    public GameObject Fruitons;
    public GameObject AddSaladButton;
    public GameObject Salads;
    public GameObject Highlight;
    public GameObject CurrentSaladObject;

    private Salad CurrentSalad;
    /// <summary> KEYS: Salad GameObjects. VALUES: Salads. </summary>
    private Dictionary<GameObject, Salad> saladDictionary;
    /// <summary> KEYS: Fruitons' GameObjects. VALUES: Fruitons' IDs. </summary>
    private Dictionary<GameObject, int> fruitonDictionary;
    /// <summary> Current translations of the potentially new Fruitons in the current Salad for each type respectively. </summary>
    private Vector3[] currentSaladTranslations;
    /// <summary> Default translations of the potentially new Fruitons in the current Salad for each type respectively. </summary>
    private Vector3[] defaultCurrentSaladTranslations;
    private Vector3 defaultCurrentSaladPosition;
    private Vector3 defaultCurrenSaladWrapperPosition;
    /// <summary> Counts of the fruitons in a valid salad for each type respectively. </summary>
    private readonly int[] typesCounts = { 1, 3, 4 };
    // Use this for initialization
    void Start()
    {
        saladDictionary = new Dictionary<GameObject, Salad>();
        fruitonDictionary = new Dictionary<GameObject, int>();
        InitializeAllFruitons();
        InitializeSalads();
        InitializeCurrentSalad();
    }

    private void InitializeCurrentSalad()
    {
        currentSaladTranslations = new Vector3[typesCounts.Length];
        defaultCurrentSaladTranslations = new Vector3[currentSaladTranslations.Length];
        CurrentSalad = null;
        defaultCurrentSaladPosition = CurrentSaladObject.transform.position;
        defaultCurrenSaladWrapperPosition = CurrentSaladWrapper.transform.position;
        Vector3 highlightTranslation = Vector3.zero;
        // Pass through all types to place the corresponding spots.
        for (int i = 0; i < typesCounts.Length; i++)
        {
            defaultCurrentSaladTranslations[i] = highlightTranslation;
            currentSaladTranslations[i] = highlightTranslation;
            int type = i + 1;
            for (int j = 0; j < typesCounts[i]; j++)
            {
                GameObject highlight = Instantiate(Resources.Load("Prefabs/Fridge/HighlightType" + type, typeof(GameObject))) as GameObject;
                highlight.transform.position = defaultCurrentSaladPosition + highlightTranslation;
                highlightTranslation.x += 50;
                highlight.transform.parent = CurrentSaladWrapper.transform;
                highlight.name = "currentSaladHighlight";
            }
        }
    }

    private void InitializeSalads()
    {
        GameManager gameManager = GameManager.Instance;
        foreach (Salad salad in gameManager.Salads.Salads)
        {
            AddSalad(salad);
        }
    }

    private void InitializeAllFruitons()
    {
        GameManager gameManager = GameManager.Instance;
        while (!gameManager.IsInitialized)
        {
            Debug.Log("Waiting for game manager to initialize");
        }
        IEnumerable<ClientFruiton> allFruitons = gameManager.AllFruitons;
        Vector3 position = Fruitons.transform.position;
        foreach (ClientFruiton fruiton in allFruitons)
        {
            Debug.Log("Loading 3D model for: " + fruiton.KernelFruiton.model);
            fruiton.FruitonObject = InstantiateFridgeFruiton(fruiton.KernelFruiton, position);
            fruitonDictionary.Add(fruiton.FruitonObject, fruiton.KernelFruiton.id);
            GameObject fruitonObject = fruiton.FruitonObject;
            position.x += 50;
            fruitonObject.transform.parent = Fruitons.transform;

        }
    }

    private GameObject InstantiateFridgeFruiton(KFruiton kernelFruiton, Vector3 position)
    {
        GameObject result = Instantiate(Resources.Load("Models/" + kernelFruiton.model, typeof(GameObject))) as GameObject;
        result.name = kernelFruiton.model;
        result.transform.position = position;
        AddCollider(result);
        GameObject highlight = Instantiate(Resources.Load("Prefabs/Fridge/HighlightType" + kernelFruiton.type, typeof(GameObject))) as GameObject;
        highlight.transform.position = new Vector3(position.x, position.y - 1, 120);
        highlight.transform.parent = FruitonsWrapper.transform;
        return result;
    }

    void Update()
    {
        foreach (ClientFruiton fruiton in GameManager.Instance.AllFruitons)
        {
            fruiton.FruitonObject.transform.Rotate(new Vector3(0, 50 * Time.deltaTime, 0));
        }

        foreach (Transform child in CurrentSaladObject.transform)
        {
            child.Rotate(new Vector3(0, 50 * Time.deltaTime, 0));
        }

        if (Input.GetMouseButtonUp(0))
        {
            LeftButtonUpLogic();
        }

        float scroll = 0;
        Ray ray = new Ray();
#if UNITY_ANDROID
        Touch[] myTouches = Input.touches;
        switch (Input.touchCount)
        {
            case 1:
                {
                    Touch touch = myTouches[0];
                    if (touch.phase == TouchPhase.Moved)
                    {
                        ray = Camera.main.ScreenPointToRay(touch.position);
                    }
                    scroll = touch.deltaPosition.x/40;
                }
                break;
        }
#endif

#if UNITY_STANDALONE || UNITY_EDITOR

        scroll = Input.GetAxis("Mouse ScrollWheel");
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
#endif
        if (scroll != 0)
        {
            ScrollLogic(scroll, ray);
        }


    }

    private void AddCollider(GameObject fruitonObject)
    {
        if (fruitonObject.GetComponent<Collider>() == null)
        {
            SphereCollider collider = fruitonObject.AddComponent<SphereCollider>();
            collider.radius = 15 * fruitonObject.transform.localScale.x;
        }
    }

    private void ScrollLogic(float scroll, Ray ray)
    {
        //TODO: Add scroll elasticity.
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            string hitName = hit.transform.name;
            if (hitName == AddSaladButton.name || hitName == "Salad" || hitName == "Panel_Salads" || hitName == "Highlight" || IsInSaladPanel(hitName))
            {
                Salads.transform.position += new Vector3(50 * scroll, 0, 0);
            }
            else if (hitName == "Panel_AllFruitons" || HitsChildOf(Fruitons, hitName) || HitsChildOf(FruitonsWrapper, hitName))
            {
                FruitonsWrapper.transform.position += new Vector3(50 * scroll, 0, 0);
            }
            else if (hitName == "Panel_CurrentSalad" || IsInCurrentSalad(hitName) || HitsChildOf(CurrentSaladWrapper, hitName))
            {
                CurrentSaladWrapper.transform.position += new Vector3(50 * scroll, 0, 0);
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
            // Add a new Salad
            if (hit.transform.name == AddSaladButton.name)
            {
                AddSalad(null);
            }
            // Switch to another Salad
            else if (hit.transform.name == "Salad")
            {
                for (int i = 0; i < currentSaladTranslations.Length; i++)
                {
                    currentSaladTranslations[i] = defaultCurrentSaladTranslations[i];
                }
                CurrentSaladWrapper.transform.position = defaultCurrenSaladWrapperPosition;
                ClearCurrentSalad();
                Vector3 hitPosition = hit.transform.position;
                Highlight.transform.position = new Vector3(hitPosition.x, hitPosition.y - 1, 120);
                Highlight.transform.parent = hit.transform;
                CurrentSalad = saladDictionary[hit.collider.gameObject];
                FruitonDatabase fruitonDatabase = GameManager.Instance.FruitonDatabase;
                foreach (int fruitonID in CurrentSalad.FruitonIDs)
                {
                    KFruiton kernelFruiton = FruitonFactory.makeFruiton(fruitonID, fruitonDatabase);
                    GameObject saladMember = Instantiate(Resources.Load("Models/" + kernelFruiton.model, typeof(GameObject))) as GameObject;
                    fruitonDictionary.Add(saladMember, kernelFruiton.id);
                    AddCollider(saladMember);
                    saladMember.name = "CurrentSalad_" + fruitonID;
                    saladMember.transform.position = defaultCurrentSaladPosition + currentSaladTranslations[kernelFruiton.type-1];
                    currentSaladTranslations[kernelFruiton.type - 1].x += 50;
                    saladMember.transform.parent = CurrentSaladObject.transform;
                }
            }
            // Pick a new Fruiton in Salad.
            else if (CurrentSalad != null && HitsChildOf(Fruitons, hit.transform.name))
            {
                AddSaladMember(hit.collider.gameObject);
            }
            // Remove a Fruiton from the current salad.
            else if (IsInCurrentSalad(hit.transform.name))
            {
                GameObject toBeDestroyed = hit.transform.gameObject;
                int id = fruitonDictionary[toBeDestroyed];
                CurrentSalad.FruitonIDs.Remove(id);
                Vector3 destroyedPosition = toBeDestroyed.transform.position;
                Destroy(toBeDestroyed);
                KFruiton kernelFruiton = FruitonFactory.makeFruiton(id, GameManager.Instance.FruitonDatabase);
                foreach (Transform child in CurrentSaladObject.transform)
                {
                    int childId = fruitonDictionary[child.gameObject];
                    KFruiton kernelChild = FruitonFactory.makeFruiton(childId, GameManager.Instance.FruitonDatabase);
                    if (kernelChild.type == kernelFruiton.type && child.transform.position.x > destroyedPosition.x)
                    {
                        child.transform.position -= new Vector3(50, 0, 0);
                    }
                }
                currentSaladTranslations[kernelFruiton.type - 1] -= new Vector3(50, 0, 0);
            }
            ProtoSerializer.Instance.SerializeSalads();
            
        }
    }

    private void AddSaladMember(GameObject pattern)
    {
        RepeatedField<int> fruitonIDsCopy = CurrentSalad.FruitonIDs.CopyRepeatedField<int>();
        fruitonIDsCopy.Add(fruitonDictionary[pattern]);
        int[] fruitonIDsCopyArray = new int[fruitonIDsCopy.Count];
        fruitonIDsCopy.CopyTo(fruitonIDsCopyArray, 0);
        ValidationMessage validationMessage = FruitonTeamValidator.validateFruitonTeam(new Array<int>(fruitonIDsCopyArray), GameManager.Instance.FruitonDatabase, true);
        if (validationMessage.success)
        {
            GameObject saladMember = Instantiate(pattern) as GameObject;
            saladMember.transform.parent = CurrentSaladObject.transform;
            fruitonDictionary.Add(saladMember, fruitonDictionary[pattern]);
            CurrentSalad.FruitonIDs.Add(fruitonDictionary[saladMember]);
            int id = fruitonDictionary[saladMember];
            KFruiton kernelFruiton = FruitonFactory.makeFruiton(id, GameManager.Instance.FruitonDatabase);
            saladMember.name = "CurrentSalad_" + saladMember.name;
            saladMember.transform.position = CurrentSaladObject.transform.position + currentSaladTranslations[kernelFruiton.type - 1];
            currentSaladTranslations[kernelFruiton.type - 1].x += 50;
        } else
        {
            // TODO: Notify the user (His choice would cause invalid salad.) 
        }
        

    }

    private void AddSalad(Salad salad)
    {
        GameManager gameManager = GameManager.Instance;
        GameObject saladObject = Instantiate(Resources.Load("Models/Salad", typeof(GameObject))) as GameObject;
        saladObject.transform.position = AddSaladButton.transform.position;
        saladObject.transform.parent = Salads.transform;
        saladObject.name = "Salad";
        AddSaladButton.transform.position += new Vector3(50, 0, 0);

        GameObject signCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        saladObject.AddComponent<SphereCollider>();
        SphereCollider collider = saladObject.GetComponent<SphereCollider>();
        collider.radius = 1;
        signCube.transform.position = saladObject.transform.position - new Vector3(0, 15, 20);
        signCube.transform.localScale = new Vector3(30, 10, 0.01f);
        signCube.GetComponent<Renderer>().material.color = new Color(0.2f, 0.3f, 0.1f);
        signCube.transform.parent = saladObject.transform;
        signCube.name = "Salad_Cube";

        GameObject text3D = new GameObject("Sign");
        TextMesh mesh = text3D.AddComponent<TextMesh>();

        mesh.anchor = TextAnchor.MiddleCenter;
        mesh.fontSize = 300;
        mesh.characterSize = 0.18f;
        text3D.transform.position = signCube.transform.position;
        text3D.transform.parent = signCube.transform;
        text3D.name = "Salad_3DText";
        Salad newSalad;
        if (salad == null)
        {
            mesh.text = "New Salad" + gameManager.Salads.Salads.Count;
            newSalad = new Salad();
            newSalad.Name = mesh.text;
            gameManager.Salads.Salads.Add(newSalad);
        }
        else
        {
            newSalad = salad;
            mesh.text = salad.Name;
        }
        saladDictionary.Add(saladObject, newSalad);
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
