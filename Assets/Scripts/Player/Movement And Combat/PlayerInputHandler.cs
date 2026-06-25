using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public static PlayerInputHandler Instance {get; private set;}
    private PlayerContext _context;
    [SerializeField] private GameObject _targetMarkerPrefab;
    private GameObject _activeTargetMarker;
    private GameObject _target;

#region PendingAction
    private GameObject _pendingTarget;
    private Vector3Int? _pendingDestination;
    private bool _hasPendingAction;
#endregion

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

    private void OnEnable()
    {
        if (_context.Animation != null)
            _context.EventBus.OnSpecialAnimationFinished += ExecutePendingAction;
    }

    private void OnDisable()
    {
        if (_context.Animation != null)
            _context.EventBus.OnSpecialAnimationFinished -= ExecutePendingAction;
    }

    public void OnMouseClick(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (_context.Animation.SpecialAnimationPlaying)
        {
            StorePendingAction();
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        ClickOnEmptyCell();

        if (Physics.Raycast(ray, out RaycastHit hit))
        {

            if (hit.collider.TryGetComponent<EnemyCombat>(out EnemyCombat enemy))
            {
                _target = enemy.gameObject;
                ClickOnEnemy();
                return;
            }

            ItemDataLoader item = hit.collider.GetComponent<ItemDataLoader>();
            if (item != null)
            {
                _target = item.gameObject;
                ClickOnItem();
                return;
            }
        }

        _context.Control.ClearCurrentTarget();
        _context.Control.ChangeState(new WalkingState());
        ClearMarker();
    }

    private void StorePendingAction()
    {
        ClearPendingAction();

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.TryGetComponent<EnemyCombat>(out _))
            {
                _pendingTarget = hit.collider.gameObject;
            }
            else if (hit.collider.TryGetComponent<ItemDataLoader>(out _))
            {
                _pendingTarget = hit.collider.gameObject;
            }
            else
            {
                _pendingDestination = AimBehaviour.Instance._lastGridCellPosition;
            }
        }
        else
        {
            _pendingDestination = AimBehaviour.Instance._lastGridCellPosition;
        }

        _hasPendingAction = true;
    }

    private void ExecutePendingAction()
    {
        if (!_hasPendingAction) return;
        
        _hasPendingAction = false;

        if (_pendingTarget != null)
        {
            _target = _pendingTarget;
            _pendingTarget = null;

            if (_target.TryGetComponent<EnemyCombat>(out _))
                ClickOnEnemy();
            if (_target.TryGetComponent<ItemDataLoader>(out _))
                ClickOnItem();
        }
        else if (_pendingDestination.HasValue)
        {
            Vector3Int destination = _pendingDestination.Value;
            _pendingDestination = null;

            if (AimBehaviour.Instance.IsWalkable(destination))
            {
                _context.Control.SetDestination(destination);
                _context.Control.ClearCurrentTarget();
                _context.Control.ChangeState(new WalkingState());
                ClearMarker();
            }
        }
    }

    private void ClearPendingAction()
    {
        _pendingTarget = null;
        _pendingDestination = null;
        _hasPendingAction = false;
    }

    private void ClickOnItem()
    {
        // Debug.Log("ClickOnItem");
        _context.Control.SetCurrentTarget(_target);

        Vector3Int player = GridManager.Instance.WorldToCell(this.transform.position);
        Vector3Int item = GridManager.Instance.WorldToCell(_target.transform.position);

        if (player == item)
        {
            _context.Control.ChangeState(new PickingItemState());
            return;
        }

        _context.Control.ChangeState(new WalkingState());
    }

    private void ClickOnEnemy()
    {
        // Debug.Log("ClickOnEnemy");

        if (_context.Control.CurrentTarget == _target) 
        {
            Debug.LogError("Same enemy, should not do anything...");
            return;
        }

        _context.Control.SetCurrentTarget(_target);

        Vector3Int player = GridManager.Instance.WorldToCell(this.transform.position);
        Vector3Int enemy = GridManager.Instance.WorldToCell(_target.transform.position);
        int playerAttackRange = _context.Stats.AttackRange;

        MarkTarget(Color.white);

        _context.Control.ChangeState(new WalkingState());
        
        if (!DistanceHelper.IsInAttackRange(player, enemy, playerAttackRange))
            return;

        _context.Control.ChangeState(new AttackingState());
    }

    private void ClickOnEmptyCell()
    {
        Vector3Int? cell = AimBehaviour.Instance._lastGridCellPosition;
        
        if (!cell.HasValue) return;

        Vector3Int targetPosition = cell.Value;

        if (!AimBehaviour.Instance.IsWalkable(targetPosition)) return;
        
        _target = null;

        _context.Control.SetDestination(targetPosition);
    }

    public void MarkTarget(Color color)
    {
        if (_targetMarkerPrefab == null)
        {
            Debug.LogError("Target marker prefab not assigned");
            return;
        }

        if (_target == null) { ClearMarker(); return;}

        ClearMarker();

        _targetMarkerPrefab.GetComponent<SpriteRenderer>().color = color;
        _activeTargetMarker = Instantiate(_targetMarkerPrefab);
        var marker = _activeTargetMarker.GetComponent<TargetMarker>();

        if (marker == null)
        {
            Debug.LogError("TargetMarker component missing on prefab");
            return;
        }

        marker.AttachTo(_target.transform);
        // Debug.Log("marked target!");
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
