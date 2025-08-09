using UnityEngine;

[RequireComponent(typeof(PlayerContext))]
public class PlayerControl : MonoBehaviour
{
    private PlayerContext _context;
    private PlayerEventBus _eventBus;

    private GameObject _currentTarget;

    private void Awake()
    {
        if (_context == null)
            TryGetComponent<PlayerContext>(out _context);

        if (_eventBus == null)
            _context.TryGetComponent<PlayerEventBus>(out _eventBus);
    }   

    public void StartCombat(GameObject enemy)
    {
        if (_currentTarget == enemy) return;

        _currentTarget = enemy;

        var data = new StartAttackData(this.gameObject, enemy);
        _eventBus.RaiseStartAttack(data);
    }

    public void StopCombat()
    {
        _currentTarget = null;
        _eventBus.RaiseStopAttack();
    }
}
