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
    [SerializeField] private Skill _stompPuddle;
    #endregion
    public Action<GameObject, bool> TryUsingStompPuddle;
    public Action<GameObject> TryCastingWaterBody;

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

    private void TryStartPlayerCast(GameObject caster, Skill skill)
    {
        var control = caster.GetComponent<PlayerControl>();

        if (control == null)
            return;

        if (!SkillResourceUserResolver.TryGet(caster, out ISkillResourceUser resourceUser) ||
            !resourceUser.HasEnoughSP(skill.SpCost))
        {
            FloatingTextPool.Instance.ShowFailMessage(caster.transform.position);
            return;
        }

        resourceUser.UseSP(skill.SpCost);
        control.Casting(skill);
        control.ChangeState(new CastingState());
    }

    private void OnDestroy()
    {
        TryUsingStompPuddle -= CastStompPuddle;
        TryCastingWaterBody -= CastWaterBody;
    }


}
