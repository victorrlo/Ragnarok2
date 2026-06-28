using System.Threading;
using UnityEngine;

[CreateAssetMenu(fileName = "StompPuddleEffect", menuName = "Scriptable Objects/Skill Effects/Stomp Puddle")]
public class StompPuddleEffect : SkillEffect
{
    public override void OnCastStarted(GameObject caster, Skill skill, CancellationToken cancellationToken)
    {
        PlayerStatsManager.Instance.UseSP(skill.SpCost);
        CastingBarPool.Instance.ShowCastingBar(caster, skill);
        DamageCellController.Instance.InvokeDamageCells?.Invoke(caster, skill);
    }
}
