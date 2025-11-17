using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyContext))]
public class EnemyCombat : MonoBehaviour
{
    private EnemyContext _enemyContext;
    private Coroutine _attackCoroutine;

    private void Awake()
    {
        if(_enemyContext== null)
            TryGetComponent<EnemyContext>(out _enemyContext);
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

    private void Die()
    {
        _enemyContext.AI.CurrentState?.Exit();
        Destroy(gameObject);
    }
}
