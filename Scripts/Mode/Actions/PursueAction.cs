// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-03-16 09:17:21
// 版 本：1.0
// ========================================================
using System;
using UnityEngine;
using System.Collections.Generic;
public class PursueAction:BaseRoleAction
{
    private Role _target;
    private int _moveInPathCount;
    private MoveAction _moveProxy;
    private int _tryReleaseSkillCount;
    private int _tryReleaseStep;
    private int _checkDistanceCount;
    private int _checkDistanceStep;
    private bool _locked;
    private float _pursueRange;
    public PursueAction(Role pursuer, Role target, float pursueRange):base(pursuer)
    {
        _target = target;
        _pursueRange = pursueRange;

        _tryReleaseSkillCount = 0;
        _checkDistanceCount = 0;
        _tryReleaseStep = 10 + UnityEngine.Random.Range(0,15);
        _checkDistanceStep = 10 + UnityEngine.Random.Range(0, 30);
        _locked = false;
    }
    public override void Update()
    {
        if (!_target.Alive)
        {
            _role.PlayMotion(Role.MOTION_STAND);
            SetFinished(true);
            return;
        }
        int moveProxyTime = _locked ? (int)_role.GetRoleInfo().MinDistance*20+UnityEngine.Random.Range(0,5) : 10+ UnityEngine.Random.Range(0, 5);
        if (_moveProxy != null && _moveInPathCount < moveProxyTime && _moveProxy.Finished == false)
        {
            _moveProxy.Update();
            _moveInPathCount++;
            _tryReleaseSkillCount++;
            return;
        }
        if (_role.Matrix != null && !_role.Matrix.Arrived &&  _role.Matrix.IsOutPursueRange(_role))
        {
            _role.PlayMotion(Role.MOTION_STAND);
            SetFinished(true);
            return;
        }
        _moveProxy = null;
        _moveInPathCount = 0;
        _locked = false;

        Vector2 targetPt = _target.GetAttackablePosition(_role);
        float offsetX = (_role.transform.position.x > targetPt.x ? 1 : -1) * (_role.GetRoleInfo().MinDistance+0.1f);
        targetPt.x += offsetX;

        float step = _role.MoveSpeed * Time.deltaTime;
        Vector2 from = _role.transform.position;
        Vector2 to = Vector2.MoveTowards(from, targetPt, step);
        if(_role.Map.CanMoveTo(to))
        {
            MoveTo(from, to);
        }
        else
        {
            List<Vector2> path = _role.Map.FindPath(from, targetPt);
            if(path.Count > 1)
            {
                _moveProxy = new MoveAction(_role, path);
                _moveProxy.Update();
            }
            else
            {
                targetPt.x = targetPt.x - offsetX * 2;//尝试另外一侧
                path = _role.Map.FindPath(from, targetPt);
                {
                    if (path.Count > 1)
                    {
                        _moveProxy = new MoveAction(_role, path);
                        _moveProxy.Update();
                        _locked = true;
                    }
                    else
                    {
                        SetFinished(true);
                        Debug.LogError(string.Format("找不到追击路径：from:x={0},y={1},to:x={2},y={3}", from.x, from.y, targetPt.x, targetPt.y));
                    }
                }
            }
        }
    }
    private void MoveTo(Vector2 from, Vector2 to)
    {
        if(_role.CurrentMotion != Role.MOTION_RUN)
        {
            _role.PlayMotion(Role.MOTION_RUN);
        }
        if (to.x < from.x)
        {
            _role.setOrientation(Role.Orientation.LEFT);
        }
        else
        {
            _role.setOrientation(Role.Orientation.RIGHT);
        }
        _role.SetPosition(to);
        bool releaseSkill = false;
        if(_tryReleaseSkillCount > _tryReleaseStep)
        {
            _tryReleaseSkillCount = 0;
            releaseSkill =  _role.TryReleaseSkill(_target);
        }
        if(!releaseSkill)
        {
            _checkDistanceCount++;
            if (_checkDistanceCount >= _checkDistanceStep)
            {
                if (_role.Matrix != null && !_role.Matrix.Arrived)
                {
                    //_role.PlayMotion(Role.MOTION_STAND);
                    SetFinished(true);
                    return;
                }
                else
                {
                    float dis = Vector2.Distance(_role.transform.position, _target.GetAttackablePosition(_role));
                    if (_pursueRange < dis)
                    {
                        if(_role.Matrix == null)
                        {
                            _role.PlayMotion(Role.MOTION_STAND);
                        }
                        SetFinished(true);
                    }
                }
            }
        }

    }
    public Role TargetRole => _target;
    public override bool Replace(IAction act)
    {
        if(act.Type == Type && _locked)
        {
            return false;
        }
        return true;
    }
    //public override bool Filter(IAction act)
    //{
    //    if (act.Type == Type)
    //    {
    //        PursueAction realAct = act as PursueAction;
    //        if(realAct.TargetRole == _target)
    //        {
    //            return true;
    //        }
    //        else
    //        {
    //            return false;
    //        }
    //    }
    //    return base.Filter(act);
    //}
    public override string Type => "PursueAction";
}
