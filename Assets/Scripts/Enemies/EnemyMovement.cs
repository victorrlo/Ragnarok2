using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyMovement : GridMovement
{
    [SerializeField] private EnemyStats _enemyStats;
    [SerializeField] private EnemyAI _enemyAI;
    private Coroutine _movementCoroutine;
    private Vector3Int _currentGridPos;
    private Transform _player;

    protected override void Awake()
    {
        _player = GameObject.FindWithTag("Player")?.transform;
    }

    private void Start()
    {
        _currentGridPos = GridManager.Instance.WorldToCell(transform.position);
    }

    public void MoveToward(Vector3 targetPosition)
    {

    }

    public IEnumerator WanderRandomly()
    {   
        Vector3Int startPos = GridManager.Instance.WorldToCell(transform.position);
        
        Vector3Int randomOffset = GetRandomDirection();
        int randomDistance = GetRandomDistance();
        
        Vector3Int targetPos = startPos + randomOffset * randomDistance;
 
        if (!NodeManager.Instance.IsWalkable(targetPos))
            yield return null;

        List<Node> path = NodeManager.Instance.FindPath(startPos, targetPos);

        if (path == null || path.Count == 0) 
            yield return null;

        if(_movementCoroutine != null)
            StopCoroutine(_movementCoroutine);
            
        yield return FollowPath(path, _enemyStats.StatsData._moveSpeed);
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

    protected override void OnStep(Vector3Int from, Vector3Int to)
    {
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
    }

    public IEnumerator ChasePlayer(System.Action onChaseComplete = null)
    {
        
        while(true)
        {
            _currentGridPos = GridManager.Instance.WorldToCell(transform.position);
            var playerTargetPos = GridManager.Instance.WorldToCell(_player.position);

            if (_currentGridPos == playerTargetPos)
                break;

            Debug.LogWarning("ENEMY WANTS TO GET YOU!!!!");
            // MoveTowards(_player);
            yield return new WaitForSeconds(0.1f);
        }
        onChaseComplete?.Invoke();
        yield return null;
    }
}
