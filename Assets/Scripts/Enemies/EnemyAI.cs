using System.Collections;
using UnityEngine;
[RequireComponent(typeof(EnemyContext))]
public class EnemyAI : MonoBehaviour
{
    private IEnemyState _currentState;
    public IEnemyState CurrentState => _currentState;
    private EnemyContext _enemyContext;
    private EnemyEventBus _enemyEventBus;
    // private EnemyMovement _enemyMovement;

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
        _currentState.Enter(this);
    }

    private void OnDamageTaken(DamageEventData data)
    {
        if (data._target != gameObject) return;

        ChangeState(new AggressiveState());
    }
}

public class PassiveState : IEnemyState
{
    private EnemyAI _enemy;
    private EnemyContext _enemyContext;
    private Transform _player;
    private Coroutine _routine;

    public void Enter(EnemyAI enemy)
    {
        _enemy = enemy;
        _enemy.TryGetComponent<EnemyContext>(out _enemyContext);

        _player = GameObject.FindWithTag("Player")?.transform;

        _routine = _enemy.StartCoroutine(WanderRoutine());
    }

    public void Execute()
    {
        if (_player == null)
            return;
            
        if (_enemyContext.Stats.Nature == MonsterStatsData.MonsterNature.Aggressive)
            if (DistanceHelper.IsPlayerInAggressiveReach(_player.transform, _enemy))
                _enemy.ChangeState(new AggressiveState());
    }

    public void Exit()
    {
        if (_routine != null)
        {
            _enemy.StopCoroutine(_routine);
            _routine = null;
        }
    }

    private IEnumerator WanderRoutine()
    {
        int randomRestingTime = UnityEngine.Random.Range(1, _enemyContext.Stats.MaximumRestTime);
        yield return new WaitForSeconds(randomRestingTime);

        // _enemyContext.Movement.StartWandering();

        randomRestingTime = UnityEngine.Random.Range(1, _enemyContext.Stats.MaximumRestTime);
        yield return new WaitForSeconds(randomRestingTime);

        _enemy.ChangeState(new PassiveState());
    }
}

public class AggressiveState : IEnemyState
{
    private EnemyAI _enemy;
    private EnemyContext _enemyContext;
    private GameObject _player;
    // private PlayerMovement _playerMovement;

    public void Enter(EnemyAI enemy)
    {
        _enemy = enemy;
        _enemy.TryGetComponent<EnemyContext>(out _enemyContext);
        
        _player = GameObject.FindWithTag("Player");
        // _player.TryGetComponent<PlayerMovement>(out _playerMovement);

        if (_player == null)
        {
            _enemy.ChangeState(new PassiveState());
            return;
        }

        if (DistanceHelper.IsPlayerInAggressiveReach(_player.transform, _enemy))
        {
            StartChase();
        }
    }
    public void Execute()
    {
        if ( _player == null || DistanceHelper.IsPlayerOutOfReach(_player.transform, _enemy)) 
        {
            Debug.Log($"{_enemy.name} Show emote for tired because out of reach");
            _enemy.ChangeState(new PassiveState());
            return;
        }
    }

    public void Exit()
    {
        
    }

    private void StartChase()
    {
        var data = new StartAttackData(_enemy.gameObject, _player);
        // _enemyContext.Movement.StartChasing(data);
    }
}

