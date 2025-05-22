using System;
using UnityEditor.Timeline.Actions;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance {get; private set;}
    [SerializeField] private int _maxHP = 100;
    [SerializeField] private int _maxSP = 50;
    private int _currentHP;
    private int _currentSP;
    public int CurrentHP {
        get => _currentHP; 
        private set
        {
            _currentHP = Mathf.Clamp(value, 0, _maxHP);
            OnHPChanged?.Invoke(_currentHP, _maxHP);
            if (_currentHP <= 0)
                Die();
        }
    }
    public int CurrentSP
    {
        get => _currentSP;
        private set
        {
            _currentSP = Mathf.Clamp(value, 0, _maxSP);
            OnSPChanged?.Invoke(_currentSP, _maxSP);
        }
    }
    public event Action<int, int> OnHPChanged;
    public event Action<int, int> OnSPChanged;
    [SerializeField] private Image _healthBar;
    [SerializeField] private Image _spiritBar;
    [SerializeField] private GameObject _statsBar;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Vector3 _offset = new Vector3(0, -30f, 0);
    [SerializeField] private GameObject _player;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        _currentHP = _maxHP;
        _currentSP = _maxSP;

        OnHPChanged += UpdateHealthBar;
        OnSPChanged += UpdateSpiritBar;
    }

    private void Start()
    {
        OnHPChanged?.Invoke(_currentHP, _maxHP);
        OnSPChanged?.Invoke(_currentSP, _maxSP);
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
