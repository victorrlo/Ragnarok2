using System;
using UnityEngine;

public class EnemyEventBus : MonoBehaviour
{
    public EnemyDamageEvent OnDamaged;

    public void RaiseDamaged(EnemyDamageEventData data) => OnDamaged.Raise(data);
}
