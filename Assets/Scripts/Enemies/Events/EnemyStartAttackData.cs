using UnityEngine;

[System.Serializable]
public struct EnemyStartAttackData
{
    public GameObject _target;

    public EnemyStartAttackData(GameObject target)
    {
        _target = target;
    }
}
