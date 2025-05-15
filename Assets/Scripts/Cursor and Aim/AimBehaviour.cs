using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class AimBehaviour : MonoBehaviour
{
    public static AimBehaviour Instance { get; private set;}
    private Camera _mainCamera;
    [SerializeField] private Grid _grid; 
    [SerializeField] private Tilemap _walkableTilemap;
    [SerializeField] private Tilemap _obstacleTilemap;
    public Vector3Int? _lastGridCellPosition { get; private set; }

    private void Awake()
    {
        // make sure there's only one instance of this in the scene as we won't want multiple aims!
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    public void OnMouseMove(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        var newGridPosition = GetMouseGridPosition();

        if (newGridPosition.HasValue)
        {
            _lastGridCellPosition = newGridPosition;
            transform.position = _grid.GetCellCenterWorld(_lastGridCellPosition.Value);
            transform.position = new Vector3(transform.position.x, 0.01f, transform.position.z);
        }
    }
    
    private Vector3Int? GetMouseGridPosition()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        Plane groundPlane = new Plane(Vector3.up, Vector3.zero); // plane at y = 0
        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3Int gridPosition = _grid.WorldToCell(hitPoint);

            if (IsWalkable(gridPosition))
            {
                return gridPosition;
            }
        }

        return null;
    }   

    public bool IsWalkable(Vector3Int gridPosition)
    {
        if (_obstacleTilemap.HasTile(gridPosition))
        {
            return false;
        }

        return _walkableTilemap.HasTile(gridPosition);
    }
}
