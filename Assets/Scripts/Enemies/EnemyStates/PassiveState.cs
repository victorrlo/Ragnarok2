using System.Collections;
using UnityEngine;

public class PassiveState : IEnemyState
{
    private EnemyAI _enemy;
    private Transform _player;
    private Coroutine _routine;

    public void Enter(EnemyAI enemy)
    {
        _enemy = enemy;
        _player = GameObject.FindWithTag("Player")?.transform;
        _routine = _enemy.StartCoroutine(WanderRoutine());
    }

    public void Execute()
    {
        // condicionais de mudanças de estado
        if (_enemy.MonsterStatsData.Nature == MonsterStatsData.MonsterNature.Aggressive)
            if (DistanceHelper.IsPlayerInRange(_player.transform, _enemy))
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
        var randomRestTime = UnityEngine.Random.Range(0, _enemy.MonsterStatsData.MaximumRestTime);
        
        yield return new WaitForSeconds(randomRestTime);
        yield return _enemy.Movement.WanderRandomly();
        yield return new WaitForSeconds(randomRestTime);

        _enemy.ChangeState(new PassiveState());
    }
}
