using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AimBehaviour : MonoBehaviour
{
    private AimBehaviour _instance;
    private Camera _mainCamera;
    [SerializeField] private LayerMask _aimLayerMask;
    [SerializeField] private Grid _grid; 
    [SerializeField] private float _raycastDistance = 2f;

    public Vector3Int? LastGridCellPosition { get; private set; }

    private void Awake()
    {
        // make sure there's only one instance of this in the scene as we won't want multiple aims!
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        _mainCamera = Camera.main;
    }

    public void OnMouseMove(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        LastGridCellPosition = GetMouseGridPosition();
        if (LastGridCellPosition.HasValue)
        {
            transform.position = _grid.GetCellCenterWorld(LastGridCellPosition.Value);
            transform.position = new Vector3(transform.position.x, 0.01f , transform.position.z);
        }
    }
    
    private Vector3Int? GetMouseGridPosition()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _aimLayerMask))
        {
            if (IsWalkable(hit.point))
            {
                return _grid.WorldToCell(hit.point);
            }
        }
        return null;
    }   

    bool IsWalkable(Vector3 position)
    {
        RaycastHit hit;

        Vector3 rayStart = new Vector3(position.x, position.y + _raycastDistance/2, position.z);

        if (Physics.Raycast(rayStart, Vector3.down, out hit, _raycastDistance, _aimLayerMask))
        {
            return hit.collider.CompareTag("Walkable");
        }

        return false;
    }
}
