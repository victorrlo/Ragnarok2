using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public static PlayerInputHandler Instance {get; private set;}
    private PlayerContext _playerContext;

    public void Awake()
    {
        if (_playerContext == null)
            TryGetComponent<PlayerContext>(out _playerContext);

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnMouseClick(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        var isEnemy = TryClickToAttack();

        if (isEnemy)
        {
            return;
        }

        var isItem = TryClickToGetItem();

        Vector3Int? cell = AimBehaviour.Instance._lastGridCellPosition;
        
        if (!cell.HasValue) return;

        Vector3Int targetPosition = cell.Value;

        if (!AimBehaviour.Instance.IsWalkable(targetPosition)) return;

        _playerContext.Control.WalkTo(targetPosition);
        // _playerContext.Movement.WalkToEmptyTile(targetPosition);
    }

    private bool TryClickToGetItem()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            ItemDataLoader item = hit.collider.GetComponent<ItemDataLoader>();
            if (item == null)
            {
                return false;
            }
                
            _playerContext.Control.GetItem(item.gameObject);
            return true;
        }

        return false;
    }

    private bool TryClickToAttack()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            EnemyCombat enemy = hit.collider.GetComponent<EnemyCombat>();
            if (enemy == null)
            {
                _playerContext.Control.StopCombat();
                return false;
            }
                
            _playerContext.Control.StartCombat(enemy.gameObject);
            return true;
        }

        return false;
    }

}
