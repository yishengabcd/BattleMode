// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-03-15 14:12:16
// 版 本：1.0
// ========================================================
using System;
using UnityEngine;
using System.Collections.Generic;
public class Wave:EventDispatcher
{
    public static readonly int PARAM_TYPE_RANDOM_NUM = 1;// 随机出现一定数量
    public static readonly int PARAM_TYPE_RANDOM_SNEAK = 2;// 在地图的随机位置出现
    private int tplId;
    private WaveTplData _tpl;
    private List<WaveConfigData> _waveConfigs;
    private float _pastTime;
    private float __arriveTimeLeft;
    private int _arriveTime;
    private bool _arrived;
    private bool _paused;
    private bool _counting;
    private int _index;//第一关为1

    public Wave(int tplId, int index)
    {
        TplId = tplId;
        _index = index;
        _pastTime = 0.0f;
        _counting = true;
        _tpl = TemplateManager.GetWaveTpl(tplId);
        if (_tpl == null)
        {
            Debug.LogErrorFormat("未找到波次模板：tplId={0}", tplId);
        }
        _waveConfigs = TemplateManager.GetWaveConfigDatas(tplId);
        __arriveTimeLeft = _tpl.Arrivetime;
        _arriveTime = _tpl.Arrivetime;
    }
    public void Update()
    {
        if (_paused) return;

        if (_arriveTime > 0)
        {
            if(_counting)
            {
                __arriveTimeLeft -= Time.deltaTime;
                int sec = Mathf.CeilToInt(__arriveTimeLeft);
                if (_arriveTime > sec)
                {
                    _arriveTime = sec;
                    dispatch(new WaveEvent(WaveEvent.COUNT));
                }
                if (_arriveTime < 1)
                {
                    _arrived = true;
                    _pastTime = 0f;
                    dispatch(new WaveEvent(WaveEvent.COUNT_STATE));
                }
            }
        }
        if (_arrived)
        {
            _pastTime += Time.deltaTime;
            while (_waveConfigs.Count > 0 && _waveConfigs[0].Time < _pastTime)
            {
                WaveConfigData configData = _waveConfigs[0];
                if (configData.Param.Length > 2 && configData.Param[0] == PARAM_TYPE_RANDOM_NUM)
                {
                    List<int> idsA = new List<int>(configData.Monsters);
                    List<int> selectedIds = new List<int>();
                    int c = UnityEngine.Random.Range(configData.Param[1], configData.Param[2]+1);
                    if(c > 0 && idsA.Count > c)
                    {
                        while (selectedIds.Count < c)
                        {
                            int idx = UnityEngine.Random.Range(0, idsA.Count);
                            int monsterId = idsA[idx];
                            idsA.RemoveAt(idx);
                            selectedIds.Add(monsterId);
                        }
                        dispatch(new WaveEvent(WaveEvent.MONSTERS_ARRIVED, selectedIds.ToArray(), configData));
                    }
                    else if(idsA.Count > 0)
                    {
                        dispatch(new WaveEvent(WaveEvent.MONSTERS_ARRIVED, configData.Monsters, configData));
                    }
                }
                else
                {
                    dispatch(new WaveEvent(WaveEvent.MONSTERS_ARRIVED, configData.Monsters, configData));
                }
                _waveConfigs.RemoveAt(0);
            }
        }
    }
    public void SkipCount()
    {
        if(!_arrived)
        {
            _pastTime = 0f;
            __arriveTimeLeft = 0f;
            _arriveTime = 0;
            _arrived = true;
            dispatch(new WaveEvent(WaveEvent.COUNT));
            dispatch(new WaveEvent(WaveEvent.COUNT_STATE));
        }
    }
    public List<int> GetMonsterIds()
    {
        List<int> monsterIds = new List<int>();
        foreach (var item in _waveConfigs)
        {
            foreach (var id in item.Monsters)
            {
                if (id != 0 && monsterIds.IndexOf(id) == -1)
                {
                    monsterIds.Add(id);
                }
            }
        }
        return monsterIds;
    }
    public int TplId { get => tplId; set => tplId = value; }
    public int ArriveTime { get => _arriveTime; }
    public bool Paused { get => _paused; set => _paused = value; }
    public bool IsAllArrived => _waveConfigs.Count == 0;
    public int Index => _index;
    public bool CanSkipCount => _index > 1 || GameModel.Instance.DieTime > -1;
    public bool Arrived => _arrived;
    public bool Counting
    {
        get
        {
            if (_arrived) return false;
            return _counting;
        }
        set
        {
            if(_counting != value && !_arrived)
            {
                _counting = value;
                dispatch(new WaveEvent(WaveEvent.COUNT_STATE));
            }
        }
    }
    public string Name
    {
        get
        {
            if (Language.Type == Language.LangType.LANG_ZH)
            {
                return _tpl.Name_ZH;
            }
            return _tpl.Name_EN;

        }
    }
}
