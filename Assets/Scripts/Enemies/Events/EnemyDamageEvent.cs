using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDamageEvent", menuName = "Scriptable Objects/EnemyDamageEvent")]
public class EnemyDamageEvent : ScriptableObject
{
    public event Action<EnemyDamageEventData> OnRaised;

    public void Raise(EnemyDamageEventData data)
    {
        #if UNITY_EDITOR
        // Debug.LogWarning($"[EnemyDamageEvent] raised for: {data._target.name}, damage: {data._damageAmount}");
        #endif

        OnRaised?.Invoke(data);
    }
}
