using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UiCustomCursor : MonoBehaviour
{
    [SerializeField] private Texture2D _normalCursorSprite;
    [SerializeField] private Texture2D _attackCursorSprite;
    private Vector2 _cursorHotSpot;
    [SerializeField] private LayerMask _enemyLayerMask;
    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Grid _grid;

    private void Awake()
    {
        if(_mainCamera == null)
            _mainCamera = Camera.main;
    }

    private void Start()
    {
        _cursorHotSpot = new Vector2(0,0);
    }

    private void Update()
    {
        Vector3Int? gridCell = GetMouseGridCell();

        if (gridCell.HasValue &&
            GridOccupancyManager.Instance.TryGetOccupant(gridCell.Value, out GameObject occupant) &&
            occupant.CompareTag("Enemy"))
        {
            Cursor.SetCursor(_attackCursorSprite, _cursorHotSpot, CursorMode.Auto);
        }
        else
        {
            Cursor.SetCursor(_normalCursorSprite, _cursorHotSpot, CursorMode.Auto);
        }
    }

    private Vector3Int? GetMouseGridCell()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            return _grid.WorldToCell(hitPoint);
        }

        return null;
    }
}
