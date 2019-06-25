// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-03-13 09:02:31
// 版 本：1.0
// ========================================================
using System;
public abstract class Command
{
    protected Role _role;
    protected bool _finished;
    protected bool _started;

    public virtual string Type => "";
    public virtual bool Finished => _finished;
    public bool Started => _started;

    public Command(Role role)
    {
        _role = role;
    }
    public void start()
    {
        _started = true;
    }
    public abstract bool CancelCommand();
    public abstract void Update();
    public void SetFinished(bool value)
    {
        _finished = value;
    }
    public Role GetRole()
    {
        return _role;
    }
}
