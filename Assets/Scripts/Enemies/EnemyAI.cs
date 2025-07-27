using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private IEnemyState _currentState;
    [SerializeField] private MonsterStatsData _monsterData;
    [SerializeField] private EnemyMovement _movement;
    [SerializeField] private EnemyEventBus _enemyEventBus;
    public MonsterStatsData MonsterStatsData => _monsterData;
    public EnemyMovement Movement => _movement;

    private void Awake()
    {
        if (_enemyEventBus == null)
            TryGetComponent<EnemyEventBus>(out _enemyEventBus);
            
        _currentState = null;
    }

    private void OnEnable()
    {
        _enemyEventBus.OnDamaged.OnRaised += OnDamageTaken;
    }

    private void Start()
    {
        ChangeState(_monsterData.Nature == MonsterStatsData.MonsterNature.Passive ?
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

        Debug.LogWarning($"{name} foi atacado!!!");
        ChangeState(new AggressiveState());
    }
}
