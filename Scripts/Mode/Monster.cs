// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-03-14 08:44:33
// 版 本：1.0
// ========================================================
using UnityEngine;
using System.Collections;

public class Monster : Role
{
    protected override void Init()
    {
        base.Init();
    }
    // Use this for initialization
    protected override void Start()
    {
        if (IsModel)
        {
            PreloadHurtShader();
            return;
        }
        _roleInfo = new MonsterInfo(RoleId);
        base.Start();
        _ai = new MonsterAi(this);
    }
    protected override void Die()
    {
        base.Die();
        GameManager.hero.ObtainExp(this);
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}
