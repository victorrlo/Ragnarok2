using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
[RequireComponent(typeof(PlayerContext))]
public class PlayerControl : MonoBehaviour
{
    private PlayerContext _context;
    private PlayerEventBus _eventBus;
    private DamageReaction _damageReaction;
    private IPlayerState _currentState = new IdleState();
    public bool _blockStateChange = false;

    public GameObject CurrentTarget {get; set;}
    public Vector3Int? CurrentDestination {get; set;}
    public Skill CurrentSkill {get; set;}

    private void Awake()
    {
        if (_context == null)
            TryGetComponent<PlayerContext>(out _context);

        if (_eventBus == null)
            _context.TryGetComponent<PlayerEventBus>(out _eventBus);

        if (_damageReaction == null && !TryGetComponent(out _damageReaction))
            _damageReaction = gameObject.AddComponent<DamageReaction>();

        _currentState = null;
    }

    private void OnEnable()
    {
        if (_damageReaction == null && !TryGetComponent(out _damageReaction))
            _damageReaction = gameObject.AddComponent<DamageReaction>();

        _damageReaction.OnReactionStarted += StopMovementForDamageReaction;
    }

    private void OnDisable()
    {
        if (_damageReaction != null)
            _damageReaction.OnReactionStarted -= StopMovementForDamageReaction;
    }

    private void Start()
    {
        ChangeState(new IdleState());    
    }

    private void Update()
    {
        _currentState?.Execute();
    }

    public void ChangeState(IPlayerState newState)
    {
        if (_blockStateChange) return;

        if (newState is WalkingState && _damageReaction != null && _damageReaction.BlocksMovement)
            return;
        
        // Debug.Log($"[Player Control] change player state to {newState}!");

        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter(gameObject);
    }

    public void Casting(Skill skill)
    {
        CurrentSkill = skill;
    }

    public void ClearSkill()
    {
        CurrentSkill = null;
    }

    public IPlayerState GetCurrentState()
    {
        return _currentState;
    }

    public void SetCurrentTarget(GameObject target )
    {
        CurrentTarget = target;
    }

    public void ClearCurrentTarget()
    {
        CurrentTarget = null;
    }

    public void SetDestination(Vector3Int position)
    {
        CurrentDestination = position;
    }

    public void ClearDestination()
    {
        CurrentDestination = null;
    }

    private void StopMovementForDamageReaction()
    {
        if (_currentState is WalkingState)
            ChangeState(new IdleState());
    }
}

public interface IPlayerState
{
    void Enter(GameObject player);
    void Execute();
    void Exit();
}

public class IdleState : IPlayerState
{
    private GameObject _player;
    private PlayerControl _playerControl;
    private PlayerContext _playerContext;
    private PlayerAnimation _playerAnimation;
    

    public void Enter(GameObject player)
    {
        _player = player;

        _playerContext = _player.GetComponent<PlayerContext>();
        _playerControl = _player.GetComponent<PlayerControl>();
        _playerAnimation = _player.GetComponent<PlayerAnimation>();
        
        _playerControl.ClearCurrentTarget();
        _playerControl.ClearDestination();
    }

    public void Execute()
    {}

    public void Exit()
    {}   
}

public class WalkingState : IPlayerState
{
    private GameObject _player;
    private PlayerControl _control;
    private PlayerContext _context;
    private Vector3Int? _destination;
    private GameObject _target;
    private List<Node> _path;
    private int _index;
    private Vector3 _nextNodePosition;
    private bool _isMoving = false;
    private float _moveSpeed;

    public void Enter(GameObject player)
    {
        _player = player;

        _control = _player.GetComponent<PlayerControl>();
        _context = _player.GetComponent<PlayerContext>();

        _moveSpeed = _context.Stats.MoveSpeed;

        _destination = _control.CurrentDestination;
        _target = _control.CurrentTarget;

        if (_target != null)
        {
            Vector3Int playerCell = GridManager.Instance.WorldToCell(_player.transform.position);
            Vector3Int targetCell = GridManager.Instance.WorldToCell(_target.transform.position);

            if (_target.TryGetComponent<EnemyCombat>(out EnemyCombat enemy) &&
                DistanceHelper.IsInAttackRange(playerCell, targetCell, _context.Stats.AttackRange))
            {
                _control.ChangeState(new AttackingState());
                return;
            }
        }

        Vector3Int playerPosition = GridManager.Instance.WorldToCell(_player.transform.position);

        if (_destination == null)
        {
            _player.transform.position = Vector3.MoveTowards
            (
                _player.transform.position,
                _nextNodePosition,
                _moveSpeed * Time.deltaTime
            );

            _control.ChangeState(new IdleState());
            return;
        }
        
        _path = NodeManager.Instance.FindPath(playerPosition, (Vector3Int) _destination);

        if (_path == null || _path.Count == 0)
        {
            _player.transform.position = Vector3.MoveTowards
            (
                _player.transform.position,
                _nextNodePosition,
                _moveSpeed * Time.deltaTime
            );

            _control.ChangeState(new IdleState());
            return;
        }

        _context.EventBus.OnPlayerMovementStateChanged(true);

        _index = 0;
        _isMoving = true;
        SetNextTargetCell();
    }

    public void Execute()
    {
        if (!_isMoving) return;

        _player.transform.position = Vector3.MoveTowards
        (
            _player.transform.position,
            _nextNodePosition,
            _moveSpeed * Time.deltaTime
        );

        Vector3 moveDirection3D = (_nextNodePosition - _player.transform.position).normalized;

        Debug.LogWarning($"moveDirection3D {moveDirection3D}");

        Vector2 moveDirection = new Vector2(moveDirection3D.x, moveDirection3D.z);

        // Debug.LogWarning($"moveDirection {moveDirection}");

        if (moveDirection != Vector2.zero)
        {
            _context.EventBus.OnPlayerMoveDirectionChanged?.Invoke(moveDirection);
        }

        if (Vector3.Distance(_player.transform.position, _nextNodePosition) < 0.1f)
        {
            // snap to grid
            _player.transform.position = _nextNodePosition;

            // move index to next cell
            _index++;

            // if targetting a monster...
            if (_target != null && _target.tag.Equals("Enemy"))
            {
                Vector3Int player = GridManager.Instance.WorldToCell(_player.transform.position);
                Vector3Int enemy = GridManager.Instance.WorldToCell(_target.transform.position);

                // reached monster
                if (DistanceHelper.IsInAttackRange(player, enemy, _context.Stats.AttackRange))
                {
                    _isMoving = false;
                    _context.EventBus.OnPlayerMovementStateChanged(false);
                    _player.transform.position = Vector3.MoveTowards
                    (
                        _player.transform.position,
                        _nextNodePosition,
                        _moveSpeed * Time.deltaTime
                    );

                    _control.ChangeState(new AttackingState());
                    return;
                }
            }

            // reached destination
            if (_index >= _path.Count)
            {
                // if targetting an item...
                if (_target != null && _target.tag.Equals("Item"))
                {
                    _isMoving = false;
                    _context.EventBus.OnPlayerMovementStateChanged(false);

                    _player.transform.position = Vector3.MoveTowards
                    (
                        _player.transform.position,
                        _nextNodePosition,
                        _moveSpeed * Time.deltaTime
                    );

                    _control.ChangeState(new PickingItemState());
                    return;
                }
                
                _isMoving = false;
                _context.EventBus.OnPlayerMovementStateChanged(false);

                _player.transform.position = Vector3.MoveTowards
                (
                    _player.transform.position,
                    _nextNodePosition,
                    _moveSpeed * Time.deltaTime
                );

                _control.ChangeState(new IdleState());
                return;
            }

            SetNextTargetCell();
        }
    }

    public void Exit()
    {
        _isMoving = false;
        _context.EventBus.OnPlayerMovementStateChanged(false);
    }

    private void SetNextTargetCell()
    {
        // general walking
        if (_index < _path.Count)
        {
            Node nextNode = _path[_index];
            _nextNodePosition = GridManager.Instance.GetCellCenterWorld(nextNode._gridPosition);
            OnStep(nextNode._gridPosition);
        }
    }

    private void OnStep(Vector3Int newCell)
    {
        // add step-related logic
        // play footstep sound, play animation etc
    }
}

public class AttackingState : IPlayerState
{
    private GameObject _player;
    private PlayerControl _control;
    private PlayerContext _context;
    private GameObject _enemy;
    private bool _isAttacking;
    private float _lastAttackTime;
    
    public void Enter(GameObject player)
    {
        _player = player;
        _context = player.GetComponent<PlayerContext>();
        _control = player.GetComponent<PlayerControl>();
        _enemy = _control.CurrentTarget;
        _lastAttackTime = 0f;
        _isAttacking = true;
    }

    public void Execute()
    {
        if (!_isAttacking) return;

        if (_enemy == null || _control.CurrentTarget == null || !_control.CurrentTarget.CompareTag("Enemy"))
        {
            _control.ChangeState(new IdleState());
            _isAttacking = false;
            return;
        }

        Vector3 toEnemy = _enemy.transform.position - _player.transform.position;
        Vector2 attackDir = new Vector2(toEnemy.x, toEnemy.z);

        _context.Animation.FaceDirection(attackDir);

        // if monster is out of range, starts walking to it
        Vector3Int player = GridManager.Instance.WorldToCell(_player.transform.position);
        Vector3Int enemy = GridManager.Instance.WorldToCell(_enemy.transform.position);

        if (player == enemy)
        {
            if (Time.time - _lastAttackTime >= _context.Stats.AttackSpeed)
            {
                Attack();
                return;
            }
        }

        if (!DistanceHelper.IsInAttackRange(player, enemy, _context.Stats.AttackRange))
        {
            Debug.Log("Enemy out of range - start chasing...");
            _control.ChangeState(new WalkingState());
            return;
        }
        
        if (Time.time - _lastAttackTime >= _context.Stats.AttackSpeed)
        {
            Attack();
        }
    }

    public void Exit()
    {
        _isAttacking = false;
    }

    private void Attack()
    {
        var enemyCombat = _enemy.GetComponent<EnemyCombat>();

        if (enemyCombat == null)
        {
            _isAttacking = false;
            _control.ChangeState(new IdleState());
            return;
        }

        _context.EventBus.OnPlayerAttackTriggered?.Invoke();

        _lastAttackTime = Time.time;

        _context.EventBus.OnPlayerAttackHit += HitEnemy;
    }

    private void HitEnemy()
    {
        _enemy.GetComponent<EnemyCombat>()?.TakeDamage(_context.Stats.Attack);
        _context.EventBus.OnPlayerAttackHit -= HitEnemy;
    }
}

public class PickingItemState : IPlayerState
{
    private GameObject _player;
    private PlayerContext _context;
    private PlayerControl _control;
    private GameObject _item;
    public void Enter(GameObject player)
    {
        _player = player;
        _context = player.GetComponent<PlayerContext>();
        _control = player.GetComponent<PlayerControl>();

        if (_control.CurrentTarget == null || !_control.CurrentTarget.tag.Equals("Item"))
        {
            _control.ChangeState(new IdleState());
            return;
        }

        _item = _control.CurrentTarget;
        _context.EventBus.OnPlayerPickUp?.Invoke();

        _context.EventBus.OnPlayerFinishedPickingUp += PickUpItem;
        
    }

    private void PickUpItem()
    {
        ItemManager.Instance.PickItem(_item);
        _context.EventBus.OnPlayerFinishedPickingUp -= PickUpItem;
    }

    public void Execute()
    {
        
    }

    public void Exit()
    {
    }
}

public class CastingState : IPlayerState
{
    private GameObject _player;
    private PlayerControl _control;
    private PlayerContext _context;
    private Skill _skill;
    private Coroutine _castingRoutine;
    
    // Usamos o CancellationTokenSource do próprio C# para gerenciar o escopo desse estado
    private CancellationTokenSource _stateCts;

    public void Enter(GameObject player)
    {
        _player = player;
        _control = player.GetComponent<PlayerControl>();
        _context = player.GetComponent<PlayerContext>();
        _skill = _control.CurrentSkill;

        if (_player == null) return;

        // 1. Criamos um CTS vinculado ao ciclo de vida do GameObject do Player.
        // Se o Player for destruído, o token cancela automaticamente.
        _stateCts = CancellationTokenSource.CreateLinkedTokenSource(_player.GetCancellationTokenOnDestroy());

        _control._blockStateChange = true;

        // 2. Iniciamos o fluxo assíncrono do Cast sem travar a Main Thread
        _ = ExecuteCastWorkflowAsync(_stateCts.Token); 
    }

    public void Execute() { }

    public void Exit()
    {
        // 5. Se o jogador sair do estado antes da hora (Ex: tomou Stun),
        // cancelamos tudo instantaneamente.
        if (_stateCts != null)
        {
            _stateCts.Cancel();
            _stateCts.Dispose();
            _stateCts = null;
        }
    }

    // O "Coração" do formato assíncrono
    private async Awaitable ExecuteCastWorkflowAsync(CancellationToken token)
    {
        try
        {
            StartCastingVisuals();
            await Awaitable.WaitForSecondsAsync(_skill.CastingTime, cancellationToken: token);
            _context.EventBus.OnPlayerAttackTriggered?.Invoke();
            _skill.Effect.OnCastFinished(_player, _skill, token);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _control._blockStateChange = false;
            _control.ChangeState(new IdleState());
        }
    }

    private void StartCastingVisuals()
    {
        _skill.Effect.OnCastStarted(_player, _skill, _stateCts.Token);
        if (_castingRoutine != null) _control.StopCoroutine(_castingRoutine);
        _castingRoutine = _control.StartCoroutine(SmoothSnapOnce());

    }

    private System.Collections.IEnumerator SmoothSnapOnce()
    {
        yield return GridHelper.SnapToNearestCellCenter(_player, 0.15f);
    }
}

public class DeadState : IPlayerState
{
    public void Enter(GameObject player){}
    public void Execute(){}
    public void Exit(){}
}
