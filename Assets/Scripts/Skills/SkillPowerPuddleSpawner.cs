using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SkillPowerPuddleSpawner : MonoBehaviour
{
    [SerializeField] private SkillPowerPuddle _puddlePrefab;
    [SerializeField] private Tilemap _walkableTilemap;
    [SerializeField] private Vector3Int _areaMinCell;
    [SerializeField] private Vector3Int _areaMaxCell;
    [SerializeField, Min(0f)] private float _spawnInterval = 8f;
    [SerializeField, Min(0)] private int _maxActivePuddles = 5;
    [SerializeField, Min(0)] private int _spawnOnStart = 0;

    private readonly List<SkillPowerPuddle> _activePuddles = new();
    private float _spawnTimer;

    private void Start()
    {
        for (int i = 0; i < _spawnOnStart; i++)
            TrySpawnPuddle();
    }

    private void Update()
    {
        _activePuddles.RemoveAll(puddle => puddle == null);

        if (_activePuddles.Count >= _maxActivePuddles)
            return;

        _spawnTimer += Time.deltaTime;

        if (_spawnTimer < _spawnInterval)
            return;

        _spawnTimer = 0f;
        TrySpawnPuddle();
    }

    private void TrySpawnPuddle()
    {
        if (_puddlePrefab == null)
            return;

        if (!TryGetRandomSpawnCell(out Vector3Int cell))
            return;

        Vector3 position = GetCellCenterWorld(cell);
        SkillPowerPuddle puddle = Instantiate(_puddlePrefab, position, _puddlePrefab.transform.rotation);
        _activePuddles.Add(puddle);
    }

    private bool TryGetRandomSpawnCell(out Vector3Int cell)
    {
        const int maxAttempts = 50;

        GetSpawnAreaBounds(out int minX, out int maxX, out int minY, out int maxY);

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            cell = new Vector3Int(
                Random.Range(minX, maxX + 1),
                Random.Range(minY, maxY + 1),
                0);

            if (SkillPowerPuddle.HasPuddleAt(cell))
                continue;

            if (_walkableTilemap != null && !_walkableTilemap.HasTile(cell))
                continue;

            if (NodeManager.Instance != null && !NodeManager.Instance.IsWalkable(cell))
                continue;

            return true;
        }

        if (_walkableTilemap != null && !IsUsingTilemapBounds(minX, maxX, minY, maxY))
        {
            BoundsInt bounds = _walkableTilemap.cellBounds;
            return TryGetRandomSpawnCellFromBounds(
                bounds.xMin,
                bounds.xMax - 1,
                bounds.yMin,
                bounds.yMax - 1,
                out cell);
        }

        cell = default;
        return false;
    }

    private bool TryGetRandomSpawnCellFromBounds(int minX, int maxX, int minY, int maxY, out Vector3Int cell)
    {
        const int maxAttempts = 100;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            cell = new Vector3Int(
                Random.Range(minX, maxX + 1),
                Random.Range(minY, maxY + 1),
                0);

            if (SkillPowerPuddle.HasPuddleAt(cell))
                continue;

            if (_walkableTilemap != null && !_walkableTilemap.HasTile(cell))
                continue;

            if (NodeManager.Instance != null && !NodeManager.Instance.IsWalkable(cell))
                continue;

            return true;
        }

        cell = default;
        return false;
    }

    private void GetSpawnAreaBounds(out int minX, out int maxX, out int minY, out int maxY)
    {
        minX = Mathf.Min(_areaMinCell.x, _areaMaxCell.x);
        maxX = Mathf.Max(_areaMinCell.x, _areaMaxCell.x);
        minY = Mathf.Min(_areaMinCell.y, _areaMaxCell.y);
        maxY = Mathf.Max(_areaMinCell.y, _areaMaxCell.y);
    }

    private bool IsUsingTilemapBounds(int minX, int maxX, int minY, int maxY)
    {
        if (_walkableTilemap == null)
            return false;

        BoundsInt bounds = _walkableTilemap.cellBounds;

        return minX == bounds.xMin &&
               maxX == bounds.xMax - 1 &&
               minY == bounds.yMin &&
               maxY == bounds.yMax - 1;
    }

    private Vector3 GetCellCenterWorld(Vector3Int cell)
    {
        if (_walkableTilemap != null)
            return _walkableTilemap.GetCellCenterWorld(cell);

        if (GridManager.Instance != null)
            return GridHelper.GetCellCenterWorld(gameObject, cell);

        return cell;
    }
}
