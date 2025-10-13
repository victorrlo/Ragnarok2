using System;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController Instance {get; private set;}
    [SerializeField] private PlayerStatsData _playerStatsData;

#region PlayerSkills
    private bool _hasWaterBodySkill = false;
    public bool HasWaterBodySkill => _hasWaterBodySkill;
    private bool _hasStompPuddleSkill = false;
    public bool HasStompPuddleSkill => _hasStompPuddleSkill;
    private bool _hasWaterBombSkill = false;
    public bool HasWaterBombSkill => _hasWaterBombSkill;
#endregion

#region PlayerItems
    public int MaxApples = 7;
    public int MaxApplesObtained = 0;
    public int Apples = 0;
#endregion

#region Events
    
    public Action<bool> TryUseApple;
    public Action<bool> FailedUsingApple;

#endregion

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        // _playerStatsData.LoadFromPrefs();
    }

    private void Start()
    {
        TryUseApple += TryConsumingApple;
    }

    private void OnDestroy()
    {
        TryUseApple -= TryConsumingApple;
    }

    private void TryConsumingApple(bool shouldUse)
    {
        if (shouldUse)
        {
            if (Apples > 0)
            {
                PlayerStatsManager.Instance.Heal();
                Debug.Log("Heals " + _playerStatsData.MaxHP*0.25);
                Apples--;
                // Debug.Log("Consumed apple!");
            }
            else
            {
                FailedUsingApple.Invoke(true);
            }
        }
    }
    public void OnApplicationQuit()
    {
        // _playerStatsData.SaveToPrefs();
    }
}
