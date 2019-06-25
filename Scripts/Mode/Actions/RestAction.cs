// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-03-15 19:44:26
// 版 本：1.0
// ========================================================
using System;
using UnityEngine;
public class RestAction:BaseRoleAction
{
    private Skill _skill;
    private float _restTime;
    private float _time;
    public RestAction(Role role, Skill skill = null, float defRest = 0.3f):base(role)
    {
        _skill = skill;
        if(_skill != null)
        {
            _restTime = _skill.Restafterskill;
        }
        else
        {
            _restTime = defRest;
        }
        _time = 0.0f;
    }
    public override void Update()
    {
        _time += Time.deltaTime;
        if(_time > _restTime)
        {
            SetFinished(true);
        }
    }
    public override bool Replace(IAction act)
    {
        return act.Type == Type;
    }
    public override string Type => "RestAction";
}
