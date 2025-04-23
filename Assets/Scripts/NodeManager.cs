using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class NodeManager : MonoBehaviour
{
    private Grid _grid;
    [SerializeField] private Tilemap _walkableTilemap;
    [SerializeField] private Tilemap _obstacleTilemap;
    private List<Node> _lastPath;
    
    private Dictionary<Vector3Int, Node> _nodes = new();

    private void Awake()
    {
        _grid = GetComponent<Grid>();
        GenerateNodes();
    }

    void GenerateNodes()
    {
        BoundsInt bounds = _walkableTilemap.cellBounds;
        
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            bool isWalkable = _walkableTilemap.HasTile(pos);
            bool isObstacle = _obstacleTilemap != null && _obstacleTilemap.HasTile(pos);

            // Node only exists if it’s a walkable or obstacle tile
            if (isWalkable || isObstacle)
            {
                _nodes[pos] = new Node(pos, isWalkable && !isObstacle);
            }
        }

        Debug.Log($"✅ Generated {_nodes.Count} total nodes");
    }

    public bool IsWalkable(Vector3Int pos)
    {
        if (_obstacleTilemap.HasTile(pos))
            return false;

        return _walkableTilemap.HasTile(pos);
    }

    public List<Node> FindPath(Vector3Int startPos, Vector3Int targetPos)
    {
        if (!_nodes.ContainsKey(startPos) || !_nodes.ContainsKey(targetPos))
            return null;

        BinaryNodeHeap openSet = new BinaryNodeHeap();
        HashSet<Vector3Int> closedSet = new();

        Node startNode = _nodes[startPos];
        Node targetNode = _nodes[targetPos];

        ResetNodes();

        startNode._gCost = 0;
        startNode._hCost = GetDistance(startNode, targetNode);
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet.Remove();
            if (currentNode == null)
                return null;

            closedSet.Add(currentNode._gridPosition);

            if (currentNode._gridPosition == targetPos)
            {
                return RetracePath(startNode, currentNode);
            }

            foreach (Node neighbor in GetNeighbors(currentNode))
            {
                if (!neighbor._isWalkable || closedSet.Contains(neighbor._gridPosition))
                {
                    continue;
                }

                int costToNeighbor = (currentNode._gridPosition.x != neighbor._gridPosition.x &&
                                        currentNode._gridPosition.y != neighbor._gridPosition.y) ? 14 : 10;

                int newGCost = currentNode._gCost + costToNeighbor;

                if (newGCost < neighbor._gCost || !openSet.Contains(neighbor))
                {
                    neighbor._gCost = newGCost;
                    neighbor._hCost = GetDistance(neighbor, targetNode);
                    neighbor._parent = currentNode;

                    if(!openSet.Contains(neighbor))
                    {
                        openSet.Add(neighbor);
                    }
                }
            }
        }
        return null;
    }

    List<Node> RetracePath(Node start, Node end)
    {
        List<Node> path = new();
        Node current = end;

        while (current != start)
        {
            path.Add(current);
            current = current._parent;
        }

        path.Reverse();
        return path;
    }

    int GetDistance(Node a, Node b)
    {
        int dx = Mathf.Abs(a._gridPosition.x - b._gridPosition.x);
        int dy = Mathf.Abs(a._gridPosition.y - b._gridPosition.y);
        return (dx + dy) *10;
    }

    List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new();
        Vector3Int[] directions = new Vector3Int[]
        {
            new Vector3Int( 0, 1, 0),   //up
            new Vector3Int( 0, -1, 0),  //down
            new Vector3Int( -1, 0, 0),  //left
            new Vector3Int( 1, 0, 0),   //right
            new Vector3Int( -1, 1, 0),  //up-left
            new Vector3Int( 1, 1, 0),   //up-right
            new Vector3Int( -1, -1, 0), //down-left
            new Vector3Int( 1, -1, 0)   //down-right   
        };

        foreach (var dir in directions)
        {
            Vector3Int neighborPos = node._gridPosition + dir;

            if (!_nodes.ContainsKey(neighborPos)) continue;

            if (Mathf.Abs(dir.x) == 1 && Mathf.Abs(dir.y) == 1)
            {
                Vector3Int horizontal = node._gridPosition + new Vector3Int(dir.x, 0, 0);
                Vector3Int vertical = node._gridPosition + new Vector3Int(0, dir.y, 0);

                if (!_nodes.ContainsKey(horizontal) || !_nodes.ContainsKey(vertical)) continue;

                if (!_nodes[horizontal]._isWalkable || !_nodes[vertical]._isWalkable)
                {
                    // Debug.Log($"Blocked diagonal at {neighborPos} due to corner clipping");
                    continue;
                }   
            }

            neighbors.Add(_nodes[neighborPos]);
        }

        return neighbors;
    }

    void ResetNodes()
    {
        foreach (var node in _nodes.Values)
        {
            node._gCost = int.MaxValue;
            node._hCost = 0;
            node._parent = null;
        }
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

        if(_lastPath != null)
        {
            foreach (Node node in _lastPath)
            {
                Vector3 pos = _grid.GetCellCenterWorld(node._gridPosition);
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(pos + new Vector3(0, 0.1f, 0), 0.2f);
            }
        }
    }

    public void DrawPath(List<Node> path)
    {
        _lastPath = path;
    }

}
