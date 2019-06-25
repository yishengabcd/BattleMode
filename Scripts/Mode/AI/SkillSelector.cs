// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-03-25 15:49:01
// 版 本：1.0
// ========================================================
using System;
using UnityEngine;
using System.Collections.Generic;
public class SkillSelector
{
    private Role _role;
    private AITplData _aiTpl;
    private Dictionary<int, SkillTplData> _skillTpls;
    private float releaseBuffSkillTime;
    private SkillTplData lastSkill;

    public SkillSelector(Role role)
    {
        releaseBuffSkillTime = 0.0f;
        _role = role;
        _aiTpl = _role.aiTpl;
        if (_aiTpl == null)
        {
            Debug.Log(string.Format("找不到AI配置，角色ID={0}", _role.GetRoleInfo().Tpl.ID));
        }
        _skillTpls = new Dictionary<int, SkillTplData>();
        for (int i = 0; i < _aiTpl.Skills.Length; i++)
        {
            int skId = _aiTpl.Skills[i];
            if (!_skillTpls.ContainsKey(skId))
            {
                _skillTpls.Add(skId, TemplateManager.GetSkillTplData(skId));
            }
        }
    }
    public void Update()
    {
        releaseBuffSkillTime -= Time.deltaTime;
    }
    public int SelectSkill(Role target)
    {
        if(target != null)
        {
            if (_aiTpl.Bloodlowskill != 0)
            {
                if ((float)_role.GetRoleInfo().Hp/(float)_role.GetRoleInfo().HpTotal < _aiTpl.Bloodlow*0.01f)
                {
                    return _aiTpl.Bloodlowskill;
                }
            }
            int skillId = SelectRandomSkill(target);
            return skillId;
        }
        return -1;
    }
    private int SelectRandomSkill(Role target)
    {
        Vector2 targetAttackPt = target.GetAttackablePosition(_role);
        int skillId = _aiTpl.Skills[UnityEngine.Random.Range(0, _aiTpl.Skills.Length)];
        SkillTplData tplData = _skillTpls[skillId];
        float distance = Vector2.Distance(targetAttackPt, _role.transform.position);
        if (tplData.Type == Skill.TYPE_BUFF_ONLY && (tplData.Targettype == Skill.TARGET_TYPE_SELF_TEAM || tplData.Targettype == Skill.TARGET_TYPE_SELF))
        {
            if(distance < _role.GetRoleInfo().SeeDistance)
            {
                if(releaseBuffSkillTime > 0 && lastSkill == tplData)
                {
                    return -1;
                }
                if(releaseBuffSkillTime > 2.0f && lastSkill != tplData)
                {
                    return -1;
                }
                lastSkill = tplData;
                releaseBuffSkillTime = tplData.Interval;
                return tplData.ID;
            }
            return -1;
        }
        if (tplData.Type == Skill.TYPE_MAGIC && target is Wall)//如果是城墙，优先使用物理攻击
        {
            foreach (var item in _skillTpls)
            {
                if(item.Value.Type == Skill.TYPE_PHYSICS || item.Value.Type == Skill.TYPE_PHYSICS_FAR) {
                    return -1;
                }
            }
        }

        if (distance < tplData.Distance && 
        Mathf.Abs(targetAttackPt.y - _role.transform.position.y) < _role.GetRoleInfo().Tpl.Vmaxdistance &&
            distance > _role.GetRoleInfo().MinDistance)
        {
            //if (InHurtZone(_role, target, tplData))
            //{
            //    lastSkill = tplData;
            //    return tplData.ID;
            //}
            lastSkill = tplData;
            return tplData.ID;
        }
        return -1;
    }
    public static bool InHurtZone(Role executor, Role target, SkillTplData skillTpl)
    {
        if (skillTpl.Affectdistance.Length > 1)
        {
            int direction = executor.getOrientation() == Role.Orientation.RIGHT ? 1 : -1;
            Vector2 rolePt = executor.transform.position;
            Vector2 targetCheckPt = target.GetAttackablePosition(executor);
            if (skillTpl.Affectdistance[0] == 2)//圆形区域
            {
                return Vector2.Distance(rolePt, targetCheckPt) < skillTpl.Affectdistance[1];
            }
            else
            {
                Vector2 nearT = new Vector2(rolePt.x + skillTpl.Affectdistance[1] * direction * 0.01f, rolePt.y + skillTpl.Affectdistance[2] * 0.01f);
                Vector2 farB = new Vector2(rolePt.x + skillTpl.Affectdistance[3] * direction * 0.01f, rolePt.y + skillTpl.Affectdistance[4] * 0.01f);
                if (direction == -1)
                {
                    float temp = nearT.x;
                    nearT.x = farB.x;
                    farB.x = temp;
                }
                if (targetCheckPt.x > nearT.x && targetCheckPt.x < farB.x && targetCheckPt.y > farB.y && targetCheckPt.y < nearT.y)
                {
                    return true;
                }
            }
        }
        else
        {
            Debug.LogError("技能没有配置受击区域,skillId=" + skillTpl.ID);
        }
        return false;
    }
}
