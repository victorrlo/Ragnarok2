using System;
using UnityEngine;

public class SkillController : MonoBehaviour
{
    public static SkillController Instance {get; private set;}

    #region PlayerSkills
    private bool _hasWaterBodySkill = true;
    public bool HasWaterBodySkill => _hasWaterBodySkill;
    private bool _hasStompPuddleSkill = true;
    public bool HasStompPuddleSkill => _hasStompPuddleSkill;
    private bool _hasWaterBombSkill = false;
    public bool HasWaterBombSkill => _hasWaterBombSkill;
    #endregion
    #region Skills
    [SerializeField] private Skill _waterBody;
    [SerializeField] private Skill _stompPuddleSkill;
    #endregion
    public Action<GameObject, bool> TryUsingStompPuddle;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        TryUsingStompPuddle += CastStompPuddle;
    }

    private void CastStompPuddle(GameObject caster, bool shouldCast)
    {   
        DamageCellController.Instance.InvokeDamageCells?.Invoke(caster, _stompPuddleSkill);
        CastingBarPool.Instance.ShowCastingBar(caster, _stompPuddleSkill);

        if (caster.CompareTag("Player"))
        {
            caster.GetComponent<PlayerMovement>().StopMovement();
        }
    }

    private void OnDestroy()
    {
        TryUsingStompPuddle -= CastStompPuddle;
    }


}
