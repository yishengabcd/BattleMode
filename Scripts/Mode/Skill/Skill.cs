// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-03-13 10:27:34
// 版 本：1.0
// ========================================================
using System;
using System.Collections.Generic;
using UnityEngine;
public class Skill:BaseRoleAction
{
    //target Type
    public static readonly int TARGET_TYPE_SELF = 1;
    public static readonly int TARGET_TYPE_SELF_TEAM = 2;
    public static readonly int TARGET_TYPE_ENEMY = 3;

    //Type
    public static readonly int TYPE_PHYSICS = 1;//近身物理伤害，物理伤害取角色的Attack
    public static readonly int TYPE_PHYSICS_FAR = 2;//远处物理伤害（射箭），物理伤害取角色的Attack
    public static readonly int TYPE_MAGIC = 3;//魔法（加血、伤害等）
    public static readonly int TYPE_BUFF_ONLY = 4;//只加buff;
    public static readonly int TYPE_CURE_VALUE = 5;//加固定血；
    public static readonly int TYPE_CURE_PERCENT = 6;//加百分比血；

    public static readonly int SUB_TYPE_CURE_ON_INT = 1;//英雄加血技，与智力有关
    //public static readonly int SUB_TYPE_GROUND_ATTACK = 2;//地面攻击（英雄狂怒技能），伤害取百分比

    //FRAME_TYPE
    public static readonly int FRAME_TYPE_PLAY_MOTION = 1;
    public static readonly int FRAME_TYPE_ADD_EFFECT = 2;
    public static readonly int FRAME_TYPE_EFFECTIVE = 3;
    public static readonly int FRAME_TYPE_MOVE = 4;
    public static readonly int FRAME_TYPE_REMOVE_EFFECT = 5;//移除特效，目前未使用

    public static readonly int FRAME_TYPE_COMPLETE = 10;
    //public static readonly int FRMAE_TYPE_HIDE_BUFF = 101;
    //public static readonly int FRAME_TYPE_SHOW_BUFF = 102;
    public static readonly int FRAME_TYPE_LOCK_POSITON = 103;//锁定被攻击的对象的位置，后续释放技能时将以此为释放点
    public static readonly int FRAME_TYPE_CANNOT_CANCEL = 104;//技能进入到无法主动撤销阶段；
    public static readonly int FRAME_TYPE_ADD_BUFFS_TO_SELF = 105;//为自己添加Buff
    public static readonly int FRAME_TYPE_REMOVE_SELF_BUFFS = 106;//移除身上指定Buff

    //AFFECT_TYPE
    public static readonly int AFFECT_TYPE_HURT = 1;//造成伤害；
    public static readonly int AFFECT_TYPE_CURE = 2;//加血；
    public static readonly int AFFECT_TYPE_BUFF = 3;//添加BUFF；
    public static readonly int AFFECT_TYPE_SHOOT = 4;//射箭；
    public static readonly int AFFECT_TYPE_SHELL = 5;//炮弹
    public static readonly int AFFECT_TYPE_MULTI = 6;//多段伤害；
    public static readonly int AFFECT_TYPE_RANDOM_SHOOT = 7;//随机射击，英雄skillID=3时
    public static readonly int AFFECT_TYPE_SHOOT_BOMB = 9;//直接发射炮弹
    //public static readonly int AFFECT_TYPE_ADD_BUFF = 99;//添加Buff

    //EFFECT_TARGET 特效释放位置
    public static readonly int EFFECT_TARGET_ROLE_TOP = 1;//角色上层
    public static readonly int EFFECT_TARGET_ROLE_BOTTOM = 2;//角色底层
    public static readonly int EFFECT_TARGET_MAP_TOP = 3;//地图上层，挡住所有的角色
    public static readonly int EFFECT_TARGET_MAP_BOTTOM = 4;//地图底层，在所有的角色下方
    public static readonly int EFFECT_TARGET_MAP_Z = 5;//地图层次，根据Z来决定在哪些角色上层以及在哪些角色下层。



    protected int _skillID;
    protected SkillTplData _skillTpl;
    protected SkillConfigData[] _skillConfigs;
    protected int _currentFrame = 0;
    protected float _pastTime = 0.0f;
    protected bool _releaseStarted;
    protected Role _targetRole;
    protected Vector2 _targetPoint;//只有_targetRole为null时才使用此字段
    protected Vector3 _shellDest;
    protected GameObject _canCancelEffect;
    protected IAction _canCancelAction;
    protected Vector2 _lockPosition = Vector2.zero;
    protected bool _canCancelPhase = true;
    protected int[] _addedSelfBuffs;

    public Skill(int skillId, Role executor,Vector2 pt, Role target = null):base(executor)
    {
        _skillID = skillId;
        _targetRole = target;
        _targetPoint = pt;
        _skillTpl = TemplateManager.GetSkillTplData(_skillID);
        if (_skillTpl == null)
        {
            Debug.LogError(string.Format("未找到技能,SkillId={0}", skillId));
        }
        _skillConfigs = TemplateManager.GetSkillConfigDatas(_skillTpl.Configid);
        if (_skillConfigs.Length == 0)
        {
            Debug.LogError(string.Format("未找到技能配置信息，SkillId={0}, ConfigId={1}", skillId, _skillTpl.Configid));
        }
    }
    public override void Update()
    {
        _pastTime += Time.deltaTime*_role.ActionSpeed;
        //int frame = Mathf.FloorToInt(_pastTime * Application.targetFrameRate) +1;
        int frame = Mathf.FloorToInt(_pastTime * 60f) + 1;
        if (_currentFrame < frame)
        {
            foreach (var item in _skillConfigs)
            {
                if (item.Frame <= frame && item.Frame > _currentFrame)
                {
                    RunFrame(item);
                }
            }
            _currentFrame = frame;
            if(_skillConfigs[_skillConfigs.Length - 1].Frame <= _currentFrame)
            {
                FinishSkill();
            }
        }
    }
    protected void FinishSkill()
    {
        SetFinished(true);
        _role.SkillComplete();
        _role.TakeARest(this);
    }
    public void RunFrame(SkillConfigData frameData)
    {
        if(frameData.Type == FRAME_TYPE_PLAY_MOTION)
        {
            PlayMotion(frameData);
        }
        else if (frameData.Type == FRAME_TYPE_ADD_EFFECT)
        {
            AddEffect(frameData);
        }
        else if (frameData.Type == FRAME_TYPE_EFFECTIVE)
        {
            ProcessEffective(frameData);
        }
        else if (frameData.Type == FRAME_TYPE_MOVE)
        {

        }
        else if (frameData.Type == FRAME_TYPE_COMPLETE)
        {
            if(_skillTpl.Hidebuffeff == 1)
            {
                _role.SetBuffVisible(true);
            }
        }
        else if (frameData.Type == FRAME_TYPE_LOCK_POSITON)
        {
            _lockPosition = _targetRole.GetAttackablePosition(_role);
        }
        else if (frameData.Type == FRAME_TYPE_CANNOT_CANCEL)
        {
            _canCancelPhase = false;
        }
        else if (frameData.Type == FRAME_TYPE_ADD_BUFFS_TO_SELF)
        {
            for (int i = 0; i < frameData.Others.Length; i++)
            {
                _role.AddBuff(int.Parse(frameData.Others[i]), _role);
            }
        }
        else if (frameData.Type == FRAME_TYPE_REMOVE_SELF_BUFFS)
        {
            for (int i = 0; i < frameData.Others.Length; i++)
            {
                _role.RemoveBuff(int.Parse(frameData.Others[i]));
            }
        }

        //else if (frameData.Type == FRMAE_TYPE_HIDE_BUFF)
        //{
        //    _role.SetBuffVisible(false);
        //}
        //else if (frameData.Type == FRAME_TYPE_SHOW_BUFF)
        //{
        //    _role.SetBuffVisible(true);
        //}
    }
    public bool CanCancelPhase => _canCancelPhase;
    public bool IsTargetRunAway()
    {
        bool inSightFlag = true;
        if (_skillTpl.Needinsight == 1)
        {
            inSightFlag = _role.IsFactTo(_targetRole.transform.position.x);
            if (inSightFlag == false)
            {
                return true;
            }
        }

        if (InHurtZone(_role.transform.position, _targetRole))
        {
            return false;
        }
        return true;
    }
    public override bool Replace(IAction act)
    {
        return true;
    }
    public override void Cancel()
    {
        base.Cancel();
        _role.SkillComplete();
        if(_canCancelEffect != null)
        {
            UnityEngine.Object.Destroy(_canCancelEffect);
        }
        if(_canCancelAction != null)
        {
            _canCancelAction.Cancel();
            _canCancelAction = null;
        }
        foreach (var item in _skillConfigs)
        {
            if (item.Type == FRAME_TYPE_REMOVE_SELF_BUFFS)
            {
                for (int i = 0; i < item.Others.Length; i++)
                {
                    _role.RemoveBuff(int.Parse(item.Others[i]));
                }
            }
        }
        if (_role.Alive)
        {
            _role.SetBuffVisible(true);
        }
    }
    public virtual void Release()
    {
        _releaseStarted = true;
        if(_targetRole)
        {
            _role.SetFaceTo(_targetRole.transform.position);
            if(_targetRole is Wall)
            {
                _shellDest = ((Wall)_targetRole).GetHitPosition(_role);
            }
            else
            {
                _shellDest = _targetRole.GetAttackablePosition(_role);
            }
        }
        else if (_skillTpl.Selecttype ==UserSkillMgr.SELECT_TYPE_GROUND)
        {
            _role.SetFaceTo(_targetPoint);
        }
        if (_skillTpl.Hidebuffeff == 1)
        {
            _role.SetBuffVisible(false);
        }
        _role.ReleaseSkill(this);
    }
    private void PlayMotion(SkillConfigData frameData)
    {
        _role.PlayMotion(frameData.Motion, frameData.Motionrepeat, Role.MOTION_STAND);
    }
    private void AddEffect(SkillConfigData frameData)
    {
        var EffectNameAndParams = frameData.Effectname.Split('|');
        var effectName = EffectNameAndParams[0];

        Vector2 pos = new Vector2(float.Parse(frameData.Others[0]), float.Parse(frameData.Others[1]));
        if(frameData.Effecttarget == EFFECT_TARGET_ROLE_TOP || frameData.Effecttarget == EFFECT_TARGET_ROLE_BOTTOM)
        {
            _role.AddEffect(SystemConsts.SKILL_EFF_PREFIX + effectName, pos, true,true,true);
        }
        else if(frameData.Effecttarget == EFFECT_TARGET_MAP_Z)
        {
            Vector3 dest = _role.transform.position;
            //dest.x += pos.x;
            dest.y += pos.y;
            dest.z = dest.y;
            if (frameData.Others.Length > 2)
            {
                dest.z += float.Parse(frameData.Others[2]);
            }
            var efGo = _role.Map.addEffect(SystemConsts.SKILL_EFF_PREFIX + effectName, dest, _role, true, pos.x, true);
            if(EffectNameAndParams.Length > 1)
            {
                if(int.Parse(EffectNameAndParams[1]) == 1)
                {
                    _canCancelEffect = efGo;
                }
            }
        }
    }
    private void ProcessEffective(SkillConfigData frameData)
    {
        if (frameData.Affecttype == AFFECT_TYPE_CURE)
        {
            string effectName = null;
            if(frameData.Effectname.Length > 0 && !frameData.Effectname.Equals(""))
            {
                var EffectNameAndParams = frameData.Effectname.Split('|');
                effectName = EffectNameAndParams[0];
            }

            if (_skillTpl.Targettype == TARGET_TYPE_SELF_TEAM)
            {
                List<Role> roles = _role.Map.GetRolesBySide(_role.Side);
                for (int i = 0; i < roles.Count; i++)
                {
                    Role role = roles[i];
                    if (InBombZone(_role.transform.position, role.transform.position))
                    {
                        if(effectName != null)
                        {
                            Vector2 pos = Vector2.zero;
                            if (frameData.Others.Length > 0)
                            {
                                pos.x = float.Parse(frameData.Others[0]);
                            }
                            if (frameData.Others.Length > 1)
                            {
                                pos.y = float.Parse(frameData.Others[1]);
                            }
                            role.AddEffect(SystemConsts.SKILL_EFF_PREFIX + effectName, pos);
                        }
                        role.DoSkillAffect(this);
                    }
                }
            }
            else if (_skillTpl.Targettype == TARGET_TYPE_SELF)
            {
                _role.DoSkillAffect(this);
            }
        }
        else if(frameData.Affecttype == AFFECT_TYPE_HURT)
        {
            bool inSightFlag = true;
            if(_skillTpl.Needinsight == 1)
            {
                inSightFlag = _role.IsFactTo(_targetRole.transform.position.x);
            }

            if (inSightFlag && InHurtZone(_role.transform.position, _targetRole))
            {
                Damage damage = _targetRole.DoSkillAffect(this);
                if (!damage.isMiss && frameData.Others.Length > 2)
                {
                    string hitEffect = frameData.Others[0];
                    float offsetX = float.Parse(frameData.Others[2]);
                    float offsetY = float.Parse(frameData.Others[3]);
                    _targetRole.addHitEffect(hitEffect, offsetX, offsetY, _role);
                }
            }
        }
        else if (frameData.Affecttype == AFFECT_TYPE_BUFF)
        {
            if (_skillTpl.Targettype == TARGET_TYPE_SELF_TEAM)
            {
                List<Role> roles = _role.Map.GetRolesBySide(_role.Side);
                for (int i = 0; i < roles.Count; i++)
                {
                    Role role = roles[i];
                    if(InBombZone(_role.transform.position, role.transform.position))
                    {
                        role.DoSkillAffect(this);
                    }
                }
            }
            else if (_skillTpl.Targettype == TARGET_TYPE_SELF)
            {
                _role.DoSkillAffect(this);
            }
        }
        else if (frameData.Affecttype == AFFECT_TYPE_SHOOT)
        {
            if (_skillTpl.Selecttype == UserSkillMgr.SELECT_TYPE_GROUND)
            {
                var action = new ShootToPoint(_role, _targetPoint, frameData, this);
                _role.Map.addAction(action);
            }
            else
            {
                Role to = _targetRole;
                if (to == null)
                {
                    to = UnityEngine.Object.FindObjectOfType<Monster>();
                }
                if (to != null)
                {
                    var action = new ShootToRoleAction(_role, to, frameData, this);
                    _role.Map.addAction(action);
                }
            }
        }
        else if(frameData.Affecttype == AFFECT_TYPE_SHELL)
        {
            var action = new ShellToAction(_role, _targetRole, _shellDest, this, frameData);
            _role.Map.addAction(action);
        }
        else if(frameData.Affecttype == AFFECT_TYPE_SHOOT_BOMB)
        {
            var action = new ShellToAction(_role, _targetRole, _lockPosition, this, frameData, true);
            _role.Map.addAction(action);
        }
        else if(frameData.Affecttype == AFFECT_TYPE_MULTI)
        {
            var action = new MultiGroundAttackAction(_role, _targetRole, this, frameData, _lockPosition);
            _role.Map.addAction(action);
            _canCancelAction = action;
        }
        else if (frameData.Affecttype == AFFECT_TYPE_RANDOM_SHOOT)
        {
            RandomShoot();
        }
    }
    protected virtual void RandomShoot()
    {

    }
    public virtual bool InBombZone(Vector2 shotPt, Vector2 targetPt)
    {
        if (_skillTpl.Affectdistance.Length > 1)
        {
            if (_skillTpl.Affectdistance[0] == 2)//圆形区域
            {
                return Vector2.Distance(shotPt, targetPt) < _skillTpl.Affectdistance[1] * 0.01f;
            }
            else
            {
                Vector2 nearT = new Vector2(shotPt.x + _skillTpl.Affectdistance[1] * 0.01f, shotPt.y + _skillTpl.Affectdistance[2] * 0.01f);
                Vector2 farB = new Vector2(shotPt.x + _skillTpl.Affectdistance[3] * 0.01f, shotPt.y + _skillTpl.Affectdistance[4] * 0.01f);
                if (targetPt.x > nearT.x && targetPt.x < farB.x && targetPt.y > farB.y && targetPt.y < nearT.y)
                {
                    return true;
                }
            }
        }
        return false;
    }
    protected bool InHurtZone(Vector2 shotPt, Role target)
    {
        if(_skillTpl.Affectdistance.Length > 1)
        {
            int direction = _role.getOrientation() == Role.Orientation.RIGHT ? 1 : -1;
            Vector2 rolePt = _role.transform.position;
            Vector2 targetCheckPt = _targetRole.GetAttackablePosition(_role);
            if (_skillTpl.Affectdistance[0] == 2)//圆形区域
            {
                return Vector2.Distance(rolePt, targetCheckPt) < _skillTpl.Affectdistance[1];
            }
            else
            {
                Vector2 nearT = new Vector2(rolePt.x + _skillTpl.Affectdistance[1] * direction*0.01f, rolePt.y+ _skillTpl.Affectdistance[2]*0.01f);
                Vector2 farB = new Vector2(rolePt.x + _skillTpl.Affectdistance[3] * direction*0.01f, rolePt.y + _skillTpl.Affectdistance[4]*0.01f);
                if(direction == -1)
                {
                    float temp = nearT.x;
                    nearT.x = farB.x;
                    farB.x = temp;
                }
                if(targetCheckPt.x > nearT.x && targetCheckPt.x < farB.x && targetCheckPt.y >farB.y && targetCheckPt.y < nearT.y)
                {
                    return true;
                }
            }
        }
        else
        {
            Debug.LogError("技能没有配置受击区域,skillId="+_skillID);
        }
        return false;
    }

    public override string Type => "Skill";
    public bool ReleaseStarted => _releaseStarted;
    public SkillTplData SkillTpl => _skillTpl;
    public bool IgnoreDodge => _skillTpl.Ignoredodge == 1;
    public float Restafterskill {
        get
        {
            float restTime = 0.3f;
            if (_role is Monster)//考虑加上随机浮动值，目前还没有加上//已加
            {
                if (!Mathf.Approximately(0.0f, _skillTpl.Restafterskillrandom))
                {
                    restTime = _skillTpl.Restafterskill * (1.0f - _role.GetRoleInfo().Speed / 800f);
                }
                else
                {
                    float ran = UnityEngine.Random.Range(-_skillTpl.Restafterskillrandom, _skillTpl.Restafterskillrandom);
                    restTime = (_skillTpl.Restafterskill + ran) * (1.0f - _role.GetRoleInfo().Speed / 800f);
                }
            }
            else
            {
                restTime = _skillTpl.Restafterskill * (1.0f - _role.GetRoleInfo().Speed / 800f);
            }
            if (restTime < 0.01f) restTime = 0.01f;
            return restTime;
        }
    }
}
