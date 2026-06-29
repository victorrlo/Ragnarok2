using System.Threading;
using UnityEngine;

public abstract class SkillEffect : ScriptableObject
{
    public virtual void OnCastStarted(GameObject caster, Skill skill, CancellationToken cancellationToken) {}
    public virtual void OnCastFinished(GameObject caster, Skill skill, CancellationToken cancellationToken) {}
    public virtual void OnCastStarted(GameObject caster, GameObject target, Skill skill, CancellationToken cancellationToken)
    {
        OnCastStarted(caster, skill, cancellationToken);
    }

    public virtual void OnCastFinished(GameObject caster, GameObject target, Skill skill, CancellationToken cancellationToken)
    {
        OnCastFinished(caster, skill, cancellationToken);
    }
}
