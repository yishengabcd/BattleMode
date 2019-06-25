// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-04-14 11:30:36
// 版 本：1.0
// ========================================================
using System;
public abstract class Duty
{
    public static int STATE_FREE = 1;//自由状态，在此状态下，角色可以无限制地行动
    public static int STATE_ON_DUTY = 2;//在尽职状态，在此状态下，外部不能改变角色的行为；
    public static int STATE_REQUIRE = 3;//要求尽职状态，在此状态下，角色只能做职责范围内的事情。

    protected int state;//状态

    protected Role worker;
    public Duty(Role worker)
    {
        this.worker = worker;
        state = STATE_FREE;
    }
    public abstract void Update();

    public int State => state;
    public Role Worker => worker;
}
