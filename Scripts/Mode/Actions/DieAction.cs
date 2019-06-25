// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-03-15 08:43:06
// 版 本：1.0
// ========================================================
using System;
using UnityEngine;
public class DieAction:BaseRoleAction
{
    private bool _started = false;
    public DieAction(Role role):base(role)
    {

    }
    public override void Update()
    {
        if(_started == false)
        {
            _started = true;
            if (!(_role is Wall))
            {
                _role.PlayMotion(Role.MOTION_DIE, 1);
                _role.addListener(RoleEvent.MOTION_COMPLETE, onDieComplete);
            }
            else
            {
                Wall wall = _role as Wall;
                wall.ShowDieView();
                SetFinished(true);
            }
        }
    }
    private void onDieComplete(EventBase e)
    {
        _role.removeListener(RoleEvent.MOTION_COMPLETE, onDieComplete);
        _role.RemoveFromMap();
        SetFinished(true);
    }
}
