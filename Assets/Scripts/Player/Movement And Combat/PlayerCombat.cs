using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private PlayerContext _playerContext;

    private void Awake()
    {
        if (_playerContext == null)
            TryGetComponent<PlayerContext>(out _playerContext);
    }

    public void TakeDamage(int amount)
    {
        FloatingTextPool.Instance.ShowDamage(transform.position, amount, Color.red);
        _playerContext.StatsManager.TakeDamage(amount);
        // = stagger player =
        // maybe send an event for each playerState
        // that triggers the HurtingState and that HurtingState receives
        // as parameter the previous playerState so that after the animation is played
        // the player goes back to the previous state.

        if (_playerContext.StatsManager.CurrentHP <= 0)
        {
            Die();
        }
    }
    
    private void Die()
    {
        Destroy(gameObject);
    }
}
