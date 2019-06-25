using UnityEngine;
using System.Collections;

public class Hero:Role
{
    // Use this for initialization
    protected override void Start()
    {
        base.Start();
        GoodsManager.Instance.EquippedBag.addListener(EventBase.CHANGE, OnEquippedItemsChange);
    }
    protected override void SetBeAttacking(bool value, bool isCallPartner = true)
    {
    }
    private void OnEquippedItemsChange(EventBase e)
    {
        (_roleInfo as HeroInfo).Refresh();
    }
    private void OnHeroLevelUp(EventBase e)
    {
        AddEffect("Prefabs/Effects/LevelUp", new Vector2(0.04f, 0.79f));
    }
    protected override void Init()
    {
        RoleId = SystemConsts.ROLE_HERO_ID;
        _roleInfo = GameModel.Instance.HeroInfo;
        base.Init();
        _ai = new HeroAi(this);
        _roleInfo.addListener(HeroInfoEvent.LEVELUP, OnHeroLevelUp);
    }
    public void ObtainExp(Role from)
    {
        LevelTplData levelTpl = TemplateManager.GetLevelTpl(from.GetRoleInfo().Level);
        int exp = Mathf.FloorToInt(levelTpl.Expreward * (1 + Random.Range(-0.03f,0.03f)))+from.GetRoleInfo().Tpl.Extraexp;
        int levelDelta = _roleInfo.Level - from.GetRoleInfo().Level;
        if (levelDelta > 0)
        {
            if (levelDelta == 1)
            {
                exp = Mathf.FloorToInt(exp * 0.97f);
            }
            else if (levelDelta == 2)
            {
                exp = Mathf.FloorToInt(exp * 0.96f);
            }
            else if (levelDelta == 3)
            {
                exp = Mathf.FloorToInt(exp * 0.95f);
            }
            else if (levelDelta == 4)
            {
                exp = Mathf.FloorToInt(exp * 0.9f);
            }
            else if (levelDelta == 5)
            {
                exp = Mathf.FloorToInt(exp * 0.70f);
            }
            else
            {
                exp = Mathf.FloorToInt(exp * 0.30f);
            }
        }
        if (exp < 1) exp = 1;
        _roleInfo.Exp += exp;
    }
    public void ObtainPastExp(int exp)
    {
        if (exp < 1) exp = 1;
        _roleInfo.Exp += exp;
    }

    public bool GetHumanCommand()
    {
        return false;
    }
    public override void RemoveFromMap()
    {
    }
    public override bool CanAction(bool force = false)
    {
        if(force == false)
        {
            if(_commandMgr.getExecutingCommand() != null)
            {
                return false;
            }
        }
        return base.CanAction(force);
    }
    public void AutoRecover(bool immediately = false)
    {
        if(!GameManager.game.BeAttacking)
        {
            if(_roleInfo.Hp < _roleInfo.HpTotal)
            {
                if(immediately)
                {
                    _roleInfo.Hp = _roleInfo.HpTotal;
                }
                else
                {
                    StartCoroutine(HpRecoverProcess());
                }
            }
        }
    }
    private IEnumerator HpRecoverProcess()
    {
        int step = 10;
        int stepHp = (_roleInfo.HpTotal - _roleInfo.Hp)/step;
        if (stepHp < 10) stepHp = 10;
        int destHp;
        for (int i = 0; i < step; i++)
        {
            destHp = _roleInfo.Hp + stepHp;
            if(destHp > _roleInfo.HpTotal)
            {
                destHp = _roleInfo.HpTotal;
                _roleInfo.Hp = destHp;
                break;
            }
            else
            {
                _roleInfo.Hp = destHp;
                yield return new WaitForSeconds(0.03f);
            }
        }
        _roleInfo.Hp = _roleInfo.HpTotal;
    }
    protected override void OnDestroy()
    {
        HeroAi heroAi = _ai as HeroAi;
        heroAi.OnDestroy();
        base.OnDestroy();
        _roleInfo.removeListener(HeroInfoEvent.LEVELUP, OnHeroLevelUp);
        GoodsManager.Instance.EquippedBag.removeListener(EventBase.CHANGE, OnEquippedItemsChange);
    }
}
