// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-03-16 20:15:24
// 版 本：1.0
// ========================================================
using System;
using UnityEngine;
using System.Collections.Generic;
public class MonsterMarchAction:BaseRoleAction
{
    private Wall _target;
    private Vector2 _destPosition;
    private int _moveInPathCount;
    private MoveAction _moveProxy;
    public MonsterMarchAction(Role role, Wall target):base(role)
    {
        _target = target;
        _destPosition = target.GetAttackablePosition(role);
    }
    public override void Update()
    {
        if (!_target.Alive)
        {
            _role.PlayMotion(Role.MOTION_STAND);
            SetFinished(true);
            return;
        }
        if (_moveProxy != null && _moveInPathCount < 20 && _moveProxy.Finished == false)
        {
            _moveProxy.Update();
            _moveInPathCount++;
            return;
        }
        _moveProxy = null;
        _moveInPathCount = 0;

        float step = _role.MarchSpeed * Time.deltaTime;
        Vector2 from = _role.transform.position;
        Vector2 to = Vector2.MoveTowards(from, _destPosition, step);
        if (_role.Map.CanMoveTo(to))
        {
            MoveTo(from, to);
        }
        else
        {
            Vector2 dest = new Vector2(from.x - 4.0f, _destPosition.y);//每次向前行400像素，避免全地图寻路.
            List<Vector2> path = _role.Map.FindPath(from, dest);
            if (path.Count > 1)
            {
                _moveProxy = new MoveAction(_role, path, true);
                _moveProxy.Update();
            }
        }
    }
    private void MoveTo(Vector2 from, Vector2 to)
    {
        if (_role.CurrentMotion != Role.MOTION_RUN)
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
    }
    public override bool Replace(IAction act)
    {
        return true;
    }
    public override bool Filter(IAction act)
    {
        if(act.Type == Type)
        {
            return true;
        }
        return base.Filter(act);
    }
    public override string Type => "MonsterMarchAction";
}
