using System.Threading;
using UnityEngine;

[CreateAssetMenu(fileName = "BashEffect", menuName = "Scriptable Objects/Skill Effects/Bash")]
public class BashEffect : SkillEffect
{
    [SerializeField, Range(0f, 1f)] private float stunChance = 0.25f;

    public override void OnCastStarted(GameObject caster, GameObject target, Skill skill, CancellationToken cancellationToken)
    {
        CastingBarPool.Instance.ShowCastingBar(caster, skill);
    }

    public override void OnCastFinished(GameObject caster, GameObject target, Skill skill, CancellationToken cancellationToken)
    {
        if (!SingleTargetSkillValidator.CanCast(caster, target, skill))
        {
            FloatingTextPool.Instance.ShowFailMessage(caster.transform.position);
            return;
        }

        if (!caster.TryGetComponent<PlayerContext>(out PlayerContext playerContext))
            return;

        int baseDamage = Mathf.RoundToInt(playerContext.StatsManager.RunTimeStats.Attack * skill.Multiplier);
        DamageResult damage = DamageCalculator.Roll(
            baseDamage,
            playerContext.StatsManager.RunTimeStats.CriticalChance,
            playerContext.StatsManager.RunTimeStats.CriticalDamageMultiplier);

        target.GetComponent<EnemyCombat>()?.TakeDamage(damage);

        if (skill.TimeOfEffect <= 0f || Random.value > stunChance)
            return;

        EnemyStatusController statusController = target.GetComponent<EnemyStatusController>();

        if (statusController == null)
            statusController = target.AddComponent<EnemyStatusController>();

        statusController.Stun(skill.TimeOfEffect);
    }
}
