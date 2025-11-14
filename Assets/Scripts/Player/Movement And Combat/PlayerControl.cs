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
        
        Debug.Log($"[Player Control] change player state to {newState}!");

        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter(this.gameObject);
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

        Vector3Int playerPosition = GridManager.Instance.WorldToCell(_player.transform.position);

        if (_destination == null)
        {
            _control.ChangeState(new IdleState());
            return;
        }
        
        _path = NodeManager.Instance.FindPath(playerPosition, (Vector3Int) _destination);

        if (_path == null || _path.Count == 0)
        {
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

            if (_index >= _path.Count)
            {
                // reached destination
                _isMoving = false;
                _control.ChangeState(new IdleState());
                return;
            }

            SetNextTargetCell();
        }
    }

    public void Exit()
    {
        
    }

    private void SetNextTargetCell()
    {
        if (_index < _path.Count)
        {
            Node nextNode = _path[_index];
            _nextNodePosition = GridManager.Instance.GetCellCenterWorld(nextNode._gridPosition);
            OnStep(nextNode._gridPosition);
        }
    }

    private void OnStep(Vector3Int newCell)
    {
        //add step-related logic
        //play footstep sound, play animation etc
    }
}

// public class WalkingState : IPlayerState
// {
//     private Vector3Int _destination;
//     private PlayerControl _playerControl;
//     private PlayerMovement _movement;
//     private GameObject _target; // Optional target

//     public WalkingState(Vector3Int destination, GameObject target = null)
//     {
//         _destination = destination;
//         _target = target;
//     }

//     public void Enter(GameObject player)
//     {
//         _playerControl = player.GetComponent<PlayerControl>();
//         _movement = player.GetComponent<PlayerMovement>();

//         _playerControl.CurrentTarget = _target;
//         _movement.WalkToEmptyTile(_destination);
//     }

//     public void Execute()
//     {
//         // Check if we reached destination
//         Vector3Int currentPos = GridManager.Instance.WorldToCell(_playerControl.transform.position);

//         if (currentPos == _destination)
//         {
//             if (_target != null)
//                 _playerControl.ChangeState(new AttackingState(_target));
//             else
//                 _playerControl.ChangeState(new IdleState());
//         }
//     }

//     public void Exit()
//     {
//         _movement.StopMovement();
//     }
// }

// public class PickingItemState : IPlayerState
// {
//     private GameObject _player;
//     private GameObject _item;
//     private Coroutine _getItemRoutine;

//     public PickingItemState(GameObject item)
//     {
//         _item = item;
//     }

//     public void Enter(GameObject player)
//     {
//         _player = player;
//         Debug.Log("[Player Control - PickingItem State] entering state...");
//         PickItem(_item);

//     }

//     public void Execute()
//     {

//     }

//     public void Exit()
//     {

//     }

//     private void PickItem(GameObject item)
//     {
//         var itemName = item.GetComponent<ItemDataLoader>().Name;

//         if (itemName == ItemName.Apple)
//         {
//             if (ItemController.Instance.MaxApplesObtained < ItemController.Instance.MaxApples) 
//             {
//                 ItemController.Instance.MaxApplesObtained++;
//             }

//             if (ItemController.Instance.Apples < ItemController.Instance.MaxApples)
//             {
//                 ItemController.Instance.Apples++;
//             }
//         }

//         UnityEngine.Object.Destroy(item);
//     }
// }

// public class AttackingState : IPlayerState
// {
//     private GameObject _enemy;
//     private PlayerControl _playerControl;
//     private PlayerContext _context;
//     private float _lastAttackTime;

//     public AttackingState(GameObject enemy)
//     {
//         _enemy = enemy;
//     }

//     public void Enter(GameObject player)
//     {
//         _playerControl = player.GetComponent<PlayerControl>();
//         _context = player.GetComponent<PlayerContext>();
//         _playerControl.CurrentTarget = _enemy;
//         _lastAttackTime = Time.time;
//     }

//     public void Execute()
//     {
//         // If enemy is gone, stop attacking
//         if (_enemy == null)
//         {
//             _playerControl.ChangeState(new IdleState());
//             return;
//         }

//         Vector3Int playerPos = GridManager.Instance.WorldToCell(_playerControl.transform.position);
//         Vector3Int enemyPos = GridManager.Instance.WorldToCell(_enemy.transform.position);

//         // If out of range, walk to enemy
//         if (!DistanceHelper.IsInAttackRange(playerPos, enemyPos, _context.Stats.AttackRange))
//         {
//             _playerControl.ChangeState(new WalkingState(enemyPos, _enemy));
//             return;
//         }

//         // Attack on cooldown
//         if (Time.time - _lastAttackTime >= _context.Stats.AttackSpeed)
//         {
//             Attack();
//             _lastAttackTime = Time.time;
//         }
//     }

//     public void Exit()
//     {
//         // Don't clear target here - let the next state decide
//     }

//     private void Attack()
//     {
//         var enemyCombat = _enemy.GetComponent<EnemyCombat>();
//         if (enemyCombat != null)
//         {
//             enemyCombat.TakeDamage(_context.Stats.Attack);
//         }
//     }
// }

public class DeadState : IPlayerState
{
    public void Enter(GameObject player){}
    public void Execute(){}
    public void Exit(){}
}

public class CastingState : IPlayerState
{
    GameObject _player;
    PlayerMovement _playerMovement;

    public void Enter(GameObject player)
    {
        _player = player;
        _player.GetComponent<PlayerControl>()._blockStateChange = true;
        // _player.GetComponent<PlayerEventBus>().RaiseStopAttack();

        if (player == null)
        {
            Debug.LogError($"[PlayerControl - Casting State] player is null. Can't enter state.");
            return;
        }
        
        _playerMovement = player.GetComponent<PlayerMovement>();

        if (player.GetComponent<PlayerMovement>() == null)
        {
            Debug.LogError($"[PlayerControl - Casting State] player component <PlayerMovement> is null. Can't enter state.");
            return;
        }

        _playerMovement.StartCasting();

        ShortcutManager.Instance.OnStopCastingSkill += LetPlayerMove;
    }

    public void Execute()
    {
        _playerMovement.StartCasting();
    }

    public void Exit()
    {
        ShortcutManager.Instance.OnStopCastingSkill -= LetPlayerMove;
    }

    private void LetPlayerMove(bool hasFinishedCasting)
    { 
        if (hasFinishedCasting) 
        {
            _player.GetComponent<PlayerControl>()._blockStateChange = false;
            _player.GetComponent<PlayerControl>().ChangeState(new IdleState());
        }
    }
}