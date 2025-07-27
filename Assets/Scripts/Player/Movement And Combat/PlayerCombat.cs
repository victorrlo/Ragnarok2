using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] PlayerMovement _playerMovement;
    [SerializeField] private GameObject _targetMarkerPrefab;
    [SerializeField] private float _attackInterval = 0.5f;
    [SerializeField] private int _damage = 1;
    private PlayerStats _playerStats;

    private GameObject _currentTarget;
    private Vector3Int _currentEnemyCell;
    private GameObject _activeTargetMarker;
    private Coroutine _combatCoroutine;
    private bool _isChasing;

    private void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();
    }

    public void StartCombat(GameObject enemy)
    {
        if (_combatCoroutine != null)
            StopCoroutine(_combatCoroutine);

        MarkTarget(enemy);
        _currentTarget = enemy;
        _currentEnemyCell = GridManager.Instance.WorldToCell(enemy.transform.position);
        
        _combatCoroutine = StartCoroutine(ChaseAndAttack(_currentTarget));
    }

    public void TryClickToAttack()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            EnemyCombat enemy = hit.collider.GetComponent<EnemyCombat>();
            if (enemy != null)
                StartCombat(enemy.gameObject);
            else
                StopCombat();
        }
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

            var enemy = target.GetComponent<EnemyCombat>();

            if (enemy != null)
                if (IsAdjacent(playerPos, enemyPos))
                {
                    Debug.Log("Attacking!");
                    // enemy.TakeDamage(_damage);
                }
                else
                    WalkToEnemy(playerPos, enemyPos);

            yield return new WaitForSeconds(_attackInterval);
        }

        StopCombat();
    }

    public void TakeDamage(int amount)
    {
        _playerStats.TakeDamage(amount);
        FloatingTextPool.Instance.ShowDamage(transform.position, amount, Color.red);
        // stagger player

        if (_playerStats.CurrentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    private bool IsAdjacent(Vector3Int a, Vector3Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);

        return dx <= 1 && dy <= 1 && (dx + dy > 0);
    }

    private void WalkToEnemy(Vector3Int playerPos, Vector3Int enemyPos)
    {
        Vector3Int tile = FindAdjacentTile(playerPos, enemyPos);
        
        List<Node> path = NodeManager.Instance.FindPath(playerPos, tile);
        if (path != null && path.Count > 0)
            _playerMovement.MoveAlongPath(path);
    }

    private Vector3Int FindAdjacentTile(Vector3Int from, Vector3Int target)
    {
        Vector3Int best = Vector3Int.zero;
        float bestDist = float.MaxValue;

        foreach (var dir in DirectionHelper._directions)
        {
            Vector3Int pos = target + dir;
            if (!NodeManager.Instance.IsWalkable(pos)) continue;
            // if (GridOccupancyManager.Instance.IsCellOccupied(pos)) continue;

            float dist = Vector3Int.Distance(from, pos);
            if (dist < bestDist)
            {
                best = pos;
                bestDist = dist;
            }
        }

        return best;
    }
}
