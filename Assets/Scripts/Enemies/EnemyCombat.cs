using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyContext))]
public class EnemyCombat : MonoBehaviour
{
    private EnemyContext _enemyContext;
    private Coroutine _currentBehaviorCoroutine;

    private void Awake()
    {
        if(_enemyContext== null)
            TryGetComponent<EnemyContext>(out _enemyContext);
    }

    private void OnEnable()
    {
        _enemyContext.EventBus.OnStartAttack += Attack;
    }

    private void OnDisable()
    {
        _enemyContext.EventBus.OnStartAttack -= Attack;
    }

    public void TakeDamage(int amount)
    {
        FloatingTextPool.Instance.ShowDamage(transform.position, amount, Color.white);
        _enemyContext.StatsManager.TakeDamage(amount);

        if (_enemyContext.StatsManager.CurrentHP <= 0)
        {
            Die();
        }
    }

    private void Attack(EnemyStartAttackData data)
    {
        data._target.TryGetComponent<PlayerCombat>(out PlayerCombat player);

        if (player != null)
            if (_enemyContext != null)
                player.TakeDamage(_enemyContext.Stats.Attack);
    }

    private void Die()
    {
        _enemyContext.AI.CurrentState?.Exit();
        Destroy(gameObject);
    }
}
