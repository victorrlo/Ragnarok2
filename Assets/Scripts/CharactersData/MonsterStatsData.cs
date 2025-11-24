using System.Collections.Generic;
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

    [field: SerializeField] public List<Skill> Skills; 

    [Header("Skill Overrides")]
    [SerializeField] private List<SkillCastingData> _skillOverrides = new List<SkillCastingData>();

    private Dictionary<Skill, float> _skillChanceCache;

    public float GetChanceOfCasting(Skill skill)
    {
        if (_skillChanceCache == null) BuildSkillCache();

        return _skillChanceCache.TryGetValue(skill, out float chance)
        ? chance
        : skill.ChanceOfCasting;
    }

    private void BuildSkillCache()
    {
        _skillChanceCache = new Dictionary<Skill, float>();

        foreach (var skillData in _skillOverrides)
        {
            if (skillData.Skill != null)
            {
                _skillChanceCache[skillData.Skill] = skillData.ChanceOfCasting;
            }
        }
    }
}

[System.Serializable]

public class SkillCastingData
{
    public Skill Skill;
    public int ChanceOfCasting;
}


