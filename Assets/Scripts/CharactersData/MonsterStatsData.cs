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
}
