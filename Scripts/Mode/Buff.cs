// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-03-21 13:22:31
// 版 本：1.0
// ========================================================
using System;
using UnityEngine;
public class Buff
{
    private int _id;
    private BuffTplData _tpl;
    protected Role _role;
    private bool _finished;
    private float _timeLeft;
    private FlipEffect _effect;
    public static readonly int HERO_INJECTION_AFTER_BUFF = 6001;//英雄使用注射技能时对自己造成的负面Buff.

    public static readonly int TYPE_DEFENSE = 1;//加防
    public static readonly int TYPE_RESTRICT = 2;//限制行动
    public static readonly int TYPE_POISON = 3;//中毒
    public static readonly int TYPE_ADD_ATTACK = 4;//加攻
    public static readonly int TYPE_SPEED_UP = 5;//加速
    public static readonly int TYPE_SLEEP = 10;//睡眠
    public static readonly int TYPE_SUCK_BLOOD = 11;//吸血
    private int _affectIndex;
    private float _affectCount;
    private Role _from;

    public static readonly int AFFECT_TYPE_DEFENSE_PERCENT = 101;
    public static readonly int AFFECT_TYPE_DIZZY_PERCENT = 201;
    public static readonly int AFFECT_TYPE_REMOVE_HP_VALUE = 301;
    public static readonly int AFFECT_TYPE_ADD_ATTACK_PERCENT = 401;
    public static readonly int AFFECT_TYPE_SPEED_UP_PERCENT = 501;
    public static readonly int AFFECT_TYPE_SPEED_DOWN_PERCENT = 601;
    public static readonly int AFFECT_TYPE_DEFENSE_BROKEN_PERCENT = 701;
    public static readonly int AFFECT_TYPE_IMMUNITY_DOWN_PERCENT = 801;
    public static readonly int AFFECT_TYPE_CRIT_PERCENT = 901;
    public static readonly int AFFECT_TYPE_SLEEP = 1001;
    public static readonly int AFFECT_TYPE_SUCK_BLOOD_PERCENT = 1101;
    public static readonly int AFFECT_TYPE_INSCREASE_PHYSICS_DEFENCE = 1201;
    public static readonly int AFFECT_TYPE_INSCREASE_BEAR = 1301;


    public Buff(int id, Role role, Role from)
    {
        _id = id;
        _tpl = TemplateManager.GetBuff(_id);
        if(_tpl == null)
        {
            Debug.LogError("未找到Buff模板,id="+_id);
        }
        _role = role;
        _from = from;
        Vector2 pt = Vector2.zero;
        if(_tpl.Resposition == 1)
        {
            pt = _role.GetBuffPosition();
        }
        else if(_tpl.Resposition == 2)
        {
            pt = _role.GetHeadPosition(true);
        }
        if(!_tpl.Res.Trim().Equals(""))
        {
            _effect = _role.AddFlipEffect(SystemConsts.SKILL_BUFF_PREFIX + _tpl.Res, pt);
        }

        _affectIndex = 0;
        _affectCount = 0.0f;

        Refresh(_from);
    }

    public void Update()
    {
        _timeLeft -= Time.deltaTime;
        if(_tpl.Affecttype == AFFECT_TYPE_REMOVE_HP_VALUE || _tpl.Affecttype == AFFECT_TYPE_SUCK_BLOOD_PERCENT)
        {
            _affectCount+= Time.deltaTime;
            if(_affectCount/_tpl.Affectfrequency > (_affectIndex+1)* _tpl.Affectfrequency)
            {
                _affectIndex++;
                int value = 0;
                if(_tpl.Affecttype == AFFECT_TYPE_REMOVE_HP_VALUE)
                {
                    value = _tpl.Affectvalue;
                }
                else if(_tpl.Affecttype == AFFECT_TYPE_SUCK_BLOOD_PERCENT)
                {
                    value = (int)(_role.GetRoleInfo().Hp * JoinedAffectValue * 0.01f);
                    int minValue = (int)(_role.GetRoleInfo().HpTotal * 0.01f);
                    if(value < minValue)
                    {
                        value = minValue;
                    }
                }
                if(value < 1) {
                    value = 1;
                }
                Damage damage = new Damage();
                damage.type = Damage.TYPE_HURT;
                damage.value = -value;
                damage.isBuffCaused = true;
                _role.TakeDamage(damage);
            }
        }
        if (_timeLeft < 0)
        {
            _finished = true;
        }
    }
    public void Refresh(Role from)
    {
        _from = from;
        _timeLeft = _tpl.Lasttime;
        if (_tpl.Lasttimejoin.Length > 2)
        {
            int prop = from.GetRoleInfo().GetPropByType(_tpl.Lasttimejoin[0]);
            if(prop > 0)
            {
                float added = (float)prop / (float)_tpl.Lasttimejoin[1] * (float)_tpl.Lasttimejoin[2];
                if (_tpl.Lasttimejoin.Length > 3 && added > _tpl.Lasttimejoin[3]) added = _tpl.Lasttimejoin[3];
                _timeLeft += added;
            }
        }
    }
    public void Clear()
    {
        if(_effect != null)
        {
            _effect.Destroy();
            _effect = null;
        }
        _finished = true;
    }

    public bool Finished => _finished;
    public int ID => _id;
    public BuffTplData Tpl => _tpl;
    public float TimeLeft => _timeLeft;
    public int Type => _tpl.Type;
    public int AffectType => _tpl.Affecttype;
    public Role From => _from;
    public int JoinedAffectValue
    {
        get
        {
            int join = _tpl.Affectvalue;
            if (_tpl.Affectjoin.Length > 2)
            {
                int prop = _from.GetRoleInfo().GetPropByType(_tpl.Affectjoin[0]);
                if (prop > 0)
                {
                    float added = (float)prop / (float)_tpl.Affectjoin[1] * (float)_tpl.Affectjoin[2];
                    if (_tpl.Affectjoin.Length > 3 && added > _tpl.Affectjoin[3]) added = _tpl.Affectjoin[3];
                    join += Mathf.RoundToInt(added);
                }
            }
            return join;
        }
    }

    public bool FlipX {
        set {
            if (_effect != null)
            {
                _effect.Flip = value;
            }
        } 
    }
    public void SetBuffVisible(bool value)
    {
        if(_effect != null)
        {
            _effect.SetVisible(value);
        }
    }
}
