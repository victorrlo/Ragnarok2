using UnityEngine;

[CreateAssetMenu(fileName = "MonsterStatsData", menuName = "Scriptable Objects/MonsterStatsData")]
public class MonsterStatsData : CharacterStatsData
{
    public enum MonsterNature
    {
        Passive,
        Aggressive
    }
    [SerializeField] private MonsterNature _monsterNature;
    [SerializeField] private float _maxRestTime; // time between new movements when wandering

    public MonsterNature Nature => _monsterNature;
    public float MaximumRestTime => _maxRestTime;
}
