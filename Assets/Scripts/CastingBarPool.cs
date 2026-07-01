using System.Collections.Generic;
using UnityEngine;

public class CastingBarPool : MonoBehaviour
{
    public static CastingBarPool Instance {get; private set;}
    private Queue<CastingBar> _castingBarPool = new();
    private Dictionary<GameObject, CastingBar> _activeCastingBars = new();
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
        if (caster == null)
            return;

        if (_activeCastingBars.TryGetValue(caster, out CastingBar activeCastingBar))
        {
            activeCastingBar.Initialize(caster, skill, ReturnCastingBarToPool);
            return;
        }

        if (_castingBarPool.Count == 0)
            CreateCastingBar();

        var castingBar = _castingBarPool.Dequeue();
        _activeCastingBars[caster] = castingBar;

        castingBar.OnCastingComplete += HandleSkillCompletion;

        castingBar.Initialize(caster, skill, ReturnCastingBarToPool);
    }

    private void HandleSkillCompletion(GameObject gameObject, Skill skill)
    {
        ShortcutManager.Instance.OnStopCastingSkill?.Invoke(true);
    }

    private void ReturnCastingBarToPool(CastingBar castingBar)
    {
        if (castingBar.CurrentCaster != null &&
            _activeCastingBars.TryGetValue(castingBar.CurrentCaster, out CastingBar activeCastingBar) &&
            activeCastingBar == castingBar)
        {
            _activeCastingBars.Remove(castingBar.CurrentCaster);
        }

        castingBar.ClearCaster();
        castingBar.gameObject.SetActive(false);
        castingBar.OnCastingComplete -= HandleSkillCompletion;
        _castingBarPool.Enqueue(castingBar);
    }
}
