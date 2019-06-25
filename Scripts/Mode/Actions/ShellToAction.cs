// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-03-18 07:59:00
// 版 本：1.0
// ========================================================
using System;
using UnityEngine;
public class ShellToAction : AbstractAction
{
    private Role _shooter;
    private Role _target;
    private SkillConfigData _frameData;
    private Skill _skill;
    private float _speed;
    private Map _map;

    private Vector3 _startPt;    //投掷物开始位置
    private Vector3 _destPt;      //投掷物结束位置
    private Vector3 _controlPt;
    private float _flyTime;      //投掷时间
    private float _maxHight;     //最高投掷高度
    private bool _straight;//是否是直线射击;

    private GameObject _shell;
    public ShellToAction(Role from, Role to, Vector3 dest, Skill skill, SkillConfigData frameData, bool straight = false)
    {
        _shooter = from;
        _target = to;
        _skill = skill;
        _frameData = frameData;
        _destPt = dest;
        _map = _shooter.Map;
        _straight = straight;

        float offsetX = float.Parse(_frameData.Others[0]);
        float offsetY = float.Parse(_frameData.Others[1]);

        _shell = (GameObject)UnityEngine.Object.Instantiate(Resources.Load(SystemConsts.SKILL_EFF_PREFIX + _frameData.Effectname));
        _shell.GetComponent<SpriteRenderer>().sortingLayerName = SystemConsts.ActiveLayer;
        _shell.GetComponent<SpriteRenderer>().sortingOrder = 100;

        var face = _shooter.getOrientation() == Role.Orientation.RIGHT ? 1 : -1;


        _startPt = _shooter.transform.position;
        _startPt.x += offsetX * face;
        _startPt.y += offsetY;

        var targetFace = _target.getOrientation() == Role.Orientation.RIGHT ? 1 : -1;

        _speed = 2.2f;

        _shell.transform.position = _startPt;
        _maxHight = 5.0f;

        float missPercent = 10f;
        if(_frameData.Others.Length > 4)
        {
            missPercent = float.Parse(_frameData.Others[4]);
        }
        if (UnityEngine.Random.Range(0, 100) < missPercent)
        {
            float disX = Math.Abs(_destPt.x - _startPt.x);
            float disY = Math.Abs(_destPt.y - _startPt.y);
            float missOffsetX = _frameData.Others.Length > 5 ? float.Parse(_frameData.Others[5]) : 0.4f;
            float missOffsetY = _frameData.Others.Length > 6 ? float.Parse(_frameData.Others[6]) : 0.2f;

            _destPt.x += UnityEngine.Random.Range(-missOffsetX * disX/5f, missOffsetX * disX / 5f);
            _destPt.y += UnityEngine.Random.Range(-missOffsetY * disY/2f, missOffsetY * disY / 2f);
        }

        //初始化贝塞尔曲线
        Vector3 distanceVec = _destPt - _startPt;

        float angle = Mathf.Atan2(distanceVec.y, distanceVec.x) * Mathf.Rad2Deg;
        _shell.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        if(_straight)
        {
            _controlPt = new Vector3(_startPt.x + (_destPt.x - _startPt.x) * 0.5f, _startPt.y + (_destPt.y - _startPt.y) * 0.5f, 0);
        }
        else
        {
            float x = distanceVec.x > 0 ? Mathf.Max(_maxHight / 10, Mathf.Min(distanceVec.x, _maxHight / 2)) : distanceVec.x / 2;
            //float x = Mathf.Max(_maxHight / 10, Mathf.Min(distanceVec.x, _maxHight / 2));
            float y = Mathf.Max(_maxHight / 2, Mathf.Min(distanceVec.y, _maxHight));
            _controlPt = _startPt + new Vector3(x, y, 0);
        }
    }
    public override void Update()
    {
        Vector3 oldPoint = CalculateCubicBezierPoint(_flyTime, _startPt, _destPt, _controlPt);
        Vector3 newPoint = CalculateCubicBezierPoint(_flyTime += (Time.deltaTime * _speed), _startPt, _destPt, _controlPt);
        float angle = Mathf.Atan2(newPoint.y-oldPoint.y, newPoint.x - oldPoint.x) * Mathf.Rad2Deg;
        _shell.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        _shell.transform.position = newPoint;

        if(_flyTime >= 1.0f)
        {
            if (_target.Alive)
            {
                if (_frameData.Others.Length > 2)
                {
                    string bombEffect = _frameData.Others[2];
                    _destPt.z = _destPt.y;
                    if(_frameData.Others.Length > 3)
                    {
                        _destPt.z += float.Parse(_frameData.Others[3]);
                    }
                    _map.addEffect(SystemConsts.SKILL_EFF_PREFIX + bombEffect, _destPt);

                    if(_skill.InBombZone(_shell.transform.position, _target.GetAttackablePosition(_shooter)))
                    {
                        Damage damage = _target.DoSkillAffect(_skill);
                        //if (!damage.isMiss && _frameData.Others.Length > 4)
                        //{
                        //    string hitEffect = _frameData.Others[4];
                        //    float offsetX = _frameData.Others.Length > 5 ? float.Parse(_frameData.Others[5]) : 0f;
                        //    float offsetY = _frameData.Others.Length > 6 ? float.Parse(_frameData.Others[6]) : 0f;
                        //    _target.addHitEffect(hitEffect, offsetX, offsetY, _target);
                        //}
                    }
                }
            }
            UnityEngine.Object.Destroy(_shell);
            SetFinished(true);
        }
    }
    private Vector3 CalculateCubicBezierPoint(float t, Vector3 startPoint, Vector3 endPoint, Vector3 controlPoint)//贝塞尔曲线公式 生成一条贝塞尔曲线
    {
        Vector3 PassPoint;
        PassPoint.x = t * t * (endPoint.x - 2 * controlPoint.x + startPoint.x) + startPoint.x + 2 * t * (controlPoint.x - startPoint.x);
        PassPoint.y = t * t * (endPoint.y - 2 * controlPoint.y + startPoint.y) + startPoint.y + 2 * t * (controlPoint.y - startPoint.y);
        PassPoint.z = t * t * (endPoint.z - 2 * controlPoint.z + startPoint.z) + startPoint.z + 2 * t * (controlPoint.z - startPoint.z);
        return PassPoint;
    }
}
