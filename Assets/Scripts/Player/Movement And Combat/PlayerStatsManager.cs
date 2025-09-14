using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsManager : MonoBehaviour
{
    public static PlayerStatsManager Instance {get; private set;}
    private PlayerContext _playerContext;
    public int CurrentHP {
        get => _playerContext.Stats.GetCurrentHP(); 
        private set
        {
            _playerContext.Stats._currentHP = Mathf.Clamp(value, 0, _playerContext.Stats.MaxHP);
            OnHPChanged?.Invoke(_playerContext.Stats._currentHP, _playerContext.Stats.MaxHP);
            if (_playerContext.Stats._currentHP <= 0)
                Die();
        }
    }
    public int CurrentSP
    {
        get => _playerContext.Stats.GetCurrentSP();
        private set
        {
            _playerContext.Stats._currentSP = Mathf.Clamp(value, 0, _playerContext.Stats.MaxSP);
            OnSPChanged?.Invoke(_playerContext.Stats._currentSP, _playerContext.Stats.MaxSP);
        }
    }
    public event Action<int, int> OnHPChanged;
    public event Action<int, int> OnSPChanged;
    [SerializeField] private Image _healthBar;
    [SerializeField] private Image _spiritBar;
    [SerializeField] private GameObject _statsBar;
    [SerializeField] private Vector3 _offset = new Vector3(0, -30f, 0);
    [SerializeField] private GameObject _player;
    private Camera _mainCamera;

    private void Awake()
    {
        if (_playerContext == null)
            TryGetComponent<PlayerContext>(out _playerContext);

        _mainCamera = Camera.main;
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // _stats.LoadFromPrefs();

        OnHPChanged += UpdateHealthBar;
        OnSPChanged += UpdateSpiritBar;

        _playerContext.Stats.ResetStats();
    }

    private void Start()
    {
        OnHPChanged?.Invoke(_playerContext.Stats._currentHP, _playerContext.Stats.MaxHP);
        OnSPChanged?.Invoke(_playerContext.Stats._currentSP, _playerContext.Stats.MaxSP);
    }

    private void LateUpdate()
    {
        if (_player == null || _mainCamera == null) return;

        SetStatsBarBellowPlayer();
    }

    private void OnDestroy()
    {
        OnHPChanged -= UpdateHealthBar;
        OnSPChanged -= UpdateSpiritBar;
    }

    private void UpdateHealthBar(int currentHP, int maxHP)
    {
        _healthBar.fillAmount = Mathf.Clamp01((float) currentHP / maxHP);
    }

    private void UpdateSpiritBar(int currentSP, int maxSP)
    {
        _spiritBar.fillAmount = Mathf.Clamp01((float) currentSP / maxSP);
    }

    public void TakeDamage(int amount)
    {
        CurrentHP -= amount;
    }

    public void UseSP(int amount)
    {
        CurrentSP -= amount;
    }

    private void Die()
    {
        Debug.Log("Player morreu");
    }

    public void Heal(int amount)
    {
        CurrentHP += amount;
    }

    private void SetStatsBarBellowPlayer()
    {
        _statsBar.SetActive(true);
        if (_mainCamera == null) _mainCamera = Camera.main;
        
        Vector3 screenPos = _mainCamera.WorldToScreenPoint(_player.transform.position);

        Vector3 targetPos = screenPos + _offset;
        
        _statsBar.transform.position = targetPos;
    }
}
