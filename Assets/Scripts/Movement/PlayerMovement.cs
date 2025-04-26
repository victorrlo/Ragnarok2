using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerMovement : GridMovement
{   
    [SerializeField] private AimBehaviour _aim;
    [SerializeField] private NodeManager _nodeManager;
    private Vector3Int? _startPos;
    private Vector3Int? _finalPos;
    public Vector3Int? StartPos => _startPos;
    public Vector3Int? FinalPos => _finalPos;
    [SerializeField] private float _moveSpeed = 3f;
    private Coroutine _movementCoroutine;

    public void OnMouseClick(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        _startPos = _grid.WorldToCell(transform.position);
        _finalPos = _aim._lastGridCellPosition;
        
        if (!_finalPos.HasValue || !_aim.IsWalkable(_finalPos.Value))
        {
            return; // not walkable
        }

        List<Node> path = _nodeManager.FindPath(_startPos.Value, _finalPos.Value);
        // _nodeManager.DrawPath(path); to see path in inspector

        if (path == null || path.Count == 0) // couldn't find a path
        {
            return;
        }
        
        if (_movementCoroutine != null) // if already walking, stop to initiate another movement
        {
            StopCoroutine(_movementCoroutine);
        }

        _movementCoroutine = StartCoroutine(FollowPath(path, _moveSpeed)); // uses gridMovement script to walk
    }

    public void OnSpeedBoost(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        Debug.Log("Aumentar AGILIDADE!");
        StartCoroutine(SpeedBoost());
    }

    private IEnumerator SpeedBoost()
    {
        float originalSpeed = _moveSpeed;
        _moveSpeed *= 2f;

        yield return new WaitForSeconds(5f);

        _moveSpeed = originalSpeed;
    }
}
