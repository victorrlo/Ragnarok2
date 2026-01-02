using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsManager : MonoBehaviour
{
    public static PlayerStatsManager Instance {get; private set;}
    [SerializeField] private PlayerStats _runtimeStats;
    public PlayerStats RunTimeStats => _runtimeStats;
    private PlayerContext _playerContext;
    public event Action<float, float> OnHPChanged;
    public event Action<float, float> OnSPChanged;
    [SerializeField] private Image _healthBar;
    [SerializeField] private Image _spiritBar;
    [SerializeField] private GameObject _statsBar;
    [SerializeField] private Vector3 _offset = new Vector3(0, -30f, 0);
    [SerializeField] private GameObject _player;
    private Camera _mainCamera;


    // sp recovery
    // doing that fixed for now, but will probably add as a stat calculation later
    private float _spRecoveryTimer = 0f;
    private float _spRecoveryInterval = 1f; // in seconds
    private int _spRecoveryRate = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        if (_playerContext == null)
            TryGetComponent<PlayerContext>(out _playerContext);

        _mainCamera = Camera.main;

        OnHPChanged += UpdateHealthBar;
        OnSPChanged += UpdateSpiritBar;
    }

    private void Start()
    {
        var baseStats = _playerContext.Stats;

        _runtimeStats
            .MaxHP = baseStats.MaxHP;
        _runtimeStats
            .MaxSP  = baseStats.MaxSP;
        _runtimeStats
            .Attack = baseStats.Attack;
        _runtimeStats
            .Agility = baseStats.Agility;
        _runtimeStats
            .MoveSpeed = baseStats.MoveSpeed;
        _runtimeStats
            .AttackRange = baseStats.AttackRange;

        Reset();
    }

    private void Update()
    {
        _spRecoveryTimer += Time.deltaTime;

        if (_spRecoveryTimer >= _spRecoveryInterval)
        {
            RecoverSP();
            _spRecoveryTimer = 0f;
        }
    }

    private void LateUpdate()
    {
        if (_player == null || _mainCamera == null) return;
    }

    private void OnDestroy()
    {
        OnHPChanged -= UpdateHealthBar;
        OnSPChanged -= UpdateSpiritBar;
    }

    public void Reset()
    {
        _runtimeStats.CurrentHP = Mathf.RoundToInt(_runtimeStats.MaxHP);
        _runtimeStats.CurrentSP = Mathf.RoundToInt(_runtimeStats.MaxSP);

        OnHPChanged?.Invoke(_runtimeStats.CurrentHP, _playerContext.Stats.MaxHP);
        OnSPChanged?.Invoke(_runtimeStats.CurrentSP, _playerContext.Stats.MaxSP);
    }

    public void Heal(float multiplier)
    {
        int oldHP = _runtimeStats.CurrentHP;
        int amount = Mathf.RoundToInt(_runtimeStats.MaxHP*multiplier);
        int newHP = _runtimeStats.CurrentHP + amount;

        int amountHealed = amount - oldHP;

        // prevent over healing
        _runtimeStats.CurrentHP = Mathf.Min(newHP, _runtimeStats.MaxHP);


        FloatingTextPool.Instance.ShowHeal(transform.position, amountHealed);
        OnHPChanged?.Invoke(_runtimeStats.CurrentHP, _playerContext.Stats.MaxHP);
    }

    public void TakeDamage(int amount)
    {
        _runtimeStats.CurrentHP -= amount;

        OnHPChanged?.Invoke(_runtimeStats.CurrentHP, _playerContext.Stats.MaxHP);

        if (_runtimeStats.CurrentHP <= 0)
            Die();
    }

    public void UseSP(int amount)
    {
        _runtimeStats.CurrentSP -= amount;

        _spRecoveryTimer = 0f; // reset timer every time uses a skill

        OnSPChanged?.Invoke(_runtimeStats.CurrentSP, _playerContext.Stats.MaxSP);
    }

    public void RecoverSP()
    {
        if (_runtimeStats.CurrentSP < _runtimeStats.MaxSP)
        {
            int oldSP = _runtimeStats.CurrentSP;

            _runtimeStats.CurrentSP = Mathf.Min(_runtimeStats.CurrentSP + _spRecoveryRate, _runtimeStats.MaxSP);

            int recoveredSP = _runtimeStats.CurrentSP - oldSP;

            if (recoveredSP > 0)
            {
                // FloatingTextPool.Instance.ShowSPRecovery(transform.position, recoveredSP);
                OnSPChanged?.Invoke(_runtimeStats.CurrentSP, _playerContext.Stats.MaxSP);
            }
        }
    }

    private void UpdateHealthBar(float currentHP, float maxHP)
    {
        _healthBar.fillAmount = Mathf.Clamp01((float) currentHP / maxHP);
    }

    private void UpdateSpiritBar(float currentSP, float maxSP)
    {
        _spiritBar.fillAmount = Mathf.Clamp01((float) currentSP / maxSP);
    }

    private void Die()
    {
        // handle death logic
        Destroy(gameObject);
    }
}
