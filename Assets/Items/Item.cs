    using System;
using UnityEngine;

public enum ItemType
{
    Consumable,
    Card
}

public enum ItemName { Apple };

public abstract class Item : ScriptableObject
{
    [SerializeField] private Sprite _sprite;
    [SerializeField] private ItemName _name;
    [SerializeField] private string _englishDescription;

    [SerializeField] private string _brazilianDescription;
    // A small apple. Still consumable and not living.
    public ItemName Name => _name;
    public Sprite Sprite => _sprite;
    public string EnglishDescription => _englishDescription;
    public string BrazilianDescription => _brazilianDescription;
    public abstract ItemType Type {get;}
    public virtual void Use(){}
}

[CreateAssetMenu(fileName = "Consumable", menuName = "Scriptable Objects/Consumable")]
public class Consumable : Item
{
    [Range(0f, 1f)] public float healPercent = 0.25f;
    public override ItemType Type => ItemType.Consumable;
    [SerializeField] private string _effectDescription;
    public string EffectDescription => _effectDescription;
    public override void Use()
    {
        int amount = Mathf.CeilToInt(PlayerStatsManager.Instance.RunTimeStats.MaxHP * healPercent);
        PlayerStatsManager.Instance.Heal(healPercent);
    }
}

// [CreateAssetMenu(fileName = "Card", menuName = "Scriptable Objects/Card")]
// public class Card : Item
// {
//     [SerializeField] private SkillDefinition skill;
//     public override ItemType Type => ItemType.Card;
// }
