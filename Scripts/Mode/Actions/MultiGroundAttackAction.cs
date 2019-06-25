// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-03-18 20:36:02
// 版 本：1.0
// ========================================================
using System;
using UnityEngine;
using System.Collections.Generic;
public class MultiGroundAttackAction:BaseRoleAction
{
    private Role _target;
    private Skill _skill;
    private SkillConfigData _frameData;
    private float _pastTime = 0.0f;
    private int _frameCount;
    private int _hurtFrequency;
    private int _hurtTotalNum;
    private int _hurtCount;
    private int _startHurtFrame;
    private Vector2 _releasePt;
    private Vector2 _lockPosition;
    private bool _inited = false;
    private float _speedScale;
    public MultiGroundAttackAction(Role attacker, Role target, Skill skill, SkillConfigData frameData, Vector2 lockPosition):base(attacker)
    {
        _target = target;

        _skill = skill;
        _frameData = frameData;
        _lockPosition = lockPosition;

        _startHurtFrame = int.Parse(_frameData.Others[0]);
        _hurtFrequency = int.Parse(_frameData.Others[1]);
        _hurtTotalNum = int.Parse(_frameData.Others[2]);


        _frameCount = 0;
        _hurtCount = 0;
        _speedScale = _skill.SkillTpl.ID == 4?_role.ActionSpeed : 1f;//只有技能4需要根据释放速度来调节速度
    }
    public override void Update()
    {
        if(_inited == false) {
            _inited = true;
            if (_frameData.Others.Length > 5)//英雄跳起攻击技能走的是这里
            {
                int direction = _role.getOrientation() == Role.Orientation.RIGHT ? 1 : -1;
                _releasePt.x = _role.transform.position.x + float.Parse(_frameData.Others[4]) * direction;
                _releasePt.y = _role.transform.position.y + float.Parse(_frameData.Others[5]);
            }
            else
            {
                if (_target != null)
                {
                    if (!Mathf.Approximately(Vector2.Distance(_lockPosition, Vector2.zero), 0f))
                    {
                        _releasePt = _lockPosition;
                    }
                    else
                    {
                        _releasePt = _target.GetAttackablePosition(_role);
                    }
                }
                else
                {
                    _releasePt = Vector2.zero;
                }
            }
            if(_frameData.Effectname != "")
            {
                Vector3 pt3 = _releasePt;
                pt3.z = _releasePt.y - 0.4f;
                _role.Map.addEffect(SystemConsts.SKILL_EFF_PREFIX + _frameData.Effectname, pt3, _role, true,0,false);
            }
        }
        _pastTime += Time.deltaTime * _speedScale;
        int frame = Mathf.FloorToInt(_pastTime * 60f) + 1;
        while(_frameCount < frame) {
            _frameCount++;
            int startedFrame = _frameCount - _startHurtFrame;
            if (startedFrame > -1 && startedFrame % _hurtFrequency == 0)
            {
                MakeDamage();
                if (_hurtCount >= _hurtTotalNum)
                {
                    SetFinished(true);
                    break;
                }
            }
        }
    }
    private void MakeDamage()
    {
        _hurtCount++;

        List<Role> targets = _role.Map.GetRolesBySide(_role.Side == Role.SIDE_LEFT?Role.SIDE_RIGHT:Role.SIDE_LEFT);
        foreach (var item in targets)
        {
            if (item.Alive && _skill.InBombZone(_releasePt, item.GetAttackablePosition(_role)))
            {
                Damage damage = item.DoSkillAffect(_skill);
                if (!damage.isMiss && _frameData.Others.Length > 3)
                {
                    string hitEffect = _frameData.Others[3];
                    float offsetX = UnityEngine.Random.Range(-0.2f,0.2f);
                    float offsetY = UnityEngine.Random.Range(-0.2f, 0.2f);
                    item.addHitEffect(hitEffect, offsetX, offsetY, item);
                }
            }
        }
    }
    public override string Type => "MultiGroundAttackAction";
}
