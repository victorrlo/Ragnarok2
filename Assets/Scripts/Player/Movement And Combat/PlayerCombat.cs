using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private PlayerContext _playerContext;
    private EnemyCombat _enemy;
    [SerializeField] private GameObject _targetMarkerPrefab;
    private GameObject _currentTarget;
    private Vector3Int _currentEnemyCell;
    private GameObject _activeTargetMarker;
    private Coroutine _combatCoroutine;
    private bool _isChasing;

    private void Awake()
    {
        if (_playerContext == null)
            TryGetComponent<PlayerContext>(out _playerContext);
    }

    public void StartCombat(GameObject enemy)
    {
        if (_currentTarget == enemy) return;

        if (_combatCoroutine != null)
            StopCoroutine(_combatCoroutine);

        MarkTarget(enemy);
        _currentTarget = enemy;
        _currentEnemyCell = GridManager.Instance.WorldToCell(enemy.transform.position);
        
        _combatCoroutine = StartCoroutine(ChaseAndAttack(_currentTarget));
    }

    public void StopCombat()
    {
        _isChasing = false;

        if (_combatCoroutine != null)
            StopCoroutine(_combatCoroutine);
        
        _combatCoroutine = null;
        _currentTarget = null;

        if (_activeTargetMarker != null)
        {
            Destroy(_activeTargetMarker);
            _activeTargetMarker = null;
        }
    }

    private void MarkTarget(GameObject target)
    {
        if (_activeTargetMarker != null)
            Destroy(_activeTargetMarker);

        _activeTargetMarker = Instantiate(_targetMarkerPrefab);
        _activeTargetMarker.GetComponent<TargetMarker>().AttachTo(target.transform);
    }

    private IEnumerator ChaseAndAttack(GameObject target)
    {
        _isChasing = true;

        while (target != null && _isChasing)
        {
            Vector3Int playerPos = GridManager.Instance.WorldToCell(transform.position);
            Vector3Int enemyPos = GridManager.Instance.WorldToCell(target.transform.position);

            _enemy = target.GetComponent<EnemyCombat>();

            if (_enemy != null)
            {
                if (DistanceHelper.IsAdjacent(playerPos, enemyPos, _playerContext.Stats.AttackRange))
                {
                    Attack();
                    yield return new WaitForSeconds(_playerContext.Stats.AttackSpeed);
                }
                else
                    StartCoroutine(_playerContext.Movement.ChaseEnemy(target));
            }

            yield return null;
        }

        StopCombat();
    }

    private void Attack()
    {
        _enemy.TakeDamage(_playerContext.Stats.Attack);
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

    // private void WalkToEnemy(Vector3Int playerPos, Vector3Int enemyPos)
    // {
        // Vector3Int tile = FindAdjacentTile(playerPos, enemyPos);
        
    //     List<Node> path = NodeManager.Instance.FindPath(playerPos, enemyPos);
    //     if (path != null && path.Count > 0)
    //         StartCoroutine(_playerContext.Movement.MoveAlongPath(path));
    // }

    private Vector3Int FindAdjacentTile(Vector3Int from, Vector3Int target)
    {
        Vector3Int best = Vector3Int.zero;
        float bestDist = float.MaxValue;

        foreach (var dir in DirectionHelper._directions)
        {
            Vector3Int pos = target + dir;
            if (!NodeManager.Instance.IsWalkable(pos)) continue;

            float dist = Vector3Int.Distance(from, pos);
            if (dist < bestDist)
            {
                best = pos;
                bestDist = dist;
            }
        }

        return best;
    }
    
    private void Die()
    {
        Destroy(gameObject);
    }
}
