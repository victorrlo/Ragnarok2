using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{   
    [SerializeField] private AimBehaviour _aim;
    [SerializeField] private NodeManager _nodeManager;
    private Vector3Int? _startPos;
    private Vector3Int? _finalPos;
    public Vector3Int? StartPos => _startPos;
    public Vector3Int? FinalPos => _finalPos;
    [SerializeField] private Grid _grid;
    [SerializeField] private float _moveSpeed;
    private Coroutine _moveCoroutine;

    public void OnMouseClick(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        _startPos = GetCurrentPlayerPosition();
        _finalPos = _aim._lastGridCellPosition;
        
        if (!_finalPos.HasValue || !_aim.IsWalkable(_finalPos.Value))
        {
            Debug.LogWarning("Invalid move: Tile is not walkable!");
        }

        List<Node> path = _nodeManager.FindPath(_startPos.Value, _finalPos.Value);
        _nodeManager.DrawPath(path);

        if (path == null || path.Count == 0)
        {
            Debug.LogWarning("No path found!");
            return;
        }
        
        if (_moveCoroutine != null) // if already walking, stop to initiate another movement
        {
            StopCoroutine(_moveCoroutine);
        }

        var destinationGrid = _grid.GetCellCenterWorld(_finalPos.Value);
        var gridCenterFinalPos = new Vector3(destinationGrid.x, 0, destinationGrid.z);

        // _moveCoroutine = StartCoroutine(MoveToPosition(gridCenterFinalPos));   
        _moveCoroutine = StartCoroutine(FollowPath(path));         
    }

    public Vector3Int GetCurrentPlayerPosition()
    {
        Vector3 worldPosition = transform.position;
        Vector3Int gridPosition = TransformGridPositionToPosition(worldPosition);

        return gridPosition;
    }

    public Vector3Int TransformGridPositionToPosition(Vector3 worldPosition)
    {
        Vector3Int position = _grid.WorldToCell(worldPosition);
        return position;
    }

    private IEnumerator FollowPath(List<Node> path)
    {
        foreach (Node node in path)
        {
            Vector3 destinationWorld = _grid.GetCellCenterWorld(node._gridPosition);
            Vector3 flatDestination = new Vector3(destinationWorld.x, 0, destinationWorld.z);

            while (Vector3.Distance(transform.position, flatDestination) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, flatDestination, _moveSpeed*Time.deltaTime);
                yield return null;
            }

            transform.position = flatDestination;
        }
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        float distanceThreshold = 0.1f; // minimal distance to set position to center of grid

        while(Vector3.Distance(transform.position, targetPosition) > distanceThreshold)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, _moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = targetPosition; // reach the final destination (grid center)
    }
}
