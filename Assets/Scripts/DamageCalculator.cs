using UnityEngine;

public static class DamageCalculator
{
    public static DamageResult Roll(int baseDamage, float criticalChance, float criticalMultiplier)
    {
        bool isCritical = Random.value <= Mathf.Clamp01(criticalChance);
        int finalDamage = isCritical
            ? Mathf.RoundToInt(baseDamage * criticalMultiplier)
            : baseDamage;

        return new DamageResult(finalDamage, isCritical);
    }
}
