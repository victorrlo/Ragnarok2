using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : GridMovement
{   
    private PlayerContext _playerContext;
    private Coroutine _movementCoroutine;
    private Coroutine _chaseCoroutine;
    public event Action<Vector3Int> OnPlayerMoved;
    private List<Node> _currentPath;
    private GameObject _enemy;

    protected override void Awake()
    {
        if (_playerContext == null)
            TryGetComponent<PlayerContext>(out _playerContext);
    }

    protected override void Start()
    {
        base.Start();
    }


    public void WalkToEmptyTile(Vector3Int targetPosition)
    {
        if (_enemy != null)
            _enemy.GetComponent<EnemyMovement>().OnEnemyMoved -= UpdateTargetPosition;

        Vector3Int playerPosition = GridManager.Instance.WorldToCell(transform.position);
        List<Node> path = NodeManager.Instance.FindPath(playerPosition, targetPosition);
        _currentPath = path;

        if(_movementCoroutine != null)
            StopCoroutine(_movementCoroutine);

        _movementCoroutine = StartCoroutine(FollowPath(_currentPath, _playerContext.Stats.MoveSpeed));
    }

    public void StartChasingEnemy(GameObject target)
    {
        if (_chaseCoroutine != null)
            StopCoroutine(_chaseCoroutine);

        _enemy = target;
        _enemy.GetComponent<EnemyMovement>().OnEnemyMoved += UpdateTargetPosition;

        _chaseCoroutine = StartCoroutine(ChaseEnemy(target));
    }

    private void UpdateTargetPosition(Vector3Int newPos)
    {
        if (this == null) return;

        Vector3Int startPos = GridManager.Instance.WorldToCell(transform.position);
        UpdatePath(startPos, newPos);
    }

    private List<Node> UpdatePath(Vector3Int from, Vector3Int to)
    {
        _currentPath = NodeManager.Instance.FindPath(from, to);

        if (_currentPath == null || _currentPath.Count == 0)
            return null;

        return _currentPath;
    }

    private IEnumerator ChaseEnemy(GameObject target)
    {
        WaitForSeconds delay = new WaitForSeconds(0.2f);
        yield return delay;

        while (true)
        {
            Vector3Int playerPosition = GridManager.Instance.WorldToCell(transform.position);
            Vector3Int enemyPosition = GridManager.Instance.WorldToCell(target.transform.position);

            if (DistanceHelper.IsInAttackRange(playerPosition, enemyPosition, _playerContext.Stats.AttackRange))
            {
                Debug.Log("Stopped movement");
                StopAllMovementCoroutines();
                _enemy.GetComponent<EnemyMovement>().OnEnemyMoved -= UpdateTargetPosition;
                yield break;
            }

            List<Node> path = NodeManager.Instance.FindPath(playerPosition, enemyPosition);

            if (path != null && path.Count >= 2)
            {
                _currentPath = path;

                _currentPath.RemoveAt(_currentPath.Count -1);

                if (_movementCoroutine != null)
                    StopCoroutine(_movementCoroutine);

                _movementCoroutine = StartCoroutine(FollowPath(_currentPath, _playerContext.Stats.MoveSpeed));
            }

            yield return null;
        }
    }

    public void StopAllMovementCoroutines()
    {
        StopCoroutine(_chaseCoroutine);
        _chaseCoroutine = null;
        StopCoroutine(_movementCoroutine);
        _movementCoroutine = null;
    }

    protected override void OnStep(Vector3Int newPos) 
    {
        OnPlayerMoved?.Invoke(newPos);
    }
}
