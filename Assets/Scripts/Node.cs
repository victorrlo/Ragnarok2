using UnityEngine;

public class Node
{
    public Vector2Int _gridPosition;
    public bool _isWalkable;
    public Node _parent;
    public float _gCost;
    public float _hCost;
    public float _fCost => _gCost + _hCost;

    public Node(Vector2Int position, bool isWalkable)
    {
        _gridPosition = position;
        _isWalkable = isWalkable;
    }
}
