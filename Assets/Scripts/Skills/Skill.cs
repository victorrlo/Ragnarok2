using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "Scriptable Objects/Skill")]
public class Skill : ScriptableObject
{
    public enum Type
    {
        Active, Passive, AreaOfEffect, SingleTarget
    }

    [field:SerializeField] public string Name {get; private set;}
    [field: SerializeField] public Type SkillType {get; private set;}
    [field: SerializeField] public string Description {get; private set;}
    [field: SerializeField] public Sprite Icon {get; private set;}
    [field: SerializeField] public int SpCost {get; private set;}
    [field: SerializeField] public float Multiplier {get; private set;}
    [field: SerializeField] public float TimeOfEffect {get; private set;}
    [field: SerializeField] public int Range {get; private set;}

    public float CastingTime => SpCost/10f;
    [field: SerializeField] public int ChanceOfCasting {get; private set;}
}
