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
    private bool _hasBashSkill = true;
    public bool HasBashSkill => _hasBashSkill;
    #endregion

    #region Skills
    [SerializeField] private Skill _waterBody;
    [SerializeField] private Skill _stompPuddle;
    [SerializeField] private Skill _bash;
    #endregion
    public Action<GameObject, bool> TryUsingStompPuddle;
    public Action<GameObject> TryCastingWaterBody;
    public Action<GameObject, GameObject> TryCastingBash;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        TryUsingStompPuddle += CastStompPuddle;
        TryCastingWaterBody += CastWaterBody;
        TryCastingBash += CastBash;
    }

    private void CastWaterBody(GameObject caster)
    {
        if (caster.CompareTag("Player"))
        {
            TryStartPlayerCast(caster, _waterBody);
        }
    }

    private void CastStompPuddle(GameObject caster, bool shouldCast)
    {   
        if (caster.CompareTag("Player"))
        {
            TryStartPlayerCast(caster, _stompPuddle);
        }
    }

    private void CastBash(GameObject caster, GameObject target)
    {
        if (caster.CompareTag("Player"))
        {
            TryStartPlayerCast(caster, _bash, target);
        }
    }

    private void TryStartPlayerCast(GameObject caster, Skill skill, GameObject target = null)
    {
        var control = caster.GetComponent<PlayerControl>();

        if (control == null)
            return;

        if (skill == null || !SingleTargetSkillValidator.CanCast(caster, target, skill))
        {
            FloatingTextPool.Instance.ShowFailMessage(caster.transform.position);
            return;
        }

        if (!SkillResourceUserResolver.TryGet(caster, out ISkillResourceUser resourceUser) ||
            !resourceUser.HasEnoughSP(skill.SpCost))
        {
            FloatingTextPool.Instance.ShowFailMessage(caster.transform.position);
            return;
        }

        resourceUser.UseSP(skill.SpCost);
        control.Casting(skill, target);
        control.ChangeState(new CastingState());
    }

    private void OnDestroy()
    {
        TryUsingStompPuddle -= CastStompPuddle;
        TryCastingWaterBody -= CastWaterBody;
        TryCastingBash -= CastBash;
    }


}
