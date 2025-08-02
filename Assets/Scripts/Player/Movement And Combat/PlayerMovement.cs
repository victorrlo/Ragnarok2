using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerMovement : GridMovement
{   
    private PlayerContext _playerContext;
    private Coroutine _movementCoroutine;
    public event Action<Vector3Int> OnPlayerMoved;
    private List<Node> _currentPath;

    protected override void Awake()
    {
        if (_playerContext == null)
            TryGetComponent<PlayerContext>(out _playerContext);
    }


    public void WalkToEmptyTile(Vector3Int targetPosition)
    {
        Vector3Int playerPosition = GridManager.Instance.WorldToCell(transform.position);
        List<Node> path = NodeManager.Instance.FindPath(playerPosition, targetPosition);
        _currentPath = path;

        if(_movementCoroutine != null)
            StopCoroutine(_movementCoroutine);

        _movementCoroutine = StartCoroutine(FollowPath(GetPath, _playerContext.Stats.MoveSpeed));
    }

    public IEnumerator ChaseEnemy(GameObject target)
    {

        Vector3Int playerPosition = GridManager.Instance.WorldToCell(transform.position);
        Vector3Int enemyPosition = GridManager.Instance.WorldToCell(target.transform.position);

        List<Node> path = NodeManager.Instance.FindPath(playerPosition, enemyPosition);
        _currentPath = path;

        if(_movementCoroutine != null)
            StopCoroutine(_movementCoroutine);

        _movementCoroutine = StartCoroutine(FollowPath(GetPath, _playerContext.Stats.MoveSpeed));

        while (true)
        {
            playerPosition = GridManager.Instance.WorldToCell(transform.position);
            enemyPosition = GridManager.Instance.WorldToCell(target.transform.position);

            if (DistanceHelper.IsAdjacent(playerPosition, enemyPosition, _playerContext.Stats.AttackRange))
            {
                if(_movementCoroutine != null)
                {
                    StopCoroutine(_movementCoroutine);
                    _movementCoroutine = null;
                }

                yield break;
            }   
            yield return null;
        }
    }

    protected override void OnStep(Vector3Int newPos) 
    {
        OnPlayerMoved?.Invoke(newPos);
    }

    private List<Node> GetPath()
    {
        return _currentPath;
    }
}
