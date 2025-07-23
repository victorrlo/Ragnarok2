using UnityEngine;

[CreateAssetMenu(fileName = "CharacterStatsData", menuName = "Scriptable Objects/CharacterStatsData")]
public class CharacterStatsData : ScriptableObject
{
    public string _characterName;
    public int _maxHP;
    public int _maxSP;
    public int _attack;
    // simplificando o sistema de jogo porque acho que não terei tempo de balancear, mais fácil apenas aumentar o HP dos monstros e ir aumentando o ataque do jogador ao passar de nível

    // public int _STR; // ataque
    // public int _VIT; // defesa
    // public int _DEX; // precisão
    // public int _INT; // força de habilidades (no caso de inimigos, defesa à habilidades também)
    // public int _AGI; // esquiva e velocidade de ataque
    public int _moveSpeed;
    public int _attackSpeed;
    public int _attackRange;

    public int MoveSpeed => _moveSpeed;
    public int AttackRange => _attackRange;
}
