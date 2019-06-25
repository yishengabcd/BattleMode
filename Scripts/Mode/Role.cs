using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DragonBones;

public class Role : MonoBehaviour, IPointerDownHandler
{
    public enum Orientation
    {
        LEFT = 1,
        RIGHT
    }
    public static readonly string MOTION_STAND = "prepare";
    public static readonly string MOTION_RUN = "run";
    public static readonly string MOTION_RAISE = "victory";//起手势，用于施法；
    public static readonly string MOTION_DIE = "die";

    public static readonly int SIDE_LEFT = 1;
    public static readonly int SIDE_RIGHT = 2;

    protected ActionMgr _actionMgr;
    protected CommandMgr _commandMgr;
    protected BuffMgr _buffMgr;
    protected Orientation _orientation = Orientation.RIGHT;

    private int roleId;
    protected RoleInfo _roleInfo;
    protected string _currentMotion;
    protected string _afterMotion;
    protected EventDispatcher _eventDispatcher;
    public DragonBones.UnityArmatureComponent ArmatureComp;
    private Map map;
    private bool _alive;
    private Skill _skill;
    public bool Alive => _alive;
    public RoleTplData Tpl => _roleInfo.Tpl;
    protected Ai _ai;
    protected bool _immunity;
    protected Vector2 _bornPosition;
    public Vector2 BornPosition => _bornPosition;

    private Coroutine _beAttackingCoroutine;
    private Role _harmfulEnemy;
    private Group group;
    private Matrix matrix;

    public bool IsModel;//是否只是模型，模型不会有任何行为

    protected void Awake()
    {
        Init();
    }
    protected virtual void Init()
    {
        //Transform obj = transform.Find("Armature");
        //PlayMotion(MOTION_STAND);
        _eventDispatcher = new EventDispatcher();
        _alive = true;
    }
    // Use this for initialization
    protected virtual void Start()
    {
        _actionMgr = new ActionMgr();
        _commandMgr = new CommandMgr();
        _buffMgr = new BuffMgr(this);

        //// Load Mecha Data
        //UnityFactory.factory.LoadDragonBonesData("DB/hero/hr021_ske");
        //UnityFactory.factory.LoadTextureAtlasData("DB/hero/hr021_tex");

        //// Build Mecha Armature
        //ArmatureComp = UnityFactory.factory.BuildArmatureComponent("Hero");
        ////
        //ArmatureComp.CloseCombineMeshs();
        //ArmatureComp.sortingOrder = 100;
        //ArmatureComp.sortingLayerName = "Active";
        //ArmatureComp.transform.parent = transform;
        //ArmatureComp.transform.position = Vector3.zero;

        _bornPosition = transform.position;
        InitArmature();
        _roleInfo.addListener(EventBase.CHANGE, OnInfoChanged);
    }
    protected virtual void InitArmature()
    {
        ArmatureComp.AddDBEventListener(DragonBones.EventObject.COMPLETE, OnMotionComplete);
        PlayMotion(MOTION_STAND);
        SetPosition(transform.position);
        ArmatureComp.animation.timeScale = ActionSpeed;
    }
    protected void OnInfoChanged(EventBase e)
    {
        if (ArmatureComp != null)
        {
            ArmatureComp.animation.timeScale = ActionSpeed;
        }
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (IsModel) return;
        _actionMgr.Update();
        _commandMgr.Update();
        _buffMgr.Update();
        if (_ai != null)
        {
            _ai.Update();
        }
    }
    public void SetPosition(Vector2 pt)
    {
        float z = pt.y;
        transform.position = new Vector3(pt.x, pt.y, z);
        ArmatureComp.transform.position = new Vector3(ArmatureComp.transform.position.x, ArmatureComp.transform.position.y, z);
    }
    public void AddAction(IAction act, bool immediately = false)
    {
        if (_actionMgr != null && (_alive|| act is DieAction))
        {
            _actionMgr.AddAction(act, immediately);
        }
    }
    public void ExecuteCommand(Command command)
    {
        if (_alive && _commandMgr != null)
            _commandMgr.Execute(command);
    }
    public void CancelActionByType(string type)
    {
        _actionMgr.CancelActionByType(type);
    }
    public void SetFaceTo(Vector2 pt)
    {
        if (transform.position.x > pt.x)
        {
            setOrientation(Orientation.LEFT);
        }
        else
        {
            setOrientation(Orientation.RIGHT);
        }
    }
    public bool IsFactTo(float x)
    {
        if (x > transform.position.x)
        {
            return _orientation == Orientation.RIGHT;
        }
        else
        {
            return _orientation == Orientation.LEFT;
        }
    }
    public void setOrientation(Orientation value)
    {
        if (_orientation != value)
        {
            _orientation = value;
            if (_orientation == Orientation.LEFT)
            {
                ArmatureComp.armature.flipX = true;
                // obj.localScale = new Vector3(-1, obj.localScale.y, obj.localScale.z);
            }
            else
            {
                ArmatureComp.armature.flipX = false;
                //obj.localScale = new Vector3(-1, obj.localScale.y, obj.localScale.z);
            }
            _buffMgr.FlipX = _orientation == Orientation.LEFT;
        }
    }
    public void PlayMotion(string motion, int repeat = -1, string afterMotion = null, bool replay = true)
    {
        if (!_alive && motion != MOTION_DIE) { return; }
        _afterMotion = afterMotion;
        //if (this is Monster)
        //{
        //    Debug.LogError(motion);
        //}
        if (_currentMotion != motion || replay)
        {
            _currentMotion = motion;
            ArmatureComp.animation.Play(motion, repeat);
        }
    }
    public string CurrentMotion => _currentMotion;

    private void OnMotionComplete(string type, DragonBones.EventObject eventObject)
    {
        if (_afterMotion != null)
        {
            PlayMotion(_afterMotion);
            _afterMotion = null;
        }
        dispatch(new RoleEvent(RoleEvent.MOTION_COMPLETE, this));
    }
    public void AddBuff(int buffId, Role from)
    {
        Buff buff = _buffMgr.AddBuff(buffId, from);
        if (buff.Type == Buff.TYPE_RESTRICT || buff.Type == Buff.TYPE_SLEEP)
        {
            CancelAllActionsAndCommands();
            PlayMotion(MOTION_STAND);
        }
    }
    public void RemoveBuff(int buffId)
    {
        _buffMgr.RemoveBuff(buffId);
    }
    public List<Buff> Buffs => _buffMgr.Buffs;
    public void ClearBuffs()
    {
        _buffMgr.Clear();
        _roleInfo.CleanBuff();
    }
    public void SetBuffVisible(bool value)
    {
        _buffMgr.SetBuffVisible(value);
    }
    public GameObject AddEffect(string res, Vector2 position, bool onTop = true, bool scaleByOrientatio = true, bool speedAjustable = false)
    {
        var direction = _orientation == Orientation.RIGHT ? 1 : -1;
        GameObject effect = (GameObject)Instantiate(Resources.Load(res), transform);
        Vector3 vector3 = position;
        if (onTop)
        {
            vector3.z = -position.y - 0.8f;
        }
        else
        {
            vector3.z = -position.y + 0.8f;
        }
        if (scaleByOrientatio)
        {
            vector3.x = vector3.x * direction;
            effect.transform.localScale = new Vector3(direction * effect.transform.localScale.x, effect.transform.localScale.y, effect.transform.localScale.z);
            effect.transform.rotation = Quaternion.Euler(0f, 0f, effect.transform.rotation.eulerAngles.z * direction);
        }

        effect.transform.localPosition = vector3;
        if (speedAjustable)
        {
            Animator animator = effect.GetComponent<Animator>();
            if (animator != null)
            {
                animator.speed = ActionSpeed;
            }
        }
        return effect;
    }
    public FlipEffect AddFlipEffect(string res, Vector2 pos)
    {
        var effect = new FlipEffect(res, pos, transform, _orientation == Orientation.LEFT);
        return effect;
    }
    public virtual void addHitEffect(string name, float offsetX, float offsetY, Role attacker, bool scaleByOrientatio = true)
    {
        Vector2 pt = GetHitLocalPosition(attacker);
        pt.x += offsetX;
        pt.y += offsetY;
        AddEffect(SystemConsts.SKILL_EFF_PREFIX + name, pt, true, scaleByOrientatio);
    }
    //处理技能产生的效果
    public Damage DoSkillAffect(Skill skill)
    {
        Damage damage = new Damage();
        damage.from = skill.GetRole();
        damage.target = this;
        damage.painTime = skill.SkillTpl.Paintime;
        Role skillRole = skill.GetRole();
        RoleInfo skillRoleInfo = skillRole.GetRoleInfo();
        if (skill.SkillTpl.Type == Skill.TYPE_BUFF_ONLY)
        {
            TryAddBuff(skill);
        }
        else
        {
            int affValue = 0;
            if (skill.SkillTpl.Targettype == Skill.TARGET_TYPE_SELF || skill.SkillTpl.Targettype == Skill.TARGET_TYPE_SELF_TEAM)
            {
                if (skill.SkillTpl.Type == Skill.TYPE_CURE_VALUE)
                {
                    damage.type = Damage.TYPE_HEAL;
                    affValue = (int)skill.SkillTpl.Affectvalue;
                    damage.value = skillRoleInfo.CaluJoinHealValue(affValue);
                    TakeDamage(damage);
                }
                else if (skill.SkillTpl.Type == Skill.TYPE_CURE_PERCENT)
                {
                    damage.type = Damage.TYPE_HEAL;
                    float percent = skill.SkillTpl.Affectvalue * 0.01f;
                    if (skill.SkillTpl.Subtype == Skill.SUB_TYPE_CURE_ON_INT)
                    {
                        float addPercent = _roleInfo.INT / 200f;
                        //if (addPercent > 1)
                        //{
                        //    addPercent = 1;
                        //}
                        percent = (1.0f + addPercent) * percent;
                    }
                    affValue = Mathf.FloorToInt(this.GetRoleInfo().HpTotal * percent);
                    damage.value = skillRoleInfo.CaluJoinHealValue(affValue);
                    TakeDamage(damage);
                }
            }
            else if (skill.SkillTpl.Targettype == Skill.TARGET_TYPE_ENEMY)
            {
                damage.type = Damage.TYPE_HURT;
                _harmfulEnemy = skill.GetRole();
                SetBeAttacking(true);
                if (skill.SkillTpl.Type == Skill.TYPE_PHYSICS || skill.SkillTpl.Type == Skill.TYPE_PHYSICS_FAR)
                {
                    //TODO 公式需要重新思考
                    bool miss = false;
                    if (!skill.IgnoreDodge)
                    {
                        if (skillRoleInfo.HitRate - _roleInfo.DodgeRate > 30f)
                        {
                            miss = false;
                        }
                        else
                        {
                            miss = UnityEngine.Random.Range(0f, 100f + skillRoleInfo.HitRate) < _roleInfo.DodgeRate;
                        }
                    }

                    if (miss)
                    {
                        damage.isMiss = true;
                        damage.value = 0;
                    }
                    else
                    {
                        bool isCrit = false;
                        if (isLife)
                        {
                            isCrit = Random.Range(0f, 100f) < skillRoleInfo.CritRate;
                            damage.isCrit = isCrit;
                        }

                        int dmg = 1;
                        if (isLife)
                        {
                            dmg = (int)(skillRoleInfo.Attack * skill.SkillTpl.Physicsscale);
                        }
                        else
                        {
                            dmg = (int)(skillRoleInfo.Attack* skillRoleInfo.Tpl.Walldmgscale * skill.SkillTpl.Physicsscale);
                        }
                        if (isCrit) dmg *= 2;
                        int sourceDmg = dmg;
                        dmg = dmg - _roleInfo.Defense;
                        if (dmg < 0) dmg = 0;

                        bool isDeadly = false;
                        if (isLife)
                        {
                            isDeadly = Random.Range(0f, 100f) < skillRoleInfo.DeadlyRate;
                        }

                        if (isDeadly)
                        {
                            damage.isDeadly = true;
                            dmg = Mathf.CeilToInt(_roleInfo.Hp * (0.68f-_roleInfo.AntiDeadlyRate));
                            if (dmg < 10)
                            {
                                dmg = 10;
                            }
                        }
                        else
                        {
                            float PhysicsAbsorbRate = _roleInfo.PhysicsAbsorbRate * 0.01f;
                            if (PhysicsAbsorbRate < 0.0f)
                            {
                                PhysicsAbsorbRate = 0f;
                            }
                            dmg = (int)(dmg * (1f - _roleInfo.PhysicsAbsorbRate * 0.01f));
                        }
                        if (dmg < 1) dmg = 1;

                        damage.isPain = FellPain(skill);
                        damage.painMotion = skill.SkillTpl.Paintype;

                        affValue = dmg;

                        if (skill.SkillTpl.Type == Skill.TYPE_PHYSICS)
                        {
                            bool isAntiShock = Random.Range(0f, 100f) < _roleInfo.AntiShockRate;
                            if (isAntiShock) skill.GetRole().DoAntiShock(sourceDmg, damage.isPain);
                        }
                    }
                }
                else if (skill.SkillTpl.Type == Skill.TYPE_MAGIC)
                {
                    bool isCrit = false;//Random.Range(0f, 100f) < skill.GetRole().GetRoleInfo().CritRate;
                    //damage.isCrit = isCrit;
                    int dmg = skillRoleInfo.MagicAttack;
                    if (isCrit) dmg *= 2;
                    dmg = dmg - _roleInfo.MagicDefense;

                    damage.isPain = FellPain(skill);
                    damage.painMotion = skill.SkillTpl.Paintype;

                    affValue = dmg;
                }
                if (affValue < 1)
                {
                    affValue = 1;
                }
                else
                {
                    affValue = (int)((Random.Range(-0.02f, 0.02f) + 1.0f) * affValue);
                    if (affValue < 1)
                    {
                        affValue = 1;
                    }
                }
                damage.value = -affValue;

                TakeDamage(damage);
            }
        }

        TryAddBuff(skill, damage);
        return damage;
    }
    protected virtual void SetBeAttacking(bool value, bool isCallPartner = true)
    {
        _roleInfo.BeAttacking = value;
        if (matrix != null && isCallPartner)
        {
            matrix.SetBeAttacking(value);
        }
        if (value)
        {
            if (_beAttackingCoroutine != null) StopCoroutine(_beAttackingCoroutine);
            _beAttackingCoroutine = StartCoroutine(BeAttackingCont());
            if (isCallPartner && aiTpl.Callpartner > 0)
            {
                List<Role> partners = map.FindZonePartners(this, aiTpl.Callrange, aiTpl.Callpartner);
                foreach (var item in partners)
                {
                    item.BeCallFromPartner(this);
                }
            }
        }
        else
        {
            _harmfulEnemy = null;
        }
    }
    private IEnumerator BeAttackingCont()
    {
        yield return new WaitForSeconds(5f);
        SetBeAttacking(false);
        _beAttackingCoroutine = null;
    }
    public void BeCallFromPartner(Role from)
    {
        SetBeAttacking(true, false);
    }
    private void DoAntiShock(int sourceDmg, bool isPain)
    {
        int dmgAs = (int)(sourceDmg * (isPain?0.5f:1f));

        dmgAs = dmgAs - (int)(_roleInfo.Defense * 0.2f);
        dmgAs = (int)((float)dmgAs*Random.Range(0.97f, 1.03f));
        //dmgAs = 129
        if (dmgAs < 1)
        {
            dmgAs = 1;
        }
        Damage damageAs = new Damage();
        damageAs.target = this;
        damageAs.value = -dmgAs;
        damageAs.type = Damage.TYPE_HURT;
        TakeDamage(damageAs);
    }
    private void TryAddBuff(Skill skill, Damage damage = null)
    {
        if (isLife == false) return;
        if (skill == null) return;
        bool isMiss = false;
        if (damage != null) isMiss = damage.isMiss;
        if (isMiss) return;
        bool triggered = skill.SkillTpl.Buffids.Length > 0 && skill.SkillTpl.Buffids[0] != 0 && Random.Range(0f, 100f) < skill.SkillTpl.Buffrate;
        if (triggered)
        {
            int buffId = skill.SkillTpl.Buffids[Random.Range(0, skill.SkillTpl.Buffids.Length)];
            if (buffId != 0)
            {
                BuffTplData buffTpl = TemplateManager.GetBuff(buffId);
                bool immune = false;
                if (buffTpl.Type == Buff.TYPE_POISON)
                {
                    immune = UnityEngine.Random.Range(-30f, 70f) < _roleInfo.AntiPoisonRate - skill.SkillTpl.Buffrate;
                }
                else if (buffTpl.Affecttype == Buff.AFFECT_TYPE_DIZZY_PERCENT)
                {
                    immune = _immunity;
                    if (immune == false)
                    {
                        immune = UnityEngine.Random.Range(-20f, 80f) < _roleInfo.AntiDizzyRate - skill.SkillTpl.Buffrate;
                    }
                }
                if (!immune)
                {
                    AddBuff(buffId, skill.GetRole());
                }
            }
        }
    }
    public void SetImmunity(bool value)
    {
        _immunity = value;
    }
    private bool FellPain(Skill skill)
    {
        if (_immunity)
        {
            return false;
        }
        bool isPain = Random.Range(0f, 100f) < skill.SkillTpl.Painrate;
        if (isPain)
        {
            int ran = Random.Range(-100, 300);
            isPain = skill.SkillTpl.Paindegree - _roleInfo.Bear > ran;
        }
        return isPain;
    }

    public void TakeDamage(Damage damage)
    {
        if (!_alive)
        {
            return;
        }
        int hp = _roleInfo.Hp + damage.value;
        if (hp > _roleInfo.HpTotal)
        {
            hp = _roleInfo.HpTotal;
        }
        else if (hp <= 0)
        {
            hp = 0;
            _alive = false;
        }
        UpdateHp(hp);
        if (damage.type == Damage.TYPE_HURT && ArmatureComp != null)
        {
            foreach (UnityEngine.Transform item in ArmatureComp.transform)
            {
                Material mat = item.GetComponent<MeshRenderer>().material;
                Color color = new Color(1.0f, 0.3f, 0.3f);
                mat.color = color;
            }
            StartCoroutine(RecoverColor());
        }

        dispatch(new RoleEvent(RoleEvent.DAMAGE, this, damage));
        GlobalEventLocator.Instance.dispatch(new RoleEvent(RoleEvent.DAMAGE, this, damage));
        if (!_alive)
        {
            Die();
        }
        else
        {
            if (damage.isPain && ArmatureComp != null)
            {
                CancelAllActionsAndCommands(true);
                IAction painAction = new PainAction(this, damage);
                AddAction(painAction);
            }
            if (!damage.isBuffCaused)
            {
                _buffMgr.RemoveSleepBuffs();
            }
            else
            {
                if (Random.Range(0f,100f) > 50f)
                {
                    _buffMgr.RemoveSleepBuffs();
                }
            }
        }
    }
    protected virtual void Die()
    {
        CancelAllActionsAndCommands();
        IAction dieAct = new DieAction(this);
        AddAction(dieAct);
    }
    //第一次受击变色时会卡顿，需要预先处理一次
    protected void PreloadHurtShader()
    {
        foreach (UnityEngine.Transform item in ArmatureComp.transform)
        {
            Material mat = item.GetComponent<MeshRenderer>().material;
            Color color = new Color(1.0f, 0.3f, 0.3f);
            mat.color = color;
        }
        //StartCoroutine(RecoverColor());
    }
    protected IEnumerator RecoverColor()
    {
        yield return new WaitForSeconds(0.3f);
        if (ArmatureComp != null)
        {
            foreach (UnityEngine.Transform item in ArmatureComp.transform)
            {
                Material mat = item.GetComponent<MeshRenderer>().material;
                Color color = new Color(1.0f, 1.0f, 1.0f);
                mat.color = color;
            }
        }
    }
    public virtual bool CanAction(bool force = false)
    {
        if (force == false && (_skill != null || GetActionByType("RestAction") != null))
        {
            return false;
        }
        else if (force)
        {
            IAction restAction = GetActionByType("RestAction");
            if(restAction != null)
            {
                restAction.Cancel();
            }
        }
        if (GetActionByType("PainAction") != null)
        {
            return false;
        }
        for (int i = 0; i < _buffMgr.Buffs.Count; i++)
        {
            Buff buff = _buffMgr.Buffs[i];
            if (buff.Type == Buff.TYPE_RESTRICT || buff.Type == Buff.TYPE_SLEEP)
            {
                return false;
            }
        }
        return true;
    }

    public void CancelAllActionsAndCommands(bool keepUnreleaseCmd = false)
    {
        if (_actionMgr != null) _actionMgr.CleanActions();
        if (_commandMgr != null) _commandMgr.CancelAll(keepUnreleaseCmd);
    }
    //public void StopCurrentAction()
    //{
    //    if (_actionMgr != null) _actionMgr.CleanActions();
    //}
    public void ReleaseSkill(Skill skill)
    {
        _skill = skill;
        AddAction(_skill);
    }
    public bool TryReleaseSkill(Role target = null)
    {
        if (_ai != null)
        {
            return _ai.TryReleaseSkill(target);
        }
        return false;
    }
    public void SkillComplete()
    {
        _skill = null;
    }
    public IAction GetActionByType(string type)
    {
        return _actionMgr.GetFirstActionByType(type);
    }
    private void UpdateHp(int hp)
    {
        if (hp != _roleInfo.Hp)
        {
            _roleInfo.Hp = hp;
            dispatch(new RoleEvent(RoleEvent.HP_CHANGE, this));
        }
    }
    public void addListener(string type, UnityAction<EventBase> listener)
    {
        _eventDispatcher.addListener(type, listener);
    }
    public void removeListener(string type, UnityAction<EventBase> listener)
    {
        _eventDispatcher.removeListener(type, listener);
    }
    public void dispatch(EventBase eventBase)
    {
        _eventDispatcher.dispatch(eventBase);
    }
    public Vector3 GetHeadPosition(bool local = false)
    {
        UnityEngine.Transform obj = transform.Find("headTopPt");
        if (obj != null)
        {
            if (local)
            {
                return obj.localPosition;
            }
            return obj.position;
        }
        Vector3 pt = new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.y);
        return pt;
    }
    public virtual Vector2 GetHitPosition(Role attacker = null)
    {
        UnityEngine.Transform hitObj = transform.Find("hitPt");
        if (hitObj != null)
        {
            return hitObj.position;
        }
        return Vector2.zero;
    }
    public virtual Vector2 GetHitLocalPosition(Role attacker = null)
    {
        UnityEngine.Transform hitObj = transform.Find("hitPt");
        if (hitObj != null)
        {
            return hitObj.localPosition;
        }
        return Vector2.zero;
    }
    public Vector2 GetBuffPosition()
    {
        UnityEngine.Transform obj = transform.Find("buffPt");
        if (obj != null)
        {
            return obj.localPosition;
        }
        return Vector2.zero;
    }
    public virtual Vector2 GetAttackablePosition(Role attacker)
    {
        return transform.position;
    }
    public int Side => _roleInfo.Tpl.Side;
    public int OppositeSide => _roleInfo.Tpl.Side == SIDE_LEFT ? SIDE_RIGHT : SIDE_LEFT;
    public IAction CurrentAction => _actionMgr.Current;
    public Command getExecutingCommand()
    {
        return _commandMgr.getExecutingCommand();
    }
    public void SetSelectable(bool value)
    {
        BoxCollider2D coll = GetComponent<BoxCollider2D>();
        if (coll != null)
        {
            coll.enabled = value;
        }
    }
    public RoleInfo GetRoleInfo()
    {
        return _roleInfo;
    }
    public Orientation getOrientation()
    {
        return _orientation;
    }
    public float MoveSpeed => _roleInfo.MoveSpeed;
    public float MarchSpeed => _roleInfo.MarchSpeed;
    public float ActionSpeed => _roleInfo.ActionSpeed;
    public float SeeDistance => _roleInfo.SeeDistance;

    public Map Map { get => map; set => map = value; }
    public void TakeARest(Skill skill = null)
    {
        RestAction act = new RestAction(this, skill);
        AddAction(act);
    }

    public virtual void RemoveFromMap()
    {
        map.RemoveRole(this);
        Destroy(gameObject);
    }
    public Skill ExecutingSkill => _skill;
    public virtual bool isLife => true;//是否是有生命的——城墙是没有生命的。

    protected virtual void OnDestroy()
    {
        if (_roleInfo != null)
        {
            _roleInfo.CleanBuff(false);
            _roleInfo.removeListener(EventBase.CHANGE, OnInfoChanged);
        }
        if (_actionMgr != null)
        {
            _actionMgr.Dispose();
            _actionMgr = null;
        }
        _commandMgr = null;
        if (ArmatureComp)
        {
            ArmatureComp.RemoveDBEventListener(DragonBones.EventObject.COMPLETE, OnMotionComplete);
        }
        if (_eventDispatcher != null)
        {
            _eventDispatcher.removeAllListeners();
        }
    }
    public bool Sleeping => _buffMgr.ContainBuff(Buff.TYPE_SLEEP);
    public AITplData aiTpl => _roleInfo == null ?TemplateManager.GetAiTplData(TemplateManager.GetRoleTplData(roleId).AI):_roleInfo.aiTpl;
    public Role GetHarmfulEnemy()
    {
        if (_harmfulEnemy != null && _harmfulEnemy.Alive) return _harmfulEnemy;

        List<Role> roles = map.GetRolesBySide(Side == SIDE_LEFT ? SIDE_RIGHT : SIDE_LEFT);
        if (roles.Count > 0)
        {
            foreach (var item in roles)
            {
                if (item.Alive && item.isLife)
                {
                    if (Vector2.Distance(item.transform.position, transform.position) < SystemConsts.IN_HARMFUL_DIS)
                    {
                        return item;
                    }
                }
            }
        }
        return null;
    }
    public bool InDanger
    {
        get
        {
            List<Role> roles = map.GetRolesBySide(Side == SIDE_LEFT ? SIDE_RIGHT : SIDE_LEFT);
            if (roles.Count > 0)
            {
                foreach (var item in roles)
                {
                    if (item.Alive && item.isLife)
                    {
                        if (Vector2.Distance(item.transform.position, transform.position) < SystemConsts.IN_DANGER_DIS)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }

    public Group Group { get => group; set => group = value; }
    public Matrix Matrix { get => matrix; set => matrix = value; }
    public int RoleId { get => roleId; 
        set {
            roleId = value; 
        } 
     }

    public void OnPointerDown(PointerEventData eventData)
    {
        GlobalEventLocator.Instance.dispatch(new GameEvent(GameEvent.SELECT_ROLE, this));
    }
}
