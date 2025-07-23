using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    private IEnemyState _currentState;
    private MonsterStatsData _monsterData;
    public MonsterStatsData MonsterStatsData => _monsterData;
    [SerializeField] private EnemyMovement _movement;
    public EnemyMovement Movement => _movement;
    [SerializeField] private EnemyDamageEvent _onDamagedEvent;

    private void Awake()
    {
        if (_movement == null)
            _movement = GetComponent<EnemyMovement>();

        _monsterData = GetComponent<EnemyStats>().StatsData;
        _currentState = null;
    }

    private void OnEnable()
    {
        _onDamagedEvent.OnRaised += OnDamageTaken;
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
        _onDamagedEvent.OnRaised -= OnDamageTaken;
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
