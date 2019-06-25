// ========================================================
 // 描述：
// 作者：Yisheng 
// 创建时间：2019-03-25 22:19:46
// 版 本：1.0
// ========================================================
using System;
using UnityEngine;
public class PainAction:BaseRoleAction
{
    private float _last;
    private float _lastCount;
    public PainAction(Role role, Damage damage):base(role)
    {
        role.PlayMotion(damage.painMotion, 1, Role.MOTION_STAND);
        _last = damage.painTime;
        _lastCount = 0.0f;
    }
    public override void Update()
    {
        _lastCount += Time.deltaTime;
        if(_lastCount > _last)
        {
            SetFinished(true);
        }
    }
    public override bool Replace(IAction act)
    {
        return false;
    }
    public override bool Filter(IAction act)
    {
        if (Type == act.Type)
        {
            return true;
        }
        return base.Filter(act);
    }
    public override string Type => "PainAction";
}
