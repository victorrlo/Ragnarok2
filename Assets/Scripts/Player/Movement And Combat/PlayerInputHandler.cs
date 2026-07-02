using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public static PlayerInputHandler Instance {get; private set;}
    private PlayerContext _context;
    [SerializeField] private GameObject _targetMarkerPrefab;
    private GameObject _activeTargetMarker;
    private GameObject _target;
    private readonly RaycastHit[] _mouseRaycastHits = new RaycastHit[64];

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

        ClickOnEmptyCell();

        if (TryGetMouseInteractable(out GameObject target, out MouseInteractableType targetType))
        {
            if (targetType == MouseInteractableType.Enemy)
            {
                _target = target;
                ClickOnEnemy();
                return;
            }

            if (targetType == MouseInteractableType.Item)
            {
                _target = target;
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

        if (TryGetMouseInteractable(out GameObject target, out MouseInteractableType targetType))
        {
            if (targetType == MouseInteractableType.Enemy || targetType == MouseInteractableType.Item)
            {
                _pendingTarget = target;
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

    private bool TryGetMouseInteractable(out GameObject target, out MouseInteractableType targetType)
    {
        target = null;
        targetType = MouseInteractableType.None;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int hitCount = Physics.RaycastNonAlloc(ray, _mouseRaycastHits);

        float closestDistance = float.MaxValue;

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit hit = _mouseRaycastHits[i];
            MouseInteractableType hitType = GetInteractableType(hit.collider, out GameObject hitTarget);

            if (hitType == MouseInteractableType.None || hit.distance >= closestDistance)
                continue;

            target = hitTarget;
            targetType = hitType;
            closestDistance = hit.distance;
        }

        return targetType != MouseInteractableType.None;
    }

    private MouseInteractableType GetInteractableType(Collider collider, out GameObject target)
    {
        target = null;

        if (collider.TryGetComponent<EnemyCombat>(out EnemyCombat enemy))
        {
            target = enemy.gameObject;
            return MouseInteractableType.Enemy;
        }

        if (collider.TryGetComponent<ItemDataLoader>(out ItemDataLoader item))
        {
            target = item.gameObject;
            return MouseInteractableType.Item;
        }

        return MouseInteractableType.None;
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

    public void ClearTargetIfMatching(GameObject target)
    {
        if (target == null)
            return;

        if (_target == target)
            _target = null;

        if (_pendingTarget == target)
            ClearPendingAction();

        if (_context.Control.CurrentTarget == target)
        {
            _context.Control.ClearCurrentTarget();
            _context.Control.ChangeState(new IdleState());
        }

        ClearMarker();
    }

    private void ClearMarker()
    {
        if (_activeTargetMarker != null)
        {
            Destroy(_activeTargetMarker);
            _activeTargetMarker = null;
        }
    }

    private enum MouseInteractableType
    {
        None,
        Enemy,
        Item
    }

}
