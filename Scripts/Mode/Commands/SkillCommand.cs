// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-03-13 09:02:31
// 版 本：1.0
// ========================================================
using System;
using UnityEngine;
public class SkillCommand:Command
{
    private Skill _skill;
    private bool _canRelease;
    private Role _target;
    public SkillCommand(Role role, int skillId, Vector2 pt, Role target = null) :base(role)
    {
        _target = target;
        _skill = SkillFactory.Create(skillId, role, pt, _target);

        _canRelease = false;
    }
    public override void Update()
    {
        if (!_started)
        {
            _canRelease = _role.CanAction(true);
            if (_canRelease && !_skill.ReleaseStarted)
            {
                UserSkillMgr.SkillReleased();
                _skill.Release();
                start();
            }
        }
    }

    public override bool CancelCommand()
    {
        UserSkillMgr.CancelSkill(_skill.SkillTpl.ID);
        _skill.Cancel();
        return true;
    }

    public override bool Finished => _skill.Finished;
    public override string Type => "SkillCommand";
}
