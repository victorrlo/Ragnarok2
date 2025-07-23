using UnityEngine;

public class AggressiveState : IEnemyState
{
    //definir uma área de efeito desse estado (o monstro não pode seguir o jogador pelo mapa inteiro, a não ser que eu quisesse por algum motivo)
    private EnemyAI _enemy;
    private Transform _player;
    private bool _isWandering;
    private bool _isChasing;
    private Coroutine _chasingCoroutine;

    public void Enter(EnemyAI enemy)
    {
        _enemy = enemy;
        _player = GameObject.FindWithTag("Player")?.transform;

        if (_player == null)
        {
            _enemy.ChangeState(new PassiveState());
            return;
        }
    }
    public void Execute()
    {
        if (!DistanceHelper.IsPlayerInRange(_player.transform, _enemy)) 
        {
            _enemy.ChangeState(new PassiveState());
            return;
        }

        if (!_isChasing)
        {
            StartChase();
        }
            
        // use IsPlayerInRange to check if Aggressive State is possible or should become Passive once again.
        // there are enemies that are always aggressive, so I think I should create a different state for attacking enemy and aggressive ones.
        
        // condicionais de mudanças de estado
    }

    public void Exit()
    {
        StopChase();
    }

    private void StartChase()
    {
        if (_chasingCoroutine == null)
        {
            _isChasing = true;
            
            _chasingCoroutine = _enemy.StartCoroutine(_enemy.Movement.ChasePlayer(() =>
            {
                _isChasing = false;
            }));
        }
    }

    private void StopChase()
    {
        if (_chasingCoroutine != null)
        {
            _enemy.StopCoroutine(_chasingCoroutine);
            _chasingCoroutine = null;
        }

        _isChasing = false;
    }


}
