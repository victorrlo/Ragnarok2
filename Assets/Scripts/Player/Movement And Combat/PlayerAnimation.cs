using System.Collections;
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

    private float ATTACK_ANIMATION_LENGTH = 0.4f; // in miliseconds!
    private float PICKUP_ANIMATION_LENGTH = 0.45f;



    private PlayerContext _context;
    [SerializeField] private Animator _playerAnimator;
    private string _currentAnimation;
    private Direction _currentFacing = Direction.Front;
    private bool _isWalking = false;
    private Vector2 _lastDirection;
    public bool SpecialAnimationPlaying {get; private set;} = false;
    private Coroutine _attackRoutine;
    private Coroutine _pickUpRoutine;



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
        _context.EventBus.OnPlayerMoveDirectionChanged          += UpdateWalkingOrIdleDirection;
        _context.EventBus.OnPlayerMovementStateChanged          += SetMovementState;
        _context.EventBus.OnPlayerAttackTriggered               += PlayAttackAnimation;
        _context.EventBus.OnPlayerPickUp                        += PlayPickUpAnimation;
    }

    private void OnDisable()
    {
        _context.EventBus.OnPlayerMoveDirectionChanged          -= UpdateWalkingOrIdleDirection;
        _context.EventBus.OnPlayerMovementStateChanged          -= SetMovementState;
        _context.EventBus.OnPlayerAttackTriggered               -= PlayAttackAnimation;
        _context.EventBus.OnPlayerPickUp                        -= PlayPickUpAnimation;
    }

    public void FaceDirection(Vector2 dir)
    {
        _lastDirection = dir;
        _currentFacing = DefineFacingDirection(dir);
    }

    private void PlayPickUpAnimation()
    {
        if (_pickUpRoutine != null)
            StopCoroutine(_pickUpRoutine);

        _pickUpRoutine = StartCoroutine(PlayPickUpRoutine());
    }

    private IEnumerator PlayPickUpRoutine()
    {
        SpecialAnimationPlaying = true;
        ChangeAnimation(GetPickUpItemAnimationString(_currentFacing));

        yield return new WaitForSeconds(PICKUP_ANIMATION_LENGTH);

        _context.EventBus.OnPlayerFinishedPickingUp?.Invoke();

        SpecialAnimationPlaying = false;
        _pickUpRoutine = null;

        UpdateWalkingOrIdleDirection(_lastDirection);        
    }

    private void PlayAttackAnimation()
    {

        if (_attackRoutine != null)
            StopCoroutine(_attackRoutine);

        _attackRoutine = StartCoroutine(PlayAttackRoutine());
    }

    private IEnumerator PlayAttackRoutine()
    {
        SpecialAnimationPlaying = true;
        ChangeAnimation(GetAttackAnimationString(_currentFacing));
        
        yield return new WaitForSeconds(ATTACK_ANIMATION_LENGTH);

        _context.EventBus.OnPlayerAttackHit?.Invoke();
        _context.EventBus.OnSpecialAnimationFinished?.Invoke();

        SpecialAnimationPlaying = false;
        _attackRoutine = null;

        UpdateWalkingOrIdleDirection(_lastDirection);
    }

    private void SetMovementState(bool isWalking)
    {
        _isWalking = isWalking;

        UpdateWalkingOrIdleDirection(_lastDirection);
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

    private void UpdateWalkingOrIdleDirection(Vector2 dir)
    {
        _lastDirection = dir;
        
        _currentFacing = DefineFacingDirection(dir);

        if (SpecialAnimationPlaying)
            return;

        var animationString = GetWalkingOrIdleAnimationString(_currentFacing);

        ChangeAnimation(animationString);
    }

    private string GetTakeDamageAnimationString(Direction dir)
    {
        string prefix = "take-damage";

        return dir switch
        {
            Direction.BackLeft      =>$"{prefix}-diagonal-back-left",
            Direction.Left          =>$"{prefix}-diagonal-back-left",

            Direction.Front         =>$"{prefix}-diagonal-front-right",
            Direction.FrontRight    =>$"{prefix}-diagonal-front-right",
            
            Direction.FrontLeft     =>$"{prefix}-diagonal-front-left",

            Direction.BackRight     =>$"{prefix}-diagonal-back-right",
            Direction.Back          =>$"{prefix}-diagonal-back-right",
            _                       =>$"{prefix}-diagonal-back-right"
        };
    }

    private string GetPickUpItemAnimationString(Direction dir)
    {
        string prefix = "pick";

        return dir switch
        {
            Direction.BackLeft      =>$"{prefix}-diagonal-back-left",
            Direction.Left          =>$"{prefix}-diagonal-back-left",

            Direction.Front         =>$"{prefix}-diagonal-front-right",
            Direction.FrontRight    =>$"{prefix}-diagonal-front-right",
            
            Direction.FrontLeft     =>$"{prefix}-diagonal-front-left",

            Direction.BackRight     =>$"{prefix}-diagonal-back-right",
            Direction.Back          =>$"{prefix}-diagonal-back-right",
            _                       =>$"{prefix}-diagonal-back-right"
        };
    }

    private string GetWalkingOrIdleAnimationString(Direction dir)
    {
        string prefix = _isWalking ? "walk" : "idle";

        return dir switch
        {
            Direction.BackRight     => $"{prefix}-diagonal-back-right",
            Direction.Back          => $"{prefix}-back",
            Direction.BackLeft      => $"{prefix}-diagonal-back-left",
            Direction.Left          => $"{prefix}-side-left",
            Direction.FrontLeft     => $"{prefix}-diagonal-front-left",
            Direction.Front         => $"{prefix}-front",
            Direction.FrontRight    => $"{prefix}-diagonal-front-right",
            _                       => $"{prefix}-side-right"
        };
    }

    private string GetAttackAnimationString(Direction dir)
    {
        string prefix = "attack";

        return dir switch
        {
            Direction.BackLeft      =>$"{prefix}-diagonal-back-left",
            Direction.Left          =>$"{prefix}-diagonal-back-left",

            Direction.Front         =>$"{prefix}-diagonal-front-right",
            Direction.FrontRight    =>$"{prefix}-diagonal-front-right",
            
            Direction.FrontLeft     =>$"{prefix}-diagonal-front-left",

            Direction.BackRight     =>$"{prefix}-diagonal-back-right",
            Direction.Back          =>$"{prefix}-diagonal-back-right",
            _                       =>$"{prefix}-diagonal-back-right"
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
