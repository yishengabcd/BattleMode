// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-04-14 14:04:01
// 版 本：1.0
// ========================================================
using UnityEngine;
using System.Collections.Generic;
public class RunToProtectPartnerAction : BaseRoleAction
{
    private Role _partner;
    private int _moveInPathCount;
    private MoveAction _moveProxy;
    private int _checkDistanceCount;
    private int _checkDistanceStep;
    private int _moveProxyTime;
    private ProtectPartnerDuty _duty;
    public RunToProtectPartnerAction(Role role, Role partner, ProtectPartnerDuty duty):base(role)
    {
        _partner = partner;
        _checkDistanceCount = 0;
        _checkDistanceStep = 10 + UnityEngine.Random.Range(0, 30);
        _moveProxyTime = 10 + UnityEngine.Random.Range(0, 5);
        _duty = duty;
    }
    public override void Update()
    {
        if (!_partner.Alive)
        {
            _role.PlayMotion(Role.MOTION_STAND);
            SetFinished(true);
            return;
        }
        if (_moveProxy != null && _moveInPathCount < _moveProxyTime && _moveProxy.Finished == false)
        {
            _moveProxy.Update();
            if (Vector2.Distance(_role.transform.position, _partner.transform.position)<1f)
            {
                SetFinished(true);
            }
            _moveInPathCount++;
            return;
        }
        _moveProxy = null;
        _moveInPathCount = 0;

        Vector2 targetPt = _partner.transform.position;

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
        if (Vector2.Distance(_role.transform.position, _partner.transform.position) < 1f)
        {
            SetFinished(true);
        }
    }
    public override bool Replace(IAction act)
    {
        //if (act.Type == Type)
        //{
        //    return false;
        //}
        return true;
    }
    public override string Type => "RunToProtectPartnerAction";
}
