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
    [SerializeField] private string _description;
    public ItemName Name => _name;
    public Sprite Sprite => _sprite;
    public string Description => _description;
    public abstract ItemType Type {get;}
    public virtual void Use(PlayerContext context){}
}

[CreateAssetMenu(fileName = "Consumable", menuName = "Scriptable Objects/Consumable")]
public class Consumable : Item
{
    [Range(0f, 1f)] public float healPercent = 0.25f;
    public override ItemType Type => ItemType.Consumable;
    [SerializeField] private string _effectDescription;
    public string EffectDescription => _effectDescription;
    public override void Use(PlayerContext context)
    {
        int amount = Mathf.CeilToInt(context.Stats.MaxHP * healPercent);
        context.StatsManager.Heal();
    }
}

// [CreateAssetMenu(fileName = "Card", menuName = "Scriptable Objects/Card")]
// public class Card : Item
// {
//     [SerializeField] private SkillDefinition skill;
//     public override ItemType Type => ItemType.Card;
// }
