using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

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

    [Header("Drops")]
    [SerializeField] private List<ItemDropData> _possibleDrops = new List<ItemDropData>();
    public List<ItemDropData> PossibleDrops => _possibleDrops;

    [Header("Monster Skill Rules")]
    [FormerlySerializedAs("_skillOverrides")]
    [SerializeField] private List<SkillCastingData> _monsterSkillRules = new List<SkillCastingData>();
    public List<Skill> Skills => _monsterSkillRules
        .Where(rule => rule.skill != null)
        .Select(rule => rule.skill)
        .ToList();

    private Dictionary<Skill, float> _skillChanceCache;
    private Dictionary<Skill, float> _skillCooldownCache;
    private Dictionary<Skill, float> _skillChargedChanceCache;

    public float GetChanceOfCasting(Skill skill)
    {
        if (_skillChanceCache == null) BuildSkillCache();

        return _skillChanceCache.TryGetValue(skill, out float chance)
        ? chance
        : 0f;
    }

    public float GetSkillCooldown(Skill skill)
    {
        if (_skillCooldownCache == null) BuildSkillCooldownCache();

        return _skillCooldownCache.TryGetValue(skill, out float cooldown)
        ? cooldown
        : 0f;
    }

    public float GetChanceOfChargedCasting(Skill skill)
    {
        if (_skillChargedChanceCache == null) BuildSkillChargedChanceCache();

        return _skillChargedChanceCache.TryGetValue(skill, out float chance)
            ? chance
            : 0f;
    }

    private void BuildSkillCache()
    {
        _skillChanceCache = new Dictionary<Skill, float>();

        foreach (var skillData in _monsterSkillRules)
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

        foreach (var skillData in _monsterSkillRules)
        {
            if (skillData.skill != null)
            {
                _skillCooldownCache[skillData.skill] = skillData.cooldown;
            }
        }
    }

    private void BuildSkillChargedChanceCache()
    {
        _skillChargedChanceCache = new Dictionary<Skill, float>();

        foreach (var skillData in _monsterSkillRules)
        {
            if (skillData.skill != null)
            {
                _skillChargedChanceCache[skillData.skill] = skillData.chanceOfChargedCasting;
            }
        }
    }
}

[System.Serializable]
public class ItemDropData
{
    public Item item;
    [Range(0f, 1f)]
    public float dropChance = 0.5f;
    public int minQuantity = 1;
    public int maxQuantity = 1;
}

[System.Serializable]

public class SkillCastingData
{
    public Skill skill;
    [Range(0f, 100f)]
    public float chanceOfCasting;
    [Range(0f, 100f)]
    public float chanceOfChargedCasting;
    [FormerlySerializedAs("delay")]
    public float cooldown;
}


