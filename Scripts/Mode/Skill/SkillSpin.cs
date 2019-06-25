// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-03-20 18:09:02
// 版 本：1.0
// ========================================================
using System;
using UnityEngine;
using System.Collections.Generic;
public class SkillSpin:Skill
{
    private bool _shooting = false;
    private float _shootLast = 1.4f;//持续时长
    private float _shootTimeCount;
    private int _arrowCount;
    private int _arrowMax;
    private float _shootStep;
    private float _range;

    public SkillSpin(int skillId, Role executor,Vector2 pt, Role target = null) : base(skillId, executor, pt, target)
    {
        //_arrowMax = 50;
        _arrowMax = 20 + (_role.GetRoleInfo().AGI / 20);
        _range  = 7.0f + (_role.GetRoleInfo().STR / 80);
        if(_range > 11f)
        {
            _range = 11f;
        }
    }
    public override void Update()
    {
        base.Update();
        if(_shooting)
        {
            _shootTimeCount += Time.deltaTime;
            int needT = Mathf.FloorToInt(_shootTimeCount / _shootStep);
            if(needT > _arrowMax)
            {
                needT = _arrowMax;
            }
            int needCurrent = needT - _arrowCount;
            List<Role> monsters = _role.Map.GetMonsters();
            List<Role> monstersInZone = new List<Role>();
            List<Role> priorRoles = new List<Role>();
            foreach (var item in monsters)
            {
                if (Vector2.Distance(_role.transform.position, item.GetAttackablePosition(_role)) < _range && item.Alive)
                {
                    monstersInZone.Add(item);
                }
            }

            if (monstersInZone.Count > 3)
            {
                monstersInZone.Sort((Role a, Role b)=>
                {
                    if(Vector2.Distance(a.transform.position,_role.transform.position) < Vector2.Distance(b.transform.position, _role.transform.position)) {
                        return -1;
                    }
                    return 1;
                });
                priorRoles.Add(monstersInZone[0]);
                priorRoles.Add(monstersInZone[1]);
                priorRoles.Add(monstersInZone[2]);
            }

            for (int i = 0; i < needCurrent; i++)
            {
                Role mon = null;
                if(priorRoles.Count > 0) {
                    mon = priorRoles[UnityEngine.Random.Range(0, priorRoles.Count)];
                }
                else
                {
                    if (monstersInZone.Count > 0)
                    {
                        mon = monstersInZone[UnityEngine.Random.Range(0, monstersInZone.Count)];
                    }
                }

                ShootToMonster(mon);
                _arrowCount++;
            }
        }
    }
    private void ShootToMonster(Role monster)
    {
        Vector2 start = Vector2.zero;
        int side = 0;
        start.x = _role.transform.position.x + UnityEngine.Random.Range(0.1f, 0.3f) * side;
        start.y = _role.transform.position.y + UnityEngine.Random.Range(0.3f, 1.2f);
        IAction action = null;
        if (monster != null)
        {
            side = monster.transform.position.x > _role.transform.position.x ? 1 : -1;
            action = new FreeShootAction(_role, monster, this, start, Vector2.zero);
            _role.Map.addAction(action);
        }
        else
        {
            Vector2 end = _role.transform.position;
            end.x = end.x + UnityEngine.Random.Range(-_range, _range);
            end.y = end.y + UnityEngine.Random.Range(-_range, _range);
            side = end.x > _role.transform.position.x ? 1 : -1;
            action = new FreeShootAction(_role, null, this, start, end);
            _role.Map.addAction(action);
        }
    }
    protected override void RandomShoot()
    {
        _shooting = true;
        _shootTimeCount = 0f;
        _arrowCount = 0;
        _shootStep = _shootLast / _arrowMax;
    }
}
