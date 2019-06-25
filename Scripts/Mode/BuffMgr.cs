// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-03-21 13:28:46
// 版 本：1.0
// ========================================================
using System;
using System.Collections.Generic;
public class BuffMgr
{
    private List<Buff> _buffs;
    private Role _role;
    private bool _effVisible = true;
    public BuffMgr(Role role)
    {
        _role = role;
        _buffs = new List<Buff>();
    }
    public void Update()
    {
        Buff[] buffsArr = _buffs.ToArray();
        foreach (var item in buffsArr)
        {
            item.Update();
        }
        for (int i = _buffs.Count - 1; i > -1; i--)
        {
            Buff buff = _buffs[i];
            if(buff.Finished)
            {
                RemoveBuff(buff);
            }
        }
    }
    public Buff AddBuff(int id, Role from)
    {
        Buff buff = null;
        for (int i = 0; i < _buffs.Count; i++)
        {
            buff = _buffs[i];
            if(id == buff.ID)
            {
                buff.Refresh(from);
                return buff;
            }
        }
        buff = new Buff(id, _role, from);
        _buffs.Add(buff);
        buff.SetBuffVisible(_effVisible);
        _role.GetRoleInfo().AddBuff(buff);
        return buff;
    }
    public void RemoveBuff(Buff buff)
    {
        _buffs.Remove(buff);
        buff.Clear();
        _role.GetRoleInfo().RemoveBuff(buff);
    }
    public void RemoveBuff(int tplId)
    {
        for (int i = _buffs.Count - 1; i > -1; i--)
        {
            if (_buffs[i].ID == tplId)
            {
                RemoveBuff(_buffs[i]);
            }
        }
    }
    public bool FlipX
    {
        set{
            foreach (var item in _buffs)
            {
                item.FlipX = value;
            }
        }
    }
    public void SetBuffVisible(bool value)
    {
        _effVisible = value;
        foreach (var item in _buffs)
        {
            item.SetBuffVisible(value);
        }
    }
    public void Clear()
    {
        foreach (var item in _buffs)
        {
            item.Clear();
        }
        _buffs = new List<Buff>();
    }
    public void RemoveSleepBuffs()
    {
        foreach (var item in _buffs)
        {
            if(item.Type == Buff.TYPE_SLEEP)
            {
                RemoveBuff(item);
                break;
            }
        }
    }
    public bool ContainBuff(int type)
    {
        foreach (var item in _buffs)
        {
            if (item.Type == type)
            {
                return true;
            }
        }
        return false;
    }
    public List<Buff> Buffs => _buffs;
}
