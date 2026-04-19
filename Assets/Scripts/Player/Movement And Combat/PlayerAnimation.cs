using UnityEngine;

public enum Direction
{
    Front,      // 0
    Back,       // 1
    Left,       // 2
    Right,      // 3
    FrontLeft,  // 4
    FrontRight, // 5
    BackLeft,   // 6
    BackRight   // 7   
}

public class PlayerAnimation : MonoBehaviour
{



    private PlayerContext _context;
    [SerializeField] private Animator _playerAnimator;
    private string _currentAnimation;
    private Direction _currentFacing = Direction.Front;
    private bool _isWalking = false;
    private Vector2 _lastDirection;



    private void Awake()
    {
        if (_playerAnimator == null)
        {
            Debug.LogError("No animator found for Player!");
            return;
        }    

        if (_context == null)
            TryGetComponent<PlayerContext>(out _context);
    }

    private void OnEnable()
    {
        _context.EventBus.OnPlayerMoveDirectionChanged += UpdateDirection;
        _context.EventBus.OnPlayerMovementStateChanged += SetMovementState;
    }

    private void OnDisable()
    {
        _context.EventBus.OnPlayerMoveDirectionChanged -= UpdateDirection;
        _context.EventBus.OnPlayerMovementStateChanged -= SetMovementState;
    }

    private void SetMovementState(bool isWalking)
    {
        _isWalking = isWalking;

        UpdateDirection(_lastDirection);
    }

    private void ChangeAnimation(string animation, float crossfade = 0.2f)
    {
        if(_playerAnimator != null)
        {
            if (_currentAnimation != animation)
            {
                _currentAnimation = animation;
                _playerAnimator.CrossFade(animation, crossfade);
            }
        }
    }

    private void UpdateDirection(Vector2 dir)
    {
        _lastDirection = dir;
        
        _currentFacing = DefineFacingDirection(dir);
        
        Debug.LogWarning($"Current Facing Direction: {_currentFacing}");

        var animationString = GetAnimationString(_currentFacing);

        ChangeAnimation(animationString);
    }

    private string GetAnimationString(Direction dir)
    {
        string prefix = _isWalking ? "walk" : "idle";

        return dir switch
        {
            Direction.BackRight => $"{prefix}-diagonal-back-right",
            Direction.Back      => $"{prefix}-back",
            Direction.BackLeft  => $"{prefix}-diagonal-back-left",
            Direction.Left      => $"{prefix}-side-left",
            Direction.FrontLeft => $"{prefix}-diagonal-front-left",
            Direction.Front     => $"{prefix}-front",
            Direction.FrontRight=> $"{prefix}-diagonal-front-right",
            _                   => $"{prefix}-side-right"
        };
    }

    private Direction DefineFacingDirection(Vector2 dir)
    {
        if (dir == Vector2.zero) return _currentFacing;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        //Atan2 gives the angle from +x axis: 0º = right, while 90 is up, -90 is down, 180 is left

        //Normalize to 0..360
        angle = (angle + 360f) % 360;

        //22.5 offset for proper sector mapping

        if (angle >= 22.5f && angle < 67.5f) return Direction.BackRight;    // up-right
        else if (angle >= 67.5f && angle < 112.5f) return Direction.Back;   // up
        else if (angle >= 112.5f && angle < 157.5f) return Direction.BackLeft;
        else if (angle >= 157.5f && angle < 202.5f) return Direction.Left;
        else if (angle >= 202.5f && angle < 247.5f) return Direction.FrontLeft;
        else if (angle >= 247.5f && angle < 292.5f) return Direction.Front;
        else if (angle >= 292.5f && angle < 337.5f) return Direction.FrontRight;
        else 
            return Direction.Right;
    }


}
