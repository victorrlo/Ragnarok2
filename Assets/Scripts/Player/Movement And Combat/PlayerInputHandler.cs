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

        TryClickToAttack();

        Vector3Int? cell = AimBehaviour.Instance._lastGridCellPosition;
        
        if (!cell.HasValue) return;

        Vector3Int targetPosition = cell.Value;

        if (!AimBehaviour.Instance.IsWalkable(targetPosition)) return;

        _playerContext.Movement.WalkToEmptyTile(targetPosition);
    }

    public void TryClickToAttack()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            EnemyCombat enemy = hit.collider.GetComponent<EnemyCombat>();
            if (enemy != null)
                _playerContext.Combat.StartCombat(enemy.gameObject);
            else
                _playerContext.Combat.StopCombat();
        }
    }

}
