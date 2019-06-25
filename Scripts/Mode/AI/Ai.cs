// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-03-16 11:13:33
// 版 本：1.0
// ========================================================
using System;
public abstract class Ai
{
    public Ai(Role _role)
    {
    }
    public abstract void Update();
    public abstract bool TryReleaseSkill(Role target = null);
}
