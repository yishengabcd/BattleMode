// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-04-19 19:19:39
// 版 本：1.0
// ========================================================
using UnityEngine;
public class MatFireBackAction : BaseRoleAction
{
    public MatFireBackAction(Role role) : base(role)
    {
        role.PlayMotion(Role.MOTION_STAND);
        role.setOrientation(Random.Range(0f, 1f) > 0.5f ? Role.Orientation.LEFT : Role.Orientation.RIGHT);
    }
    public override void Update()
    {
    }
    public override bool Replace(IAction act)
    {
        return true;
    }
    public override string Type => "MatStandByAction";
}
