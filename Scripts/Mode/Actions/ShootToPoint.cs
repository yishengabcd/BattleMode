// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-04-18 21:27:44
// 版 本：1.0
// ========================================================
using System;
using UnityEngine;
using System.Collections.Generic;
public class ShootToPoint : AbstractAction
{
    private Role _shooter;
    private SkillConfigData _frameData;
    private float _speed;
    private Vector2 _fromPt;
    private Vector2 _toPt;
    private float _speedPerFrame;
    private Skill _skill;

    private GameObject _arrow;
    public ShootToPoint(Role from, Vector2 pt, SkillConfigData frameData, Skill skill)
    {
        _shooter = from;
        _toPt = pt;
        _frameData = frameData;
        _skill = skill;

        float offsetX = float.Parse(_frameData.Others[0]);
        float offsetY = float.Parse(_frameData.Others[1]);

        _arrow = (GameObject)UnityEngine.Object.Instantiate(Resources.Load(SystemConsts.SKILL_EFF_PREFIX + _frameData.Effectname));
        _arrow.GetComponent<SpriteRenderer>().sortingLayerName = SystemConsts.ActiveLayer;
        _arrow.GetComponent<SpriteRenderer>().sortingOrder = 100;

        var face = _shooter.getOrientation() == Role.Orientation.RIGHT ? 1 : -1;
        _fromPt = _shooter.transform.position;
        _fromPt.x += offsetX * face;
        _fromPt.y += offsetY;

        _speed = 24.0f;
        _speedPerFrame = _speed / 60f; //(float)Application.targetFrameRate;

        _arrow.transform.position = _fromPt;

        Vector2 direction = _toPt - _fromPt;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        _arrow.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    public override void Update()
    {
        float step = _speed * Time.deltaTime;
        Vector2 to = Vector2.MoveTowards(_arrow.transform.position, _toPt, step);

        _arrow.transform.position = new Vector3(to.x, to.y, to.y);
        if (_speedPerFrame > Vector2.Distance(to, _toPt))
        {
            if (_frameData.Others.Length > 2)
            {
                int effectTarget = int.Parse(_frameData.Others[3]);

                if (effectTarget == Skill.EFFECT_TARGET_ROLE_TOP || effectTarget == Skill.EFFECT_TARGET_ROLE_BOTTOM)
                {
                    string hitEffect = SystemConsts.SKILL_EFF_PREFIX+_frameData.Others[2];
                    //float offsetX = float.Parse(_frameData.Others[4]);
                    //float offsetY = float.Parse(_frameData.Others[5]);
                    Vector3 pt = _toPt;
                    pt.z = pt.y;
                    _shooter.Map.addEffect(hitEffect, pt, _shooter);
                }
            }
            List<Role> targets = _shooter.Map.GetRolesBySide(_shooter.OppositeSide);
            foreach (var target in targets)
            {
                if (target.Alive) {
                    if (_skill.InBombZone(_toPt, target.GetAttackablePosition(_shooter)))
                    {
                        Damage damage = target.DoSkillAffect(_skill);
                        if (!damage.isMiss && _frameData.Others.Length > 2)
                        {
                            //int effectTarget = int.Parse(_frameData.Others[3]);

                            //if (effectTarget == Skill.EFFECT_TARGET_ROLE_TOP || effectTarget == Skill.EFFECT_TARGET_ROLE_BOTTOM)
                            //{
                            //    string hitEffect = _frameData.Others[2];
                            //    float offsetX = float.Parse(_frameData.Others[4]);
                            //    float offsetY = float.Parse(_frameData.Others[5]);
                            //    target.addHitEffect(hitEffect, offsetX, offsetY, target);
                            //}
                        }
                    }
                }
            }
            UnityEngine.Object.Destroy(_arrow);
            SetFinished(true);
        }
    }
}
