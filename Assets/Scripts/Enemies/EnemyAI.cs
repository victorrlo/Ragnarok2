using System;
using System.Collections;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [SerializeField] private EnemyMovement _enemyMovement;
    private enum EnemyStates
    {
        Passive,
        Aggressive,
        Attacking
    }
    private EnemyStates _currentState;
    private EnemyStates _previousState;
    private Coroutine _wanderCoroutine;
    private bool _isMoving;

    private void Awake()
    {
        _isMoving = false;

        if (_enemyMovement == null)
            _enemyMovement = GetComponent<EnemyMovement>();

        _currentState = GetComponent<EnemyStats>().StatsData._monsterNature == MonsterStatsData.MonsterNature.Passive ? EnemyStates.Passive : EnemyStates.Aggressive;
    }

    private void Update()
    {
        if (_currentState == EnemyStates.Passive && !_isMoving)
        {
            OnStateChanged(_currentState);
        }
            
        if (_currentState != _previousState)
        {
            OnStateChanged(_currentState);
            _previousState = _currentState;
        }
    }

    private void OnStateChanged(EnemyStates currentState)
    {
        if (_wanderCoroutine != null)
        {
            StopCoroutine(_wanderCoroutine);
            _isMoving = false;
        }

        switch (_currentState)
        {
            case EnemyStates.Passive:
                _wanderCoroutine = StartCoroutine(PassiveWanderLoop());
                break;
            case EnemyStates.Aggressive:
                // AggressiveStateMovement();
                break;
            case EnemyStates.Attacking:
                // AttackingStateMovement();
                break;
        }
    }

    private IEnumerator PassiveWanderLoop()
    {
        _isMoving = true;
        _enemyMovement.WanderRandomly();
        yield return new WaitForSeconds(4f);
        _isMoving = false;
    }
}
