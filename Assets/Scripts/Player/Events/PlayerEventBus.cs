using System;
using UnityEngine;

public class PlayerEventBus : MonoBehaviour
{
    public Action<Vector2> OnPlayerMoveDirectionChanged;
    public Action<bool> OnPlayerMovementStateChanged;
    public Action<Vector2> OnPlayerStopped;

    public Action<Transform> OnPlayerFaceTarget;
}
