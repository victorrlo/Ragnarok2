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
    [SerializeField] private float _restTime; // time between new movements when wandering

    public MonsterNature Nature => _monsterNature;
    public float RestTime => _restTime;
}
