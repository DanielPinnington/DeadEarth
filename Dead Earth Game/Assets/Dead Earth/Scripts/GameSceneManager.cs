using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerInfo
{
    public Collider collider = null;
    public CharacterManager characterManager = null;
    public Camera camera = null;
    public CapsuleCollider meleeTrigger = null;
}

// -------------------------------------------------------------------------
// CLASS	:	GameSceneManager
// Desc		:	Singleton class that acts as the scene database
// -------------------------------------------------------------------------
public class GameSceneManager : MonoBehaviour
{
    // Inspector Assigned Variables
    [SerializeField] private ParticleSystem _bloodParticles = null;

    // Statics
    private static GameSceneManager _instance = null;
    public static GameSceneManager instance
    {
        get
        {
            if (_instance == null)
                _instance = (GameSceneManager)FindObjectOfType(typeof(GameSceneManager));
            return _instance;
        }
    }

    // Private
    private Dictionary<int, AIStateMachine> _stateMachines = new Dictionary<int, AIStateMachine>();
    private Dictionary<int, PlayerInfo> _playerInfos = new Dictionary<int, PlayerInfo>();

    // Properties
    public ParticleSystem bloodParticles { get { return _bloodParticles; } }

    // Public Methods
    // --------------------------------------------------------------------
    // Name	:	RegisterAIStateMachine
    // Desc	:	Stores the passed state machine in the dictionary with
    //			the supplied key
    // --------------------------------------------------------------------
    public void RegisterAIStateMachine(int key, AIStateMachine stateMachine)
    {
        if (!_stateMachines.ContainsKey(key))
        {
            _stateMachines[key] = stateMachine;
        }
    }

    // --------------------------------------------------------------------
    // Name	:	GetAIStateMachine
    // Desc	:	Returns an AI State Machine reference searched on by the
    //			instance ID of an object
    // --------------------------------------------------------------------
    public AIStateMachine GetAIStateMachine(int key)
    {
        AIStateMachine machine = null;
        if (_stateMachines.TryGetValue(key, out machine))
        {
            return machine;
        }

        return null;
    }

    // --------------------------------------------------------------------
    // Name	:	RegisterPlayerInfo
    // Desc	:	Stores the passed PlayerInfo in the dictionary with
    //			the supplied key
    // --------------------------------------------------------------------
    public void RegisterPlayerInfo(int key, PlayerInfo playerInfo)
    {
        if (!_playerInfos.ContainsKey(key))
        {
            _playerInfos[key] = playerInfo;
        }
    }

    // --------------------------------------------------------------------
    // Name	:	GetPlayerInfo
    // Desc	:	Returns a PlayerInfo reference searched on by the
    //			instance ID of an object
    // --------------------------------------------------------------------
    public PlayerInfo GetPlayerInfo(int key)
    {
        PlayerInfo info = null;
        if (_playerInfos.TryGetValue(key, out info))
        {
            return info;
        }

        return null;
    }
}
