using UnityEngine;

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