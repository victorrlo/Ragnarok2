using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    [SerializeField] private Animator _enemyAnimator;
    [SerializeField] private float _attackAnimationLength = 0.4f;
    [SerializeField] private float _deathAnimationLength = 0.5f;
    [SerializeField] private float _deathLastFrameHoldDuration = 1f;

    private DamageReaction _damageReaction;
    private string _currentAnimation;
    private Direction _currentFacing = Direction.FrontLeft;
    private Vector2 _lastDirection = new Vector2(-1f, -1f);
    private bool _isWalking;
    private bool _specialAnimationPlaying;
    private bool _damageReactionAnimationPlaying;
    private Coroutine _attackRoutine;
    private Coroutine _deathRoutine;

    private void Awake()
    {
        if (_enemyAnimator == null)
            _enemyAnimator = GetComponentInChildren<Animator>();

        if (_enemyAnimator == null)
            Debug.LogError($"No animator found for Enemy {name}!");

        if (_damageReaction == null && !TryGetComponent(out _damageReaction))
            _damageReaction = gameObject.AddComponent<DamageReaction>();
    }

    private void OnEnable()
    {
        if (_damageReaction == null && !TryGetComponent(out _damageReaction))
            _damageReaction = gameObject.AddComponent<DamageReaction>();

        _damageReaction.OnReactionStarted += PlayTakeDamageAnimation;
        _damageReaction.OnReactionFinished += StopTakeDamageAnimation;

        UpdateWalkingOrIdleDirection(_lastDirection);
    }

    private void OnDisable()
    {
        if (_damageReaction != null)
        {
            _damageReaction.OnReactionStarted -= PlayTakeDamageAnimation;
            _damageReaction.OnReactionFinished -= StopTakeDamageAnimation;
        }

        if (_enemyAnimator != null)
            _enemyAnimator.speed = 1f;
    }

    public void SetMovementState(bool isWalking)
    {
        _isWalking = isWalking;
        UpdateWalkingOrIdleDirection(_lastDirection);
    }

    public void FaceDirection(Vector2 dir)
    {
        _lastDirection = dir;
        _currentFacing = DefineFacingDirection(dir);

        if (!_specialAnimationPlaying && !_damageReactionAnimationPlaying)
            UpdateWalkingOrIdleDirection(_lastDirection);
    }

    public void PlayAttackAnimation(Vector2 attackDirection)
    {
        FaceDirection(attackDirection);

        if (_attackRoutine != null)
            StopCoroutine(_attackRoutine);

        _attackRoutine = StartCoroutine(PlayAttackRoutine());
    }

    public float PlayDeathAnimation(Vector2 deathDirection)
    {
        FaceDirection(deathDirection);

        if (_attackRoutine != null)
        {
            StopCoroutine(_attackRoutine);
            _attackRoutine = null;
        }

        if (_deathRoutine != null)
            StopCoroutine(_deathRoutine);

        if (_enemyAnimator != null)
            _enemyAnimator.speed = 1f;

        _specialAnimationPlaying = true;
        _damageReactionAnimationPlaying = false;
        string deathAnimation = ChangeAnimation(GetDeathAnimationNames(_currentFacing), 0f);

        if (!string.IsNullOrEmpty(deathAnimation))
            _deathRoutine = StartCoroutine(FreezeOnLastDeathFrame(deathAnimation));

        return _deathAnimationLength + _deathLastFrameHoldDuration;
    }

    private System.Collections.IEnumerator PlayAttackRoutine()
    {
        _damageReactionAnimationPlaying = false;
        _specialAnimationPlaying = true;
        ChangeAnimation(GetAttackAnimationNames(_currentFacing));

        yield return new WaitForSeconds(_attackAnimationLength);

        _specialAnimationPlaying = false;
        _attackRoutine = null;

        UpdateWalkingOrIdleDirection(_lastDirection);
    }

    private void PlayTakeDamageAnimation()
    {
        if (_specialAnimationPlaying)
            return;

        _damageReactionAnimationPlaying = true;
        ChangeAnimation(GetTakeDamageAnimationNames(_currentFacing));
    }

    private void StopTakeDamageAnimation()
    {
        _damageReactionAnimationPlaying = false;

        if (!_specialAnimationPlaying)
            UpdateWalkingOrIdleDirection(_lastDirection);
    }

    private void UpdateWalkingOrIdleDirection(Vector2 dir)
    {
        _lastDirection = dir;
        _currentFacing = DefineFacingDirection(dir);

        if (_specialAnimationPlaying || _damageReactionAnimationPlaying)
            return;

        ChangeAnimation(GetWalkingOrIdleAnimationNames(_currentFacing));
    }

    private System.Collections.IEnumerator FreezeOnLastDeathFrame(string deathAnimation)
    {
        yield return new WaitForSeconds(_deathAnimationLength);

        if (_enemyAnimator == null)
            yield break;

        _enemyAnimator.Play(deathAnimation, 0, 0.999f);
        _enemyAnimator.Update(0f);
        _enemyAnimator.speed = 0f;
    }

    private string ChangeAnimation(string[] animationCandidates, float crossfade = 0.2f)
    {
        if (_enemyAnimator == null)
            return null;

        foreach (string animation in animationCandidates)
        {
            if (!_enemyAnimator.HasState(0, Animator.StringToHash(animation)))
                continue;

            if (_currentAnimation == animation)
                return animation;

            _currentAnimation = animation;
            _enemyAnimator.CrossFade(animation, crossfade);
            return animation;
        }

        return null;
    }

    private string[] GetWalkingOrIdleAnimationNames(Direction dir)
    {
        string suffix = GetFourDirectionSuffix(dir);
        string playerSuffix = GetPlayerDirectionSuffix(dir);

        if (_isWalking)
        {
            return new[]
            {
                $"walking-{suffix}",
                $"walk-{suffix}",
                $"walk-{playerSuffix}",
                $"idle-{suffix}",
                $"idle-{playerSuffix}"
            };
        }

        return new[]
        {
            $"idle-{suffix}",
            $"idle-{playerSuffix}"
        };
    }

    private string[] GetAttackAnimationNames(Direction dir)
    {
        string suffix = GetFourDirectionSuffix(dir);
        string playerSuffix = GetPlayerAttackDirectionSuffix(dir);

        return new[]
        {
            $"attack-{suffix}",
            $"attack-{playerSuffix}"
        };
    }

    private string[] GetTakeDamageAnimationNames(Direction dir)
    {
        string suffix = GetFourDirectionSuffix(dir);
        string playerSuffix = GetPlayerAttackDirectionSuffix(dir);

        return new[]
        {
            $"hurt-{suffix}",
            $"attacked-{suffix}",
            $"take-damage-{playerSuffix}"
        };
    }

    private string[] GetDeathAnimationNames(Direction dir)
    {
        string suffix = GetFourDirectionSuffix(dir);
        string playerSuffix = GetPlayerAttackDirectionSuffix(dir);

        return new[]
        {
            "death-front-right"
        };
    }

    private string GetFourDirectionSuffix(Direction dir)
    {
        return dir switch
        {
            Direction.BackLeft => "back-left",
            Direction.Left => "front-left",
            Direction.FrontLeft => "front-left",
            Direction.Front => "front-right",
            Direction.FrontRight => "front-right",
            Direction.Right => "front-right",
            Direction.BackRight => "back-right",
            Direction.Back => "back-right",
            _ => "front-left"
        };
    }

    private string GetPlayerDirectionSuffix(Direction dir)
    {
        return dir switch
        {
            Direction.BackRight => "diagonal-back-right",
            Direction.Back => "back",
            Direction.BackLeft => "diagonal-back-left",
            Direction.Left => "side-left",
            Direction.FrontLeft => "diagonal-front-left",
            Direction.Front => "front",
            Direction.FrontRight => "diagonal-front-right",
            _ => "side-right"
        };
    }

    private string GetPlayerAttackDirectionSuffix(Direction dir)
    {
        return dir switch
        {
            Direction.BackLeft => "diagonal-back-left",
            Direction.Left => "diagonal-back-left",
            Direction.Front => "diagonal-front-right",
            Direction.FrontRight => "diagonal-front-right",
            Direction.FrontLeft => "diagonal-front-left",
            Direction.BackRight => "diagonal-back-right",
            Direction.Back => "diagonal-back-right",
            _ => "diagonal-back-right"
        };
    }

    private Direction DefineFacingDirection(Vector2 dir)
    {
        if (dir == Vector2.zero)
            return _currentFacing;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        angle = (angle + 360f) % 360;

        if (angle >= 22.5f && angle < 67.5f) return Direction.BackRight;
        if (angle >= 67.5f && angle < 112.5f) return Direction.Back;
        if (angle >= 112.5f && angle < 157.5f) return Direction.BackLeft;
        if (angle >= 157.5f && angle < 202.5f) return Direction.Left;
        if (angle >= 202.5f && angle < 247.5f) return Direction.FrontLeft;
        if (angle >= 247.5f && angle < 292.5f) return Direction.Front;
        if (angle >= 292.5f && angle < 337.5f) return Direction.FrontRight;

        return Direction.Right;
    }
}
