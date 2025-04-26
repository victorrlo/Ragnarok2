using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : GridMovement
{
    private Transform _player;
    private NodeManager _nodeManager;
    private Coroutine _movementCoroutine;

    private enum EnemyState
    {
        Passive,
        Attacking,
        RangedAttacking,
        SpecialAttacking
    }
    private EnemyState _currentState = EnemyState.Passive;
    [SerializeField] private int _detectionRange = 3; // if 3 it means, for example an area of 7x7 around the enemy
    [SerializeField] private int _tiringRange = 6; // it means if the player is far than this area, the enemy will give up attacking
    protected override void Awake()
    {
        base.Awake();
        _nodeManager = FindFirstObjectByType<NodeManager>();
    }

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        // moves to player each 2 seconds
        InvokeRepeating(nameof(UpdatePath), 0f, 2f);
    }

    private void UpdatePath()
    {
        if (_player == null || _nodeManager == null) return;

        Vector3Int enemyGridPos = _grid.WorldToCell(transform.position);
        Vector3Int playerGridPos = _grid.WorldToCell(_player.position);

        int distanceX = Mathf.Abs(enemyGridPos.x - playerGridPos.x);
        int distanceY = Mathf.Abs(enemyGridPos.y - playerGridPos.y);

        bool playerInDetectionRange = (distanceX <= _detectionRange && distanceY <= _detectionRange);

        switch(_currentState)
        {
            case EnemyState.Passive:
                if (playerInDetectionRange)
                {
                    _currentState = EnemyState.Attacking;
                    Debug.Log("Jogador detectado!");
                }
                else
                {
                    WanderRandomly();
                }
                break;

            case EnemyState.Attacking:
            {
                bool playerInTiringRange = (distanceX <= _tiringRange && distanceY <= _tiringRange);
                if (!playerInTiringRange)
                {
                    _currentState = EnemyState.Passive;
                    Debug.Log("Monstro cansou de correr atrás do jogador!");
                }
                else
                {
                    ChasePlayer(enemyGridPos, playerGridPos);
                }
                break;
            }
        }

        // List<Node> path = _nodeManager.FindPath(enemyGridPos, playerGridPos);

        // if (path == null || path.Count == 0) return;

        // if (_movementCoroutine != null)
        //     StopCoroutine(_movementCoroutine);

        // _movementCoroutine = StartCoroutine(FollowPath(path));
    }

    private void WanderRandomly()
    {
        Vector3Int startPos = _grid.WorldToCell(transform.position);
        Vector3Int randomOffset = Vector3Int.zero;
        int rand = UnityEngine.Random.Range(0,4);

        if (rand == 0) randomOffset = Vector3Int.up;
        else if (rand==1) randomOffset = Vector3Int.down;
        else if (rand==2) randomOffset = Vector3Int.left;
        else if (rand==3) randomOffset = Vector3Int.right;

        Vector3Int targetPos = startPos + randomOffset;

        if (!_nodeManager.IsWalkable(targetPos)) return;

        List<Node> path = _nodeManager.FindPath(startPos, targetPos);

        if (path == null || path.Count == 0) return;

        if (_movementCoroutine != null)
            StopCoroutine(_movementCoroutine);

        _movementCoroutine = StartCoroutine(FollowPath(path));
    }

    private void ChasePlayer(Vector3Int enemyPos, Vector3Int playerPos)
    {
        List<Node> path = _nodeManager.FindPath(enemyPos, playerPos);

        if (path == null || path.Count == 0) return;

        if (_movementCoroutine != null)
            StopCoroutine(_movementCoroutine);
        
        _movementCoroutine = StartCoroutine(FollowPath(path));
    }
}
