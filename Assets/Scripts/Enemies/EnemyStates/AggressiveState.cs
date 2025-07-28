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

        _playerMovement.OnPlayerMoved += MoveEnemy;
        StartChase();
    }
    public void Execute()
    {
        if (!DistanceHelper.IsPlayerInRange(_player.transform, _enemy)) 
        {
            _enemy.ChangeState(new PassiveState());
            // animation for tired emote to show enemy is tired of chasing the player and gave up
            Debug.Log("Show Emote for tired");
            return;
        }

        if (_player == null)
        {
            _enemy.ChangeState(new PassiveState());
            return;
        }
    }

    public void Exit()
    {
        _playerMovement.OnPlayerMoved -= MoveEnemy;
    }

    private void MoveEnemy(Vector3Int newPos)
    {
        Vector3Int startPos = GridManager.Instance.WorldToCell(_enemy.transform.position);
        _enemyContext.Movement.UpdatePath(startPos, newPos);
    }

    private void StartChase()
    {
        _enemyContext.Movement.StartChasing();
    }
}
