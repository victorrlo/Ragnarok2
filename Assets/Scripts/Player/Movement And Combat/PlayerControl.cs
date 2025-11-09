using System.Collections;
using UnityEngine;
[RequireComponent(typeof(PlayerContext))]
public class PlayerControl : MonoBehaviour
{
    private PlayerContext _context;
    private PlayerEventBus _eventBus;
    private GameObject _currentTarget;
    private IPlayerState _currentState = new IdleState();

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
        // StartCoroutine(PeriodicStateCheck());
    }

    private void Update()
    {
        _currentState?.Execute();
    }
    
    public void ChangeState(IPlayerState newState)
    {
        if (_currentState != null && _currentState.GetType() == newState.GetType()) 
            return;

        // Debug.Log($"[Player Control] change player state to {newState}!");

        _currentState?.Exit();
        _currentState = newState;
        _currentState.Enter(this.gameObject);
    }

    public IPlayerState GetCurrentState()
    {
        return _currentState;
    }

    public void WalkTo(Vector3Int position)
    {
        _eventBus.RaiseOnWalk(position);
    }

    // combat control
    public void StartCombat(GameObject enemy)
    {
        if (_currentTarget == enemy) return;

        _currentTarget = enemy;

        var data = new StartAttackData(this.gameObject, enemy);
        _eventBus.RaiseStartAttack(data);
    }

    public void StopCombat()
    {
        _currentTarget = null;
        _eventBus.RaiseStopAttack();
    }

    public void GetItem(GameObject item)
    {
        if (_currentTarget == item) return;

        _currentTarget = item;

        _eventBus.RaiseGetItem(item);
    }
    
    // private IEnumerator PeriodicStateCheck()
    // {
    //     while (true)
    //     {
    //         Debug.Log($"[Player Control] current state is {_currentState}");
    //         yield return new WaitForSeconds(1f);
    //     }
    // }
}

public interface IPlayerState
{
    void Enter(GameObject player);
    void Execute();
    void Exit();
}

public class IdleState : IPlayerState
{
    GameObject _player;

    public void Enter(GameObject player)
    {
        _player = player;
        ShortcutManager.Instance.OnStartCastingSkill += StopAllMovement;
    }

    public void Execute()
    {

    }

    public void Exit()
    {
        ShortcutManager.Instance.OnStartCastingSkill -= StopAllMovement;
    }

    private void StopAllMovement(bool isCasting)
    {
        // Debug.Log("[PlayerControl] stopping all movement to change state to Casting State!");
        _player.GetComponent<PlayerControl>().ChangeState(new CastingState());
    }
}

public class WalkingState : IPlayerState
{
    public void Enter(GameObject player){}
    public void Execute(){}
    public void Exit(){}
}

public class AttackingState : IPlayerState
{
    public void Enter(GameObject player)
    {

    }

    public void Execute()
    {

    }

    public void Exit()
    {

    }
}

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
        if (hasFinishedCasting) _player.GetComponent<PlayerControl>().ChangeState(new IdleState());
    }
}