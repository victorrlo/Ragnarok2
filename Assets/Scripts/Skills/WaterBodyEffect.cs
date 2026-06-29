using System.Threading;
using UnityEngine;

[CreateAssetMenu(fileName = "WaterBodyEffect", menuName = "Scriptable Objects/Skill Effects/Water Body")]
public class WaterBodyEffect : SkillEffect
{
    [SerializeField] private CharacterVisualEffectController.SpriteOverlaySettings _visualEffect = new()
    {
        Id = "water-body",
        Color = new Color(0.25f, 0.65f, 1f, 0.55f),
        SortingOrderOffset = 1
    };

    public override void OnCastStarted(GameObject caster, Skill skill, CancellationToken cancellationToken)
    {
        CastingBarPool.Instance.ShowCastingBar(caster, skill);
    }

    public override void OnCastFinished(GameObject caster, Skill skill, CancellationToken cancellationToken)
    {
        if (caster.TryGetComponent(out DamageReaction damageReaction))
            _ = damageReaction.SuppressReactionFor(skill.TimeOfEffect, damageReaction.destroyCancellationToken);

        CharacterVisualEffectController visualEffectController = GetOrCreateVisualEffectController(caster);
        visualEffectController.PlayOverlay(_visualEffect, skill.TimeOfEffect);
    }

    private CharacterVisualEffectController GetOrCreateVisualEffectController(GameObject caster)
    {
        if (!caster.TryGetComponent(out CharacterVisualEffectController visualEffectController))
            visualEffectController = caster.AddComponent<CharacterVisualEffectController>();

        return visualEffectController;
    }
}
