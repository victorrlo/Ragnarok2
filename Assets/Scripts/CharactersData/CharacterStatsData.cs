using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStatsData", menuName = "Scriptable Objects/CharacterStatsData")]
public class CharacterStatsData : ScriptableObject
{
    [Header("Constants for characters behaviour")]
    [field: SerializeField] public string CharacterName {get; private set;}
    [field: SerializeField] public int MaxHP {get; private set;}
    [field: SerializeField] public int MaxSP {get; private set;}
    [field: SerializeField] public int Attack {get; private set;}
    [field: SerializeField] public int MoveSpeed {get; private set;}
    [field: SerializeField] public int AttackRange {get; private set;}
    [field: SerializeField, Range(0f, 1f)] public float CriticalChance {get; private set;} = 0.1f;
    [field: SerializeField] public float CriticalDamageMultiplier {get; private set;} = 1.5f;

    [Header("Attributes")]
    // simplificando o sistema de jogo porque acho que não terei tempo de balancear, mais fácil apenas aumentar o HP dos monstros e ir aumentando o ataque do jogador ao passar de nível
    [field: SerializeField] public int Agility {get; private set;} //velocidade de ataque (talvez esquiva mais para frente)
    // public int _STR; // ataque
    // public int _VIT; // defesa
    // public int _DEX; // precisão
    // public int _INT; // força de habilidades (no caso de inimigos, defesa à habilidades também)
    
    [Header("Calculations")]
    public float AttackSpeed => 1f / Mathf.Clamp(Agility, 1, 7);
}
