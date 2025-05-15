using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    public static PlayerInputHandler Instance {get; private set;}
    [SerializeField] public PlayerMovement _playerMovement;
    [SerializeField] public PlayerCombat _playerCombat;

    public void OnMouseClick(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        _playerCombat.TryClickToAttack();

        Vector3Int? cell = AimBehaviour.Instance._lastGridCellPosition;
        if (!cell.HasValue) return;

        Vector3Int playerPos = GridManager.Instance.WorldToCell(transform.position);

        // if (!GridOccupancyManager.Instance.TryGetOccupant(cell.Value, out GameObject occupant)
            // || !occupant.CompareTag("Enemy"))
                // _playerCombat.StopCombat();
        
        if (!AimBehaviour.Instance.IsWalkable(cell.Value)) return;

        List<Node> path = NodeManager.Instance.FindPath(playerPos, cell.Value);

        if (path != null && path.Count > 0)
            _playerMovement.MoveAlongPath(path);

        

        // if (GridOccupancyManager.Instance.TryGetOccupant(cell.Value, out occupant)
            // && occupant.CompareTag("Enemy"))
        {
            // _playerCombat.StartCombat(occupant);
            // return;
        }
    }

}
