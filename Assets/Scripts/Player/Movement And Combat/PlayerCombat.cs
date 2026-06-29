using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private PlayerContext _playerContext;
    private DamageReaction _damageReaction;

    private void Awake()
    {
        if (_playerContext == null)
            TryGetComponent<PlayerContext>(out _playerContext);

        if (_damageReaction == null && !TryGetComponent(out _damageReaction))
            _damageReaction = gameObject.AddComponent<DamageReaction>();
    }

    public void TakeDamage(int amount)
    {
        TakeDamage(new DamageResult(amount, false));
    }

    public void TakeDamage(DamageResult damage)
    {
        Color damageColor = damage.IsCritical ? Color.yellow : Color.red;
        FloatingTextPool.Instance.ShowDamage(transform.position, damage.Amount, damageColor);
        _playerContext.StatsManager.TakeDamage(damage.Amount);
        _damageReaction.React();
    }
}
