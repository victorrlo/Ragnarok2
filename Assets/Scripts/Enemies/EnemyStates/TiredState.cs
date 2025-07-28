using System.Collections;
using UnityEngine;

public class TiredState : IEnemyState
{
    private EnemyAI _enemy;
    private EnemyContext _enemyContext;
    private Coroutine _restCoroutine;

    public void Enter(EnemyAI enemy)
    {
        _enemy = enemy;
        _enemy.TryGetComponent<EnemyContext>(out _enemyContext);

        Debug.Log("enemy is tired. stopping path updates.");
        _restCoroutine = _enemy.StartCoroutine(RestAndReturnToPassive());
    }

    public void Execute()
    {
        // enemy is tired and resting
    }

    public void Exit()
    {
        if (_restCoroutine != null)
        {
            _enemy.StopCoroutine(_restCoroutine);
            _restCoroutine = null;
        }
    }

    private IEnumerator RestAndReturnToPassive()
    {
        yield return new WaitForSeconds(_enemyContext.Stats.MaximumRestTime);

        _enemy.ChangeState(new PassiveState());
    }
}
