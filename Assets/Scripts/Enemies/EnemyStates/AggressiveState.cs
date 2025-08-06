using Unity.VisualScripting;
using UnityEngine;

public class AggressiveState : IEnemyState
{
    private EnemyAI _enemy;
    private EnemyContext _enemyContext;
    private GameObject _player;
    private PlayerMovement _playerMovement;

    public void Enter(EnemyAI enemy)
    {
        _enemy = enemy;
        _enemy.TryGetComponent<EnemyContext>(out _enemyContext);
        
        _player = GameObject.FindWithTag("Player");
        _player.TryGetComponent<PlayerMovement>(out _playerMovement);

        if (_player == null)
        {
            _enemy.ChangeState(new PassiveState());
            return;
        }

        if (DistanceHelper.IsPlayerInAggressiveReach(_player.transform, _enemy))
        {
            StartChase();
        }
    }
    public void Execute()
    {
        if ( _player == null || DistanceHelper.IsPlayerOutOfReach(_player.transform, _enemy)) 
        {
            Debug.Log($"{_enemy.name} Show emote for tired because out of reach");
            _enemy.ChangeState(new PassiveState());
            return;
        }
    }

    public void Exit()
    {
        
    }

    private void StartChase()
    {
        var data = new StartAttackData(_enemy.gameObject, _player);
        _enemyContext.Movement.StartChasing(data);
    }
}
