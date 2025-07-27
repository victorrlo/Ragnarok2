using UnityEngine;
using UnityEngine.UI;

public class EnemyStats : MonoBehaviour
{
    [SerializeField] private EnemyEventBus _enemyEventBus;
    [SerializeField] private GameObject _monster;
    [SerializeField] private MonsterStatsData _statsData;
    private int _currentHP;
    [SerializeField] private GameObject _statsBar;
    [SerializeField] private Image _healthBar;
    private bool hasBeenDamaged = false;
    private Camera _mainCamera;
    [SerializeField] private Vector3 _offset = new Vector3(0, -30f, 0);

    public MonsterStatsData StatsData => _statsData;
    public int MaxHP => _statsData._maxHP;

    private void Awake()
    {
        if (_enemyEventBus == null)
            TryGetComponent<EnemyEventBus>(out _enemyEventBus);

        _mainCamera = Camera.main;
        _currentHP = _statsData._maxHP;
        _statsBar.SetActive(false);
        _healthBar.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (_monster == null || _mainCamera == null) return;

        ShowHealthBar();
    }

    private void ShowHealthBar()
    {
        if (!hasBeenDamaged) return;
        else
        {
            _healthBar.gameObject.SetActive(true);
            SetHealthBarPosition();
        }
    }

    public void TakeDamage(int amount)
    {
        _currentHP -= amount;
        _healthBar.fillAmount = (float)_currentHP / _statsData._maxHP;
        hasBeenDamaged = true;
        var data = new EnemyDamageEventData(gameObject, amount);
        _enemyEventBus.OnDamaged.Raise(data);
    }

    private void SetHealthBarPosition()
    {
        _statsBar.SetActive(true);
        if (_mainCamera == null) _mainCamera = Camera.main;
        
        Vector3 screenPos = _mainCamera.WorldToScreenPoint(_monster.transform.position);

        Vector3 targetPos = screenPos + _offset;
        
        _statsBar.transform.position = targetPos;
    }
}
