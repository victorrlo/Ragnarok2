using UnityEngine;
[RequireComponent(typeof(EnemyContext))]
public class EnemyAI : MonoBehaviour
{
    private IEnemyState _currentState;
    public IEnemyState CurrentState => _currentState;
    private EnemyContext _enemyContext;
    private EnemyEventBus _enemyEventBus;
    private EnemyMovement _enemyMovement;

    private void Awake()
    {
        if (_enemyContext == null)
            TryGetComponent<EnemyContext>(out _enemyContext);

        if (_enemyEventBus == null)
            TryGetComponent<EnemyEventBus>(out _enemyEventBus);

        if (_enemyMovement == null)
            TryGetComponent<EnemyMovement>(out _enemyMovement);

        _currentState = null;
    }

    private void OnEnable()
    {
        _enemyEventBus.OnDamaged.OnRaised += OnDamageTaken;
    }

    private void Start()
    {
        ChangeState(_enemyContext.Stats.Nature == MonsterStatsData.MonsterNature.Passive ?
                                    new PassiveState() : new AggressiveState());
    }

    private void Update()
    {
        _currentState?.Execute();
    }

    private void OnDisable()
    {
        _enemyEventBus.OnDamaged.OnRaised -= OnDamageTaken;
    }

    public void ChangeState(IEnemyState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter(this);
    }

    private void OnDamageTaken(EnemyDamageEventData data)
    {
        if (data._target != gameObject) return;

        ChangeState(new AggressiveState());
    }
}
