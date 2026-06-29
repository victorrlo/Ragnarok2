using UnityEngine;

public interface ISkillResourceUser
{
    bool HasEnoughSP(int amount);
    void UseSP(int amount);
}

public static class SkillResourceUserResolver
{
    public static bool TryGet(GameObject caster, out ISkillResourceUser resourceUser)
    {
        resourceUser = null;

        if (caster == null)
            return false;

        MonoBehaviour[] behaviours = caster.GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is ISkillResourceUser foundResourceUser)
            {
                resourceUser = foundResourceUser;
                return true;
            }
        }

        return false;
    }
}
