// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-04-18 21:58:47
// 版 本：1.0
// ========================================================
using UnityEngine;
public class SkillSleep : Skill
{
    public SkillSleep(int skillId, Role executor, Vector2 pt, Role target = null) : base(skillId, executor, pt, target)
    {
    }
    public override bool InBombZone(Vector2 shotPt, Vector2 targetPt)
    {
        if (_skillTpl.Affectdistance.Length > 1)
        {
            if (_skillTpl.Affectdistance[0] == 2)//圆形区域
            {
                float distance = _skillTpl.Affectdistance[1] * 0.01f;
                if (_role.GetRoleInfo().INT > 0) {
                    distance *= Mathf.Min(2.6f, 1f+(float)(_role.GetRoleInfo().INT-160)/30f*0.2f);
                }
                return Vector2.Distance(shotPt, targetPt) < distance;
            }
        }
        return false;
    }
}
