using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(EnemyContext))]
public class EnemyStatsManager : MonoBehaviour
{
    private EnemyContext _enemyContext;
    private int _currentHP;
    public int CurrentHP => _currentHP;
    [SerializeField] private GameObject _statsBar;
    [SerializeField] private Image _healthBar;
    private bool hasBeenDamaged = false;
    private Camera _mainCamera;
    [SerializeField] private Vector3 _offset = new Vector3(0, -30f, 0);

    private void Awake()
    {
        if (_enemyContext == null)
            TryGetComponent<EnemyContext>(out _enemyContext);

        _mainCamera = Camera.main;
        _currentHP = _enemyContext.Stats.MaxHP;
        _statsBar.SetActive(false);
        _healthBar.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (this.gameObject == null || _mainCamera == null) return;

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
        _healthBar.fillAmount = (float)_currentHP / _enemyContext.Stats.MaxHP;
        hasBeenDamaged = true;
        var data = new DamageEventData(gameObject, amount);
        _enemyContext.EventBus.RaiseOnDamaged(data);
    }

    private void SetHealthBarPosition()
    {
        _statsBar.SetActive(true);
        if (_mainCamera == null) _mainCamera = Camera.main;
        
        Vector3 screenPos = _mainCamera.WorldToScreenPoint(this.transform.position);

        Vector3 targetPos = screenPos + _offset;
        
        _statsBar.transform.position = targetPos;
    }
}
