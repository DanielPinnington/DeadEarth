using UnityEngine;
using System.Collections;

public class CharacterManager : MonoBehaviour
{
    // Inspector Assigned
    [SerializeField] private CapsuleCollider _meleeTrigger = null;
    [SerializeField] private CameraBloodEffect _cameraBloodEffect = null;
    [SerializeField] private Camera _camera = null;
    [SerializeField] private float _health = 100.0f;
    [SerializeField] private int _ammo = 6;

    public AudioSource _shootSound = null;
    public AudioSource _emptyGun = null;

    // Private
    private Collider _collider = null;
    private FPSController _fpsController = null;
    private CharacterController _characterController = null;
    private GameSceneManager _gameSceneManager = null;
    private int _aiBodyPartLayer = -1;

    // Use this for initialization
    void Start()
    {
        _collider = GetComponent<Collider>();
        _fpsController = GetComponent<FPSController>();
        _characterController = GetComponent<CharacterController>();
        _gameSceneManager = GameSceneManager.instance;

        _aiBodyPartLayer = LayerMask.NameToLayer("AI Body Part");

        if (_gameSceneManager != null)
        {
            PlayerInfo info = new PlayerInfo();
            info.camera = _camera;
            info.characterManager = this;
            info.collider = _collider;
            info.meleeTrigger = _meleeTrigger;

            _gameSceneManager.RegisterPlayerInfo(_collider.GetInstanceID(), info);
        }
    }

    public void TakeDamage(float amount)
    {
        _health = Mathf.Max(_health - (amount * Time.deltaTime), 0.0f);
        if (_cameraBloodEffect != null)
        {
            _cameraBloodEffect.minBloodAmount = (1.0f - _health / 100.0f);
            _cameraBloodEffect.bloodAmount = Mathf.Min(_cameraBloodEffect.minBloodAmount + 0.3f, 1.0f);
        }
    }

    public void DoDamage(int hitDirection = 0)
    {
        if (_camera == null) return;
        if (_gameSceneManager == null) return;

        // Local Variables
        Ray ray;
        RaycastHit hit;
        bool isSomethingHit = false;

        ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        isSomethingHit = Physics.Raycast(ray, out hit, 1000.0f, 1 << _aiBodyPartLayer);

        if (isSomethingHit)
        {
            AIStateMachine stateMachine = _gameSceneManager.GetAIStateMachine(hit.rigidbody.GetInstanceID());
            Debug.Log("Body part is: " + hit.rigidbody.GetInstanceID());
            if (stateMachine)
            {
                Debug.Log("This has worked");
                stateMachine.TakeDamage(hit.point, ray.direction * 1.0f, 25, hit.rigidbody, this, 0);
            }
        }

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && _ammo > 0)
        {
            if (_ammo >= 0)
            {
                _ammo -= 1;
                _shootSound.Play();
                DoDamage();
            }
            //_shootSound.Play();
            //DoDamage();
            print("" + _ammo);
        }
        if (Input.GetMouseButtonDown(0) && _ammo <= 0)
        {
            _emptyGun.Play();
        }
    }
}

