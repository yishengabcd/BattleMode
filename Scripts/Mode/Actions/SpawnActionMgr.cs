// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-03-14 17:08:01
// 版 本：1.0
// ========================================================
using System;
using System.Collections.Generic;
public class SpawnActionMgr
{
    private List<IAction> _actions;
    public SpawnActionMgr()
    {
        _actions = new List<IAction>();
    }
    public void Update()
    {
        foreach (var act in _actions)
        {
            act.Update();
        }
        for (int i = _actions.Count - 1; i > -1; i--)
        {
            IAction act = _actions[i];
            if(act.Finished)
            {
                _actions.RemoveAt(i);
            }
        }
    }
    public void AddAction(IAction act, bool immediately = false)
    {
        _actions.Add(act);
        if(immediately)
        {
            act.Update();
        }
    }
    public void CleanActions()
    {
        _actions = new List<IAction>();
    }
}
