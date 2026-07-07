using System.Collections.Generic;

public interface IEnemySkillRuleProvider
{
    List<SkillCastingData> GetSkillRules();
}
