// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-04-19 09:01:23
// 版 本：1.0
// ========================================================
using System;
using UnityEngine;
using System.Collections.Generic;
public class Matrix
{
    public static readonly int STATE_GROUP = 0;
    public static readonly int STATE_MARCHING = 1;
    public static readonly int STATE_BEATTACKING = 2;
    public static readonly int STATE_ARRIVED = 3;
    private int _state;
    private bool arrived;
    private Map _map;
    private int _matrixId;
    private List<Role> members;
    private Dictionary<Role, int> rolePositionRecord;
    private Dictionary<int, Vector2> positionDict;
    private float _groupingTime;
    private Vector2 _matrixPosition;
    private Vector2 _topLeftPosition;
    private MatrixTplData _tpl;
    private Rect _dangerRect;
    private float _matrixW;
    private float _matrixH=1;
    private float _outRange = 0.3f;
    private float _beattackRange = 0.0f;
    private int _checkDangerCount;
    //private static float centerY = -1.77f;
    //private int _martchCount = 0;

    public int MatrixId { get => _matrixId; }
    public int State { get => _state;}
    public bool Arrived { get => arrived; }
    public MatrixTplData Tpl { get => _tpl; }

    public static readonly int JOB_FREE_ATTACT = 1;
    public static readonly int JOB_ATTACK_WALL_ONLY = 2;
    public static readonly int JOB_GUARD = 3;
    public static readonly int JOB_REMOTE_ATTACK = 4;

    public Matrix(int matrixId, Map map)
    {
        _groupingTime = 0f;
        _state = STATE_GROUP;
        _matrixId = matrixId;
        _map = map;
        members = new List<Role>();
        rolePositionRecord = new Dictionary<Role, int>();
        _tpl = TemplateManager.GetMatrixTpl(_matrixId);
        _matrixPosition = _map.GetMatrixGroupPosition();
        _matrixPosition.y = _tpl.Dest[1];
        _topLeftPosition = new Vector2(_matrixPosition.x, _matrixPosition.y + _tpl.Vgap*2);
        positionDict = new Dictionary<int, Vector2>();
        _dangerRect = new Rect();
    }
    public void Update()
    {
        if (_state == STATE_GROUP)
        {
            _groupingTime+= Time.deltaTime;
            if (_groupingTime > 1.5f)
            {
                _state = STATE_MARCHING;
                foreach (var role in members)
                {
                    if (role.CanAction())
                    {
                        role.AddAction(new MatMemberMarchAction(role));
                    }
                }
            }
        }
        else if(_state == STATE_MARCHING)
        {
            float step = _tpl.Movespeed * Time.deltaTime;
            _matrixPosition.x -= step;
            if (_matrixPosition.x < _tpl.Dest[0])
            {
                _matrixPosition.x = _tpl.Dest[0];
                _state = STATE_ARRIVED;
                arrived = true;
            }
            _topLeftPosition.x = _matrixPosition.x;

            foreach (var item in rolePositionRecord)
            {
                positionDict[item.Value] = GetPositionByIndex(item.Value);
            }
            if(_map.hero != null && _map.hero.Alive) {
                _dangerRect.x = _topLeftPosition.x - _tpl.Dangerrange[0];
                //_dangerRect.y = _topLeftPosition.y + _tpl.Dangerrange[1];
                _dangerRect.width = _tpl.Dangerrange[0] + _matrixW + _tpl.Dangerrange[2];
                //_dangerRect.height = _tpl.Dangerrange[1] + _matrixH + _tpl.Dangerrange[3];
                bool inDanger = false;
                Vector2 heroPt = _map.hero.transform.position;
                inDanger = _map.hero.Alive && _map.hero.Alive && _dangerRect.x - _beattackRange < heroPt.x && _dangerRect.x + _dangerRect.width + _beattackRange > heroPt.x;
                if (inDanger)
                {
                    _checkDangerCount = 0;
                    _state = STATE_BEATTACKING;
                }
            }
        }
        else if (_state == STATE_BEATTACKING)
        {
            _checkDangerCount++;
            if (_checkDangerCount > 30)
            {
                _checkDangerCount = 0;

                bool inDanger = false;
                Vector2 heroPt = _map.hero.transform.position;
                inDanger = _map.hero.Alive && _dangerRect.x - _outRange - _beattackRange < heroPt.x && _dangerRect.x + _dangerRect.width + _outRange+ _beattackRange > heroPt.x;
                //if (!arrived)
                //{
                //    if (IsMostMemberArrived())
                //    {
                //        _state = STATE_MARCHING;
                //        _matrixPosition.x = _tpl.Dest[0];
                //        return;
                //    }
                //}
                if (!inDanger)
                {
                    if (arrived)
                    {
                        _state = STATE_ARRIVED;
                        foreach (var role in members)
                        {
                            if(role.aiTpl.Matrixjob != JOB_FREE_ATTACT && role.CanAction())
                            {
                                role.AddAction(new MatRunToPosition(role));
                            }
                        }
                    }
                    else
                    {
                        _state = STATE_MARCHING;
                        RelocateMatrixPositionAfterAttacking();

                        foreach (var role in members)
                        {
                            if (role.CanAction())
                            {
                                role.AddAction(new MatMemberMarchAction(role));
                            }
                        }
                    }
                }
            }
        }
        else if (_state == STATE_ARRIVED)
        {
            if (_map.hero != null && _map.hero.Alive)
            {
                _dangerRect.x = _topLeftPosition.x - _tpl.Dangerrange[0];
                _dangerRect.width = _tpl.Dangerrange[0] + _matrixW + _tpl.Dangerrange[2];
                bool inDanger = false;
                Vector2 heroPt = _map.hero.transform.position;
                inDanger = _dangerRect.x - _beattackRange < heroPt.x && _dangerRect.x + _dangerRect.width + _beattackRange > heroPt.x;
                if (inDanger)
                {
                    _checkDangerCount = 0;
                    _state = STATE_BEATTACKING;
                }
            }
        }
    }
    private void RelocateMatrixPositionAfterAttacking()
    {
        float x = 100f;
        foreach (var role in members)
        {
            if (role.transform.position.x < x)
            {
                x = role.transform.position.x;
            }
        }
        if(_matrixPosition.x > x) _matrixPosition.x = x;
    }
    private bool IsMostMemberArrived()
    {
        int count = 0;
        float line = _tpl.Dest[0] + 3;
        foreach (var role in members)
        {
            if (role.transform.position.x < line)
            {
                count++;
            }
        }
        if ((float)count > (float)members.Count * 0.7f) return true;
        return false;
    }
    public void AddMember(Role role, int pos)
    {
        if (role.Alive && members.IndexOf(role) == -1)
        {
            role.Matrix = this;
            members.Add(role);
            rolePositionRecord[role] = pos;
            positionDict[pos] = GetPositionByIndex(pos);
            _matrixW = (pos / 5) * _tpl.Hgap+1;
            _matrixH = Mathf.Max(_matrixH, (pos%5)*_tpl.Vgap);
        }

    }
    public void SetBeAttacking(bool value)
    {
        if (value)
        {
            _beattackRange = 30f;
        }
        else
        {
            _beattackRange = 0.0f;
        }
    }
    public bool PursueHero(Role monster)
    {
        return true;
    }
    private Vector2 GetPositionByIndex(int index)
    {
        Vector2 pt = new Vector2(_topLeftPosition.x + (index/5)*_tpl.Hgap, _topLeftPosition.y - (index%5)*_tpl.Vgap);
        if(!_map.CanMoveTo(pt))
        {
            Vector2 start = new Vector2(pt.x, _matrixPosition.y);
            if (_map.CanMoveTo(start))
            {
                pt = _map.GetNearstPoint(start, pt);
            }
            else
            {
                pt.y = _map.MonsterBornPositions[index].transform.position.y;
            }
        }

        return pt;
    }
    public bool IsOutPursueRange(Role mem)
    {
        return false;
        //_checkDangerCount = 0;

        //bool inDanger = false;
        //Vector2 heroPt = _map.hero.transform.position;
        //inDanger = _dangerRect.x - _outRange < heroPt.x && _dangerRect.x + _dangerRect.width + _outRange > heroPt.x;

        //return !inDanger;
    }
    public void RemoveMember(Role mem)
    {
        members.Remove(mem);
        mem.Matrix = null;
        rolePositionRecord.Remove(mem);
    }
    public bool Dimiss()
    {
        bool dimiss = members.Count == 0;
        if(!dimiss)
        {
            foreach (var id in _tpl.Holdcondition)
            {
                bool contain = false;
                foreach (var mem in members)
                {
                    if (mem.RoleId == id)
                    {
                        contain = true;
                        break;
                    }
                }
                if (!contain)
                {
                    dimiss = true;
                    break;
                }
            }
        }

        if (dimiss)
        {
            for (int i = members.Count - 1; i > -1; i--)
            {
                Role mem = members[i];
                mem.Matrix = null;
            }
            members = null;
            _map.DimissMatrix(this);
        }
        return dimiss;
    }
    public Vector2 GetMatrixPosition(Role member)
    {
        int ptIndex;
        if (rolePositionRecord.TryGetValue(member, out ptIndex))
        {
            Vector2 pt = positionDict[ptIndex];
            return pt;
        }
        return Vector2.zero;
    }
    public bool MemberIsInPosition(Role mem)
    {
        if (Vector2.Distance(mem.transform.position, GetMatrixPosition(mem)) > 0.1f)
        {
            return false;
        }
        return true;
    }
}
