// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-03-15 15:16:45
// 版 本：1.0
// ========================================================
using System;
public class CommandMgr
{
    private Command _executing;
    private Command _nextCommand;
    public CommandMgr()
    {
    }
    public void Update()
    {
        if(_executing != null)
        {
            _executing.Update();
            if (_executing.Finished)
            {
                _executing = _nextCommand;
            }
        }
    }
    public void Execute(Command command)
    {
        if(_executing != null)
        {
            if(_executing.CancelCommand())
            {
                _executing = null;
                _executing = command;
                _nextCommand = null;
            }
            else
            {
                _nextCommand = command;
            }
        }
        else
        {
            _executing = command;
        }
    }
    public void CancelAll(bool keepUnreleased = false)
    {
        if(_executing != null)
        {
            if(!_executing.Started && keepUnreleased)
            {

            }
            else
            {
                _executing.CancelCommand();
                _executing = null;
            }

        }
        if(keepUnreleased == false)
        {
            _nextCommand = null;
        }
    }
    public Command getExecutingCommand()
    {
        return _executing;
    }
}
