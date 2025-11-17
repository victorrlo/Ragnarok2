using System;
using UnityEngine;

public class EnemyEventBus : MonoBehaviour
{   
    public event Action<DamageEventData> OnDamaged;
    public event Action<StartAttackData> OnTargetMovedAway;

    public void RaiseOnDamaged(DamageEventData data)
    {
        OnDamaged?.Invoke(data);
    }

    public void RaiseOnTargetMovedAway(StartAttackData data)
    {
        OnTargetMovedAway?.Invoke(data);
    }
}
