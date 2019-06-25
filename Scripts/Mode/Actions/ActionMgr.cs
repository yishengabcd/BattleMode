using System;
using System.Collections.Generic;
using UnityEngine;
public class ActionMgr
{
    private IAction _current;
    private List<IAction> _actions;
    public ActionMgr()
    {
        _actions = new List<IAction>();
    }
    public void Update()
    {
        if(_current == null)
        {
            ShiftAction();
        }
        if(_current == null)
        {
            return;
        }
        if(_current.Finished)
        {
            _current.Dispose();
            _current = null;
            if(_actions.Count <=0)
            {
                //dispatchEvent(new ActionsEvent(ActionsEvent.ACTION_ALL_COMPLETE));
            }
        }
        else
        {
            _current.Update();
            if (_current != null && _current.Finished)
            {
                _current.Dispose();
                _current = null;
                if(_actions.Count == 0)
                {
                    //dispatchEvent(new ActionsEvent(ActionsEvent.ACTION_ALL_COMPLETE));
                }
            }
        }
    }
    protected void ShiftAction()
    {
        if(_actions.Count > 0)
        {
            _current = _actions[0];
            _actions.RemoveAt(0);
            _current.Prepare();
        }
    }
    public void Synchronization()
    {
        if (_current != null) _current.Synchronization();
        IAction act = null;
        while(_actions.Count > 0)
        {
            act = _actions[_actions.Count];
            act.Prepare();
            act.Synchronization();
            _actions.RemoveAt(_actions.Count - 1);
        }
    }
    public int Size => _actions.Count;
    public IAction Current => _current;
    public void AddAction(IAction act, bool immediately = false)
    {
        if (_current != null && !_current.Finished)
        {
            if(_current.Filter(act))
            {
                act.Cancel();
                act.Dispose();
                return;
            }
            if(_current.Replace(act))
            {
                _current.Cancel();
                _current.Dispose();
                _current = null;
            }
        }
        IAction temp = null;
        for (int i = 0; i < _actions.Count; i++)
        {
            temp = _actions[i];
            if (temp.Filter(act))
            {
                act.Cancel();
                act.Dispose();
                return;
            }
            if (temp.Replace(act))
            {
                temp.Cancel();
                temp.Dispose();
                temp = null;
                _actions[i] = act;
            }
        }
        if(immediately)
        {
            _actions.Insert(0, act);
        }
        else
        {
            _actions.Add(act);
        }
    }
    public void CancelActionByType(string type)
    {
        for (int i = 0; i < _actions.Count; i++)
        {
            IAction act = _actions[i];
            if(act.Type == type)
            {
                act.Cancel();
            }
        }
        if(_current != null && _current.Type == type)
        {
            _current.Cancel();
        }
    }
    public List<IAction> GetActions()
    {
        return _actions;
    }
    public void CleanActions()
    {
        if(_current != null)
        {
            _current.Cancel();
            //_current = null;解开注释会导致遍历时报错
        }
        if(_actions != null)
        {
            foreach (var item in _actions)
            {
                item.Cancel();
            }
        }
        _actions = new List<IAction>();
    }
    public IAction GetFirstActionByType(string type)
    {
        if(_current != null)
        {
            if(_current.Type == type)
            {
                return _current;
            }
        }
        foreach (var item in _actions)
        {
            if(item.Type == type)
            {
                return item;
            }
        }
        return null;
    }
    public void Dispose()
    {
        for (int i = 0; i < _actions.Count; i++)
        {
            IAction act = _actions[i];
            act.Dispose();
        }
        _actions = null;
        if(_current != null)
        {
            _current.Dispose(); _current = null;
        }
    }
}
