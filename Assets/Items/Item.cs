    using System;
using UnityEngine;

public enum ItemType
{
    Consumable,
    Card
}

public abstract class Item : ScriptableObject
{
    [SerializeField] private Sprite _sprite;
    [SerializeField] private string _name;
    [SerializeField] private string _englishDescription;

    [SerializeField] private string _brazilianDescription;
    // A small apple. Still consumable and not living.
    public string Name => _name;
    public Sprite Sprite => _sprite;
    public string EnglishDescription => _englishDescription;
    public string BrazilianDescription => _brazilianDescription;
    public abstract ItemType Type {get;}
    public virtual void Use(){}
}

// [CreateAssetMenu(fileName = "Card", menuName = "Scriptable Objects/Card")]
// public class Card : Item
// {
//     [SerializeField] private SkillDefinition skill;
//     public override ItemType Type => ItemType.Card;
// }
