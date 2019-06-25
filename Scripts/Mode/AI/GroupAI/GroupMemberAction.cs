// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-04-14 20:03:53
// 版 本：1.0
// ========================================================
using System;
using UnityEngine;
public class GroupMemberAction
{
    public static readonly int USE_SKILL = 1;
    private int _type;
    protected Role member;
    private float pastTime;
    public GroupMemberAction(Role member, int type)
    {
        this.member = member;
        _type = type;
        pastTime = 0f;
    }
    public void Update()
    {
        pastTime += Time.deltaTime;
    }
    public int Type => _type;
    public Role Member => member;
    public float PastTime => pastTime;
}
