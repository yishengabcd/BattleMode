// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-03-20 14:25:19
// 版 本：1.0
// ========================================================
using System;
using UnityEngine;
using System.Collections.Generic;
public class SkillInjection : Skill
{
    private int _moveInPathCount;
    private MoveAction _moveProxy;
    private int _phase;
    private static readonly int PHASE_PURSUE = 1;
    private static readonly int PHASE_HIT = 2;
    private static readonly string RUN_MOTION = "run2";
    private int _frameCount = 0;
    private GameObject _drillEf;

    public SkillInjection(int skillId, Role executor,Vector2 pt, Role target = null) : base(skillId, executor,pt, target)
    {
        _phase = PHASE_PURSUE;
    }
    public override void Update()
    {
        _frameCount++;
        if (!_targetRole.Alive)
        {
            _role.PlayMotion(Role.MOTION_STAND);
            //SetFinished(true);
            FinishSkill();
            return;
        }
        if (_phase == PHASE_PURSUE && TryToHit())
        {
            return;
        }

        if (_phase == PHASE_PURSUE)
        {
            if (_frameCount == 1)
            {
                _drillEf = _role.AddEffect(SystemConsts.SKILL_EFF_PREFIX + "Hero/HrInjectDrill", new Vector2(0.32f, 0.98f));
                _role.PlayMotion(RUN_MOTION, 1000);
                _role.SetImmunity(true);
            }
            int moveProxyTime = 10;
            if (_moveProxy != null && _moveInPathCount < moveProxyTime && _moveProxy.Finished == false)
            {
                _moveProxy.Update();
                _moveInPathCount++;
                TryToHit();
                return;
            }
            _moveProxy = null;
            _moveInPathCount = 0;

            Vector2 targetPt = _targetRole.GetAttackablePosition(_role);

            float step = _role.MoveSpeed * 2 * Time.deltaTime;
            Vector2 from = _role.transform.position;
            Vector2 to = Vector2.MoveTowards(from, targetPt, step);
            if (_role.Map.CanMoveTo(to))
            {
                MoveTo(from, to);
            }
            else
            {
                List<Vector2> path = _role.Map.FindPath(from, targetPt);
                if (path.Count > 1)
                {
                    _moveProxy = new MoveAction(_role, path, false, RUN_MOTION);
                    _moveProxy.Update();
                }
                else
                {
                    SetFinished(true);
                    Debug.LogError(string.Format("找不到追击路径：from:x={0},y={1},to:x={2},y={3}", from.x, from.y, targetPt.x, targetPt.y));
                }
            }
        }
        else
        {
            if(_frameCount == 25)
            {
                if (InHurtZone(_role.transform.position, _targetRole))
                {
                    _targetRole.DoSkillAffect(this);
                }
                _role.SetImmunity(false);
                _role.AddBuff(Buff.HERO_INJECTION_AFTER_BUFF, _role);
            }
            else if(_frameCount > 35)
            {
                FinishSkill();
            }
        }
    }
    private void MoveTo(Vector2 from, Vector2 to)
    {
        if (to.x < from.x)
        {
            _role.setOrientation(Role.Orientation.LEFT);
        }
        else
        {
            _role.setOrientation(Role.Orientation.RIGHT);
        }
        _role.SetPosition(to);
        TryToHit();
    }
    private void RemoveDrillEffect()
    {
        if (_drillEf != null)
        {
            UnityEngine.Object.Destroy(_drillEf);
            _drillEf = null;
        }
    }
    private bool TryToHit()
    {
        Vector2 targetPt = _targetRole.GetAttackablePosition(_role);
        float dis = Vector2.Distance(_role.transform.position, targetPt);
        if (dis < 1.2f && Mathf.Abs(targetPt.y - _role.transform.position.y) < 0.1f)
        {
            _phase = PHASE_HIT;
            RemoveDrillEffect();
            _role.PlayMotion("inject", 1, Role.MOTION_STAND);
            _frameCount = 0;
            //_role.AddEffect(SystemConsts.SKILL_EFF_PREFIX + "Hero/HrDrillHit", new Vector2(0.72f,0.8f));

            Vector2 pos = new Vector2(0.72f, 0.8f);
            Vector3 dest = _role.transform.position;
            dest.y += pos.y;
            dest.z = dest.y - 2.0f;
            _role.Map.addEffect(SystemConsts.SKILL_EFF_PREFIX + "Hero/HrDrillHit", dest, _role, true, pos.x);
            return true;
        }
        return false;
    }
    public override void Cancel()
    {
        base.Cancel();
        RemoveDrillEffect();
    }
    public override bool Replace(IAction act)
    {
        return true;
    }
    public override string Type => "SkillInjection";
    public override void Release()
    {
        base.Release();
    }
}
