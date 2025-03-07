using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AimBehaviour : MonoBehaviour
{
    private Camera _mainCamera;
    [SerializeField] private LayerMask _aimLayerMask;
    [SerializeField] private Grid _grid; 
    private float _gridSize;

    public Vector3Int? LastGridCellPosition { get; private set; }

    private void Start()
    {
        _mainCamera = Camera.main;
        _gridSize = _grid.cellSize.x;
    }

    public void OnMouseMove(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        LastGridCellPosition = GetMouseGridPosition();
        if (LastGridCellPosition.HasValue)
        {
            transform.position = _grid.GetCellCenterWorld(LastGridCellPosition.Value);
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        }
    }
    
    private Vector3Int? GetMouseGridPosition()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _aimLayerMask))
        {
            return _grid.WorldToCell(hit.point);
        }
        return null;
    }   
}
