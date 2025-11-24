using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// [RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(EnemyAI))]
[RequireComponent(typeof(EnemyEventBus))]
[RequireComponent(typeof(EnemyStatsManager))]

public class EnemyContext : MonoBehaviour
{
    [field: SerializeField] public MonsterData Stats {get; private set;}
    // [field: SerializeField] public EnemyMovement Movement {get; private set;}
    [field: SerializeField] public EnemyAI AI {get; private set;}
    [field: SerializeField] public EnemyEventBus EventBus {get; private set;}
    [field: SerializeField] public EnemyStatsManager StatsManager {get; private set;}

    private Dictionary<Skill, float> _skillCooldowns = new Dictionary<Skill, float>();
    public bool IsOnCooldown(Skill skill)
    {
        if (!_skillCooldowns.ContainsKey(skill))
        {
            return false;
        }

        var isOnCooldown = Time.time < _skillCooldowns[skill];

        return isOnCooldown;
    }

    public void PutOnCooldown(Skill skill)
    {
        var cooldown = Stats.GetSkillCooldown(skill);

        if (cooldown > 0)
        {
            _skillCooldowns[skill] = Time.time + cooldown;
        }
    }
}
