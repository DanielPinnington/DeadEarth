using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class GameState
{
    public string Key = null;
    public string Value = null;
}
public class ApplicationManager : MonoBehaviour
{

    //This holds any states you wish set at game startup
    [SerializeField] private List<GameState> _startingGameStates = new List<GameState>();

    //Used to store the key/value pauis in the above list in a more efficient dictionairy for runtime loopup
    private Dictionary<string, string> _gameStateDictionary = new Dictionary<string, string>();

    //Singleton Design
    private static ApplicationManager _Instance = null;
    public static ApplicationManager instance
    {
        get
        {
            //If we dont get an instance yet find it in the scene hierarchy
            if (_Instance == null) { _Instance = (ApplicationManager)FindObjectOfType(typeof(ApplicationManager)); }

            //return the instance
            return _Instance;
        }
    }

    private void Awake()
    {
        //This object must live for the entire application
        DontDestroyOnLoad(gameObject);

        //Copy starting game states into game dictionary
        for(int i = 0; i < _startingGameStates.Count; i++)
        {
            GameState gs = _startingGameStates[i];
            _gameStateDictionary[gs.Key] = gs.Value;
        }
    }

    public string GetGameState(string key)
    {
        string result = null;
        _gameStateDictionary.TryGetValue(key, out result);
        return result;
    }

    public bool SetGameState(string key, string value)
    {
        if (key == null || value == null) return false;
        _gameStateDictionary[key] = value;

        return true;
    }
}
