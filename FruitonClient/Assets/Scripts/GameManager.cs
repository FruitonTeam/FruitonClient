using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using UnityEngine;

public enum FractionNames { None, GuacamoleGuerrillas, CranberryCrusade, TzatzikiTsardom }

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    #region Fields

    private string userName = null;
    private string userPassword = null;
    private bool? stayLoggedIn;
    private IEnumerable<string> myFruitonsIDs;
    private Fruitons allFruitons;
    private bool isInitialized = false;
    private TextAsset fruitonDefs;
    private List<Salad> salads;
    #endregion

    #region Properties

    public bool StayLoggedIn
    {
        get
        {
            return stayLoggedIn ?? (PlayerPrefs.GetInt("stayloggedin", 0) == 1);
        }
        set
        {
            stayLoggedIn = value;
            PlayerPrefs.SetInt("stayloggedin", 1);
        }
    }

    // getters and setters for UserData, currently saved via PlayerPrefs
    public string UserName {
        get
        {
            if (userName == null)
            {
                userName = PlayerPrefs.GetString("username", "");
            }
            return userName;
        }
        set {
            userName = value;
            if (StayLoggedIn)
            {
                PlayerPrefs.SetString("username", userName);
            }
            else
            {
                PlayerPrefs.SetString("username", "");
            }
            IsUserValid = false;
        }
    }

    public string UserPassword
    {
        get
        {
            if (userPassword == null)
            {
                userPassword = PlayerPrefs.GetString("userpassword", "");
            }
            return userPassword;
        }
        set
        {
            userPassword = value;
            if (StayLoggedIn)
            {
                PlayerPrefs.SetString("userpassword", value);
            }
            else
            {
                PlayerPrefs.SetString("userpassword", "");
            }
            IsUserValid = false;
        }
    }

    public bool IsUserValid
    {
        get
        {
            return (PlayerPrefs.GetInt("ValidUser", 0) == 1)? true: false;
        }
        set
        {
            UserFraction = FractionNames.None;
            PlayerPrefs.SetInt("ValidUser", (value) ? 1 : 0);
        }
    }

    public FractionNames UserFraction
    {
        get
        {
            return (FractionNames) PlayerPrefs.GetInt("Fraction", (int) FractionNames.None);
        }
        set
        {
            PlayerPrefs.SetInt("Fraction", (int) value);
        }
    }

    public IEnumerable<string> MyFruitonsIDs
    {
        get
        {
            return myFruitonsIDs;
        }
    }

    public Fruitons AllFruitons
    {
        get
        {
            return allFruitons;
        }
    }

    public bool IsInitialized
    {
        get
        {
            return isInitialized;
        }
        set
        {
            isInitialized = value;
        }
    }

    List<Salad> Salads
    {
        get
        {
            return salads;
        }
        set
        {
            salads = value; 
        }
    }

    #endregion


    #region Public 

    // online check of the LoginData combination, true if online connection was possible 
    public bool OnlineLoginDataCheck()
    {
        //--TODO-- !!!!!!!!
        //+load and set UserFraction if it's already set => skip fraction panel

        //............Debug..........
        bool online = true;
        if (online)
        {
            //ConnectionHandler.Instance.LoginCasual(UserName, UserPassword, true);
        }
        return false;
    }

    public bool HasRememberedUser()
    {
        return (UserName != "" && UserPassword != "");
    }

    public void LoginOffline()
    {

    }

    public void Initialize()
    {
        ParseFruitonsXML();
        IsInitialized = true;
    }

    #endregion

    #region Private

    private void ParseFruitonsXML()
    {
        
        XmlSerializer deserializer = new XmlSerializer(typeof(Fruitons));
        var reader = new System.IO.StringReader(fruitonDefs.text);
        object obj = deserializer.Deserialize(reader);
        allFruitons = (Fruitons)obj;
        Debug.Log("All fruitons deserialized: " + allFruitons);
        reader.Close();
    }

    private void SerializeBinary(object toBeSerialized, string filename)
    {
        FileStream stream = File.Create(filename);
        var formatter = new BinaryFormatter();
        formatter.Serialize(stream, toBeSerialized);
        stream.Close();
    }

    private  T DeserializeBinary<T>(string filename)
    {
        FileStream stream = File.OpenRead(filename);
        var formatter = new BinaryFormatter();
        T deserialized = (T)formatter.Deserialize(stream);
        stream.Close();
        return deserialized;
    }

    #endregion

    void Start()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
            fruitonDefs = (TextAsset)Resources.Load("XML/FruitonsDefs");
            Initialize();
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
}
