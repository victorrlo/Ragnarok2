using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(EnemyContext))]
public class EnemyStatsManager : MonoBehaviour
{
    private EnemyContext _enemyContext;
    private float _currentHP;
    public float CurrentHP => _currentHP;
    private float _currentSP;
    public float CurrentSP => _currentSP;
    [SerializeField] private GameObject _statsBar;
    [SerializeField] private Image _healthBar;
    [SerializeField] private Image _spiritBar;
    private bool hasBeenDamaged = false;
    private Camera _mainCamera;
    [SerializeField] private Vector3 _offset = new Vector3(0, -30f, 0);

    private void Awake()
    {
        if (_enemyContext == null)
            TryGetComponent<EnemyContext>(out _enemyContext);

        _mainCamera = Camera.main;
        _currentHP = _enemyContext.Stats.MaxHP;
        _currentSP = _enemyContext.Stats.MaxSP;
        _statsBar.SetActive(false);
        _healthBar.gameObject.SetActive(false);
        _spiritBar.gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (this.gameObject == null || _mainCamera == null) return;

        ShowStatsBar();
    }

    private void ShowStatsBar()
    {
        if (!hasBeenDamaged) return;
        else
        {
            _healthBar.gameObject.SetActive(true);
            // _spiritBar.gameObject.SetActive(true);
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

    public void UseSP(int amount)
    {
        _currentSP -= amount;
        _spiritBar.fillAmount = (float)_currentSP / _enemyContext.Stats.MaxSP;
    }
}
