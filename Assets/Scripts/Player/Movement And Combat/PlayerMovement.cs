using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerMovement : GridMovement
{   
    [SerializeField] private PlayerStatsData _stats;
    private Coroutine _movementCoroutine;

    public void MoveAlongPath(List<Node> path)
    {
        if(_movementCoroutine != null)
            StopCoroutine(_movementCoroutine);

        _movementCoroutine = StartCoroutine(FollowPath(path, _stats._moveSpeed));
    }
}
