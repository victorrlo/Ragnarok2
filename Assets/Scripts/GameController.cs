using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance {get; private set;}
    [SerializeField] private PlayerStatsData _playerStatsData;

    //SKILLS
    private bool _hasWaterBodySkill = false;
    public bool HasWaterBodySkill => _hasWaterBodySkill;
    private bool _hasStompPuddleSkill = false;
    public bool HasStompPuddleSkill => _hasStompPuddleSkill;
    private bool _hasWaterBombSkill = false;
    public bool HasWaterBombSkill => _hasWaterBombSkill;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        // _playerStatsData.LoadFromPrefs();
    }

    private void Start()
    {

    }
    public void OnApplicationQuit()
    {
        // _playerStatsData.SaveToPrefs();
    }
}
