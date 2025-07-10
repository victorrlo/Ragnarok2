using UnityEngine;

public interface IEnemyState
{
    void Enter(EnemyAI enemy, MonsterStatsData monsterData);
    void Execute();
    void Exit();
}
