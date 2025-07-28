using UnityEngine;

public static class EnemyStateExtensions
{
    public static bool IsSameTypeAs(this IEnemyState current, IEnemyState other)
    {
        if (current == null || other == null) return false;

        return current.GetType() == other.GetType();
    }
}
