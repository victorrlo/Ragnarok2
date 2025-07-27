using System.ComponentModel;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStatsData", menuName = "Scriptable Objects/CharacterStatsData")]
public class CharacterStatsData : ScriptableObject
{
    [Header("Constants for characters behaviour")]
    [SerializeField] private float _basicTimeForEachAttack = 60f;

    [Space]
    public string _characterName;
    public int _maxHP;
    public int _maxSP;
    public int _attack;
    
    public int _moveSpeed;
    public int _attackRange;

    public int MoveSpeed => _moveSpeed;
    public int AttackRange => _attackRange;

    [Header("Attributes")]
    // simplificando o sistema de jogo porque acho que não terei tempo de balancear, mais fácil apenas aumentar o HP dos monstros e ir aumentando o ataque do jogador ao passar de nível
    public int _agility; //velocidade de ataque (talvez esquiva mais para frente)
    // public int _STR; // ataque
    // public int _VIT; // defesa
    // public int _DEX; // precisão
    // public int _INT; // força de habilidades (no caso de inimigos, defesa à habilidades também)
    
    [Header("Calculations")]
    public float AttackSpeed => _basicTimeForEachAttack/_agility;
}
