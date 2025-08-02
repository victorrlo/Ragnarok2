using System;
using UnityEngine;

public static class DistanceHelper
{
    public static int GetCellDistance(Vector3Int a, Vector3Int b)
    {
        // chebyshev distance
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return Mathf.Max(dx, dy);
    }

    // sight range used to determine if enemy should start aggresive behavior
    public static bool IsPlayerInAggressiveReach(Transform player, EnemyAI enemy)
    {
        var playerPosition = GridManager.Instance.WorldToCell(player.transform.position);
        var enemyPosition = GridManager.Instance.WorldToCell(enemy.transform.position);
        var distance = GetCellDistance(playerPosition, enemyPosition);
        
        enemy.TryGetComponent<EnemyContext>(out var enemyContext);
        var sightRange = enemyContext.Stats.AggressiveStateSightRange;

        if (distance <= sightRange) 
            return true;

        return false;
    }

    // sight range used to determine if enemy should keep chasing the player or not
    public static bool IsPlayerOutOfReach(Transform player, EnemyAI enemy)
    {
        var playerPosition = GridManager.Instance.WorldToCell(player.transform.position);
        var enemyPosition = GridManager.Instance.WorldToCell(enemy.transform.position);
        var distance = GetCellDistance(playerPosition, enemyPosition);
        
        enemy.TryGetComponent<EnemyContext>(out var enemyContext);
        var sightRange = enemyContext.Stats.SightRange;

        if (distance <= sightRange) 
            return false;

        return true;
    }

    public static bool IsAdjacent(Vector3Int a, Vector3Int b, int range)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return dx <= range && dy <= range;
    }
}
