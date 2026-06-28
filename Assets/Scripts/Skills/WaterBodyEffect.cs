using System.Threading;
using UnityEngine;

[CreateAssetMenu(fileName = "WaterBodyEffect", menuName = "Scriptable Objects/Skill Effects/Water Body")]
public class WaterBodyEffect : SkillEffect
{
    public override void OnCastStarted(GameObject caster, Skill skill, CancellationToken cancellationToken)
    {
        PlayerStatsManager.Instance.UseSP(skill.SpCost);
        CastingBarPool.Instance.ShowCastingBar(caster, skill);
    }

    public override void OnCastFinished(GameObject caster, Skill skill, CancellationToken cancellationToken)
    {
        if (!caster.TryGetComponent(out DamageReaction damageReaction))
            return;

        _ = damageReaction.SuppressReactionFor(skill.TimeOfEffect, damageReaction.destroyCancellationToken);
    }
}
