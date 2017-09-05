using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class Scenes
{
    public const string CHAT_SCENE = "ChatScene";
    public const string MAIN_MENU = "MainMenu";
    public const string TEAMS_MANAGEMENT_SCENE = "TeamsManagementScene";
    public const string BATTLE_SCENE = "BattleScene";

    private static Dictionary<string, string> parameters;

    public static void Load(string sceneName, Dictionary<string, string> parameters = null)
    {
        Scenes.parameters = parameters;
        SceneManager.LoadScene(sceneName);
    }

    public static void Load(string sceneName, string paramKey, string paramValue)
    {
        Scenes.parameters = new Dictionary<string, string>();
        Scenes.parameters.Add(paramKey, paramValue);
        SceneManager.LoadScene(sceneName);
    }

    public static Dictionary<string, string> getSceneParameters()
    {
        return parameters;
    }

    public static string getParam(string paramKey)
    {
        if (parameters == null) return "";
        return parameters[paramKey];
    }

    public static void setParam(string paramKey, string paramValue)
    {
        if (parameters == null)
        {
            Scenes.parameters = new Dictionary<string, string>();
        }
        Scenes.parameters.Add(paramKey, paramValue);
    }

    private Scenes()
    {
        
    }
}