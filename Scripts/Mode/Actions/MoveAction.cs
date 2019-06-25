using System;
using UnityEngine;
using System.Collections.Generic;
public class MoveAction:BaseRoleAction
{
    protected List<Vector2> _path;
    private Vector2 _currentEndPoint;
    private Vector2 _startPoint;
    private bool _marching;
    private bool _started;
    private string _motion;
    public MoveAction(Role role, List<Vector2> path, bool marching = false, string motion = null) :base(role)
    {
        _path = path;
        _marching = marching;
        _motion = motion;
        if (_motion == null) _motion = Role.MOTION_RUN;
        path.RemoveAt(0);//移除第一个元素，这个是自身位置
        _started = false;
    }
    private void MoveNextPoint()
    {
        if(_path.Count > 0)
        {
            _role.PlayMotion(_motion, -1, null, false);
            _startPoint = _role.transform.position;
            _currentEndPoint = _path[0];
            _path.RemoveAt(0);

            if (_currentEndPoint.x < _startPoint.x)
            {
                _role.setOrientation(Role.Orientation.LEFT);
            }
            else
            {
                _role.setOrientation(Role.Orientation.RIGHT);
            }
        }
        else
        {
            _role.PlayMotion(Role.MOTION_STAND);
            SetFinished(true);
        }
    }
    public override void Update()
    {
        base.Update();
        if(!_started)
        {
            MoveNextPoint();
            _started = true;
        }
        float moveSpeed;
        if(_marching)
        {
            moveSpeed = _role.MarchSpeed;
        }
        else
        {
            moveSpeed = _role.MoveSpeed;
        }
        float step = moveSpeed * Time.deltaTime;
        Vector2 from = _role.transform.position;
        Vector2 to = Vector2.MoveTowards(_role.transform.position, _currentEndPoint, step);

        //_role.transform.position = to;
        _role.SetPosition(to);
        if(moveSpeed / 60 > Vector2.Distance(to, _currentEndPoint))//if(moveSpeed / Application.targetFrameRate > Vector2.Distance(to, _currentEndPoint))
        {
            MoveNextPoint();
        }
    }
    public override string Type => "MoveAction";
    public override bool Replace(IAction act)
    {
        return act.Type == Type;
    }
    public override void Cancel()
    {
        base.Cancel();
        //_role.PlayMotion(Role.MOTION_STAND);
    }
}
