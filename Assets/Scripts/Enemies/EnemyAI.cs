using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
[RequireComponent(typeof(EnemyContext))]
public class EnemyAI : MonoBehaviour
{
    private IEnemyState _currentState;
    public IEnemyState CurrentState => _currentState;
    private EnemyContext _enemyContext;
    private EnemyEventBus _enemyEventBus;
    public GameObject CurrentTarget { get; private set; }
    public Vector3Int? CurrentDestination { get; private set; }

    private bool _blockStateChange = false;

    private void Awake()
    {
        if (_enemyContext == null)
            TryGetComponent<EnemyContext>(out _enemyContext);

        if (_enemyEventBus == null)
            _enemyContext.TryGetComponent<EnemyEventBus>(out _enemyEventBus);

        _currentState = null;
    }

    private void OnEnable()
    {
        _enemyEventBus.OnDamaged += OnDamageTaken;
    }

    private void Start()
    {
        SnapToNearestGrid();
        ChangeState(new PassiveState());
    }

    private void Update()
    {
        _currentState?.Execute();
    }

    private void OnDisable()
    {
        _enemyEventBus.OnDamaged -= OnDamageTaken;
    }

    public void ChangeState(IEnemyState newState)
    {
        if (_blockStateChange) return;

        if (_currentState != null && _currentState.GetType() == newState.GetType()) 
            return;
        
        Debug.Log($"Changing state to {newState}");
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter(gameObject);
    }

    private void SnapToNearestGrid()
    {
        if (GridManager.Instance == null) return;

        Vector3Int currentCell = GridManager.Instance.WorldToCell(this.transform.position);
        Vector3 snappedPosition = GridManager.Instance.GetCellCenterWorld(currentCell);
        this.transform.position = snappedPosition;
    }

    private void OnDamageTaken(DamageEventData data)
    {
        if (data._target != gameObject) return;

        var player = GameObject.FindGameObjectWithTag("Player");

        SetTarget(player);
        ChangeState(new AggressiveState());
    }

    public void SetTarget(GameObject target) => CurrentTarget = target;
    public void ClearTarget() => CurrentTarget = null;
    public void SetDestination(Vector3Int position) => CurrentDestination = position;
    public void ClearDestination() => CurrentDestination = null;
    public void SetStateChangeBlock(bool block) => _blockStateChange = block;
}

public interface IEnemyState
{
    void Enter(GameObject enemy);
    void Execute();
    void Exit();
}

public class PassiveState : IEnemyState
{
    // the passive state is the same as the walking state for the player
    // the enemy wanders randomly and if it's aggressive, it changes to aggressive state, for example,
    // which is basically the same as attacking state for the player.
    private GameObject _self;
    private EnemyContext _context;
    private EnemyAI _ai;
    private float _timer;
    private Vector3Int? _destination;
    private GameObject _player;
    private List<Node> _path;
    private int _index;
    private Vector3 _nextNodePosition;
    private bool _isMoving = false;
    private float _moveSpeed;
    private int _wanderRange;


    public void Enter(GameObject enemy)
    {
        // Debug.Log($"{enemy.name} enter passive state");

        _self = enemy;
        _self.TryGetComponent<EnemyContext>(out _context);
        _self.TryGetComponent<EnemyAI>(out _ai);

        _player = GameObject.FindWithTag("Player");

        _ai.ClearTarget();

        _moveSpeed = _context.Stats.MoveSpeed;
        _timer = _context.Stats.RestTime;
        _wanderRange = 2;

        Wander();
    }

    public void Execute()
    {            

        // if monster is aggressive, check if player is in range
        if (_context.Stats.Nature == MonsterData.MonsterNature.Aggressive)
            if (_player != null)
            {
                Vector3Int monsterPosition = GridManager.Instance.WorldToCell(_self.transform.position);
                Vector3Int playerPosition = GridManager.Instance.WorldToCell(_player.transform.position);

                if (DistanceHelper.IsInAttackRange(monsterPosition, playerPosition, _context.Stats.SightRangeToTurnAgressive))
                {
                    _ai.SetTarget(_player);
                    _ai.ChangeState(new AggressiveState());
                    return;
                }
            }

        // if is moving, handle movement
        if (_isMoving && _destination.HasValue)
        {
            Move();
        }
        // if is not moving
        else
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
            {
                Wander();
                _timer = _context.Stats.RestTime;
            }
        }

    }

    public void Exit()
    {
        _isMoving = false;
        _destination = null;
    }

    private void Move()
    {
        if (!_isMoving || _path == null || _index >= _path.Count) return;

        // move towards next node
        _self.transform.position = Vector3.MoveTowards
        (
            _self.transform.position,
            _nextNodePosition,
            _moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(_self.transform.position, _nextNodePosition) < 0.1f)
        {
            // snap to grid
            _self.transform.position = _nextNodePosition;

            // move index to next cell
            _index++;

            // reached destination
            if (_index >= _path.Count)
            {
                _isMoving = false;
                _destination = null;

                return;
            }

            SetNextTargetCell();
        }
    }

    private void Wander()
    {
        Vector3Int monster = GridManager.Instance.WorldToCell(_self.transform.position);
        
        Vector3Int cell = GetRandomBorderCell(monster);

        if (NodeManager.Instance.IsWalkable(cell))
        {
            GoTo(cell);
        }
        // chosen border cell is not walkable or a valid one
        else
        {
            TryFindingAnotherCell(monster);
        }
    }

    private void TryFindingAnotherCell(Vector3Int center)
    {
        var directions = 8; // 8 directional

        for (int attempt = 1; attempt <= _wanderRange; attempt++)
        {
            for (int i = 0; i < directions; i++)
            {
                Vector3Int testCell = center + GetDirectionOffset(i) * attempt;

                if (NodeManager.Instance.IsWalkable(testCell))
                {
                    GoTo(testCell);
                    return;
                }
            }
        }

        _isMoving = false;
        _destination = null;
    }

    private Vector3Int GetDirectionOffset(int direction)
    {
        switch (direction % 8)
        {
            case 0: return new Vector3Int(1,0,0); // right
            case 1: return new Vector3Int(1,1,0); // up-right
            case 2: return new Vector3Int(0,1,0); // up
            case 3: return new Vector3Int(-1,1,0); // up-left
            case 4: return new Vector3Int(-1,0,0); // left
            case 5: return new Vector3Int(-1,-1,0); // down-left
            case 6: return new Vector3Int(0,-1,0); // down
            case 7: return new Vector3Int(1,-1,0); // down-right
            default: return Vector3Int.zero;
        }
    }

    private void GoTo(Vector3Int cell)
    {
        Vector3Int monster = GridManager.Instance.WorldToCell(_self.transform.position);

        _path = NodeManager.Instance.FindPath(monster, cell);

        if (_path == null || _path.Count == 0)
        {
            _isMoving = false;
            return;
        }

        _destination = cell;
        _index = 0;
        _isMoving = true;
        SetNextTargetCell();
    }

    private void SetNextTargetCell()
    {
        if (_index < _path.Count)
        {
            Node nextNode = _path[_index];
            _nextNodePosition = GridManager.Instance.GetCellCenterWorld(nextNode._gridPosition);
            OnStep(nextNode._gridPosition);
        }
    }

    private void OnStep(Vector3Int newCell)
    {
        // add step-related logic
        // play footstep sound, play animation etc
    }

    private Vector3Int GetRandomBorderCell(Vector3Int center)
    {
        var range = _wanderRange;
        // random border (1 = top, 2 = right, 3 = bottom, 4 = left)
        int border = UnityEngine.Random.Range(1,5);

        int x, y;

        switch(border)
        {
            case 1: // top border
                x = UnityEngine.Random.Range(center.x - range, center.x + range + 1);
                y = center.y + range;
                break;
            case 2: // right border
                x = center.x + range;
                y = UnityEngine.Random.Range(center.y - range, center.y + range + 1);
                break;
            case 3: // bottom border
                x = UnityEngine.Random.Range(center.x - range, center.x + range + 1);
                y = center.y - range;
                break;
            case 4: // left border
                x = center.x - range;
                y = UnityEngine.Random.Range(center.y - range, center.y + range + 1);
                break;
            default:
                x = center.x;
                y = center.y;
                break;
        }

        return new Vector3Int(x, y, 0);
    }
}

public class AggressiveState : IEnemyState
{
    private GameObject _target;
    private GameObject _self;
    private EnemyContext _context;
    private EnemyAI _ai;
    private bool _isAttacking;
    private float _lastAttackTime;
    private float _chaseTimer;
    private float _maxChaseTime;
    private float _sightRange;
    private List<Node> _path;
    private int _index;
    private Vector3 _nextNodePosition;
    private bool _isChasing = false;
    private float _moveSpeed;
    private Skill _chosenSkillToCast;
    public void Enter(GameObject enemy)
    {
        // Debug.Log($"{enemy.name} entered aggressive state!");

        _self = enemy;
        _context = _self.GetComponent<EnemyContext>();
        _ai = _self.GetComponent<EnemyAI>();

        _target = _ai.CurrentTarget;        

        _moveSpeed = _context.Stats.MoveSpeed;
        _sightRange = _context.Stats.SightRange;
        _maxChaseTime = _context.Stats.StaminaToChaseInSeconds;
        _isAttacking = true;
        _lastAttackTime = Time.time;
        _chaseTimer = 0f;
        _chosenSkillToCast = null;

        _isChasing = false;
        _path = null;
        _index = 0;

        RecalculatePath();
    }

    public void Execute()
    {

        if (!_isAttacking) return;

        if (_target == null)
        {
            _ai.ChangeState(new PassiveState());
            return;
        }

        Vector3Int self = GridManager.Instance.WorldToCell(_self.transform.position);
        Vector3Int target = GridManager.Instance.WorldToCell(_target.transform.position);

        float distance = Vector3Int.Distance(self, target);

        // if target is too far away, lose target
        if (distance > _sightRange)
        {
            // Debug.Log($"{_self.name} lost sight of player");
            // show emote of drop of water
            _ai.ClearTarget();
            _ai.ChangeState(new PassiveState());
            return;
        }

        if (DistanceHelper.IsInAttackRange(self, target, _context.Stats.AttackRange))
        {
            _chaseTimer = 0f; // reset timer
            Attack();
        }
        // target is out of attack range
        else
        {
            _chaseTimer += Time.deltaTime;

            if (_chaseTimer >= _maxChaseTime)
            {
                Debug.Log($"{_self.name} gave up chasing after {_maxChaseTime} seconds");
                _ai.ClearTarget();
                _ai.ChangeState(new PassiveState());
                return;
            }

            if (_isChasing)
            {
                Chase();
            }
            else
            {
                RecalculatePath();
            }

        }
    }

    public void Exit()
    {
        _isAttacking = false;
        _isChasing = false;
    }

    private void Attack()
    {
        // whenever attacking, snap to grid
        _self.transform.position = Vector3.MoveTowards
        (
            _self.transform.position,
            _nextNodePosition,
            _moveSpeed * Time.deltaTime
        );

        if (Time.time - _lastAttackTime >= _context.Stats.AttackSpeed)
        {
            var player = _target.GetComponent<PlayerCombat>();

            if (player != null)
            {
                if (TryCastingSkill())
                {
                    CastSkill(_chosenSkillToCast);
                    return;
                }

                player.TakeDamage(_context.Stats.Attack);
                _lastAttackTime = Time.time;
            }
        }
    }

    private void Chase()
    {
        if (!_isChasing || _path == null || _index >= _path.Count) return;

        _self.transform.position = Vector3.MoveTowards
        (
            _self.transform.position,
            _nextNodePosition,
            _moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(_self.transform.position, _nextNodePosition) < 0.1f)
        {
            _self.transform.position = _nextNodePosition;

            _index++;

            Vector3Int currentPosition = GridManager.Instance.WorldToCell(_self.transform.position);
            Vector3Int positionTarget = GridManager.Instance.WorldToCell(_target.transform.position);

            if (DistanceHelper.IsInAttackRange(currentPosition, positionTarget, _context.Stats.AttackRange))
            {
                _isChasing = false;
                return;
            }

            if (_index >= _path.Count)
            {
                RecalculatePath();
                return;
            }

            SetNextTargetCell();
        }
    }

    private void RecalculatePath()
    {
        if (_target == null) return;

        Vector3Int self = GridManager.Instance.WorldToCell(_self.transform.position);
        Vector3Int target = GridManager.Instance.WorldToCell(_target.transform.position);

        // find a adjacent cell to player cell
        Vector3Int attackPosition = FindAttackPosition(self, target, _context.Stats.AttackRange);

        _path = NodeManager.Instance.FindPath(self, attackPosition);

        if (_path == null || _path.Count == 0)
        {
            _isChasing = false;
            return;
        }

        _index = 0;
        _isChasing = true;
        SetNextTargetCell();
    }

    private Vector3Int FindAttackPosition(Vector3Int positionSelf, Vector3Int positionTarget, int attackRange) 
    {
        // if already in attack range, do not move
        if (DistanceHelper.IsInAttackRange(positionSelf, positionTarget, attackRange))
        {
            return positionSelf;
        }

        List<Vector3Int> validPositions = new List<Vector3Int>();

        for (int x = -attackRange; x <= attackRange; x++)
        {
            for (int y = -attackRange; y <= attackRange; y++)
            {
                Vector3Int potentialPosition = positionTarget + new Vector3Int(x, y, 0);

                if (DistanceHelper.IsInAttackRange(potentialPosition, positionTarget, attackRange) &&
                    potentialPosition != positionTarget &&              // to prevent monster to step into player cell
                    NodeManager.Instance.IsWalkable(potentialPosition))
                {
                    validPositions.Add(potentialPosition);
                }
            }
        }

        Vector3Int closestPosition = positionTarget;
        float closestDistance = float.MaxValue;

        foreach (Vector3Int position in validPositions)
        {
            float distance = Vector3Int.Distance(positionSelf, position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPosition = position;
            }
        }

        return closestPosition;
    }

    private void SetNextTargetCell()
    {
        if (_index < _path.Count)
        {
            Node nextNode = _path[_index];
            _nextNodePosition = GridManager.Instance.GetCellCenterWorld(nextNode._gridPosition);
            OnStep(nextNode._gridPosition);
        }
    }

    private void OnStep(Vector3Int newCell)
    {
        // add step-related logic
        // play footstep sound, play animation etc
    }

    private bool TryCastingSkill()
    {
        var monsterSkills = _context.Stats.Skills;

        foreach(var skill in monsterSkills)
        {
            if (_context.IsOnCooldown(skill))
            {
                Debug.Log($"{skill.name} is on cooldown, skipping");
                continue;
            }

            if (_context.StatsManager.CurrentSP < skill.SpCost)
            {
                Debug.Log($"Not enough SP for {skill.name}, skipping");
                continue;
            }

            var chanceOfCasting = _context.Stats.GetChanceOfCasting(skill);
            var roll = UnityEngine.Random.Range(0f, 100f);

            Debug.Log($"Rolled {roll:F1} for {skill.name} (needed <= {chanceOfCasting}%)");

            if (roll <= chanceOfCasting)
            {
                Debug.Log("Chosen skill to cast!");
                _chosenSkillToCast = skill;
                return true;
            }
        }

        return false;
    }

    private void CastSkill(Skill skill)
    {
        _ai.ChangeState(new EnemyCastingState(skill));
        _context.PutOnCooldown(skill);
        _context.StatsManager.UseSP(skill.SpCost);
    }
}

public class EnemyCastingState : IEnemyState
{
    private GameObject _monster;
    private EnemyContext _context;
    private EnemyAI _ai;
    private Skill _skill;
    
    private CancellationTokenSource _cancellationTokenSource;
    public EnemyCastingState(Skill skill)
    {
        _skill = skill;
    }

    public void Enter(GameObject monster)
    {
        _monster = monster;
        _context = monster.GetComponent<EnemyContext>();
        _ai = monster.GetComponent<EnemyAI>();

        _cancellationTokenSource = new CancellationTokenSource();

        _ai.SetStateChangeBlock(true);

        StartCasting(_cancellationTokenSource.Token).Forget();
    }

    public void Execute()
    {
    }

    public void Exit()
    {
        _ai.SetStateChangeBlock(false);
    }

    private async UniTask StartCasting(CancellationToken cancellationToken)
    {
        try 
        {
            IsMonsterValid();

            await SnapToGrid(cancellationToken);

            if (!IsMonsterValid() || cancellationToken.IsCancellationRequested) return;


            CastingBarPool.Instance.ShowCastingBar(_monster, _skill);
            DamageCellController.Instance.InvokeDamageCells?.Invoke(_monster, _skill);

            await UniTask.Delay(TimeSpan.FromSeconds(_skill.CastingTime), cancellationToken: cancellationToken);

            if (!IsMonsterValid() || cancellationToken.IsCancellationRequested) return;
            
            if (IsMonsterValid())
            {
                _ai.SetStateChangeBlock(false); 
                _ai.ChangeState(new AggressiveState());
            }
        }
        catch (Exception e)
        {
            // Debug.LogError($"Casting failed: {e.Message}");
            
            if (_ai != null)
            {
                _ai.SetStateChangeBlock(false);
                _ai.ChangeState(new AggressiveState());
            }
        }
    }

    private async UniTask SnapToGrid(CancellationToken cancellationToken)
    {
        if (!IsMonsterValid()) return;
        
        Vector3 targetPosition = GridManager.Instance.GetCellCenterWorld(GridManager.Instance.WorldToCell(_monster.transform.position));

        float duration = 0.3f;
        float elapsed = 0f;
        Vector3 startPosition = _monster.transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _monster.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsed / duration);

            await UniTask.Yield(cancellationToken);
        }

        if (IsMonsterValid())
            _monster.transform.position = targetPosition;
    }

    private bool IsMonsterValid()
    {
        return _monster != null && _context != null && _ai != null;
    }
}