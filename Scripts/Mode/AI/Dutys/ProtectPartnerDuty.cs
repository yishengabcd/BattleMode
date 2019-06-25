// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-04-14 12:39:19
// 版 本：1.0
// ========================================================
using UnityEngine;
using System.Collections.Generic;
public class ProtectPartnerDuty:Duty
{
    private int _targetTplId;
    private int _near;
    private int _far;
    private float _centerDis;
    private float _outDis;
    private Role _target;

    private int _checkCount;
    private int _checkStep;
    private Vector2 _lastPt;

    private int _stateRefreshCount;

    //private RunToProtectPartnerAction _runToAction;
    private PursueAction _pursueAction;

    public ProtectPartnerDuty(Role worker, int targetTplId, int near, int far):base(worker)
    {
        _targetTplId = targetTplId;
        _near = near;
        _far = far;
        _checkCount = 0;
        _stateRefreshCount = 0;
         _checkStep = Random.Range(18,26);//错开检测方法
        _centerDis = _near + (_far - _near) * 0.5f;
        _outDis = _far + _far * 0.5f;
    }

    public override void Update()
    {
        _checkCount++;
        if(_target == null && _checkCount > _checkStep)
        {
            List<Role> roles = worker.Map.FindZoneRoles(worker.transform.position,worker.Side, (int)worker.SeeDistance, 1, _targetTplId);
            if (roles.Count > 0)
            {
                _target = roles[0];
                RefreshState();
            }

            _checkCount = 0;
        }

        if (_target != null)
        {
            _stateRefreshCount++;
            if(_stateRefreshCount > 10)
            {
                RefreshState();
                _stateRefreshCount = 0;
            }
        }
    }
    private void RefreshState()
    {
        if (_target.transform == null && worker.transform == null) return;
        float dis = Vector2.Distance(_target.transform.position, worker.transform.position);
        if (_pursueAction != null && !_pursueAction.Finished)
        {
            if (Vector2.Distance(worker.transform.position, _pursueAction.TargetRole.transform.position) < worker.SeeDistance)
            {
                _pursueAction.Cancel();
                _pursueAction = null;

                state = STATE_FREE;
            }
            else if (Vector2.Distance(_target.transform.position, _pursueAction.TargetRole.transform.position) > SystemConsts.IN_HARMFUL_DIS * 1.5f)
            {
                _pursueAction.Cancel();
                _pursueAction = null;
                state = STATE_FREE;
            }
        }
        else
        {
            Role enemy = _target.GetHarmfulEnemy();

            if (enemy != null)
            {
                if (Vector2.Distance(worker.transform.position, enemy.transform.position) > worker.SeeDistance)
                {
                    _pursueAction = new PursueAction(worker, enemy, 100f);
                    worker.AddAction(_pursueAction);
                    state = STATE_ON_DUTY;
                }
                else
                {
                    state = STATE_FREE;
                }
            }
            else
            {
                state = STATE_FREE;
            }
        }
    }
}
//以下代码逻辑有点复杂，绕不出来，重新改写成简化后的版本
//using UnityEngine;
//using System.Collections.Generic;
//public class ProtectPartnerDuty : Duty
//{
//    private int _targetTplId;
//    private int _near;
//    private int _far;
//    private float _centerDis;
//    private Role _target;

//    private int _checkCount;
//    private int _checkStep;

//    private int _stateRefreshCount;

//    private RunToProtectPartnerAction _runToAction;
//    private PursueAction _pursueAction;

//    public ProtectPartnerDuty(Role worker, int targetTplId, int near, int far) : base(worker)
//    {
//        _targetTplId = targetTplId;
//        _near = near;
//        _far = far;
//        _checkCount = 0;
//        _stateRefreshCount = 0;
//        _checkStep = Random.Range(18, 26);//错开检测方法
//        _centerDis = _near + (_far - _near) * 0.5f;
//    }

//    public override void Update()
//    {
//        _checkCount++;
//        if (_target == null && _checkCount > _checkStep)
//        {
//            List<Role> roles = worker.Map.FindZonePartners(worker, (int)worker.SeeDistance, 1);
//            if (roles.Count > 0) _target = roles[0];

//            RefreshState();
//            _checkCount = 0;
//        }

//        if (_target != null)
//        {
//            _stateRefreshCount++;
//            if (_stateRefreshCount > 10)
//            {
//                RefreshState();
//                _stateRefreshCount = 0;
//            }
//        }
//    }
//    private void RefreshState()
//    {
//        float dis = Vector2.Distance(_target.transform.position, worker.transform.position);

//        if (_runToAction != null && !_runToAction.Finished)
//        {
//            if (dis < 0.2f)
//            {
//                _runToAction.Cancel();
//                _runToAction = null;
//            }
//            else if (dis < _centerDis)
//            {
//                state = STATE_ON_DUTY;

//                List<Role> enemies = worker.Map.FindZoneRoles(worker.transform.position, worker.OppositeSide, 3, 1);
//                if (enemies.Count > 0)
//                {
//                    _runToAction.Cancel();
//                    _runToAction = null;

//                    _pursueAction = new PursueAction(worker, enemies[0]);
//                    worker.AddAction(_pursueAction);
//                }
//                else
//                {
//                    if (!_target.InDanger)
//                    {
//                        state = STATE_FREE;
//                        _runToAction.Cancel();
//                        _runToAction = null;
//                    }
//                }
//            }
//            return;
//        }
//        if (_pursueAction != null)
//        {
//            return;
//        }

//        if (state == STATE_FREE)
//        {
//            if (_target.InDanger)
//            {
//                if (dis > _far)
//                {
//                    state = STATE_REQUIRE;
//                }
//                else
//                {
//                    state = STATE_ON_DUTY;
//                    List<Role> enemies = worker.Map.FindZoneRoles(worker.transform.position, worker.OppositeSide, 3, 1);
//                    if (enemies.Count > 0)
//                    {
//                        _pursueAction = new PursueAction(worker, enemies[0]);
//                        worker.AddAction(_pursueAction);
//                    }
//                    else
//                    {
//                        _runToAction = new RunToProtectPartnerAction(worker, _target, this);
//                        worker.AddAction(_runToAction);
//                    }
//                }
//            }
//        }
//        else if (state == STATE_ON_DUTY)
//        {

//        }
//        else if (state == STATE_REQUIRE)
//        {

//        }
//    }
//}