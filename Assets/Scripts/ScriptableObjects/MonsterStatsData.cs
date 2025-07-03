using UnityEngine;

[CreateAssetMenu(fileName = "MonsterStatsData", menuName = "Scriptable Objects/MonsterStatsData")]
public class MonsterStatsData : CharacterStatsData
{
    public enum MonsterNature
    {
        Passive,
        Aggressive
    }
    public MonsterNature _monsterNature;
}
