// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-03-25 12:50:00
// 版 本：1.0
// ========================================================
using System;
public class Damage
{
    public static readonly int TYPE_HEAL = 1;
    public static readonly int TYPE_HURT = 2;
   // public static readonly int TYPE_BUFF = 3;//仅仅添加BUFF

    public int type;
    public int value = 0;
    public bool isCrit = false;//type为TYPE_HURT时有效
    public bool isPain = false;//为ture，会中断对方技能；type为TYPE_HURT时有效
    public string painMotion = "hurt";
    public bool isMiss = false;//type为TYPE_HURT时有效
    public float painTime = 0.2f;
    public bool isBuffCaused = false;
    public bool isDeadly = false;//是否是致命的

    public Role from;
    public Role target;

    public Skill skill;

    public Damage()
    {
    }
}
