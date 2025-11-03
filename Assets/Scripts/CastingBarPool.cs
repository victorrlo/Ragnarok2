using System.Collections.Generic;
using UnityEngine;

public class CastingBarPool : MonoBehaviour
{
    public static CastingBarPool Instance {get; private set;}
    private Queue<CastingBar> _castingBarPool = new();
    [SerializeField] private GameObject _castingBarPrefab;
    private int _poolSize = 10;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        for (int i = 0; i < _poolSize; i++)
        {
            CreateCastingBar();
        }
    }

    private CastingBar CreateCastingBar()
    {
        GameObject gameObject = Instantiate(_castingBarPrefab, transform);
        gameObject.SetActive(false);
        var castingBar = gameObject.GetComponent<CastingBar>();
        _castingBarPool.Enqueue(castingBar);
        return castingBar;
    }

    public void ShowCastingBar(GameObject caster, Skill skill)
    {
        if (_castingBarPool.Count == 0)
            CreateCastingBar();

        var castingBar = _castingBarPool.Dequeue();

        castingBar.OnCastingComplete += HandleSkillCompletion;

        castingBar.Initialize(caster, skill, ReturnCastingBarToPool);
    }

    private void HandleSkillCompletion(GameObject gameObject, Skill skill)
    {
        Debug.Log("Applying skill effects!");
    }

    private void ReturnCastingBarToPool(CastingBar castingBar)
    {
        castingBar.gameObject.SetActive(false);
        castingBar.OnCastingComplete -= HandleSkillCompletion;
        _castingBarPool.Enqueue(castingBar);
    }
}
