// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-03-15 11:10:20
// 版 本：1.0
// ========================================================
using UnityEngine;
using System.Collections;

public class Wall : Role
{
    public Transform FootUp;
    public Transform FootDown;
    private Vector2 _footUpPt;
    private Vector2 _footDownPt;
    private float _attackableXCalcFactor;
    public GameObject DieView;
    protected override void InitArmature()
    {
    }
    public new void PlayMotion(string motion, int repeat = -1, string afterMotion = null, bool replay = true)
    {

    }
    protected override void SetBeAttacking(bool value, bool isCallPartner = true)
    {
    }
    public override Vector2 GetHitPosition(Role attacker = null)
    {
        Vector2 pt = GetAttackablePosition(attacker);
        pt.y += 0.6f;
        pt.x += -0.6f;
        return pt;
    }
    public override Vector2 GetHitLocalPosition(Role attacker = null)
    {
        Vector2 pt = GetHitPosition(attacker);
        pt.x = pt.x - transform.position.x;
        pt.y = pt.y - transform.position.y;
        return pt;
    }
    public override Vector2 GetAttackablePosition(Role attacker)
    {
        Vector2 pt = transform.position;
        if(Vector2.Distance(pt, attacker.transform.position) < 3)
        {
            pt.y = attacker.transform.position.y;
        }
        else
        {
            pt.y = attacker.BornPosition.y;
        }
        if(pt.y > _footUpPt.y)
        {
            pt.y = _footUpPt.y;
        }
        else if(pt.y < _footDownPt.y)
        {
            pt.y = _footDownPt.y;
        }
        pt.x = _footDownPt.x + (pt.y - _footDownPt.y) * _attackableXCalcFactor;
        return pt;
    }

    protected override void Init()
    {
        _roleInfo = GameModel.Instance.WallInfo;
        base.Init();
        _footUpPt = FootUp.transform.position;
        _footDownPt = FootDown.transform.position;
        _attackableXCalcFactor = (_footUpPt.x - _footDownPt.x) / (_footUpPt.y - _footDownPt.y);
    }
    // Use this for initialization
    protected override void Start()
    {
        base.Start();
    }
    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }
    public override void addHitEffect(string name, float offsetX, float offsetY, Role attacker, bool scaleByOrientatio = true)
    {
        Vector2 pt = GetHitLocalPosition(attacker);
        pt.x += offsetX;
        pt.y += offsetY;
        AddEffect(SystemConsts.SKILL_EFF_PREFIX + name, pt, true, scaleByOrientatio);
    }
    public override void RemoveFromMap()
    {
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
    public override bool isLife => false;
    public void ShowDieView()
    {
        DieView.SetActive(true);
    }
}
