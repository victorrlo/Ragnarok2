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
    [SerializeField] private float _maxRestTime; // time between new movements when wandering
    [SerializeField] private int _sightRange = 1;

    public MonsterNature Nature => _monsterNature;
    public float MaximumRestTime => _maxRestTime;
    public int SightRange => _sightRange;
    [field: SerializeField] public int AggressiveStateSightRange; 

    [field: SerializeField] public float StaminaToChaseInSeconds; 
}
