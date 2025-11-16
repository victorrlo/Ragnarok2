using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(EnemyContext))]
public class EnemyAI : MonoBehaviour
{
    private IEnemyState _currentState;
    public IEnemyState CurrentState => _currentState;
    private EnemyContext _enemyContext;
    private EnemyEventBus _enemyEventBus;
    // private EnemyMovement _enemyMovement;
    public GameObject CurrentTarget { get; private set; }
    public Vector3Int? CurrentDestination { get; private set; }

    private void Awake()
    {
        if (_enemyContext == null)
            TryGetComponent<EnemyContext>(out _enemyContext);

        if (_enemyEventBus == null)
            _enemyContext.TryGetComponent<EnemyEventBus>(out _enemyEventBus);

        // if (_enemyMovement == null)
        //     _enemyContext.TryGetComponent<EnemyMovement>(out _enemyMovement);

        _currentState = null;
    }

    private void OnEnable()
    {
        _enemyEventBus.OnDamaged += OnDamageTaken;
    }

    private void Start()
    {
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
        if (_currentState != null &&  _currentState.GetType() == newState.GetType()) return;

        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter(gameObject);
    }

    private void OnDamageTaken(DamageEventData data)
    {
        if (data._target != gameObject) return;

        // ChangeState(new AggressiveState());
    }

    public void SetTarget(GameObject target) => CurrentTarget = target;
    public void ClearTarget() => CurrentTarget = null;
    public void SetDestination(Vector3Int position) => CurrentDestination = position;
    public void ClearDestination() => CurrentDestination = null;
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
    private GameObject _enemy;
    private EnemyContext _context;
    private EnemyAI _ai;
    private float _timer;
    private Vector3Int? _destination;
    // private Coroutine _routine;
    private GameObject _player;
    private List<Node> _path;
    private int _index;
    private Vector3 _nextNodePosition;
    private bool _isMoving = false;
    private float _moveSpeed;
    private int _wanderRange;


    public void Enter(GameObject enemy)
    {
        Debug.Log($"{enemy.name} enter passive state");

        _enemy = enemy;
        _enemy.TryGetComponent<EnemyContext>(out _context);
        _enemy.TryGetComponent<EnemyAI>(out _ai);

        _player = GameObject.FindWithTag("Player");

        _ai.ClearTarget();

        _moveSpeed = _context.Stats.MoveSpeed;
        _timer = _context.Stats.RestTime;
        _wanderRange = 2;

        Wander();
        // _routine = _ai.StartCoroutine(WanderRoutine());
    }

    public void Execute()
    {            
        // if (_context.Stats.Nature == MonsterStatsData.MonsterNature.Aggressive)
            // if (_player != null && DistanceHelper.IsPlayerInAggressiveReach(_player, _enemy))
            // {
                // _ai.ChangeState(new AggressiveState());
            //     return;
            // }

        // if is move, handle movement
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
        // if (_routine != null)
        // {
        //     _ai.StopCoroutine(_routine);
        //     _routine = null;
        // }
        _isMoving = false;
        _destination = null;
    }

    private void Move()
    {
        if (!_isMoving || _path == null || _index >= _path.Count) return;

        // move towards next node
        _enemy.transform.position = Vector3.MoveTowards
        (
            _enemy.transform.position,
            _nextNodePosition,
            _moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(_enemy.transform.position, _nextNodePosition) < 0.1f)
        {
            // snap to grid
            _enemy.transform.position = _nextNodePosition;

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
        Vector3Int monster = GridManager.Instance.WorldToCell(_enemy.transform.position);
        
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
        Vector3Int monster = GridManager.Instance.WorldToCell(_enemy.transform.position);

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
        int border = Random.Range(1,5);

        int x, y;

        switch(border)
        {
            case 1: // top border
                x = Random.Range(center.x - range, center.x + range + 1);
                y = center.y + range;
                break;
            case 2: // right border
                x = center.x + range;
                y = Random.Range(center.y - range, center.y + range + 1);
                break;
            case 3: // bottom border
                x = Random.Range(center.x - range, center.x + range + 1);
                y = center.y - range;
                break;
            case 4: // left border
                x = center.x - range;
                y = Random.Range(center.y - range, center.y + range + 1);
                break;
            default:
                x = center.x;
                y = center.y;
                break;
        }

        return new Vector3Int(x, y, 0);
    }

    // private IEnumerator WanderRoutine()
    // {
    //     int randomRestingTime = UnityEngine.Random.Range(1, _context.Stats.MaximumRestTime);
    //     yield return new WaitForSeconds(randomRestingTime);

    //     // _enemyContext.Movement.StartWandering();

    //     randomRestingTime = UnityEngine.Random.Range(1, _context.Stats.MaximumRestTime);
    //     yield return new WaitForSeconds(randomRestingTime);

    //     _ai.ChangeState(new PassiveState());
    // }
}

// public class AggressiveState : IEnemyState
// {
//     private GameObject _enemy;
//     private EnemyContext _enemyContext;
//     private GameObject _player;
//     // private PlayerMovement _playerMovement;

//     public void Enter(GameObject enemy)
//     {
//         _enemy = enemy;
//         _enemy.TryGetComponent<EnemyContext>(out _enemyContext);
        
//         _player = GameObject.FindWithTag("Player");
//         // _player.TryGetComponent<PlayerMovement>(out _playerMovement);

//         if (_player == null)
//         {
//             _enemy.ChangeState(new PassiveState());
//             return;
//         }

//         if (DistanceHelper.IsPlayerInAggressiveReach(_player.transform, _enemy))
//         {
//             StartChase();
//         }
//     }
//     public void Execute()
//     {
//         if ( _player == null || DistanceHelper.IsPlayerOutOfReach(_player.transform, _enemy)) 
//         {
//             Debug.Log($"{_enemy.name} Show emote for tired because out of reach");
//             _enemy.ChangeState(new PassiveState());
//             return;
//         }
//     }

//     public void Exit()
//     {
        
//     }

//     private void StartChase()
//     {
//         var data = new StartAttackData(_enemy.gameObject, _player);
//         // _enemyContext.Movement.StartChasing(data);
//     }
// }

// public class TiredState : IEnemyState
// {
//     private EnemyAI _enemy;
//     private EnemyContext _enemyContext;
//     private Coroutine _restCoroutine;

//     public void Enter(EnemyAI enemy)
//     {
//         _enemy = enemy;
//         _enemy.TryGetComponent<EnemyContext>(out _enemyContext);

//         Debug.Log("enemy is tired. stopping path updates.");
//         _restCoroutine = _enemy.StartCoroutine(RestAndReturnToPassive());
//     }

//     public void Execute()
//     {
//         // enemy is tired and resting
//     }

//     public void Exit()
//     {
//         if (_restCoroutine != null)
//         {
//             _enemy.StopCoroutine(_restCoroutine);
//             _restCoroutine = null;
//         }
//     }

//     private IEnumerator RestAndReturnToPassive()
//     {
//         yield return new WaitForSeconds(_enemyContext.Stats.MaximumRestTime);

//         _enemy.ChangeState(new PassiveState());
//     }
// }

