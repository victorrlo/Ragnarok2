using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private AimBehaviour _aimBehaviour;
    [SerializeField] private Grid _grid;

    private void Start()
    {
        _aimBehaviour = FindObjectOfType<AimBehaviour>();
        _grid = FindObjectOfType<Grid>();
    }

    public void OnMouseClick(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (_aimBehaviour.LastGridCellPosition.HasValue)
        {
            transform.position = _grid.GetCellCenterWorld(_aimBehaviour.LastGridCellPosition.Value);
            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
        }
    }
}
