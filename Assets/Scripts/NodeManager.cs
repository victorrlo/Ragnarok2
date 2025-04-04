using UnityEngine;
using UnityEngine.Tilemaps;

public class NodeManager : MonoBehaviour
{
    private Grid _grid;
    [SerializeField] private Tilemap _walkableTilemap;
    [SerializeField] private Tilemap _obstacleTilemap;

    private void Awake()
    {
        _grid = GetComponent<Grid>();
    }

    void OnDrawGizmos()
    {
        if (_walkableTilemap != null)
            DrawTileMapGizmos(_walkableTilemap, Color.green);

        if (_obstacleTilemap != null)
        {
            DrawTileMapGizmos(_obstacleTilemap, Color.red);
        }
    }

    void DrawTileMapGizmos(Tilemap tilemap, Color color)
    {   
        BoundsInt bounds = tilemap.cellBounds; // this gets the boundaries of the tilemap in cell size.

        foreach (Vector3Int pos in bounds.allPositionsWithin) // all the positions of tilemap
        {
            if (tilemap.HasTile(pos))
            {
                Vector3 worldPos = tilemap.CellToWorld(pos) + new Vector3(0.5f, 0, 0.5f);
                Gizmos.color = color;
                Gizmos.DrawWireCube(worldPos, new Vector3(1f, 0.1f, 1f));
            }
        }
    }
}
