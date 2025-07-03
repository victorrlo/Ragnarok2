using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class EnemyMovementOLD : GridMovement
{
    private Transform _player;
    private Coroutine _movementCoroutine;
    private Vector3Int _lastKnownPlayerPosition;
    private Vector3Int _currentGridPos;
    public Vector3Int CurrentGridPos => _currentGridPos;

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
    [SerializeField] private int _tiringRange = 12; // it means if the player is far than this area, the enemy will give up attacking
    [SerializeField] MonsterStatsData _stats;
    private EnemyCombat _enemyCombat;
    private bool _isAttacking = false;

    protected override void Awake()
    {
        base.Awake();
        _enemyCombat = GetComponent<EnemyCombat>();
    }

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        InvokeRepeating(nameof(UpdatePath), 0f, 2f);

        _currentGridPos = GridManager.Instance.WorldToCell(transform.position);
    }

    private void Update()
    {
        if (_player == null || GridManager.Instance == null) return;

        Vector3Int playerGridPos = GridManager.Instance.WorldToCell(_player.position);
        if (playerGridPos != _lastKnownPlayerPosition && (_currentState == EnemyState.Attacking 
                                                        || _currentState == EnemyState.Active 
                                                        || _currentState == EnemyState.RangedAttacking 
                                                        || _currentState == EnemyState.SpecialAttacking))
        {
            _lastKnownPlayerPosition = playerGridPos;
            UpdatePath();
        }
    }

    private void UpdatePath()
    {
        if (_player == null || GridManager.Instance == null) return;

        Vector3Int enemyGridPos = GridManager.Instance.WorldToCell(transform.position);
        Vector3Int playerGridPos = GridManager.Instance.WorldToCell(_player.position);

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
                }
                else
                {
                    WanderRandomly();
                }
                break;

            case EnemyState.Attacking:
            {
                bool playerInRange = (distanceX <= _tiringRange && distanceY <= _tiringRange);
                if (!playerInRange)
                {
                    _isAttacking = false;
                    _currentState = EnemyState.Active;
                    Debug.Log("Monstro cansou de correr atrás do jogador!");
                }
                else
                {
                    if (!_isAttacking)
                    {
                        _isAttacking = true;
                        ChasePlayer(enemyGridPos, playerGridPos);
                        StartCoroutine(Attacking());
                    }
                } 
                break;
            }
        }
    }

    private IEnumerator Attacking()
    {
        while (_currentState == EnemyState.Attacking)
        {
            yield return _enemyCombat.Attack(_player.gameObject);
            // yield return new WaitForSeconds(_stats._attackSpeed);
        }
    }

    private void WanderRandomly()
    {
        Vector3Int startPos = GridManager.Instance.WorldToCell(transform.position);
        Vector3Int randomOffset = Vector3Int.zero;
        int rand = UnityEngine.Random.Range(0,4);

        if (rand == 0) randomOffset = Vector3Int.up;
        else if (rand==1) randomOffset = Vector3Int.down;
        else if (rand==2) randomOffset = Vector3Int.left;
        else if (rand==3) randomOffset = Vector3Int.right;

        Vector3Int targetPos = startPos + randomOffset;

        if (!NodeManager.Instance.IsWalkable(targetPos)) return;

        List<Node> path = NodeManager.Instance.FindPath(startPos, targetPos);

        if (path == null || path.Count == 0) return;

        if (_movementCoroutine != null)
            StopCoroutine(_movementCoroutine);

        _movementCoroutine = StartCoroutine(FollowPath(path, _stats._moveSpeed));
    }

    public void OnDamagedByPlayer()
    {
        _currentState = EnemyState.Attacking;
    }

    private void ChasePlayer(Vector3Int enemyPos, Vector3Int playerPos)
    {
        Vector3Int targetPos = FindAdjacentTile(enemyPos, playerPos);

        if (targetPos == Vector3Int.zero)
        {
            return;
        }

        List<Node> path = NodeManager.Instance.FindPath(enemyPos, targetPos);

        if (path == null || path.Count == 0) return;

        if (_movementCoroutine != null)
            StopCoroutine(_movementCoroutine);
        
        _movementCoroutine = StartCoroutine(FollowPath(path, _stats._moveSpeed));
    }

    protected override void OnStep(Vector3Int from, Vector3Int to)
    {
        _currentGridPos = to;
    }

    protected override void OnPathComplete(Vector3Int finalCell)
    {
        _currentGridPos = finalCell;
        StopMovement();
    }

    private Vector3Int FindAdjacentTile(Vector3Int enemyPos, Vector3Int playerPos)
    {
        Vector3Int bestTile = Vector3Int.zero;
        float bestDistance = float.MaxValue;

        foreach (var dir in DirectionHelper._directions)
        {
            Vector3Int adjacent = playerPos + dir;
            
            if (NodeManager.Instance.IsWalkable(adjacent)) // only walkable tiles that are different from player
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

        if (bestTile == Vector3Int.zero) Debug.LogError("nenhum tile adjacente válido foi encontrado!");

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

    public void SetCurrentGridPosition(Vector3Int validatedPos)
    {
        _currentGridPos = validatedPos;
    }
}
