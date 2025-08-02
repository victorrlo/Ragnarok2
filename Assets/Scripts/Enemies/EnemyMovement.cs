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

    private void OnDestroy()
    {
        if (_player == null) return; 

        _player.GetComponent<PlayerMovement>().OnPlayerMoved -= UpdateTargetPosition;
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
        
        Vector3Int randomDirection = GetRandomDirection();
        int randomDistance = GetRandomDistance();
        
        Vector3Int targetPos = startPos + randomDirection * randomDistance;

        if (!NodeManager.Instance.IsWalkable(targetPos))
            yield return null;

        UpdatePath(startPos, targetPos);
            
        yield return FollowPath(GetPath, _enemyContext.Stats.MoveSpeed);
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

        if (_player == null) yield break;
        _player.GetComponent<PlayerMovement>().OnPlayerMoved += UpdateTargetPosition;

        while(true)
        {
            _currentGridPos = GridManager.Instance.WorldToCell(transform.position);
            var playerPosition = GridManager.Instance.WorldToCell(_player.position);

            if (DistanceHelper.IsAdjacent(_currentGridPos, playerPosition, _enemyContext.Stats.AttackRange))
            {
                _chaseStartTime = Time.time;
                SwitchToBehavior(IsNearPlayer());
                yield break;
            } 

            // this part has to be a different script that inherits this function
            // so that it is possible to customize enemy behavior
            // for now I'll keep it, but it needs to be for distance and random time
            if (Time.time - _chaseStartTime > _enemyContext.Stats.StaminaToChaseInSeconds)
            {
                SwitchToBehavior(RestAndReturnToPassive());
                yield break;
            }

            _currentPath = NodeManager.Instance.FindPath(_currentGridPos, playerPosition);

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
        if (_player == null)
        {
            _player.GetComponent<PlayerMovement>().OnPlayerMoved -= UpdateTargetPosition;
            return;
        }
        var data = new EnemyStartAttackData(_player.gameObject);
        _enemyContext.EventBus.RaiseStartAttack(data);
    }

    private IEnumerator RestAndReturnToPassive()
    {
        _player.GetComponent<PlayerMovement>().OnPlayerMoved -= UpdateTargetPosition;

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

    private void UpdateTargetPosition(Vector3Int newPos)
    {
        if (this == null) return;

        Vector3Int startPos = GridManager.Instance.WorldToCell(transform.position);
        _enemyContext.Movement.UpdatePath(startPos, newPos);
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
