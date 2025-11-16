using UnityEngine;

[CreateAssetMenu(fileName = "MonsterStatsData", menuName = "Scriptable Objects/MonsterStatsData")]
public class MonsterStatsData : CharacterStatsData
{
    public enum MonsterNature
    {
        Passive,
        Aggressive
    }

    [Header("Monster Nature")]
    [SerializeField] private MonsterNature _monsterNature;
    [SerializeField] private int _sightRange = 1;

    public MonsterNature Nature => _monsterNature;

    [field: SerializeField] public int  RestTime {get; private set;} // time between new movements when wandering
    public int SightRange => _sightRange;
    [field: SerializeField] public int AggressiveStateSightRange; 

    [field: SerializeField] public float StaminaToChaseInSeconds; 
}
