using System.Collections;
using UnityEngine;

public class PassiveState : IEnemyState
{
    private EnemyAI _enemy;
    private Coroutine _routine;
    private float _maxRestTime = 4f;

    public void Enter(EnemyAI enemy, MonsterStatsData monsterData)
    {
        _enemy = enemy;
        _maxRestTime = monsterData.MaximumRestTime;
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
        
        var randomRestTime = UnityEngine.Random.Range(0, _maxRestTime);
        yield return new WaitForSeconds(randomRestTime);

        _enemy.ChangeState(new PassiveState());
    }
}
