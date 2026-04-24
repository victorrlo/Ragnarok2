using System;
using UnityEngine;

public class PlayerEventBus : MonoBehaviour
{
    public Action<Vector2> OnPlayerMoveDirectionChanged;
    public Action<bool> OnPlayerMovementStateChanged;

    public Action OnSpecialAnimationFinished;
    public Action OnPlayerAttackTriggered;
    public Action<Vector2, Skill> OnPlayerCastingStarted;
    public Action<Vector2> OnPlayerDeath;
    public Action OnPlayerPickUp;
    public Action OnPlayerFinishedPickingUp;
}
