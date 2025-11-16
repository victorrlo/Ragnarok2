using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public static PlayerInputHandler Instance {get; private set;}
    private PlayerContext _context;
    [SerializeField] private GameObject _targetMarkerPrefab;
    private GameObject _activeTargetMarker;
    private GameObject _target;

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

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        ClickOnEmptyCell();

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            EnemyCombat enemy = hit.collider.GetComponent<EnemyCombat>();
            
            if (enemy != null)
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
    }

    private void ClickOnItem()
    {
        Debug.Log("ClickOnItem");
        _context.Control.SetCurrentTarget(_target);

        Vector3Int player = GridManager.Instance.WorldToCell(this.transform.position);
        Vector3Int item = GridManager.Instance.WorldToCell(_target.transform.position);

        MarkTarget(Color.yellow);

        _context.Control.ChangeState(new WalkingState());

        if (!DistanceHelper.IsInAttackRange(player, item, 1))
            return;

        _context.Control.ChangeState(new PickingItemState());
    }

    private void ClickOnEnemy()
    {
        Debug.Log("ClickOnEnemy");
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
        _context.Control.ClearCurrentTarget();
        _context.Control.ChangeState(new WalkingState());

        ClearMarker();
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
        Debug.Log("marked target!");
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
