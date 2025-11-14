using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public static PlayerInputHandler Instance {get; private set;}
    private PlayerContext _context;
    [SerializeField] private GameObject _targetMarkerPrefab;
    private GameObject _activeTargetMarker;
    private GameObject _target;
    private GameObject _previousTarget;

    public void Awake()
    {
        if (_context == null)
            TryGetComponent<PlayerContext>(out _context);

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

        // var isEnemy = CheckIfIsEnemy();

        // if (isEnemy)
        // {
        //     if (_previousTarget == _target) return;

        //     var enemyPosition = GridManager.Instance.WorldToCell(_target.transform.position);
            
        //     // _playerContext.Control.ChangeState(new WalkingState(enemyPosition, _target));
            
        //     _previousTarget = _target;

        //     _playerContext.Control.CurrentTarget = _target;
            
        //     MarkTarget(_target);
            
        //     return;
        // }

        // var isItem = CheckIfIsItem();

        // if (isItem)
        // {
        //     if (_previousTarget == _target) return;

        //     var itemPosition = GridManager.Instance.WorldToCell(_target.transform.position);
            
        //     // _playerContext.Control.ChangeState(new WalkingState(itemPosition, _target));
            
        //     _previousTarget = _target;

        //     _playerContext.Control.CurrentTarget = _target;

        //     ClearMarker();
        //     return;
        // }

        Vector3Int? cell = AimBehaviour.Instance._lastGridCellPosition;
        
        if (!cell.HasValue) return;

        Vector3Int targetPosition = cell.Value;

        if (!AimBehaviour.Instance.IsWalkable(targetPosition)) return;
        
        _target = null;
        _previousTarget = null;
        _context.Control.CurrentTarget = null;

        // _playerContext.Control.ChangeState(new WalkingState(targetPosition, null));
        _context.Control.SetDestination(targetPosition);
        _context.Control.ChangeState(new WalkingState());

        ClearMarker();
    }

    private bool CheckIfIsItem()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            ItemDataLoader item = hit.collider.GetComponent<ItemDataLoader>();
            if (item == null)
            {
                return false;
            }

            // Vector3Int itemPosition = GridManager.Instance.WorldToCell(item.gameObject.transform.position);
            // _playerContext.Control.ChangeState(new WalkingState(itemPosition));
            // _playerContext.Control.GetItem(item.gameObject);
            // ClearMarker();
            _target = item.gameObject;
            return true;
        }

        // ClearMarker();
        return false;
    }

    private bool CheckIfIsEnemy()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            EnemyCombat enemy = hit.collider.GetComponent<EnemyCombat>();
            if (enemy == null)
            {
                return false;
            }

            _target = enemy.gameObject;
            return true;
        }

        return false;
    }

    public void MarkTarget(GameObject target)
    {
        if (_targetMarkerPrefab == null)
        {
            Debug.LogError("Target marker prefab not assigned");
            return;
        }

        if (target == null) { ClearMarker(); return;}

        ClearMarker();

        _activeTargetMarker = Instantiate(_targetMarkerPrefab);
        var marker = _activeTargetMarker.GetComponent<TargetMarker>();

        if (marker == null)
        {
            Debug.LogError("TargetMarker component missing on prefab");
            return;
        }

        marker.AttachTo(target.transform);
    }

    private void ClearMarker()
    {
        if (_activeTargetMarker != null)
        {
            Destroy(_activeTargetMarker);
            _activeTargetMarker = null;
        }
    }

}
