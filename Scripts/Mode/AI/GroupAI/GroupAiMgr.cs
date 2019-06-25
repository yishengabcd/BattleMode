// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-04-14 19:30:44
// 版 本：1.0
// ========================================================
using System;
using System.Collections.Generic;

public class GroupAiMgr
{
    private Dictionary<int, Group> groups;
    private int _checkCount;
    private Game _game;
    public GroupAiMgr(Game game)
    {
        _game = game;
        groups = new Dictionary<int, Group>();
        _checkCount = 0;
    }
    public void Update()
    {
        foreach (var item in groups)
        {
            item.Value.Update();
        }
        _checkCount++;
        if(_checkCount > 20)//每20帧检查一次，不每帧去检查
        {
            _checkCount=0;
            List<Role> monsters = _game.map.GetMonsters();
            foreach (var item in monsters)
            {
                if (item.Alive && item.aiTpl.Groupid != 0)
                {
                    Group group = null;
                    if(!groups.TryGetValue(item.aiTpl.Groupid, out group))
                    {
                        group = CreateGroup(item.aiTpl.Groupid, item.aiTpl.Groupai, item.aiTpl.Groupparam);
                        groups.Add(item.aiTpl.Groupid, group);
                    }
                    group.AddMember(item);
                }
            }
        }
    }
    private Group CreateGroup(int groupId, int groupAi,int[] param)
    {
        Group group = null;
        if (groupAi == 1)
        {
            group = new DiseperseAttackGroup(groupId, groupAi, param);
        }
        return group;
    }
}
