using System.Collections;
using UnityEngine;

public class PassiveState : IEnemyState
{
    private EnemyAI _enemy;
    private EnemyContext _enemyContext;
    private Transform _player;
    private Coroutine _routine;

    public void Enter(EnemyAI enemy)
    {
        Debug.Log("Enter Passive State");
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

        _enemyContext.Movement.StartWandering();

        randomRestingTime = UnityEngine.Random.Range(1, _enemyContext.Stats.MaximumRestTime);
        yield return new WaitForSeconds(randomRestingTime);

        _enemy.ChangeState(new PassiveState());
    }
}
