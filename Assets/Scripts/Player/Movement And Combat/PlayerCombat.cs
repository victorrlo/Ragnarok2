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
        FloatingTextPool.Instance.ShowDamage(transform.position, amount, Color.red);
        _playerContext.StatsManager.TakeDamage(amount);
        _damageReaction.React();
    }
}
