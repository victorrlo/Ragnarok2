using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class DamageCellController : MonoBehaviour
{
    [SerializeField] GameObject _damageCell;
    [SerializeField] private Tilemap _walkableTilemap;

    private List<Vector3Int> _allWalkableTilePositions = new List<Vector3Int>();


    private void Awake()
    {
        AssertReferences();
        var grid = GridManager.Instance;

        _allWalkableTilePositions.Clear();

        BoundsInt bounds = _walkableTilemap.cellBounds;

        foreach (Vector3Int position in bounds.allPositionsWithin)
        {
            if (_walkableTilemap.HasTile(position))
            {
                _allWalkableTilePositions.Add(position);
            }
        }

        foreach (Vector3Int position in _allWalkableTilePositions)
        {
            Vector3 worldPosition = _walkableTilemap.GetCellCenterWorld(position);
            Instantiate(_damageCell, worldPosition, _damageCell.transform.rotation);
        }

    }

    private void AssertReferences()
    {
        if (_walkableTilemap == null) 
        {
            Debug.LogError("Walkable Tilemap not found!");
            return;
        }

        if (_damageCell == null)
        {
            Debug.LogError("Damage Cell Prefab not found!");
            return;
        }
    }
}
