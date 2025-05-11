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
        Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero); //plane to fix raycast not hitting things

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Debug.DrawRay(hitPoint, Vector3.up * 0.5f, Color.magenta);

            if (Physics.Raycast(hitPoint + Vector3.up *2f, Vector3.down, out RaycastHit hit, 5f))
            {
                Debug.Log("Hit: " + hit.collider.name);
                Cursor.SetCursor(_attackCursorSprite, _cursorHotSpot, CursorMode.Auto);
            }
            else
            {
                Cursor.SetCursor(_normalCursorSprite, _cursorHotSpot, CursorMode.Auto);
            }
        }
        else
        {
            Cursor.SetCursor(_normalCursorSprite, _cursorHotSpot, CursorMode.Auto);
        }

        
    }
}
