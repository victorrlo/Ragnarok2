using UnityEngine;

public static class GridHelper
{
    public static void SnapToNearestCellCenter(GameObject gameObject)
    {
        Vector3Int cell = GridManager.Instance.WorldToCell(gameObject.transform.position);
        Vector3 center = GridManager.Instance.GetCellCenterWorld(cell);

        gameObject.transform.position = new Vector3(center.x, gameObject.transform.position.y, center.z);
    }
}
