using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyMovement : GridMovement
{
    [SerializeField] private EnemyStats _enemyStats;
    [SerializeField] private EnemyAI _enemyAI;
    private Coroutine _currentBehaviorCoroutine;
    private Vector3Int _currentGridPos;
    private Transform _player;

    private List<Node> _currentPath;

    protected override void Awake()
    {
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
        Vector3Int startPos = GridManager.Instance.WorldToCell(transform.position);
        
        Vector3Int randomOffset = GetRandomDirection();
        int randomDistance = GetRandomDistance();
        
        Vector3Int targetPos = startPos + randomOffset * randomDistance;
 
        if (!NodeManager.Instance.IsWalkable(targetPos))
            yield return null;

        UpdatePath(startPos, targetPos);
            
        yield return FollowPath(GetPath, _enemyStats.StatsData.MoveSpeed);
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
        bool isChasing = true;

        WaitForSeconds delay = new WaitForSeconds(0.2f);
        yield return delay;
        
        while(isChasing)
        {
            _currentGridPos = GridManager.Instance.WorldToCell(transform.position);
            var playerTargetPos = GridManager.Instance.WorldToCell(_player.position);

            if (DistanceHelper.IsAdjacent(_currentGridPos, playerTargetPos, _enemyStats.StatsData.AttackRange))
            {
                isChasing = false;
                yield break;
            } 

            _currentPath = NodeManager.Instance.FindPath(_currentGridPos, playerTargetPos);

            if (_currentPath == null || _currentPath.Count < 2)
            {
                yield return delay;
                continue;
            }

            _currentPath.RemoveAt(_currentPath.Count-1);
            isChasing = false;
            yield return FollowPath(GetPath, _enemyStats.StatsData.MoveSpeed);

            yield return delay;
        }
    }

    private IEnumerator IsAdjacentToPlayer()
    {
        bool isAdjacent = true;

        WaitForSeconds delay = new WaitForSeconds(0.2f);
        yield return delay;

        while(isAdjacent)
        {
            _currentGridPos = GridManager.Instance.WorldToCell(transform.position);
            var playerTargetPos = GridManager.Instance.WorldToCell(_player.position);

            //checks if player has moved away from the enemy
            if (!DistanceHelper.IsAdjacent(_currentGridPos, playerTargetPos, _enemyStats.StatsData.AttackRange))
            {
                isAdjacent = false;
                yield break;
            } 
        }
    }

    public void SwitchToBehavior(IEnumerator newBehavior)
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
