using System;
public abstract class AbstractAction:IAction
{
    protected bool _finished;
    protected bool _paused;
    public AbstractAction()
    {
    }

    public virtual bool Finished => _finished;
    public void SetFinished(bool value)
    {
        _finished = value;
    }

    public virtual int Priority => 0;

    public virtual string Type => "AbstractAction";

    public virtual void Cancel()
    {
        _finished = true;
    }

    public virtual void Dispose()
    {
    }

    public virtual bool Filter(IAction act)
    {
        return false;
    }

    public virtual int GetCode()
    {
        return 0;
    }

    public virtual void Pause()
    {
        _paused = true;
    }

    public virtual void Prepare()
    {
    }

    public virtual bool Ready(int liftTime = 0)
    {
        return true;
    }

    public virtual bool Replace(IAction act)
    {
        return false;
    }

    public virtual void Resume()
    {
        _paused = false;
    }

    public virtual void Synchronization()
    {
    }

    public virtual void Update()
    {
    }
}
