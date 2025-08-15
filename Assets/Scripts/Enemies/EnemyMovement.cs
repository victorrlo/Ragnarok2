using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyContext))]
public class EnemyMovement : GridMovement
{   
    private EnemyContext _enemyContext;
    private Coroutine _currentBehaviorCoroutine;
    private Vector3Int _currentPosition;
    private Transform _player;
    private bool _isAttacking;
    private Vector3Int _playerPosition;
    private List<Node> _currentPath;
    private float _chaseStartTime;
    public event Action<Vector3Int> OnEnemyMoved;

    protected override void Awake()
    {
        base.Awake();
        if (_enemyContext == null)
            TryGetComponent<EnemyContext>(out _enemyContext);

        _player = GameObject.FindWithTag("Player")?.transform;
    }

    protected override void Start()
    {
        base.Start();
        _currentPosition = GridManager.Instance.WorldToCell(transform.position);
    }

    private void OnEnable()
    {
        _enemyContext.EventBus.OnTargetMovedAway += StartChasing;
    }

    private void Update()
    {
        if (_player == null) return;
        
        _playerPosition = GridManager.Instance.WorldToCell(_player.position);
    }

    private bool EnemyHasPriorityOver(PlayerMovement player)
    {
        return GetInstanceID() > player.GetInstanceID();
    }

    private bool CanEnemyStepInto(Vector3Int nextCell, PlayerMovement player)
    {
        if (player == null) return true;

        var playerCurrentCell = player.CurrentCell;
        var playerIntent = player.IntendedNextCell;
        var currentEnemyCell = _currentPosition;

        if (nextCell == playerCurrentCell) return false;

        if (playerIntent.HasValue && playerIntent.Value == currentEnemyCell && nextCell == playerCurrentCell)
            return EnemyHasPriorityOver(player);

        if (playerIntent.HasValue && playerIntent.Value == nextCell)
            return EnemyHasPriorityOver(player);

        return true;
    }

    private void OnDisable()
    {
        _enemyContext.EventBus.OnTargetMovedAway -= StartChasing;
    }

    private void OnDestroy()
    {
        if (_player == null) return; 

        _player.GetComponent<PlayerMovement>().OnPlayerMoved -= UpdateTargetPosition;
    }

    public void StartWandering()
    {
        SwitchToBehavior(WanderRandomly());
    }

    public void StartChasing(StartAttackData data)
    {
        var target = data.target;
        SwitchToBehavior(IsChasing(target));
    }

    private IEnumerator WanderRandomly()
    {   
        yield return new WaitForSeconds(1f);
        
        Vector3Int startPos = GridManager.Instance.WorldToCell(transform.position);
        
        Vector3Int randomDirection = GetRandomDirection();
        int randomDistance = GetRandomDistance();
        
        Vector3Int targetPos = startPos + randomDirection * randomDistance;

        if (!NodeManager.Instance.IsWalkable(targetPos))
            yield return null;

        UpdatePath(startPos, targetPos);
            
        yield return FollowPath(_currentPath, _enemyContext.Stats.MoveSpeed);
    }

    private Vector3Int GetRandomDirection()
    {
        // should change it to be omnidirectional, 
        // more akin to a player clicking in a random grid

        Vector3Int randomOffset = Vector3Int.zero;
        int rand = UnityEngine.Random.Range(0,4);

        if (rand == 0) randomOffset = Vector3Int.up;
        else if (rand==1) randomOffset = Vector3Int.down;
        else if (rand==2) randomOffset = Vector3Int.left;
        else if (rand==3) randomOffset = Vector3Int.right;

        return randomOffset;
    }

    private int GetRandomDistance()
    {
        int rand = UnityEngine.Random.Range(1,10);
        return rand;
    }

    protected override void OnPathComplete(Vector3Int finalCell)
    {
        _currentPosition = finalCell;
    }

    private IEnumerator IsChasing(GameObject target)
    {
        _isAttacking = false;
        int attackRange = _enemyContext.Stats.AttackRange;

        WaitForSeconds delay = new WaitForSeconds(0.2f);
        yield return delay;
        
        _chaseStartTime = Time.time;

        if (target == null) yield break;
        target.GetComponent<PlayerMovement>().OnPlayerMoved += UpdateTargetPosition;

        while(true)
        {
            _currentPosition = GridManager.Instance.WorldToCell(transform.position);
            var playerPosition = GridManager.Instance.WorldToCell(_player.position);

            if (DistanceHelper.IsInAttackRange(_currentPosition, playerPosition, attackRange))
            { 
                StartAttack(target);
                // target.GetComponent<PlayerMovement>().OnPlayerMoved -= UpdateTargetPosition;
                StartCoroutine(GridHelper.SnapToNearestCellCenter(this.gameObject, 0.15f));
                yield break;
            }
            else
            {
                _isAttacking = false;
            }

            _currentPath = NodeManager.Instance.FindPath(_currentPosition, playerPosition);

            if (_currentPath == null || _currentPath.Count < 2)
            {
                yield return null;
                continue;
            }

            int stopIndex = Mathf.Max(1, _currentPath.Count - attackRange);

            if (_currentPath.Count > stopIndex)
                _currentPath.RemoveRange(stopIndex, _currentPath.Count - stopIndex);

            while (_currentPath.Count > 1)
            {
                var endCell = _currentPath[_currentPath.Count - 1]._gridPosition;
                int dx = Mathf.Abs(endCell.x - playerPosition.x);
                int dy = Mathf.Abs(endCell.y - playerPosition.y);

                // stop BEFORE being inside range OR aligned
                bool insideRange = (dx < attackRange && dy < attackRange);
                bool aligned     = (dx == 0 || dy == 0);

                if (!insideRange && !aligned) break;  // good end cell
                _currentPath.RemoveAt(_currentPath.Count - 1); // pop one more step
            }

            var player = _player.GetComponent<PlayerMovement>();

            bool shouldCancelEnemy() =>
                DistanceHelper.IsInAttackRange(
                    GridManager.Instance.WorldToCell(transform.position),
                    GridManager.Instance.WorldToCell(player.transform.position),
                    _enemyContext.Stats.AttackRange
                );

            yield return FollowPath(
                _currentPath, 
                _enemyContext.Stats.MoveSpeed,
                nextCell => CanEnemyStepInto(nextCell, player),
                shouldCancelEnemy);
        }
    }

    private void StartAttack(GameObject target)
    {
        if (_isAttacking) return;

        _isAttacking = true;
        var data = new StartAttackData(this.gameObject, _player.gameObject);
        _enemyContext.EventBus.RaiseStartAttack(data);
    }

    private IEnumerator RestAndReturnToPassive()
    {
        _player.GetComponent<PlayerMovement>().OnPlayerMoved -= UpdateTargetPosition;

        // animation for tired emote to show enemy is tired of chasing the player and gave up
        Debug.Log("Show emote for tired"); // should probably have a different script to handle emotes and only fire an event here

        yield return new WaitForSeconds(_enemyContext.Stats.MaximumRestTime);

        _enemyContext.AI.ChangeState(new PassiveState());
    }

    private void SwitchToBehavior(IEnumerator newBehavior)
    {
        if (_currentBehaviorCoroutine != null)
        {
            StopCoroutine(_currentBehaviorCoroutine);
        }

        _currentBehaviorCoroutine = StartCoroutine(newBehavior);
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

    public void StopBehavior()
    {
        if (_currentBehaviorCoroutine != null)
            StopCoroutine(_currentBehaviorCoroutine);
        _currentBehaviorCoroutine = null;
    }

    protected override void OnStep(Vector3Int newPos) 
    {
        _currentPosition = newPos;
        OnEnemyMoved?.Invoke(newPos);
    }
}
