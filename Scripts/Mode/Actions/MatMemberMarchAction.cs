// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-04-19 15:10:03
// 版 本：1.0
// ========================================================
using System;
using UnityEngine;
using System.Collections.Generic;
public class MatMemberMarchAction : BaseRoleAction
{
    private MatRunToPosition _runToPosition;
    private bool _started = false;
    public MatMemberMarchAction(Role role) : base(role)
    {
    }
    public override void Update()
    {
        if (!_role.Alive || _role.Matrix == null)
        {
            SetFinish();
            return;
        }
        if (_role.Matrix.State != Matrix.STATE_MARCHING)
        {
            SetFinish();
            return;
        }
        if (!_started)
        {
            _started = true;
            if (_role.Alive)
            {
                if (!_role.Matrix.MemberIsInPosition(_role))
                {
                    _runToPosition = new MatRunToPosition(_role);
                    //_role.AddAction(_runToPosition);
                }
                else
                {
                    MoveTo(_role.transform.position, _role.Matrix.GetMatrixPosition(_role));
                }
            }
        }
        else
        {
            if(_runToPosition != null)
            {
                _runToPosition.Update();
                if (_runToPosition.Finished)
                {
                    _runToPosition = null;
                }
            }
            if(_runToPosition == null)
            {
                MoveTo(_role.transform.position, _role.Matrix.GetMatrixPosition(_role));
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
    private void SetFinish()
    {
        if (_role.Alive)
        {
            _role.PlayMotion(Role.MOTION_STAND);
            _role.setOrientation(Role.Orientation.LEFT);
        }
        SetFinished(true);
    }
    public override bool Filter(IAction act)
    {
        if (act.Type == Type)
        {
            return true;
        }
        return base.Filter(act);
    }
    public override string Type => "MatMemberMarchAction";
}
