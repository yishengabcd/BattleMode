// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-03-20 21:07:38
// 版 本：1.0
// ========================================================
using System;
using UnityEngine;
//目前只能用于英雄的技能3,如需通用，需重构。
public class FreeShootAction: AbstractAction
{
    private Role _shooter;
    private Role _target;
    private float _speed;
    private Vector2 _fromPt;
    private Vector2 _toPt;
    private float _speedPerFrame;

    private Skill _skill;
    private GameObject _arrow;
    public FreeShootAction(Role from, Role to, Skill skill,Vector2 start, Vector2 dest)
    {
        _shooter = from;
        _target = to;
        _skill = skill;

        _arrow = (GameObject)UnityEngine.Object.Instantiate(Resources.Load(SystemConsts.SKILL_EFF_PREFIX + "Hero/HrSpinShoot"));
        _arrow.GetComponent<SpriteRenderer>().sortingLayerName = SystemConsts.ActiveLayer;
        _arrow.GetComponent<SpriteRenderer>().sortingOrder = 100;

        _fromPt = start;

        if(_target != null)
        {
            var targetFace = _target.getOrientation() == Role.Orientation.RIGHT ? 1 : -1;
            _toPt = _target.transform.position;
            _toPt.x = _toPt.x + _target.GetHitLocalPosition(_shooter).x * targetFace;
            _toPt.y = _target.GetHitPosition(_shooter).y;
        }
        else
        {
            _toPt = dest;
        }

        _speed = 30.0f;
        _speedPerFrame = _speed / 60.0f;

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
            if (_target != null && _target.Alive)
            {
                float offsetX = UnityEngine.Random.Range(-0.2f, 0.2f);
                float offsetY = UnityEngine.Random.Range(-0.2f, 0.2f);
                _target.addHitEffect("Hero/HrSpinHit", offsetX, offsetY, _target);
                _target.DoSkillAffect(_skill);
            }
            UnityEngine.Object.Destroy(_arrow);
            SetFinished(true);
        }
    }
}
