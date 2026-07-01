using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsManager : MonoBehaviour, ISkillResourceUser
{
    public static PlayerStatsManager Instance {get; private set;}
    [SerializeField] private PlayerStats _runtimeStats;
    public PlayerStats RunTimeStats => _runtimeStats;
    private bool _isRecoveringSP = false;
    private PlayerContext _playerContext;
    public event Action<float, float> OnHPChanged;
    public event Action<float, float> OnSPChanged;
    [SerializeField, Range(0f, 1f)] private float _hpRecoveryOnDamageChance = 0.1f;
    [SerializeField, Range(0f, 1f)] private float _maxHpRecoveryFromDamagePercent = 0.1f;
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
            .CriticalChance = baseStats.CriticalChance;
        _runtimeStats
            .CriticalDamageMultiplier = baseStats.CriticalDamageMultiplier;
        _runtimeStats
            .MoveSpeed = baseStats.MoveSpeed;
        _runtimeStats
            .AttackRange = baseStats.AttackRange;

        Reset();
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
        int amount = Mathf.RoundToInt(_runtimeStats.MaxHP*multiplier);
        HealAmount(amount);
    }

    public void TryRecoverHPFromDamageDealt(int damageDealt)
    {
        if (damageDealt <= 0 || _runtimeStats.CurrentHP >= _runtimeStats.MaxHP)
            return;

        if (UnityEngine.Random.value > Mathf.Clamp01(_hpRecoveryOnDamageChance))
            return;

        int maxRecovery = Mathf.FloorToInt(_runtimeStats.MaxHP * _maxHpRecoveryFromDamagePercent);
        int amount = Mathf.Min(damageDealt, maxRecovery);

        HealAmount(amount);
    }

    private void HealAmount(int amount)
    {
        if (amount <= 0)
            return;

        int oldHP = _runtimeStats.CurrentHP;
        _runtimeStats.CurrentHP = Mathf.Min(_runtimeStats.CurrentHP + amount, _runtimeStats.MaxHP);
        int amountHealed = _runtimeStats.CurrentHP - oldHP;

        if (amountHealed <= 0)
            return;

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
        OnSPChanged?.Invoke(_runtimeStats.CurrentSP, _playerContext.Stats.MaxSP);

        _spRecoveryTimer = 0f; // reset timer every time uses a skill

        StartSPRecoveryLoop();
    }

    public bool HasEnoughSP(int amount)
    {
        return _runtimeStats.CurrentSP >= amount;
    }

    private void StartSPRecoveryLoop()
    {
        if(_isRecoveringSP) return;

        _= RecoverSPLoopAsync(destroyCancellationToken);
    }

    private async Awaitable RecoverSPLoopAsync(CancellationToken token)
    {
        _isRecoveringSP = true;

        while (_runtimeStats.CurrentSP < _runtimeStats.MaxSP)
        {
            try
            {
                await Awaitable.WaitForSecondsAsync(_spRecoveryInterval, token);

                if (token.IsCancellationRequested) return;

                _runtimeStats.CurrentSP = Mathf.Min(_runtimeStats.CurrentSP + _spRecoveryRate, _runtimeStats.MaxSP);
                OnSPChanged?.Invoke(_runtimeStats.CurrentSP, _playerContext.Stats.MaxSP);
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }

        _isRecoveringSP = false;
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
