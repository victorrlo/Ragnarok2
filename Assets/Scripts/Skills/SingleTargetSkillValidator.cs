using UnityEngine;

public static class SingleTargetSkillValidator
{
    public static bool CanCast(GameObject caster, GameObject target, Skill skill)
    {
        if (caster == null || skill == null)
            return false;

        if (skill.SkillType != Skill.Type.SingleTarget)
            return true;

        if (target == null)
            return false;

        if (!target.CompareTag("Enemy"))
            return false;

        if (!target.TryGetComponent<EnemyCombat>(out _))
            return false;

        Vector3Int casterCell = GridManager.Instance.WorldToCell(caster.transform.position);
        Vector3Int targetCell = GridManager.Instance.WorldToCell(target.transform.position);
        int effectiveRange = skill.Range <= 0 ? 1 : skill.Range;

        return DistanceHelper.IsInAttackRange(casterCell, targetCell, effectiveRange);
    }
}
