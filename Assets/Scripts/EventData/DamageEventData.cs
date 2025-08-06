using UnityEngine;

[System.Serializable]
public struct DamageEventData
{
    public GameObject _target;
    public int _damageAmount;

    public DamageEventData(GameObject target, int amount)
    {
        _target = target;
        _damageAmount = amount;
    }
}
