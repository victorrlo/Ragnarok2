using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStartAttackEvent", menuName = "Scriptable Objects/EnemyStartAttackEvent")]
public class EnemyStartAttackEvent : ScriptableObject
{
    public event Action<StartAttackData> OnRaised;

    public void Raise(StartAttackData data)
    {
        #if UNITY_EDITOR
        // Debug.LogWarning($"[EnemyStartAttackEvent] raised for: {data._target.name}");
        #endif

        OnRaised?.Invoke(data);
    }
}
