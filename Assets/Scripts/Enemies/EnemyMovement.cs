using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : GridMovement
{
    [SerializeField] private EnemyStats _enemyStats;
    private Coroutine _movementCoroutine;
    private Vector3Int _currentGridPos;
    private bool _isMoving = false;
    public bool IsMoving => _isMoving;


    private void Start()
    {
        _currentGridPos = GridManager.Instance.WorldToCell(transform.position);
    }
    public void WanderRandomly()
    {
        if (_isMoving) return;
        
        Vector3Int startPos = GridManager.Instance.WorldToCell(transform.position);
        Vector3Int randomOffset = GetRandomDirection();
        Vector3Int targetPos = startPos + randomOffset;

        if (!NodeManager.Instance.IsWalkable(targetPos)) return;

        List<Node> path = NodeManager.Instance.FindPath(startPos, targetPos);

        if (path == null || path.Count == 0) return;

        if(_movementCoroutine != null)
            StopCoroutine(_movementCoroutine);

        _movementCoroutine = StartCoroutine(FollowPath(path, _enemyStats.StatsData._moveSpeed));
    }

    private Vector3Int GetRandomDirection()
    {
        Vector3Int randomOffset = Vector3Int.zero;
        int rand = UnityEngine.Random.Range(0,4);

        if (rand == 0) randomOffset = Vector3Int.up;
        else if (rand==1) randomOffset = Vector3Int.down;
        else if (rand==2) randomOffset = Vector3Int.left;
        else if (rand==3) randomOffset = Vector3Int.right;

        return randomOffset;
    }

    protected override void OnStep(Vector3Int from, Vector3Int to)
    {
        _isMoving = true;
        _currentGridPos = to;
    }

    protected override void OnPathComplete(Vector3Int finalCell)
    {
        _currentGridPos = finalCell;
        StopMovement();
    }

    private void StopMovement()
    {
        if (_movementCoroutine != null)
        {
            StopCoroutine(_movementCoroutine);
            _movementCoroutine = null;
        }
        _isMoving = false;
    }
}
