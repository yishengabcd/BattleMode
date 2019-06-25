// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-03-15 18:36:13
// 版 本：1.0
// ========================================================
using System;
using System.Collections.Generic;
using UnityEngine;
public class MoveCommand : Command
{
    private MoveAction _action;
    private List<Vector2> _path;

    public MoveCommand(Role role, List<Vector2> path) : base(role)
    {
        _path = path;
    }

    public override bool CancelCommand()
    {
        if (_action != null)
        {
            _action.Cancel();
        }
        SetFinished(true);
        return true;
    }

    public override void Update()
    {
        if(!_started)
        {
            if(_role.CanAction(true))
            {
                start();
                _action = new MoveAction(_role, _path);
                _role.AddAction(_action);
            }
        }
    }
    public override bool Finished
    {
        get
        {
            if (_action != null)
            {
                return _action.Finished;
            }
            return base.Finished;
        }
    }
}