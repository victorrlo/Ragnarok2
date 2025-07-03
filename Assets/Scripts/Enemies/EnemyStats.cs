using UnityEngine;
using UnityEngine.UI;

public class EnemyStats : MonoBehaviour
{
    [SerializeField] private GameObject _monster;
    [SerializeField] private MonsterStatsData _statsData;
    public MonsterStatsData StatsData => _statsData;
    private int _currentHP;
    public int MaxHP => _statsData._maxHP;
    [SerializeField] private GameObject _statsBar;
    [SerializeField] private Image _healthBar;
    private bool hasBeenDamaged = false;
    private Camera _mainCamera;
    [SerializeField] private Vector3 _offset = new Vector3(0, -30f, 0);

    private void Awake()
    {
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
