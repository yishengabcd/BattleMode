// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-04-14 19:36:55
// 版 本：1.0
// ========================================================
using System;
using System.Collections.Generic;
public class Group
{
    public static readonly int TYPE_DISPERSE_ATTACK = 1;//分散攻击，即不同时攻击
    protected int groupId;
    protected int groupAi;
    protected int[] groupParam;
    protected List<Role> members;

    public int GroupId { get => groupId;}
    public int GroupAi { get => groupAi;}
    public List<Role> Members { get => members; }

    public Group(int groupId, int groupAi, int[] param)
    {
        this.groupId = groupId;
        this.groupAi = groupAi;
        groupParam = param;
        members = new List<Role>();
    }
    public virtual void Update()
    {
        for (int i = members.Count - 1; i > -1 ; i--)
        {
            Role mem = members[i];
            if (!mem.Alive)
            {
                members.Remove(mem);
                mem.Group = null;
            }
        }
    }
    public virtual void AddMember(Role role)
    {
        if(role.Alive && members.IndexOf(role) == -1)
        {
            role.Group = this;
            members.Add(role);
        }
    }
    public virtual bool MemberAction(GroupMemberAction action)
    {
        return true;
    }
}
