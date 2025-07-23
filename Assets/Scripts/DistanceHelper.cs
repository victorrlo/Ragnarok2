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

    public static bool IsPlayerInRange(Transform player, EnemyAI enemy)
    {
        var playerPosition = GridManager.Instance.WorldToCell(player.transform.position);
        var enemyPosition = GridManager.Instance.WorldToCell(enemy.transform.position);
        var distance = GetCellDistance(playerPosition, enemyPosition);
        var sightRange = enemy.MonsterStatsData.SightRange;

        if (distance <= sightRange) 
            return true;

        return false;
    }

    public static bool IsAdjacent(Vector3Int a, Vector3Int b, int range)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return dx <= range && dy <= range && (dx + dy) > 0;
    }
}
