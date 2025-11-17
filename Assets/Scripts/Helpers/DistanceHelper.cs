using System;
using UnityEngine;

public static class DistanceHelper
{
    public static bool IsInAttackRange(Vector3Int a, Vector3Int b, int range)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return dx <= range && dy <= range;
    }
}
