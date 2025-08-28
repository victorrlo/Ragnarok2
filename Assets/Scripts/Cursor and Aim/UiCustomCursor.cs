using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UiCustomCursor : MonoBehaviour
{
    [SerializeField] private Texture2D _normalCursorSprite;
    [SerializeField] private Texture2D _attackCursorSprite;
    [SerializeField] private Texture2D _rotateCursorSprite;
    private Texture2D _currentCursorTexture;
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
        if (Mouse.current != null && Mouse.current.rightButton.isPressed)
        {
            SetCursorIfChanged(_rotateCursorSprite);
            return;
        }

        SetCursorIfChanged(IsMouseOverEnemy() ? _attackCursorSprite : _normalCursorSprite);
    }

    private bool IsMouseOverEnemy()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.collider.GetComponent<EnemyMovement>() != null;
        }

        return false;
    }

    private void SetCursorIfChanged(Texture2D texture)
    {
        if (texture == _currentCursorTexture) return;
        _currentCursorTexture = texture;
        Cursor.SetCursor(texture, _cursorHotSpot, CursorMode.Auto);
    }
}
