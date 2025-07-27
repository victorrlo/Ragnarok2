using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerMovement : GridMovement
{   
    [SerializeField] private PlayerStatsData _stats;
    private Coroutine _movementCoroutine;
    public event Action<Vector3Int> OnPlayerMoved;
    private List<Node> _currentPath;

    public void MoveAlongPath(List<Node> path)
    {
        _currentPath = path;
        if(_movementCoroutine != null)
            StopCoroutine(_movementCoroutine);

        _movementCoroutine = StartCoroutine(FollowPath(GetPath, _stats._moveSpeed));
    }

    protected override void OnStep(Vector3Int newPos) 
    {
        OnPlayerMoved?.Invoke(newPos);
    }

    private List<Node> GetPath()
    {
        return _currentPath;
    }
}
