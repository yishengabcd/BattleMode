// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-03-15 23:15:30
// 版 本：1.0
// ========================================================
using System;
using UnityEngine;
public class MonsterAi:Ai
{
    private Monster _monster;
    private Hero _hero;
    private Wall _wall;
    private Role _attackingTarget;
    private SkillSelector _skillSelector;
    private AITplData _aiTpl;
    private Duty _duty;

    public MonsterAi(Monster monster):base(monster)
    {
        _monster = monster;
        _skillSelector = new SkillSelector(_monster);
        _aiTpl = _monster.aiTpl;
        if(_aiTpl.Protect.Length > 2)
        {
            _duty = new ProtectPartnerDuty(monster, _aiTpl.Protect[0], _aiTpl.Protect[1], _aiTpl.Protect[2]);
        }
    }

    public override bool TryReleaseSkill(Role target = null)
    {
        Skill skill = SelectSkill(target);
        if (skill != null)
        {
            skill.Release();
            return true;
        }
        return false;
    }

    public override void Update()
    {
        _skillSelector.Update();
        _hero = _monster.Map.hero;
        _wall = _monster.Map.wall;
        if (!_monster.Alive) return;
        if (_monster.ExecutingSkill != null)
        {
            if(_monster.aiTpl.Cancancelskill == 1)
            {
                if(_monster.ExecutingSkill.CanCancelPhase && _monster.ExecutingSkill.IsTargetRunAway())
                {
                    _monster.ExecutingSkill.Cancel();
                    _monster.AddAction(new RestAction(_monster, null, 0.05f));
                }
            }
            return;
        }
        if (_monster.CanAction())
        {
            if(_duty != null)
            {
                _duty.Update();
                if (_duty.State != Duty.STATE_FREE) return;
            }

            Role target = null;
            float distance = 0.0f;
            bool matrixLimited = false;
            if (_monster.Matrix != null)
            {
                if (_monster.Matrix.Arrived)
                {
                    if (_monster.Matrix.State == Matrix.STATE_BEATTACKING && _monster.aiTpl.Matrixjob != Matrix.JOB_ATTACK_WALL_ONLY)
                    {
                        matrixLimited = true;
                        AttackToHero();
                    }
                    else
                    {
                        if (_monster.aiTpl.Matrixjob != Matrix.JOB_FREE_ATTACT)
                        {
                            matrixLimited = true;
                            if (_monster.aiTpl.Matrixjob == Matrix.JOB_GUARD)
                            {
                                //if (_monster.Matrix.MemberIsInPosition(_monster))
                                //{

                                //}
                                //else
                                //{

                                //}
                            }
                            else if (_monster.aiTpl.Matrixjob == Matrix.JOB_ATTACK_WALL_ONLY)
                            {
                                if (_monster.Matrix.MemberIsInPosition(_monster))
                                {
                                    if (_wall != null && _wall.Alive)
                                    {
                                        target = _wall;
                                        Skill skill = null;
                                        bool GroupAllow = true;

                                        skill = SelectSkill(target);
                                        if (skill != null)
                                        {
                                            if (target.isLife && _monster.Group != null)
                                            {
                                                GroupMemberAction action = new GroupMemberAction(_monster, GroupMemberAction.USE_SKILL);
                                                GroupAllow = _monster.Group.MemberAction(action);
                                            }
                                            if (GroupAllow)
                                            {
                                                skill.Release();
                                            }
                                        }
                                        if (skill == null)
                                        {
                                            Debug.LogError("无法攻击城墙，monsterId=" + _monster.RoleId);
                                        }
                                    }
                                }
                                else if (!(_monster.CurrentAction is MatRunToPosition))
                                {
                                    _monster.AddAction(new MatRunToPosition(_monster));
                                }

                            }
                            else if (_monster.aiTpl.Matrixjob == Matrix.JOB_REMOTE_ATTACK)
                            {

                            }
                        }
                    }
                }
                else
                {
                    if (_monster.Matrix.State == Matrix.STATE_GROUP)
                    {
                        if (!_monster.Matrix.MemberIsInPosition(_monster))
                        {
                            if (!(_monster.CurrentAction is MatRunToPosition))
                            {
                                MatRunToPosition act = new MatRunToPosition(_monster);
                                _monster.AddAction(act);
                            }
                        }
                        matrixLimited = true;
                    }
                    else if (_monster.Matrix.State == Matrix.STATE_MARCHING)
                    {
                        matrixLimited = true;
                        if (!(_monster.CurrentAction is MatMemberMarchAction))
                        {
                            _monster.AddAction(new MatMemberMarchAction(_monster));
                        }
                    }
                    else if (_monster.Matrix.State == Matrix.STATE_BEATTACKING)
                    {
                        //matrixLimited = true;
                        //if (_monster.aiTpl.Matrixjob != Matrix.JOB_FREE_ATTACT)
                        //{

                        //}
                        matrixLimited = true;
                        AttackToHero();
                    }
                }

            }
            if (matrixLimited)
            {
                return;
            }
            if (_hero != null && _hero.Alive)
            {
                distance = Vector2.Distance(_hero.GetAttackablePosition(_monster), _monster.transform.position);
                if (distance < _monster.GetRoleInfo().SeeDistance)
                {
                    target = _hero;
                }
            }
            if (target == null)
            {
                if (_wall != null && _wall.Alive && _monster.aiTpl.Willattackwalk == 1)
                {
                    distance = Vector2.Distance(_wall.GetAttackablePosition(_monster), _monster.transform.position);
                    if (distance < _monster.GetRoleInfo().SeeDistance)
                    {
                        target = _wall;
                    }
                }
            }
            if (target != null)
            {
                AttackTo(target);
            }
            else
            {
                if (_wall != null && _wall.Alive && _monster.aiTpl.Willattackwalk == 1)
                {
                    if (!(_monster.CurrentAction is MonsterMarchAction))
                    {
                        IAction march = new MonsterMarchAction(_monster, _wall);
                        _monster.AddAction(march);
                    }
                }
            }
        }
    }
    private void AttackToHero()
    {
        if (!(_monster.CurrentAction is PursueAction))
        {
            if (_hero != null && _hero.Alive)
            {
                Role target = _hero;

                Skill skill = null;
                bool GroupAllow = true;

                skill = SelectSkill(target);
                if (skill != null)
                {
                    if (target.isLife && _monster.Group != null)
                    {
                        GroupMemberAction action = new GroupMemberAction(_monster, GroupMemberAction.USE_SKILL);
                        GroupAllow = _monster.Group.MemberAction(action);
                    }
                    if (GroupAllow)
                    {
                        skill.Release();
                    }
                }

                if (skill == null)
                {
                    if (_monster.aiTpl.Matrixjob != Matrix.JOB_REMOTE_ATTACK)
                    {
                        PursueAction action = new PursueAction(_monster, target, _monster.GetRoleInfo().PursueRange);
                        _monster.AddAction(action);
                    }
                }
            }
        }
    }
    private void AttackTo(Role target)
    {
        _attackingTarget = target;
        Skill skill = null;
        bool GroupAllow = true;

        skill = SelectSkill(target);
        if (skill != null)
        {
            if (target.isLife && _monster.Group != null)
            {
                GroupMemberAction action = new GroupMemberAction(_monster, GroupMemberAction.USE_SKILL);
                GroupAllow = _monster.Group.MemberAction(action);
            }
            if (GroupAllow)
            {
                _monster.SetFaceTo(target.transform.position);
                skill.Release();
            }
        }
        if(skill == null)
        {
            PursueAction action = new PursueAction(_monster, target, _monster.GetRoleInfo().PursueRange);
            _monster.AddAction(action);
        }
    }
    private Skill SelectSkill(Role target)
    {
        int skillId = _skillSelector.SelectSkill(target);
        if(skillId != -1)
        {
            return SkillFactory.Create(skillId, _monster,Vector2.zero, target); 
        }
        return null;
    }
}
