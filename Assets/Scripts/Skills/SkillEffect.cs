using System.Threading;
using UnityEngine;

public abstract class SkillEffect : ScriptableObject
{
    public virtual void OnCastStarted(GameObject caster, Skill skill, CancellationToken cancellationToken) {}
    public virtual void OnCastFinished(GameObject caster, Skill skill, CancellationToken cancellationToken) {}
}
