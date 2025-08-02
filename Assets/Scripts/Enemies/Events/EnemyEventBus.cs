using System;
using UnityEngine;

public class EnemyEventBus : MonoBehaviour
{
    // public EnemyDamageEvent OnDamaged;
    // public EnemyStartAttackEvent OnStartAttack;
    
    public event Action<EnemyDamageEventData> OnDamaged;
    public event Action<EnemyStartAttackData> OnStartAttack;

    public void RaiseStartAttack(EnemyStartAttackData data)
    {
        OnStartAttack?.Invoke(data);
    }

    public void RaiseOnDamaged(EnemyDamageEventData data)
    {
        OnDamaged?.Invoke(data);
    }

    // public void RaiseDamaged(EnemyDamageEventData data) => OnDamaged.Raise(data);
}
