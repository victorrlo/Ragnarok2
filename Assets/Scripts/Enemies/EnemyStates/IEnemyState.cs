using UnityEngine;

public interface IEnemyState
{
    void Enter(EnemyAI enemy);
    void Execute();
    void Exit();
}
