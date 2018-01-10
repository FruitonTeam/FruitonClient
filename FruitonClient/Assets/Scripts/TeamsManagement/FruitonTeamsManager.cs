
using Cz.Cuni.Mff.Fruiton.Dto;
using fruiton.fruitDb;
using fruiton.fruitDb.factories;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KFruiton = fruiton.kernel.Fruiton;
using System;
using System.Linq;
using fruiton.kernel;
using fruiton.kernel.fruitonTeam;
using Google.Protobuf.Collections;
using haxe.root;
using UnityEngine.SceneManagement;
using Networking;
using Spine.Unity;

public class FruitonTeamsManager : MonoBehaviour
{
    public Camera FruitonCamera;
    public GameObject FruitonsWrapper;
    public GameObject CurrentFruitonTeamWrapper;
    public GameObject Fruitons;
    public GameObject AddFruitonTeamButton;
    public GameObject FruitonTeams;
    public GameObject Highlight;
    public GameObject CurrentFruitonTeamObject;
    public GameObject PanelAllFruitons;
    public GameObject PanelCurrenFruitonTeams;
    public GameObject PanelFruitonTeams;
    public GameObject ButtonPlay;
    public GameObject ButtonBin;

    private IEnumerable<GameObject> allClientFruitons;
    private FruitonTeam currentFruitonTeam;
    /// <summary> KEYS: FruitonTeam GameObjects. VALUES: FruitonTeams. </summary>
    private Dictionary<GameObject, FruitonTeam> fruitonTeamsDictionary;
    /// <summary> KEYS: Fruitons' GameObjects. VALUES: Fruitons' IDs. </summary>
    private Dictionary<GameObject, int> fruitonDictionary;
    /// <summary> Current translations of the potentially new Fruitons in the current Fruiton Team for each type respectively. </summary>
    private Vector3[] currentFruitonTeamTranslations;
    /// <summary> Default translations of the potentially new Fruitons in the current Fruiton Team for each type respectively. </summary>
    private Vector3[] defaultCurrentFruitonTeamTranslations;
    private Vector3 defaultCurrentFruitonTeamPosition;
    private Vector3 defaultCurrenFruitonTeamWrapperPosition;
    /// <summary> Counts of the fruitons in a valid Fruiton Team for each type respectively. </summary>
    private readonly int[] typesCounts = { 1, 4, 5 };
    private bool teamManagementState;
    // Do not process clicks while the mouse is over the UI objects.
    private bool mouseOverUI;
    private bool removingGaps;

    /// <summary> Stores the ordered (by x coord) GameObjects representing fruiton teams. Used when deleting teams. </summary>
    private IOrderedEnumerable<GameObject> orderedTeamObjects;

    public static readonly string TEAM_MANAGEMENT_STATE = "teamManagementState";
    /// <summary> The horizontal distance between fruitons (or teams) in this scene. </summary>
    private static readonly int OBJECTS_DISTANCE = 50;
#if UNITY_ANDROID
    private static readonly int TOUCH_SPEED_REDUCTION = 40;
#endif

    // Use this for initialization
    void Start()
    {
        fruitonTeamsDictionary = new Dictionary<GameObject, FruitonTeam>();
        fruitonDictionary = new Dictionary<GameObject, int>();
        teamManagementState = bool.Parse(Scenes.GetParam(TEAM_MANAGEMENT_STATE));
        if (teamManagementState)
        {
            ButtonPlay.SetActive(false);
            InitializeAllFruitons();
        } 
        else
        {
            PanelFruitonTeams.transform.position = PanelCurrenFruitonTeams.transform.position;
            PanelCurrenFruitonTeams.transform.position = PanelAllFruitons.transform.position;
            PanelAllFruitons.SetActive(false);
            AddFruitonTeamButton.SetActive(false);
            ButtonBin.SetActive(false);
        }
        InitializeFruitonTeams(teamManagementState);
        InitializeCurrentFruitonTeam();
    }

    private void Update()
    {
        if (removingGaps)
        {
            FillGapsBetweenTeams();
            return;
        }
        if (Input.GetMouseButtonUp(0) && !mouseOverUI)
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
                    scroll = touch.deltaPosition.x/TOUCH_SPEED_REDUCTION;
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

    private void InitializeCurrentFruitonTeam()
    {
        currentFruitonTeamTranslations = new Vector3[typesCounts.Length];
        defaultCurrentFruitonTeamTranslations = new Vector3[currentFruitonTeamTranslations.Length];
        currentFruitonTeam = null;
        defaultCurrentFruitonTeamPosition = CurrentFruitonTeamObject.transform.position;
        defaultCurrenFruitonTeamWrapperPosition = CurrentFruitonTeamWrapper.transform.position;
        Vector3 highlightTranslation = Vector3.zero;
        // Pass through all types to place the corresponding spots.
        for (int i = 0; i < typesCounts.Length; i++)
        {
            defaultCurrentFruitonTeamTranslations[i] = highlightTranslation;
            currentFruitonTeamTranslations[i] = highlightTranslation;
            int type = i + 1;
            for (int j = 0; j < typesCounts[i]; j++)
            {
                GameObject highlight = Instantiate(Resources.Load("Prefabs/Fridge/HighlightType" + type, typeof(GameObject))) as GameObject;
                highlight.transform.position = defaultCurrentFruitonTeamPosition + highlightTranslation + new Vector3(0, 0, 20);
                highlightTranslation.x += OBJECTS_DISTANCE;
                highlight.transform.parent = CurrentFruitonTeamWrapper.transform;
                highlight.name = "currentFruitonTeamHighlight";
            }
        }
    }

    private void InitializeFruitonTeams(bool includeIncomplete)
    {
        GameManager gameManager = GameManager.Instance;
        foreach (FruitonTeam fruitonTeam in gameManager.FruitonTeamList.FruitonTeams)
        {
            if (includeIncomplete || IsTeamComplete(fruitonTeam))
            {
                AddFruitonTeam(fruitonTeam);
            }
        }
    }

    private bool IsTeamComplete(FruitonTeam fruitonTeam)
    {
        int[] fruitonIDsArray = new int[fruitonTeam.FruitonIDs.Count];
        fruitonTeam.FruitonIDs.CopyTo(fruitonIDsArray, 0);
        return FruitonTeamValidator.validateFruitonTeam(new Array<int>(fruitonIDsArray), GameManager.Instance.FruitonDatabase).complete;
    }

    private void InitializeAllFruitons()
    {
        PlayerHelper.GetAvailableFruitons(UpdateAvailableFruitons, Debug.Log);
        List<GameObject> fruitons = new List<GameObject>();
        GameManager gameManager = GameManager.Instance;
        //while (!gameManager.IsInitialized)
        //{
        //    Debug.Log("Waiting for game manager to initialize");
        //}
        IEnumerable<KFruiton> allFruitons = gameManager.AllFruitons;
        Vector3 position = Fruitons.transform.position;
        foreach (KFruiton fruiton in allFruitons)
        {
            GameObject fruitonObject = InstantiateFridgeFruiton(fruiton, position);
            fruitonObject.AddComponent<ClientFruiton>().KernelFruiton = fruiton;
            fruitonDictionary.Add(fruitonObject, fruiton.id);
            position.x += OBJECTS_DISTANCE;
            fruitonObject.transform.parent = Fruitons.transform;
            fruitons.Add(fruitonObject);
            if (!gameManager.AvailableFruitons.Contains(fruiton.id))
            {
                fruitonObject.SetActive(false);
            }
        }
        allClientFruitons = fruitons;
    }

    private GameObject InstantiateFridgeFruiton(KFruiton kernelFruiton, Vector3 position)
    {
        var result = Instantiate(Resources.Load("Models/TeamManagement/BoyFighter", typeof(GameObject))) as GameObject;
        result.GetComponent<SkeletonAnimation>().Skeleton.SetSkin(kernelFruiton.model);
        result.name = kernelFruiton.model;
        result.transform.position = new Vector3(position.x, position.y - 20, position.z);
        AddCollider(result);
        var highlight = Instantiate(Resources.Load("Prefabs/Fridge/HighlightType" + kernelFruiton.type, typeof(GameObject))) as GameObject;
        highlight.transform.position = new Vector3(position.x, position.y - 1, 120);
        highlight.transform.parent = FruitonsWrapper.transform;
        return result;
    }

    private void UpdateAvailableFruitons(List<int> fruitons)
    {
        Serializer.SaveAvailableFruitons(fruitons);
        foreach (GameObject fruitonObject in allClientFruitons)
        {
            KFruiton fruiton = fruitonObject.GetComponent<ClientFruiton>().KernelFruiton;
            if (!fruitons.Contains(fruiton.id))
            {
                fruitonObject.SetActive(false);
            }
            else
            {
                fruitonObject.SetActive(true);
            }
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
            GameObject HitObject = hit.transform.gameObject;
            var translation = new Vector3(OBJECTS_DISTANCE * scroll, 0, 0);

            if (HitObject == AddFruitonTeamButton || HitObject == PanelFruitonTeams || HitObject == Highlight || HitsChildOf(FruitonTeams, HitObject))
            {
                FruitonTeams.transform.position += translation;
            }
            else if (HitObject == PanelAllFruitons || HitsChildOf(Fruitons, HitObject) || HitsChildOf(FruitonsWrapper, HitObject))
            {
                FruitonsWrapper.transform.position += translation;
            }
            else if (HitObject == PanelCurrenFruitonTeams || HitsChildOf(CurrentFruitonTeamObject, HitObject) || HitsChildOf(CurrentFruitonTeamWrapper, HitObject))
            {
                CurrentFruitonTeamWrapper.transform.position += translation;
            }

        }
    }


    private bool HitsChildOf(GameObject gameObject, GameObject hitObject)
    {
        foreach (Transform transform in gameObject.transform)
        {
            if (transform.gameObject == hitObject)
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

            GameObject hitObject = hit.transform.gameObject;
            // Add a new Fruiton Team
            if (hit.transform.name == AddFruitonTeamButton.name)
            {
                AddFruitonTeam();
            }
            // Switch to the another FruitonTeam
            else if (HitsChildOf(FruitonTeams, hitObject))
            {
                for (int i = 0; i < currentFruitonTeamTranslations.Length; i++)
                {
                    currentFruitonTeamTranslations[i] = defaultCurrentFruitonTeamTranslations[i];
                }
                CurrentFruitonTeamWrapper.transform.position = defaultCurrenFruitonTeamWrapperPosition;
                ClearCurrentFruitonTeam();
                Vector3 hitPosition = hit.transform.position;
                Highlight.SetActive(true);
                Highlight.transform.position = new Vector3(hitPosition.x, hitPosition.y - 1, 120);
                Highlight.transform.parent = hit.transform;
                currentFruitonTeam = fruitonTeamsDictionary[hit.collider.gameObject];
                if (!teamManagementState)
                {
                    GameManager.Instance.CurrentFruitonTeam = currentFruitonTeam;
                }
                FruitonDatabase fruitonDatabase = GameManager.Instance.FruitonDatabase;
                foreach (int fruitonID in currentFruitonTeam.FruitonIDs)
                {
                    KFruiton kernelFruiton = FruitonFactory.makeFruiton(fruitonID, fruitonDatabase);
                    var fruitonTeamMember = Instantiate(Resources.Load("Models/TeamManagement/BoyFighter", typeof(GameObject))) as GameObject;
                    fruitonTeamMember.GetComponent<SkeletonAnimation>().Skeleton.SetSkin(kernelFruiton.model);
                    fruitonDictionary.Add(fruitonTeamMember, kernelFruiton.id);
                    AddCollider(fruitonTeamMember);
                    fruitonTeamMember.name = "CurrentFruitonTeam_" + fruitonID;
                    fruitonTeamMember.transform.position = defaultCurrentFruitonTeamPosition + currentFruitonTeamTranslations[kernelFruiton.type-1] - new Vector3(0, 20);
                    currentFruitonTeamTranslations[kernelFruiton.type - 1].x += OBJECTS_DISTANCE;
                    fruitonTeamMember.transform.parent = CurrentFruitonTeamObject.transform;
                }
            }
            // Pick a new Fruiton in Fruiton Team.
            else if (currentFruitonTeam != null && HitsChildOf(Fruitons, hit.transform.gameObject))
            {
                AddFruitonTeamMember(hit.collider.gameObject);
            }
            // Remove a Fruiton from the current Fruiton Team.
            else if (teamManagementState && HitsChildOf(CurrentFruitonTeamObject, hitObject))
            {
                GameObject toBeDestroyed = hit.transform.gameObject;
                int id = fruitonDictionary[toBeDestroyed];
                currentFruitonTeam.FruitonIDs.Remove(id);
                Vector3 destroyedPosition = toBeDestroyed.transform.position;
                Destroy(toBeDestroyed);
                KFruiton kernelFruiton = FruitonFactory.makeFruiton(id, GameManager.Instance.FruitonDatabase);
                foreach (Transform child in CurrentFruitonTeamObject.transform)
                {
                    int childId = fruitonDictionary[child.gameObject];
                    KFruiton kernelChild = FruitonFactory.makeFruiton(childId, GameManager.Instance.FruitonDatabase);
                    if (kernelChild.type == kernelFruiton.type && child.transform.position.x > destroyedPosition.x)
                    {
                        child.transform.position -= new Vector3(OBJECTS_DISTANCE, 0, 0);
                    }
                }
                currentFruitonTeamTranslations[kernelFruiton.type - 1] -= new Vector3(OBJECTS_DISTANCE, 0, 0);
            }
            if (currentFruitonTeam != null)
            {
                SetPositionsOfFruitonTeam(currentFruitonTeam);
                PlayerHelper.UploadFruitonTeam(currentFruitonTeam, Debug.Log, Debug.Log);
            }
            Serializer.SerializeFruitonTeams();
            
        }
    }

    private void AddFruitonTeamMember(GameObject pattern)
    {
        RepeatedField<int> fruitonIDsCopy = currentFruitonTeam.FruitonIDs.Copy<int>();
        fruitonIDsCopy.Add(fruitonDictionary[pattern]);
        int[] fruitonIDsCopyArray = new int[fruitonIDsCopy.Count];
        fruitonIDsCopy.CopyTo(fruitonIDsCopyArray, 0);
        ValidationResult validationResult = FruitonTeamValidator.validateFruitonTeam(new Array<int>(fruitonIDsCopyArray), GameManager.Instance.FruitonDatabase);
        if (validationResult.valid)
        {
            GameObject fruitonTeamMember = Instantiate(pattern);
            fruitonTeamMember.transform.parent = CurrentFruitonTeamObject.transform;
            fruitonDictionary.Add(fruitonTeamMember, fruitonDictionary[pattern]);
            currentFruitonTeam.FruitonIDs.Add(fruitonDictionary[fruitonTeamMember]);
            int id = fruitonDictionary[fruitonTeamMember];
            KFruiton kernelFruiton = FruitonFactory.makeFruiton(id, GameManager.Instance.FruitonDatabase);
            fruitonTeamMember.name = "CurrentFruitonTeam_" + fruitonTeamMember.name;
            fruitonTeamMember.transform.position = CurrentFruitonTeamObject.transform.position + currentFruitonTeamTranslations[kernelFruiton.type - 1];
            currentFruitonTeamTranslations[kernelFruiton.type - 1].x += OBJECTS_DISTANCE;
            fruitonTeamMember.GetComponent<ClientFruiton>().KernelFruiton = kernelFruiton;
        } else
        {
            // TODO: Notify the user (His choice would cause invalid Fruiton Team.) 
        }
        

    }

    private void AddFruitonTeam(FruitonTeam fruitonTeam = null)
    {
        GameManager gameManager = GameManager.Instance;
        GameObject fruitonTeamObject = Instantiate(Resources.Load("Models/TeamManagement/FruitonTeam", typeof(GameObject))) as GameObject;
        fruitonTeamObject.transform.position = AddFruitonTeamButton.transform.position;
        fruitonTeamObject.transform.parent = FruitonTeams.transform;
        fruitonTeamObject.name = fruitonTeam == null ? "Fruiton team" : fruitonTeam.Name;
        
        AddFruitonTeamButton.transform.position += new Vector3(OBJECTS_DISTANCE, 0, 0);

        GameObject signCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fruitonTeamObject.AddComponent<SphereCollider>();
        SphereCollider collider = fruitonTeamObject.GetComponent<SphereCollider>();
        collider.radius = 1;
        signCube.transform.position = fruitonTeamObject.transform.position - new Vector3(0, 15, 20);
        signCube.transform.localScale = new Vector3(30, 10, 0.01f);
        signCube.GetComponent<Renderer>().material.color = new Color(0.2f, 0.3f, 0.1f);
        signCube.transform.parent = fruitonTeamObject.transform;
        signCube.name = "FruitonTeam_Cube";

        GameObject text3D = new GameObject("Sign");
        TextMesh mesh = text3D.AddComponent<TextMesh>();

        mesh.anchor = TextAnchor.MiddleCenter;
        mesh.fontSize = 300;
        mesh.characterSize = 0.18f;
        text3D.transform.position = signCube.transform.position;
        text3D.transform.parent = signCube.transform;
        text3D.name = "FruitonTeam_3DText";
        FruitonTeam newFruitonTeam;
        if (fruitonTeam == null)
        {
            mesh.text = GetNextAvailableTeamName();
            fruitonTeamObject.name = mesh.text;
            newFruitonTeam = new FruitonTeam();
            newFruitonTeam.Name = mesh.text;
            gameManager.FruitonTeamList.FruitonTeams.Add(newFruitonTeam);
        }
        else
        {
            newFruitonTeam = fruitonTeam;
            mesh.text = fruitonTeam.Name;
        }
        fruitonTeamsDictionary.Add(fruitonTeamObject, newFruitonTeam);
        PlayerHelper.UploadFruitonTeam(newFruitonTeam, Debug.Log, Debug.Log);
    }

    /// <summary>
    /// Finds the next name for the fruiton team in the following way:
    /// "New Team N" where N is the smallest available positive integer,
    /// whilst by available is meant that no other fruiton team has the same name.
    /// </summary>
    /// <returns></returns>
    private string GetNextAvailableTeamName()
    {
        GameObject[] teamObjects = fruitonTeamsDictionary.Keys.ToArray();
        for (int i = 1;; i++)
        {
            string potentialName = "New Team " + i;
            if (teamObjects.All(go => go.name != potentialName))
            {
                return potentialName;
            }
        }
    }

    private void ClearCurrentFruitonTeam()
    {
        foreach (Transform child in CurrentFruitonTeamObject.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void SetPositionsOfFruitonTeam(FruitonTeam fruitonTeam)
    {
        int i, j;
        var majorRow = 0;
        var minorRow = 1;
        var majorCounter = 2;
        var minorCounter = 2;
        var fruitonDatabase = GameManager.Instance.FruitonDatabase;
        currentFruitonTeam.Positions.Clear();
        foreach (var id in fruitonTeam.FruitonIDs)
        {
            var kernelFruiton = FruitonFactory.makeFruiton(id, fruitonDatabase);
            switch ((FruitonType)kernelFruiton.type)
            {
                case FruitonType.KING:
                {
                    i = GameState.WIDTH / 2;
                    j = majorRow;
                }
                    break;
                case FruitonType.MAJOR:
                {
                    i = GameState.WIDTH/2 - majorCounter;
                    j = majorRow;
                    if (--majorCounter == 0) --majorCounter;
                }
                    break;
                case FruitonType.MINOR:
                {
                    i = GameState.WIDTH / 2 - minorCounter;
                    j = minorRow;
                    --minorCounter;
                }
                    break;
                default:
                {
                    throw new UndefinedFruitonTypeException();
                }
            }
            currentFruitonTeam.Positions.Add(new Position { X = i, Y = j });
        }
    }

    public void MouseEntersUI()
    {
        mouseOverUI = true;
    }

    public void MouseLeavesUI()
    {
        mouseOverUI = false;
    }

    public void DeleteTeam()
    {
        if (currentFruitonTeam != null)
        {
            GameManager.Instance.FruitonTeamList.FruitonTeams.Remove(currentFruitonTeam);
            PlayerHelper.RemoveFruitonTeam(currentFruitonTeam, Debug.Log, Debug.Log);
            KeyValuePair<GameObject, FruitonTeam> keyValuePair = fruitonTeamsDictionary.First(kvPair => kvPair.Value.Equals(currentFruitonTeam));
            GameObject teamGameObject = keyValuePair.Key;
            fruitonTeamsDictionary.Remove(teamGameObject);
            fruitonDictionary.Remove(teamGameObject);
            Highlight.transform.parent = null;
            Highlight.SetActive(false);
            Destroy(teamGameObject);
            removingGaps = true;
            var gameObjects = new List<GameObject>();
            foreach (Transform child in FruitonTeams.transform)
            {
                if (!child.gameObject.Equals(teamGameObject))
                {
                    gameObjects.Add(child.gameObject);
                }
            }
            orderedTeamObjects = gameObjects.OrderBy(x => x.transform.position.x);
            currentFruitonTeam = null;
        }
    }

    /// <summary>
    /// Moves the fruiton teams' game objects closer so that they do not form big gaps.
    /// The distance between each pair of neighboring objects should be app. OBJECTS_DISTANCE eventually.
    /// </summary>
    private void FillGapsBetweenTeams()
    {
        // This may be called when ordered
        if (orderedTeamObjects == null)
        {
            return;
        }

        removingGaps = false;

        GameObject previous = null;
        foreach (GameObject current in orderedTeamObjects)
        {
            if (previous != null && current.transform.position.x - previous.transform.position.x > OBJECTS_DISTANCE)
            {
                removingGaps = true;
                current.transform.position += 100 * Time.deltaTime * Vector3.left;
            }
            previous = current;
        }
    }
}
