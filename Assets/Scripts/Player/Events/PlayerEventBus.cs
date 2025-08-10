using System;
using UnityEngine;

public class PlayerEventBus : MonoBehaviour
{
    public event Action<Vector3Int> OnWalk;
    public event Action<DamageEventData> OnDamaged;
    public event Action<StartAttackData> OnStartAttack;
    public event Action OnStopAttack;
    public event Action<StartAttackData> OnTargetMovedAway;

    public void RaiseOnWalk(Vector3Int position)
    {
        OnWalk?.Invoke(position);
    }
    public void RaiseStartAttack(StartAttackData data)
    {
        OnStartAttack?.Invoke(data);
    }

    public void RaiseStopAttack()
    {
        OnStopAttack?.Invoke();
    }

    public void RaiseOnDamaged(DamageEventData data)
    {
        OnDamaged?.Invoke(data);
    }

    public void RaiseOnTargetMovedAway(StartAttackData data)
    {
        OnTargetMovedAway?.Invoke(data);
    }
}
