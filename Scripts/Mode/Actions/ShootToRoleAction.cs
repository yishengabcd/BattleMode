using System;
using UnityEngine;
public class ShootToRoleAction:AbstractAction
{
    private Role _shooter;
    private Role _target;
    private SkillConfigData _frameData;
    private float _speed;
    private Vector2 _fromPt;
    private Vector2 _toPt;
    private float _speedPerFrame;
    private Skill _skill;
    private Vector2 _arrowOffset;

    private GameObject _arrow;
    public ShootToRoleAction(Role from, Role to, SkillConfigData frameData, Skill skill)
    {
        _shooter = from;
        _target = to; 
        _frameData = frameData;
        _skill = skill;

        float offsetX = float.Parse(_frameData.Others[0]);
        float offsetY = float.Parse(_frameData.Others[1]);

        _arrow = (GameObject)UnityEngine.Object.Instantiate(Resources.Load(SystemConsts.SKILL_EFF_PREFIX+ _frameData.Effectname));
        _arrow.GetComponent<SpriteRenderer>().sortingLayerName = SystemConsts.ActiveLayer;
        _arrow.GetComponent<SpriteRenderer>().sortingOrder = 100;

        var face = _shooter.getOrientation() == Role.Orientation.RIGHT ? 1 : -1;
        _fromPt = _shooter.transform.position;
        _fromPt.x += offsetX* face;
        _fromPt.y += offsetY;

        var targetFace = _target.getOrientation() == Role.Orientation.RIGHT ? 1 : -1;
        _toPt = _target.GetAttackablePosition(_shooter);
        _arrowOffset = _target.GetHitLocalPosition(_shooter);
        _arrowOffset.x = _arrowOffset.x * targetFace;
        _arrowOffset.y = _target.GetHitPosition(_shooter).y - _toPt.y;
        _toPt.x = _toPt.x + _arrowOffset.x;
        _toPt.y = _toPt.y + _arrowOffset.y;

        _speed = 24.0f;
        _speedPerFrame = _speed / 60f;//(float)Application.targetFrameRate;

        _arrow.transform.position = _fromPt;

        Vector2 direction = _toPt - _fromPt;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        _arrow.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }
    public override void Update()
    {
        float step = _speed * Time.deltaTime;
        Vector2 to = Vector2.MoveTowards(_arrow.transform.position, _toPt, step);

        _arrow.transform.position = new Vector3(to.x,to.y,to.y);
        if (_speedPerFrame > Vector2.Distance(to, _toPt))
        {
            if (_target.Alive)
            {
                Vector2 hitPt = _toPt;
                hitPt.x -= _arrowOffset.x;
                hitPt.y -= _arrowOffset.y;
                if (_skill.InBombZone(hitPt, _target.GetAttackablePosition(_shooter)))
                {
                    Damage damage = _target.DoSkillAffect(_skill);
                    if (!damage.isMiss && _frameData.Others.Length > 2)
                    {
                        int effectTarget = int.Parse(_frameData.Others[3]);

                        if (effectTarget == Skill.EFFECT_TARGET_ROLE_TOP || effectTarget == Skill.EFFECT_TARGET_ROLE_BOTTOM)
                        {
                            string hitEffect = _frameData.Others[2];
                            float offsetX = float.Parse(_frameData.Others[4]);
                            float offsetY = float.Parse(_frameData.Others[5]);
                            _target.addHitEffect(hitEffect, offsetX, offsetY, _shooter);
                        }
                    }
                }
            }
            UnityEngine.Object.Destroy(_arrow);
            SetFinished(true);
        }
    }
}
