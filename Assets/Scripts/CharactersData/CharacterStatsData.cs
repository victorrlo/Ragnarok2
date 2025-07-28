using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStatsData", menuName = "Scriptable Objects/CharacterStatsData")]
public class CharacterStatsData : ScriptableObject
{
    [Header("Constants for characters behaviour")]
    [SerializeField] private float _basicTimeForEachAttack = 60f;
    [field: SerializeField] public string CharacterName {get; private set;}
    [field: SerializeField] public int MaxHP {get; private set;}
    [field: SerializeField] public int MaxSP {get; private set;}
    [field: SerializeField] public int Attack {get; private set;}
    [field: SerializeField] public int MoveSpeed {get; private set;}
    [field: SerializeField] public int AttackRange {get; private set;}

    [Header("Attributes")]
    // simplificando o sistema de jogo porque acho que não terei tempo de balancear, mais fácil apenas aumentar o HP dos monstros e ir aumentando o ataque do jogador ao passar de nível
    [field: SerializeField] public int Agility {get; private set;} //velocidade de ataque (talvez esquiva mais para frente)
    // public int _STR; // ataque
    // public int _VIT; // defesa
    // public int _DEX; // precisão
    // public int _INT; // força de habilidades (no caso de inimigos, defesa à habilidades também)
    
    [Header("Calculations")]
    public float AttackSpeed => _basicTimeForEachAttack / Agility;
}
