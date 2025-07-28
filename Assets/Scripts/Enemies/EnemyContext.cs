using UnityEngine;

[RequireComponent(typeof(EnemyAI))]
[RequireComponent(typeof(EnemyEventBus))]
[RequireComponent(typeof(EnemyMovement))]
public class EnemyContext : MonoBehaviour
{
    [field: SerializeField] public MonsterStatsData Stats {get; private set;}
    [field: SerializeField] public EnemyMovement Movement {get; private set;}
    [field: SerializeField] public EnemyAI AI {get; private set;}
    [field: SerializeField] public EnemyEventBus EventBus {get; private set;}
    [field: SerializeField] public EnemyStatsManager StatsManager {get; private set;}
}
