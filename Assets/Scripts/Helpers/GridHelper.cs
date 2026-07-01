using System.Collections;
using UnityEngine;

public static class GridHelper
{
    public static Vector3 GetNearestCellCenterWorld(GameObject gameObject)
    {
        Vector3Int cell = GridManager.Instance.WorldToCell(gameObject.transform.position);
        return GetCellCenterWorld(gameObject, cell);
    }

    public static Vector3 GetCellCenterWorld(GameObject gameObject, Vector3Int cell)
    {
        Vector3 center = GridManager.Instance.GetCellCenterWorld(cell);
        return new Vector3(center.x, gameObject.transform.position.y, center.z);
    }

    public static void SnapToNearestCellCenter(GameObject gameObject)
    {
        gameObject.transform.position = GetNearestCellCenterWorld(gameObject);
    }

    public static IEnumerator SnapToNearestCellCenter(GameObject gameObject, float duration)
    {
        Vector3 center = GetNearestCellCenterWorld(gameObject);

        Vector3 startPos = gameObject.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            t = Mathf.SmoothStep(0, 1, t);

            gameObject.transform.position = Vector3.Lerp(startPos, center, t);
            yield return null;
        }

        gameObject.transform.position = center;
    }
}
