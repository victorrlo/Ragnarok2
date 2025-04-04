using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{   
    private Pathfinding _pathFinding;
    [SerializeField] private AimBehaviour _aim;
    [SerializeField] private Tilemap _walkableTilemap;
    private Vector3Int _startPos;
    [SerializeField] private Grid _grid;
    [SerializeField] private float _moveSpeed;
    private Coroutine _moveCoroutine;

    private void Awake()
    {
        _pathFinding = GetComponent<Pathfinding>();
    }
    
    public void OnMouseClick(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        var targetGrid = _aim._lastGridCellPosition;
        _startPos = GetCurrentPlayerPosition();
        
        if (!targetGrid.HasValue || !_aim.IsWalkable(targetGrid.Value))
        {
            Debug.LogWarning("Invalid move: Tile is not walkable!");
        }

        var destinationGrid = _grid.GetCellCenterWorld(targetGrid.Value);
        var newFinalPosition = new Vector3(destinationGrid.x, 0, destinationGrid.z);
        
        if (_moveCoroutine != null) // if already walking, stop to initiate another movement
        {
            StopCoroutine(_moveCoroutine);
        }

        _moveCoroutine = StartCoroutine(MoveToPosition(newFinalPosition));            
    
    }

    public Vector3Int GetCurrentPlayerPosition()
    {
        Vector3 worldPosition = transform.position;
        Vector3Int gridPosition = _grid.WorldToCell(worldPosition);

        return gridPosition;
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
