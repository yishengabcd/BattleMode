// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-03-15 15:16:45
// 版 本：1.0
// ========================================================
using System;
using System.Collections.Generic;
using UnityEngine;
public class HeroAi:Ai
{
    private Hero _hero;
    private HeroInfo _heroInfo;
    private int _autoSkillId = 0;
    private SkillTplData skillTplData;
    private Role _attackingMonster;
    private float _canReleaseDis = -1f;
    private int _releasedTimeCount;
    public HeroAi(Hero hero) : base(hero)
    {
        _hero = hero;
        skillTplData = TemplateManager.GetSkillTplData(_autoSkillId);

        _heroInfo = _hero.GetRoleInfo() as HeroInfo;
        _heroInfo.addListener(EventBase.CHANGE, OnHeroInfoChange);

        _releasedTimeCount = -1;
        OnHeroInfoChange(null);
    }
    private void OnHeroInfoChange(EventBase e)
    {
        _canReleaseDis = skillTplData.Distance * (1.0f + _heroInfo.STR / 150f);
    }
    public override bool TryReleaseSkill(Role target = null)
    {
        return false;
    }

    public override void Update()
    {
        if (!_hero.Alive) return;
        if (_releasedTimeCount > -1)
        {
            _releasedTimeCount--;
            return;
        }
        if (_hero.CanAction())
        {
            Role target = ChooseMonster();
            if(target != null)
            {
                AttackMonster(target);
            }
        }
    }

    private void AttackMonster(Role target)
    {
        _attackingMonster = target;
        Skill skill = SkillFactory.Create(_autoSkillId, _hero,Vector2.zero, target);
        skill.Release();
        _releasedTimeCount = 3;
    }
    private Role ChooseMonster()
    {
        if(_attackingMonster && _attackingMonster.Alive && !_attackingMonster.Sleeping)
        {
            if(_canReleaseDis > Vector2.Distance(_hero.transform.position, _attackingMonster.transform.position))
            {
                return _attackingMonster;
            }
        }
        return FindNearestMonster();
    }
    private Role FindNearestMonster()
    {
        List<Role> monsters = _hero.Map.GetMonsters();
        Role monster = null;
        float nearestDis = 0;
        foreach (var item in monsters)
        {
            if (!item.Alive || item.Sleeping) continue; 

            float dis = Vector2.Distance(_hero.transform.position, item.GetAttackablePosition(_hero));
            if (_canReleaseDis > dis)
            { 
                if((monster != null && nearestDis > dis) || monster == null)
                {
                    monster = item;
                    nearestDis = dis;
                }
            }
        }
        return monster;
    }
    public void OnDestroy()
    {
        _heroInfo.removeListener(EventBase.CHANGE, OnHeroInfoChange);
    }
}
