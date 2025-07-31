using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStartAttackEvent", menuName = "Scriptable Objects/EnemyStartAttackEvent")]
public class EnemyStartAttackEvent : ScriptableObject
{
    public event Action<EnemyStartAttackData> OnRaised;

    public void Raise(EnemyStartAttackData data)
    {
        #if UNITY_EDITOR
        // Debug.LogWarning($"[EnemyStartAttackEvent] raised for: {data._target.name}");
        #endif

        OnRaised?.Invoke(data);
    }
}
