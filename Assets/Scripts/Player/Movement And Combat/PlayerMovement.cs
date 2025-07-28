using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerMovement : GridMovement
{   
    private PlayerContext _playerContext;
    private Coroutine _movementCoroutine;
    public event Action<Vector3Int> OnPlayerMoved;
    private List<Node> _currentPath;

    protected override void Awake()
    {
        if (_playerContext == null)
            TryGetComponent<PlayerContext>(out _playerContext);
    }

    public void MoveAlongPath(List<Node> path)
    {
        _currentPath = path;
        if(_movementCoroutine != null)
            StopCoroutine(_movementCoroutine);

        _movementCoroutine = StartCoroutine(FollowPath(GetPath, _playerContext.Stats.MoveSpeed));
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
