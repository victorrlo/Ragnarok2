using System.Threading;
using UnityEngine;

[CreateAssetMenu(fileName = "StompPuddleEffect", menuName = "Scriptable Objects/Skill Effects/Stomp Puddle")]
public class StompPuddleEffect : SkillEffect
{
    public override void OnCastStarted(GameObject caster, Skill skill, CancellationToken cancellationToken)
    {
        OnCastStarted(caster, null, skill, cancellationToken, false);
    }

    public override void OnCastStarted(GameObject caster, GameObject target, Skill skill, CancellationToken cancellationToken, bool charged)
    {
        CastingBarPool.Instance.ShowCastingBar(caster, skill);
        DamageCellController.Instance.CastDamageCellsOnGround(
            caster,
            skill,
            charged,
            () => !charged &&
                  ShortcutManager.Instance != null &&
                  ShortcutManager.Instance.IsStompPuddleShortcutHeld);
    }
}
