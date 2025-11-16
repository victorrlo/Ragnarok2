using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UiCustomCursor : MonoBehaviour
{
    [SerializeField] private Texture2D _normalCursorSprite;
    [SerializeField] private Texture2D _attackCursorSprite;
    [SerializeField] private Texture2D _itemCursorSprite;
    private Texture2D _currentCursorTexture;
    private Vector2 _cursorHotSpot;
    [SerializeField] private Camera _mainCamera;

    private void Awake()
    {
        if(_mainCamera == null)
            _mainCamera = Camera.main;
    }

    private void Start()
    {
        _cursorHotSpot = new Vector2(0,1);
    }

    private void Update()
    {

        if (IsMouseOverEnemy())
        {
            SetCursorState(_attackCursorSprite);
            return;
        }

        if (IsMouseOverItem())
        {
            SetCursorState(_itemCursorSprite);
            return;
        }

        if (GetCursorState() == _normalCursorSprite) return;

        SetCursorState(_normalCursorSprite);
    }

    private bool IsMouseOverEnemy()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.collider.tag.Equals("Enemy");
        }

        return false;
    }

    private bool IsMouseOverItem()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            return hit.collider.GetComponent<ItemDataLoader>() != null;
        }

        return false;
    }

    private void SetCursorState(Texture2D texture)
    {
        if (texture == _currentCursorTexture) return;
        _currentCursorTexture = texture;
        Cursor.SetCursor(texture, _cursorHotSpot, CursorMode.ForceSoftware);
    }

    private Texture2D GetCursorState()
    {
        return _currentCursorTexture;
    }
}
