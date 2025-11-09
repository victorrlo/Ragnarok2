using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class PlayerMovement : GridMovement
{   
    private PlayerContext _playerContext;
    private Coroutine _movementCoroutine;
    private Coroutine _decisionCoroutine;
    private bool _isWaitingDecision;
    private bool _isSnapping;
    private Vector3Int _currentPosition;
    private Vector3Int _targetPosition;
    public event Action<Vector3Int> OnPlayerMoved;
    private List<Node> _currentPath;
    private GameObject _enemy;

    protected override void Awake()
    {
        if (_playerContext == null)
            TryGetComponent<PlayerContext>(out _playerContext);
    }

    private void OnEnable()
    {
        _playerContext.EventBus.OnWalk += WalkToEmptyTile;
        
        _playerContext.EventBus.OnStartAttack += StartChasing;
    }

    protected override void Start()
    {
        base.Start();
        _currentPosition = GridManager.Instance.WorldToCell(transform.position);
    }

    private void Update()
    {
        if (_enemy == null) return;

        _currentPosition = GridManager.Instance.WorldToCell(transform.position);
        _targetPosition = GridManager.Instance.WorldToCell(_enemy.transform.position);

        if (DistanceHelper.IsInAttackRange(_currentPosition, _targetPosition, _playerContext.Stats.AttackRange))
        {
            EnterDecisionWindow();
            StopMovement();
            StartCoroutine(GridHelper.SnapToNearestCellCenter(this.gameObject, 0.15f));
            _movementCoroutine = StartCoroutine(WaitForEnemyMovement());
        }
    }

    private void OnDisable()
    {
        _playerContext.EventBus.OnWalk -= WalkToEmptyTile;
        _playerContext.EventBus.OnStartAttack -= StartChasing;
    }

    private void EnterDecisionWindow()
    {
        if (_isWaitingDecision) return;
        _isWaitingDecision = true;

        StopMovement();

        if(!_isSnapping)
            StartCoroutine(SmoothSnapOnce());

        if(_decisionCoroutine != null)
            StopCoroutine(_decisionCoroutine);
        _decisionCoroutine = StartCoroutine(DecisionWindow());
    }

    private IEnumerator SmoothSnapOnce()
    {
        _isSnapping = true;
        yield return GridHelper.SnapToNearestCellCenter(gameObject, 0.15f);
        _isSnapping = false;
    }

    private IEnumerator DecisionWindow()
    {
        float bias = (GetInstanceID() & 1) == 0 ? -0.05f : 0.05f;
        yield return new WaitForSeconds(1f + bias);

        _currentPosition = GridManager.Instance.WorldToCell(transform.position);
        _targetPosition = GridManager.Instance.WorldToCell(_enemy.transform.position);

        _isWaitingDecision = false;
        _decisionCoroutine = null;

        if (!DistanceHelper.IsInAttackRange(_currentPosition, _targetPosition, _playerContext.Stats.AttackRange))
        {
            StartChasing(new StartAttackData(gameObject, _enemy));
        }
    }

    private bool PlayerHasPriorityOver(EnemyMovement enemy)
    {
        if (enemy == null) return true;
        return GetInstanceID() > enemy.GetInstanceID();
    }

    private bool CanPlayerStepInto(Vector3Int nextCell, EnemyMovement enemy)
    {
        if (enemy == null) return true;

        var enemyCurrentCell = enemy.CurrentCell;
        var enemyIntent = enemy.IntendedNextCell;
        var playerCurrentCell = _currentPosition;

        if (nextCell == enemyCurrentCell) return false;

        if (enemyIntent.HasValue && enemyIntent.Value == playerCurrentCell && nextCell == enemyCurrentCell)
            return PlayerHasPriorityOver(enemy);

        return true;
    }

    private IEnumerator WaitForEnemyMovement()
    {
        yield return new WaitForSeconds(1f);
        
        StartChasing(new StartAttackData(this.gameObject, _enemy));
    }


    public void WalkToEmptyTile(Vector3Int targetPosition)
    {
        if (this.GetComponent<PlayerControl>().GetCurrentState() is CastingState) return;
        
        StopMovement();

        if (_enemy != null)
        {
            _enemy.GetComponent<EnemyMovement>().OnEnemyMoved -= UpdateTargetPosition;
            _enemy = null;
        }
            
        _currentPosition = GridManager.Instance.WorldToCell(transform.position);
        _targetPosition = targetPosition;

        List<Node> path = NodeManager.Instance.FindPath(_currentPosition, _targetPosition);
        
        _currentPath = path;

        if(_movementCoroutine != null)
            StopCoroutine(_movementCoroutine);

        _movementCoroutine = StartCoroutine(FollowPath(_currentPath, _playerContext.Stats.MoveSpeed));
    }

    public void StartChasing(StartAttackData data)
    {
        if (this.GetComponent<PlayerControl>().GetCurrentState() is CastingState) return;
        
        StopMovement();
        
        if (_enemy != null)
            _enemy.GetComponent<EnemyMovement>().OnEnemyMoved -= UpdateTargetPosition;

        _enemy = data.target;
        _enemy.GetComponent<EnemyMovement>().OnEnemyMoved += UpdateTargetPosition;

        _movementCoroutine = StartCoroutine(ChaseEnemy(_enemy));
    }

    private IEnumerator ChaseEnemy(GameObject target)
    {
        int attackRange = _playerContext.Stats.AttackRange;
        WaitForSeconds delay = new WaitForSeconds(0.2f);

        yield return delay;

        if (target == null) yield break;

        while(true)
        {
            _currentPosition = GridManager.Instance.WorldToCell(transform.position);
            _targetPosition = GridManager.Instance.WorldToCell(target.transform.position);

            if (DistanceHelper.IsInAttackRange(_currentPosition, _targetPosition, attackRange))
            { 
                EnterDecisionWindow();
                yield break;
            } 

            _currentPath = NodeManager.Instance.FindPath(_currentPosition, _targetPosition);
            if (_currentPath == null || _currentPath.Count < 2)
            {
                yield return delay;
                continue;
            }

            int stopIndex = Mathf.Max(1, _currentPath.Count - attackRange);

            if (_currentPath.Count > stopIndex)
                _currentPath.RemoveRange(stopIndex, _currentPath.Count - stopIndex);

            while (_currentPath.Count > 1)
            {
                var endCell = _currentPath[_currentPath.Count - 1]._gridPosition;
                int dx = Mathf.Abs(endCell.x - _targetPosition.x);
                int dy = Mathf.Abs(endCell.y - _targetPosition.y);

                // stop BEFORE being inside range OR aligned
                bool insideRange = (dx <= attackRange && dy <= attackRange);
                bool aligned     = (dx == 0 || dy == 0);

                if (!insideRange && !aligned) break;  // good end cell
                _currentPath.RemoveAt(_currentPath.Count - 1); // pop one more step
            }

            var enemy = _enemy.GetComponent<EnemyMovement>();

            bool shouldCancel() =>
                DistanceHelper.IsInAttackRange(
                    GridManager.Instance.WorldToCell(transform.position),
                    GridManager.Instance.WorldToCell(enemy.transform.position),
                    _playerContext.Stats.AttackRange);

            yield return FollowPath(
                _currentPath, 
                _playerContext.Stats.MoveSpeed, 
                nextCell => CanPlayerStepInto(nextCell, enemy), 
                shouldCancel);
        }
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

    public void StopMovement()
    {
        if (_movementCoroutine != null)
            StopCoroutine(_movementCoroutine);
        _movementCoroutine = null;
    }

    public void StartCasting()
    {
        var coroutine = StartCoroutine(SmoothSnapOnce());
        if (_movementCoroutine != null)
            StopCoroutine(_movementCoroutine);
        _movementCoroutine = null;
        
        coroutine = null;
    }

    protected override void OnStep(Vector3Int newPos) 
    {
        _currentPosition = newPos;
        UpdatePath(_currentPosition, _targetPosition);
        OnPlayerMoved?.Invoke(newPos);
    }

    protected override void OnPathComplete(Vector3Int finalCell)
    {
        _currentPosition = finalCell;
    }
}
