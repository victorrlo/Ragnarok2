using System;
using UnityEngine;
using UnityEngine.XR;

public class EnemyAI : MonoBehaviour
{
    private IEnemyState _currentState;
    private MonsterStatsData _monsterData;
    [SerializeField] private EnemyMovement _movement;

    private void Awake()
    {
        if (_movement == null)
            _movement = GetComponent<EnemyMovement>();

        _monsterData = GetComponent<EnemyStats>().StatsData;
        _currentState = _monsterData.Nature == MonsterStatsData.MonsterNature.Passive ?
                                    new PassiveState() : new AggressiveState();
    }

    private void Start()
    {
        ChangeState(_currentState);
    }

    public void ChangeState(IEnemyState newState)
    {
        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter(this, _monsterData);
    }

    public EnemyMovement Movement => _movement;
}
