// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-03-20 14:25:19
// 版 本：1.0
// ========================================================
using System;
using UnityEngine;
public class SkillFactory
{
    public SkillFactory()
    {
    }
    public static Skill Create(int skillId, Role executor, Vector2 pt, Role target = null)
    {
        Skill skill = null;
        switch(skillId)
        {
            case 2:
                skill = new SkillInjection(skillId, executor,pt, target);
                break;
            case 3:
                skill = new SkillSpin(skillId, executor,pt, target);
                break;
            case 5:
                skill = new SkillSleep(skillId, executor, pt, target);
                break;
            default:
                skill = new Skill(skillId, executor,pt, target);
                break;
        }
        return skill;
    }
}
