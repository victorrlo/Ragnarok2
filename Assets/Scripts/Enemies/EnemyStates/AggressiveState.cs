using System.Collections;
using UnityEngine;

public class AggressiveState : IEnemyState
{
    private EnemyAI _enemy;
    private bool _isWandering;

    public void Enter(EnemyAI enemy, MonsterStatsData monsterData)
    {
        _enemy = enemy;
        Debug.LogWarning("Entered Aggressive State...");
    }

    public void Execute()
    {
        // condicionais de mudanças de estado
    }

    public void Exit()
    {
        Debug.LogWarning("Exiting Aggressive State...");
    }
}
