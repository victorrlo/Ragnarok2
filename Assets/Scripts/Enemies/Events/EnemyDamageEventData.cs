using UnityEngine;

[System.Serializable]
public struct EnemyDamageEventData
{
    public GameObject _target;
    public int _damageAmount;

    public EnemyDamageEventData(GameObject target, int amount)
    {
        _target = target;
        _damageAmount = amount;
    }
}
