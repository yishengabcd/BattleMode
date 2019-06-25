// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-04-19 11:42:06
// 版 本：1.0
// ========================================================
using System;
using UnityEngine;
using System.Collections.Generic;
public class MatRunToPosition: BaseRoleAction
{
    private int _moveInPathCount;
    private MoveAction _moveProxy;
    private int _moveProxyTime;
    public MatRunToPosition(Role role) : base(role)
    {
        _moveProxyTime = 3 + UnityEngine.Random.Range(0, 5);
    }
    public override void Update()
    {
        if (_role.Matrix == null)
        {
            OnFinished();
            return;
        }
        if (_moveProxy != null && _moveInPathCount < _moveProxyTime && _moveProxy.Finished == false)
        {
            _moveProxy.Update();
            if (Vector2.Distance(_role.transform.position, _role.Matrix.GetMatrixPosition(_role)) < 0.1f)
            {
                OnFinished();
            }
            _moveInPathCount++;
            return;
        }
        _moveProxy = null;
        _moveInPathCount = 0;

        Vector2 targetPt = _role.Matrix.GetMatrixPosition(_role);

        float step = _role.MoveSpeed * Time.deltaTime;
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
                _moveProxy = new MoveAction(_role, path);
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
        if (Vector2.Distance(_role.transform.position, _role.Matrix.GetMatrixPosition(_role)) < 0.1f)
        {
            OnFinished();
        }
    }
    private void OnFinished()
    {
        _role.PlayMotion(Role.MOTION_STAND);
        _role.setOrientation(Role.Orientation.LEFT);
        SetFinished(true);
    }
    public override void Cancel()
    {
        base.Cancel();
    }
    public override bool Replace(IAction act)
    {
        return true;
    }
    public override string Type => "MatRunToPosition";
}
