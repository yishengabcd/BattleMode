// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-04-08 18:51:12
// 版 本：1.0
// ========================================================
using System;
using System.Collections.Generic;
using UnityEngine;

public class PickupItemCommand:Command
{
    private MoveAction _action;
    private List<Vector2> _path;
    private MapItem mapItem;
    public PickupItemCommand(Role role, List<Vector2> path, MapItem mapItem):base(role)
    {
        _path = path;
        this.mapItem = mapItem;
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
        if (!_started)
        {
            if (_role.CanAction(true))
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
                if (_action.Finished) {

                    if (_role.Alive)
                    {
                        _role.Map.PickUpMapItem(mapItem);
                    }
                    return true;
                }

                return false;
            }
            return base.Finished;
        }
    }
}