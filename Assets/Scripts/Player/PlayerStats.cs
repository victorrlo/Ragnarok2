using UnityEngine;

[System.Serializable]
public class PlayerStats : MonoBehaviour
{
    public int MaxHP;
    public int MaxSP;
    public int Attack;
    public int AttackRange;
    public int Agility;
    public int AttackSpeed => Mathf.RoundToInt( 1f / Agility );
    public int MoveSpeed;

    // runtime values
    public int CurrentHP;
    public int CurrentSP;
}
