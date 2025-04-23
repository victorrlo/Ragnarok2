using UnityEngine;

public class Node
{
    public Vector3Int _gridPosition {get; set;}
    public bool _isWalkable {get;}
    public Node _parent;

    public int _gCost = int.MaxValue; // infinity cost
    public int _hCost;
    public int _fCost => _gCost + _hCost;

    public Node(Vector3Int position, bool isWalkable)
    {
        _gridPosition = position;
        _isWalkable = isWalkable;
    }
}
