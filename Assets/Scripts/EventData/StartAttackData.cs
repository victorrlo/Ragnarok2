using UnityEngine;

[System.Serializable]
public struct StartAttackData
{
    private GameObject _target;
    private GameObject _source;
    public GameObject target => _target;
    public GameObject source => _source;

    public StartAttackData(GameObject source, GameObject target)
    {
        _target = target;
        _source = source;
    }
}
