using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class EnemyMovement : GridMovement
{
    private Transform _player;
    private NodeManager _nodeManager;
    private Coroutine _movementCoroutine;
    private Vector3Int _lastKnownPlayerPosition;

    private enum EnemyState
    {
        Passive,
        Active,
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

    private void Update()
    {
        if (_player == null || _nodeManager == null) return;

        Vector3Int playerGridPos = _grid.WorldToCell(_player.position);
        if (playerGridPos != _lastKnownPlayerPosition && _currentState == EnemyState.Attacking)
        {
            _lastKnownPlayerPosition = playerGridPos;
            UpdatePath();
        }
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
                WanderRandomly();
                break;
            case EnemyState.Active:
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
                    _currentState = EnemyState.Active;
                    Debug.Log("Monstro cansou de correr atrás do jogador!");
                }
                else
                {
                    ChasePlayer(enemyGridPos, playerGridPos);
                } 
                break;
            }
        }
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
        Vector3Int targetPos = FindAdjacentTile(enemyPos, playerPos);

        if (targetPos == Vector3Int.zero)
        {
            return;
        }

        List<Node> path = _nodeManager.FindPath(enemyPos, targetPos);

        if (path == null || path.Count == 0) return;

        if (_movementCoroutine != null)
            StopCoroutine(_movementCoroutine);
        
        _movementCoroutine = StartCoroutine(FollowPath(path));
    }

    private Vector3Int FindAdjacentTile(Vector3Int enemyPos, Vector3Int playerPos)
    {
        Vector3Int bestTile = Vector3Int.zero;
        float bestDistance = float.MaxValue;

        foreach (var dir in DirectionHelper._directions)
        {
            Vector3Int adjacent = playerPos + dir;
            
            if (_nodeManager.IsWalkable(adjacent)) // only walkable tiles that are different from player
            {
                if (adjacent == enemyPos) continue; // to not go back to the same tile

                float distance = Vector3Int.Distance(enemyPos, adjacent);

                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestTile = adjacent;
                }
                    
            }
        }
        // if (bestTile == Vector3Int.zero) Debug.LogError("nenhum tile adjacente válido foi encontrado!");

        return bestTile;
    }

    private void StopMovement()
    {
        if (_movementCoroutine != null)
        {
            StopCoroutine(_movementCoroutine);
            _movementCoroutine = null;
        }
    }
}
