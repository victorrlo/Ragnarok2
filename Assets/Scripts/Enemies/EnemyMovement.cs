using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(EnemyContext))]
public class EnemyMovement : GridMovement
{   
    private EnemyContext _enemyContext;
    private Coroutine _currentBehaviorCoroutine;
    private Vector3Int _currentGridPos;
    private Transform _player;
    private List<Node> _currentPath;
    private float _chaseStartTime;
    private float _lastAttackSignalTime;

    protected override void Awake()
    {
        if (_enemyContext == null)
            TryGetComponent<EnemyContext>(out _enemyContext);

        _player = GameObject.FindWithTag("Player")?.transform;
    }

    private void Start()
    {
        _currentGridPos = GridManager.Instance.WorldToCell(transform.position);
    }

    public void StartWandering()
    {
        SwitchToBehavior(WanderRandomly());
    }

    public void StartChasing()
    {
        SwitchToBehavior(ChasePlayer());
    }

    private IEnumerator WanderRandomly()
    {   
        yield return new WaitForSeconds(1f);
        
        Vector3Int startPos = GridManager.Instance.WorldToCell(transform.position);
        
        Vector3Int randomOffset = GetRandomDirection();
        int randomDistance = GetRandomDistance();
        
        Vector3Int targetPos = startPos + randomOffset * randomDistance;

        if (!NodeManager.Instance.IsWalkable(targetPos))
            yield return null;

        UpdatePath(startPos, targetPos);
            
        yield return FollowPath(GetPath, _enemyContext.Stats.MoveSpeed);
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

    private int GetRandomDistance()
    {
        int rand = UnityEngine.Random.Range(1,4);
        return rand;
    }

    protected override void OnStep(Vector3Int newPos)
    {
        _currentGridPos = newPos;
    }

    protected override void OnPathComplete(Vector3Int finalCell)
    {
        _currentGridPos = finalCell;
    }

    private IEnumerator ChasePlayer()
    {
        WaitForSeconds delay = new WaitForSeconds(0.2f);
        yield return delay;
        
        _chaseStartTime = Time.time;

        while(true)
        {
            _currentGridPos = GridManager.Instance.WorldToCell(transform.position);
            var playerTargetPos = GridManager.Instance.WorldToCell(_player.position);

            if (DistanceHelper.IsAdjacent(_currentGridPos, playerTargetPos, _enemyContext.Stats.AttackRange))
            {
                _chaseStartTime = Time.time;
                SwitchToBehavior(IsNearPlayer());
                yield break;
            } 

            if (Time.time - _chaseStartTime > _enemyContext.Stats.StaminaToChaseInSeconds)
            {
                SwitchToBehavior(RestAndReturnToPassive());
                yield break;
            }

            _currentPath = NodeManager.Instance.FindPath(_currentGridPos, playerTargetPos);

            if (_currentPath == null || _currentPath.Count < 2)
            {
                yield return delay;
                continue;
            }

            _currentPath.RemoveAt(_currentPath.Count-1);
            yield return FollowPath(GetPath, _enemyContext.Stats.MoveSpeed);
        }
    }

    private IEnumerator IsNearPlayer()
    {
        WaitForSeconds delay = new WaitForSeconds(0.2f);
        yield return delay;

        while(true)
        {
            _currentGridPos = GridManager.Instance.WorldToCell(transform.position);
            var playerTargetPos = GridManager.Instance.WorldToCell(_player.position);

            if (!DistanceHelper.IsAdjacent(_currentGridPos, playerTargetPos, _enemyContext.Stats.AttackRange))
            {
                SwitchToBehavior(ChasePlayer());
                yield break;
            } 

            SwitchToBehavior(IsAttacking());
            
            yield return null;
        }
    }

    private IEnumerator IsAttacking()
    {
        while (true)
        {
            if (_player == null) 
                yield break;
                
            var playerTargetPos = GridManager.Instance.WorldToCell(_player.position);
            _currentGridPos = GridManager.Instance.WorldToCell(transform.position);
            
            if (!DistanceHelper.IsAdjacent(_currentGridPos, playerTargetPos, _enemyContext.Stats.AttackRange))
            {
                SwitchToBehavior(ChasePlayer());
                yield break;
            }

            Attack();
            yield return new WaitForSeconds(_enemyContext.Stats.AttackSpeed);
        }
    }

    private void Attack()
    {
        var data = new EnemyStartAttackData(_player.gameObject);
        _enemyContext.EventBus.OnStartAttack?.Raise(data);
    }

    private IEnumerator RestAndReturnToPassive()
    {
        // animation for tired emote to show enemy is tired of chasing the player and gave up
        Debug.Log("Show emote for tired");

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

    public List<Node> UpdatePath(Vector3Int from, Vector3Int to)
    {
        _currentPath = NodeManager.Instance.FindPath(from, to);
        
        if (_currentPath == null || _currentPath.Count == 0) 
            return null;

        return _currentPath; 
    }

    private List<Node> GetPath()
    {
        return _currentPath;
    }
}
