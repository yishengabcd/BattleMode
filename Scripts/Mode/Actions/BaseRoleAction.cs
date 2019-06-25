// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-03-12 11:14:56
// 版 本：1.0
// ========================================================
using System;
using UnityEngine;
public class BaseRoleAction : AbstractAction
{
    protected Role _role;
    public BaseRoleAction(Role role)
    {
        _role = role;
    }
    public override string Type => "RoleAction";
    public Role GetRole()
    {
        return _role;
    }
}
