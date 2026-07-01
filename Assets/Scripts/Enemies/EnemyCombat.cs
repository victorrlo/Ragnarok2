using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
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
        TakeDamage(new DamageResult(amount, false));
    }

    public void TakeDamage(DamageResult damage)
    {
        Color damageColor = damage.IsCritical ? Color.yellow : Color.white;
        int damageDealt = Mathf.Clamp(damage.Amount, 0, Mathf.CeilToInt(_enemyContext.StatsManager.CurrentHP));

        FloatingTextPool.Instance.ShowDamage(transform.position, damage.Amount, damageColor);
        _enemyContext.StatsManager.TakeDamage(damage.Amount);
        PlayerStatsManager.Instance?.TryRecoverHPFromDamageDealt(damageDealt);

        if (_enemyContext.StatsManager.CurrentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        _enemyContext.AI.CurrentState?.Exit();
        
        if (ItemDropManager.Instance != null)
        {
            ItemDropManager.Instance.DropItems(_enemyContext.Stats, transform.position);
        }
        
        Destroy(gameObject);
    }
}
