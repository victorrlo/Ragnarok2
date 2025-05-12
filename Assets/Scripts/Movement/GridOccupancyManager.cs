using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridOccupancyManager : MonoBehaviour
{
    public static GridOccupancyManager Instance {get; private set;}
    private Dictionary <Vector3Int, GameObject> _occupants = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool IsCellOccupied(Vector3Int cell)
    {
        return _occupants.ContainsKey(cell);
    }

    public bool TryGetOccupant(Vector3Int cell, out GameObject occupant)
    {
        return _occupants.TryGetValue(cell, out occupant);
    }

    public void RegisterOccupant(Vector3Int cell, GameObject obj)
    {
        _occupants[cell] = obj;
    }

    public void UnregisterOccupant(Vector3Int cell)
    {
        if (_occupants.ContainsKey(cell))
        {
            _occupants.Remove(cell);
        }
    }

    public void MoveOccupant(Vector3Int from, Vector3Int to)
    {
        if (_occupants.TryGetValue(from, out GameObject obj))
        {
            _occupants.Remove(from);
            _occupants[to] = obj;
        }
    }

    public void Cleanup()
    {
        List<Vector3Int> keysToRemove = new();

        foreach (var pair in _occupants)
        {
            if (pair.Value == null)
            {
                keysToRemove.Add(pair.Key);
            }
        }

        foreach (var key in keysToRemove)
        {
            _occupants.Remove(key);
        }
    }
}
