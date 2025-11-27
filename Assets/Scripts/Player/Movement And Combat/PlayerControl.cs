using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Tilemaps;
using UnityEngine;
[RequireComponent(typeof(PlayerContext))]
public class PlayerControl : MonoBehaviour
{
    private PlayerContext _context;
    private PlayerEventBus _eventBus;
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

        _currentState = null;
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
    

    public void Enter(GameObject player)
    {
        _player = player;

        _playerContext = _player.GetComponent<PlayerContext>();
        _playerControl = _player.GetComponent<PlayerControl>();
        
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

            // if targetting an item...
            if (_target != null && _target.tag.Equals("Item"))
            {
                Vector3Int player = GridManager.Instance.WorldToCell(_player.transform.position);
                Vector3Int item = GridManager.Instance.WorldToCell(_target.transform.position);

                if (DistanceHelper.IsInAttackRange(player, item, _context.Stats.AttackRange))
                {
                    _isMoving = false;

                    _player.transform.position = Vector3.MoveTowards
                    (
                        _player.transform.position,
                        _nextNodePosition,
                        _moveSpeed * Time.deltaTime
                    );

                    _control.ChangeState(new PickingItemState());
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
        _lastAttackTime = Time.time;
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

        _lastAttackTime = Time.time;

        enemyCombat.TakeDamage(_context.Stats.Attack);
    }
}

public class PickingItemState : IPlayerState
{
    private GameObject _player;
    private PlayerControl _control;
    private GameObject _item;
    public void Enter(GameObject player)
    {
        _player = player;
        _control = player.GetComponent<PlayerControl>();

        if (_control.CurrentTarget == null || !_control.CurrentTarget.tag.Equals("Item"))
        {
            _control.ChangeState(new IdleState());
            return;
        }
        
        _item = _control.CurrentTarget;

        ItemManager.Instance.PickItem(_item);
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
    private Skill _skill;
    private Coroutine _castingRoutine;

    public void Enter(GameObject player)
    {
        _player = player;
        _control = player.GetComponent<PlayerControl>();
        _skill = _control.CurrentSkill;

        if (player == null)
        {
            Debug.LogError($"[PlayerControl - Casting State] player is null. Can't enter state.");
            return;
        }

        _control._blockStateChange = true;

        StartCasting();

        ShortcutManager.Instance.OnStopCastingSkill += LetPlayerMove;
    }

    public void Execute()
    {
        // StartCasting();
    }

    public void Exit()
    {
        ShortcutManager.Instance.OnStopCastingSkill -= LetPlayerMove;
    }

    private void LetPlayerMove(bool hasFinishedCasting)
    { 
        if (hasFinishedCasting) 
        {
            ApplySkillEffect();

            _player.GetComponent<PlayerControl>()._blockStateChange = false;
            _player.GetComponent<PlayerControl>().ChangeState(new IdleState());
        }
    }

    private void ApplySkillEffect()
    {
        if (_skill != null)
        {
            switch (_skill.Name)
            {
                case "Stomp Puddle":

                    break;
            }
        }
    }

    private IEnumerator SmoothSnapOnce()
    {
        yield return GridHelper.SnapToNearestCellCenter(_player, 0.15f);
    }

    public void StartCasting()
    {
        if (_castingRoutine != null)
            _control.StopCoroutine(_castingRoutine);
        _castingRoutine = null;
        
        _castingRoutine = _control.StartCoroutine(SmoothSnapOnce());

        DamageCellController.Instance.InvokeDamageCells?.Invoke(_player, _skill);
        CastingBarPool.Instance.ShowCastingBar(_player, _skill);
    }
}

public class DeadState : IPlayerState
{
    public void Enter(GameObject player){}
    public void Execute(){}
    public void Exit(){}
}