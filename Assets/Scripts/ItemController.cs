using System;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    public static ItemController Instance {get; private set;}
    [SerializeField] private Consumable _apple;

#region PlayerItems
    public int MaxApples = 99;
    public int MaxApplesObtained = 99;
    public int Apples = 99;
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
                _apple.Use();
                Apples--;
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
