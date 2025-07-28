using System.Collections.Generic;
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

        _playerContext.Combat.TryClickToAttack();

        Vector3Int? cell = AimBehaviour.Instance._lastGridCellPosition;
        if (!cell.HasValue) return;

        Vector3Int playerPos = GridManager.Instance.WorldToCell(transform.position);

        // if (!GridOccupancyManager.Instance.TryGetOccupant(cell.Value, out GameObject occupant)
            // || !occupant.CompareTag("Enemy"))
                // _playerCombat.StopCombat();
        
        if (!AimBehaviour.Instance.IsWalkable(cell.Value)) return;

        List<Node> path = NodeManager.Instance.FindPath(playerPos, cell.Value);

        if (path != null && path.Count > 0)
            _playerContext.Movement.MoveAlongPath(path);

        

        // if (GridOccupancyManager.Instance.TryGetOccupant(cell.Value, out occupant)
            // && occupant.CompareTag("Enemy"))
        {
            // _playerCombat.StartCombat(occupant);
            // return;
        }
    }

}
