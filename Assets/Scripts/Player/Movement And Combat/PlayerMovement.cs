using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerMovement : GridMovement
{   
    private Vector3Int? _startPos;
    private Vector3Int? _finalPos;
    private float _moveSpeed = 2f;
    
    private bool _isChasing;
    private Coroutine _movementCoroutine;
    private GameObject _currentTarget;

    public void MoveAlongPath(List<Node> path)
    {
        if(_movementCoroutine != null)
            StopCoroutine(_movementCoroutine);

        _movementCoroutine = StartCoroutine(FollowPath(path, _moveSpeed));
    }

    // public void OnSpeedBoost(InputAction.CallbackContext context)
    // {
    //     if (!context.performed) return;
    //     Debug.Log("Aumentar AGILIDADE!");
    //     StartCoroutine(SpeedBoost());
    // }

    // private IEnumerator SpeedBoost()
    // {
    //     float originalSpeed = _moveSpeed;
    //     _moveSpeed = 6f;

    //     yield return new WaitForSeconds(5f);

    //     _moveSpeed = originalSpeed;
    // }
}
