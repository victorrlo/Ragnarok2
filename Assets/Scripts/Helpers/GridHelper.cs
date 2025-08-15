using System.Collections;
using UnityEngine;

public static class GridHelper
{
    public static IEnumerator SnapToNearestCellCenter(GameObject gameObject, float duration)
    {
        Vector3Int cell = GridManager.Instance.WorldToCell(gameObject.transform.position);
        Vector3 center = GridManager.Instance.GetCellCenterWorld(cell);

        center = new Vector3(center.x, gameObject.transform.position.y, center.z);

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
