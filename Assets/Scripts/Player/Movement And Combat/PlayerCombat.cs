using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private PlayerContext _playerContext;
    private PlayerEventBus _eventBus;

    [SerializeField] private GameObject _targetMarkerPrefab;

    private GameObject _currentTarget;
    private GameObject _activeTargetMarker;
    private Coroutine _combatCoroutine;

    private void Awake()
    {
        if (_playerContext == null)
            TryGetComponent<PlayerContext>(out _playerContext);

        if (_eventBus == null)
            _playerContext.TryGetComponent<PlayerEventBus>(out _eventBus);
    }

    private void OnEnable()
    {
        _eventBus.OnStartAttack += StartCombat;
        _eventBus.OnStopAttack += StopCombat;
    }

    private void OnDisable()
    {
        _eventBus.OnStartAttack -= StartCombat;
        _eventBus.OnStopAttack -= StopCombat;
    }

    public void StartCombat(StartAttackData data)
    {
        var enemy = data.target;

        if (_combatCoroutine != null)
            StopCoroutine(_combatCoroutine);

        _currentTarget = enemy;
        _combatCoroutine = StartCoroutine(TargetEnemy(_currentTarget));
    }

    public void StopCombat()
    {
        if (_combatCoroutine != null)
            StopCoroutine(_combatCoroutine);
        
        _combatCoroutine = null;

        _currentTarget = null;

        ClearMarker();
    }

    public void MarkTarget(GameObject target)
    {
        if (_targetMarkerPrefab == null)
        {
            Debug.LogError("Target marker prefab not assigned");
            return;
        }

        if (target == null) { ClearMarker(); return;}

        ClearMarker();

        _activeTargetMarker = Instantiate(_targetMarkerPrefab);
        var marker = _activeTargetMarker.GetComponent<TargetMarker>();

        if (marker == null)
        {
            Debug.LogError("TargetMarker component missing on prefab");
            return;
        }

        marker.AttachTo(target.transform);
    }

    private void ClearMarker()
    {
        if (_activeTargetMarker != null)
        {
            Destroy(_activeTargetMarker);
            _activeTargetMarker = null;
        }
    }

    private void Attack(EnemyCombat enemy)
    {
        if (enemy == null) 
        {
            return;
        }

        // var data = new StartAttackData(_enemy.gameObject);
        // _playerContext.EventBus.RaiseStartAttack(data);


        enemy.TakeDamage(_playerContext.Stats.Attack);
    }

    private IEnumerator TargetEnemy(GameObject target)
    {
        MarkTarget(target);

        if (target == null) yield return null;

        while (true)
        {
            Vector3Int playerPos = GridManager.Instance.WorldToCell(transform.position);
            Vector3Int enemyPos = GridManager.Instance.WorldToCell(target.transform.position);

            var enemy = target.GetComponent<EnemyCombat>();

            if (enemy == null)
            {
                yield break;
            }
            
            if (DistanceHelper.IsInAttackRange(playerPos, enemyPos, _playerContext.Stats.AttackRange))
            {   
                // _playerContext.Movement.StopAllMovementCoroutines(); movement routines shouldnt be in player combat
                yield return new WaitForSeconds(_playerContext.Stats.AttackSpeed);
                Attack(enemy);
            }
            // else
            // {
            //     _playerContext.Movement.StartChasingEnemy(target);
            // }
            yield return null;
        }
    }

    public void TakeDamage(int amount)
    {
        _playerContext.StatsManager.TakeDamage(amount);
        FloatingTextPool.Instance.ShowDamage(transform.position, amount, Color.red);
        // stagger player

        if (_playerContext.StatsManager.CurrentHP <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        Destroy(gameObject);
    }
}
