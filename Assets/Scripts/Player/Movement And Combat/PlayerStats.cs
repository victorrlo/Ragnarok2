using System;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance {get; private set;}
    [SerializeField] private PlayerStatsData _stats;
    public int CurrentHP {
        get => _stats.GetCurrentHP(); 
        private set
        {
            _stats._currentHP = Mathf.Clamp(value, 0, _stats.MaxHP);
            OnHPChanged?.Invoke(_stats._currentHP, _stats.MaxHP);
            if (_stats._currentHP <= 0)
                Die();
        }
    }
    public int CurrentSP
    {
        get => _stats.GetCurrentSP();
        private set
        {
            _stats._currentSP = Mathf.Clamp(value, 0, _stats.MaxSP);
            OnSPChanged?.Invoke(_stats._currentSP, _stats.MaxSP);
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
    }

    private void Start()
    {
        OnHPChanged?.Invoke(_stats._currentHP, _stats.MaxHP);
        OnSPChanged?.Invoke(_stats._currentSP, _stats.MaxSP);
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

    private void SetStatsBarBellowPlayer()
    {
        _statsBar.SetActive(true);
        if (_mainCamera == null) _mainCamera = Camera.main;
        
        Vector3 screenPos = _mainCamera.WorldToScreenPoint(_player.transform.position);

        Vector3 targetPos = screenPos + _offset;
        
        _statsBar.transform.position = targetPos;
    }
}
