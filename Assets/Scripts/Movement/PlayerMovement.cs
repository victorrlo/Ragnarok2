using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerMovement : GridMovement
{   
    [SerializeField] private AimBehaviour _aim;
    [SerializeField] private NodeManager _nodeManager;
    private Vector3Int? _startPos;
    private Vector3Int? _finalPos;
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _attackInterval = 0.5f;
    [SerializeField] private int _damage = 1;
    private Coroutine _combatCoroutine;
    private bool _isChasing;
    private Coroutine _movementCoroutine;
    private GameObject _currentTarget;
    [SerializeField] private GameObject _targetMarkerPrefab;
    private GameObject _activeTargetMarker;

    public void OnMouseClick(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        _startPos = _grid.WorldToCell(transform.position);
        _finalPos = _aim._lastGridCellPosition;
        if (!_finalPos.HasValue) return;

        if (GridOccupancyManager.Instance.TryGetOccupant(_finalPos.Value, out GameObject occupant) &&
            occupant.CompareTag("Enemy"))
        {
            StartCombat(occupant);
            return;
        }

        if(!_aim.IsWalkable(_finalPos.Value))
        {
            return; // not walkable
        }

        _isChasing = false;
        _currentTarget = null;
        MarkEnemyAsTarget(null, false);
        if (_combatCoroutine != null)
        {
            StopCoroutine(_combatCoroutine);
            _combatCoroutine = null;
        }

        List<Node> path = _nodeManager.FindPath(_startPos.Value, _finalPos.Value);
        // _nodeManager.DrawPath(path); to see path in inspector

        if (path == null || path.Count == 0) // couldn't find a path
        {
            return;
        }
        
        if (_movementCoroutine != null) // if already walking, stop to initiate another movement
        {
            StopCoroutine(_movementCoroutine);
        }

        _movementCoroutine = StartCoroutine(FollowPath(path, _moveSpeed)); // uses gridMovement script to walk
    }

    private Vector3Int FindAdjacentWalkableTile(Vector3Int from, Vector3Int target)
    {
        Vector3Int best = Vector3Int.zero;
        float bestDistance = float.MaxValue;

        foreach (var dir in DirectionHelper._directions)
        {
            Vector3Int adjacent = target + dir;

            if (!_nodeManager.IsWalkable(adjacent)) continue;
            if(GridOccupancyManager.Instance.IsCellOccupied(adjacent)) continue;

            float dist = Vector3Int.Distance(from, adjacent);
            if (dist < bestDistance)
            {
                bestDistance = dist;
                best = adjacent;
            }
        }

        return best;
    }

    // private IEnumerator FollowPathAndAttack(List<Node> path)
    // {
    //     yield return StartCoroutine(FollowPath(path, _moveSpeed));

    //     yield return new WaitForSeconds(0.1f);

    //     if (_currentTarget != null)
    //     {
    //         var health = _currentTarget.GetComponent<EnemyHealth>();
    //         if (health != null)
    //         {
    //             Debug.Log("tomou 1 de dano!");
    //             health.TakeDamage(1);
    //         }
    //     }
    // }

    private void StartCombat(GameObject enemy)
    {
        if (_combatCoroutine != null)
            StopCoroutine(_combatCoroutine);

        MarkEnemyAsTarget(enemy, true);

        _currentTarget = enemy;
        _combatCoroutine = StartCoroutine(ChaseAndAttack(enemy));
    }

    private void MarkEnemyAsTarget(GameObject enemy, bool active)
    {
        if (active)
        {
            if (_activeTargetMarker != null)
                Destroy(_activeTargetMarker);
            
            _activeTargetMarker = Instantiate(_targetMarkerPrefab);
            _activeTargetMarker.GetComponent<TargetMarker>().AttachTo(enemy.transform);
        }
        else
        {
            if (_activeTargetMarker != null)
            {
                Destroy(_activeTargetMarker);
                _activeTargetMarker = null;
            }
        }
    }

    private IEnumerator ChaseAndAttack(GameObject target)
    {
        _isChasing = true;

        while (target != null && _isChasing)
        {
            Vector3Int playerPos = NodeManager.Instance.WorldToCell(transform.position);
            Vector3Int enemyPos = NodeManager.Instance.WorldToCell(target.transform.position);

            bool isAdjacent = Vector3Int.Distance(playerPos, enemyPos) <= 1.1f;

            if (!isAdjacent)
            {
                Vector3Int tile = FindAdjacentWalkableTile(playerPos, enemyPos);
                if (tile == Vector3Int.zero) break;

                List<Node> path = _nodeManager.FindPath(playerPos, tile);
                if (path != null && path.Count > 0)
                    yield return StartCoroutine(FollowPath(path, _moveSpeed));

                yield return null;
                continue;
            }

            var health = target.GetComponent<EnemyHealth>();
            if (health != null)
            {
                Debug.Log("Atacando...");
                health.TakeDamage(_damage);
            }

            yield return new WaitForSeconds(_attackInterval);
        }

        MarkEnemyAsTarget(null, false);
        _combatCoroutine = null;
    }

    // public void OnSpeedBoost(InputAction.CallbackContext context)
    // {
    //     if (!context.performed) return;
    //     Debug.Log("Aumentar AGILIDADE!");
    //     StartCoroutine(SpeedBoost());
    // }

    // private IEnumerator SpeedBoost()
    // {
    //     float originalSpeed = _moveSpeed;
    //     _moveSpeed = 6f;

    //     yield return new WaitForSeconds(5f);

    //     _moveSpeed = originalSpeed;
    // }
}
