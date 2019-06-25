// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-04-14 21:10:08
// 版 本：1.0
// ========================================================
using System;
using System.Collections.Generic;
public class DiseperseAttackGroup:Group
{
    private GroupMemberAction currentMemberAction;
    private float waitTime;
    public DiseperseAttackGroup(int groupId, int groupAi, int[] param) :base(groupId, groupAi, param)
    {
        waitTime = groupParam[0] * 0.001f;
    }
    public override void Update()
    {
        base.Update();
        if(currentMemberAction != null)
        {
            currentMemberAction.Update();
            if(currentMemberAction.PastTime > waitTime)
            {
                currentMemberAction = null;
            }
        }
    }
    public override bool MemberAction(GroupMemberAction action)
    {
        if (currentMemberAction != null)
        {
            return false;
        }
        currentMemberAction = action;
        return true;
    }
}
