using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterStatsData", menuName = "Scriptable Objects/MonsterStatsData")]
public class MonsterData : CharacterStatsData
{
    public enum MonsterNature
    {
        Passive,
        Aggressive
    }

    [Header("Monster Nature")]
    [SerializeField] private MonsterNature _monsterNature;
    public MonsterNature Nature => _monsterNature;
    [SerializeField] private int _sightRange = 1;
    public int SightRange => _sightRange;
    [field: SerializeField] public int SightRangeToTurnAgressive; 
    [field: SerializeField] public int  RestTime {get; private set;} // time between new movements when wandering
    [field: SerializeField] public float StaminaToChaseInSeconds; 

    [Header("Skill Overrides")]
    [SerializeField] private List<SkillCastingData> _skillOverrides = new List<SkillCastingData>();
    public List<Skill> Skills => _skillOverrides
        .Where(overrideData => overrideData.skill != null)
        .Select(overrideData => overrideData.skill)
        .ToList();

    private Dictionary<Skill, float> _skillChanceCache;
    private Dictionary<Skill, float> _skillCooldownCache;

    public float GetChanceOfCasting(Skill skill)
    {
        if (_skillChanceCache == null) BuildSkillCache();

        return _skillChanceCache.TryGetValue(skill, out float chance)
        ? chance
        : skill.ChanceOfCasting;
    }

    public float GetSkillCooldown(Skill skill)
    {
        if (_skillCooldownCache == null) BuildSkillCooldownCache();

        return _skillCooldownCache.TryGetValue(skill, out float delay)
        ? delay
        : 0f;
    }

    private void BuildSkillCache()
    {
        _skillChanceCache = new Dictionary<Skill, float>();

        foreach (var skillData in _skillOverrides)
        {
            if (skillData.skill != null)
            {
                _skillChanceCache[skillData.skill] = skillData.chanceOfCasting;
            }
        }
    }

    private void BuildSkillCooldownCache()
    {
        _skillCooldownCache = new Dictionary<Skill, float>();

        foreach (var skillData in _skillOverrides)
        {
            if (skillData.skill != null)
            {
                _skillCooldownCache[skillData.skill] = skillData.delay;
            }
        }
    }
}

[System.Serializable]

public class SkillCastingData
{
    public Skill skill;
    public float chanceOfCasting;
    public float delay;
}


