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
    private readonly RaycastHit[] _mouseRaycastHits = new RaycastHit[64];

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
        MouseInteractableType interactableType = GetMouseInteractableType();

        if (interactableType == MouseInteractableType.Enemy)
        {
            SetCursorState(_attackCursorSprite);
            return;
        }

        if (interactableType == MouseInteractableType.Item)
        {
            SetCursorState(_itemCursorSprite);
            return;
        }

        if (GetCursorState() == _normalCursorSprite) return;

        SetCursorState(_normalCursorSprite);
    }

    private MouseInteractableType GetMouseInteractableType()
    {
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        int hitCount = Physics.RaycastNonAlloc(ray, _mouseRaycastHits);

        MouseInteractableType closestType = MouseInteractableType.None;
        float closestDistance = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = _mouseRaycastHits[i];
            MouseInteractableType hitType = GetInteractableType(hit.collider);

            if (hitType == MouseInteractableType.None || hit.distance >= closestDistance)
                continue;

            closestType = hitType;
            closestDistance = hit.distance;
        }

        return closestType;
    }

    private MouseInteractableType GetInteractableType(Collider collider)
    {
        if (collider.TryGetComponent<EnemyCombat>(out _))
            return MouseInteractableType.Enemy;

        if (collider.TryGetComponent<ItemDataLoader>(out _))
            return MouseInteractableType.Item;

        return MouseInteractableType.None;
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

    private enum MouseInteractableType
    {
        None,
        Enemy,
        Item
    }
}
