using System.Collections;
using UnityEngine;

public class PassiveState : IEnemyState
{
    private EnemyAI _enemy;
    private Coroutine _routine;
    private float _restTime = 4f;

    public void Enter(EnemyAI enemy, MonsterStatsData monsterData)
    {
        _enemy = enemy;
        _restTime = monsterData.RestTime;
        _routine = _enemy.StartCoroutine(WanderRoutine());
    }

    public void Execute()
    {
        // condicionais de mudanças de estado
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
        yield return _enemy.Movement.WanderRandomly();

        yield return new WaitForSeconds(_restTime);

        _enemy.ChangeState(new PassiveState());
    }
}
