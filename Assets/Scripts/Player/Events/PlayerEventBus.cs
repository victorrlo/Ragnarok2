using System;
using UnityEngine;

public class PlayerEventBus : MonoBehaviour
{
    public event Action<DamageEventData> OnDamaged;
    public event Action<StartAttackData> OnStartAttack;
    public event Action<StartAttackData> OnTargetMovedAway;

    public void RaiseStartAttack(StartAttackData data)
    {
        OnStartAttack?.Invoke(data);
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
