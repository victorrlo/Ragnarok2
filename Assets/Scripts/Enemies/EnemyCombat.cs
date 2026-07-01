using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

[RequireComponent(typeof(EnemyContext))]
public class EnemyCombat : MonoBehaviour
{
    private EnemyContext _enemyContext;
    private DamageReaction _damageReaction;
    private EnemyAnimation _animation;
    private Coroutine _attackCoroutine;
    private bool _isDead;
    public bool IsDead => _isDead;

    private void Awake()
    {
        if(_enemyContext== null)
            TryGetComponent<EnemyContext>(out _enemyContext);

        if (_damageReaction == null && !TryGetComponent(out _damageReaction))
            _damageReaction = gameObject.AddComponent<DamageReaction>();

        TryGetComponent(out _animation);
    }

    public void TakeDamage(int amount)
    {
        TakeDamage(new DamageResult(amount, false));
    }

    public void TakeDamage(DamageResult damage)
    {
        if (_isDead)
            return;

        Color damageColor = damage.IsCritical ? Color.yellow : Color.white;
        int damageDealt = Mathf.Clamp(damage.Amount, 0, Mathf.CeilToInt(_enemyContext.StatsManager.CurrentHP));

        FloatingTextPool.Instance.ShowDamage(transform.position, damage.Amount, damageColor);
        _enemyContext.StatsManager.TakeDamage(damage.Amount);
        _damageReaction?.React();
        PlayerStatsManager.Instance?.TryRecoverHPFromDamageDealt(damageDealt);

        if (_enemyContext.StatsManager.CurrentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (_isDead)
            return;

        _isDead = true;
        DisableGameplayAfterDeath();
        
        if (ItemDropManager.Instance != null)
        {
            ItemDropManager.Instance.DropItems(_enemyContext.Stats, transform.position);
        }

        float deathDelay = _animation != null
            ? _animation.PlayDeathAnimation(Vector2.zero)
            : 0f;

        if (deathDelay > 0f)
        {
            Destroy(gameObject, deathDelay);
            return;
        }

        Destroy(gameObject);
    }

    private void DisableGameplayAfterDeath()
    {
        PlayerInputHandler.Instance?.ClearTargetIfMatching(gameObject);

        _enemyContext.AI.CurrentState?.Exit();
        _enemyContext.AI.ClearTarget();
        _enemyContext.AI.enabled = false;

        if (_enemyContext.StatsManager != null)
        {
            _enemyContext.StatsManager.HideStatsBar();
            _enemyContext.StatsManager.enabled = false;
        }

        if (TryGetComponent(out EnemyStatusController statusController))
            statusController.enabled = false;

        foreach (Collider collider in GetComponentsInChildren<Collider>())
            collider.enabled = false;

        tag = "Untagged";
    }
}
