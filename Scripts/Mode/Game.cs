// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-03-17 09:45:25
// 版 本：1.0
// ========================================================
#if UNITY_ANDROID || UNITY_IOS || UNITY_TIZEN || UNITY_TVOS || UNITY_WEBGL || UNITY_WSA || UNITY_PS4 || UNITY_WII || UNITY_XBOXONE || UNITY_SWITCH
#define DISABLESTEAMWORKS
#endif
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Map))]
public class Game : MonoBehaviour
{
    public static readonly int BATTLE_TYPE_GUARD = 1;
    public static readonly int BATTLE_TYPE_TREEHOLE = 2;
    public static readonly int BATTLE_TYPE_CHAPTER = 3;
    protected Map _map;
    protected Hero _hero;
    protected Wall _wall;
    protected Wave _wave;
    public Wave wave => _wave;
    protected bool _beAttacking;
    protected bool _battleEnd = false;
    public bool IsGameOver => _battleEnd;
    public Map map => _map;
    public virtual int BattleType => BATTLE_TYPE_GUARD;

    public bool BeAttacking => _beAttacking;
    private GroupAiMgr groupAiMgr;

    protected int _creatingMonsterCount = 0;
    // Use this for initialization
    protected virtual void Awake()
    {
        if (!GameModel.Instance.FinalWin) StartWave();
        GameManager.SetGame(this);
    }
    void Start()
    {
        _map = GetComponent<Map>();
        _hero = _map.hero;
        _wall = _map.wall;
        _hero.AutoRecover();
        groupAiMgr = new GroupAiMgr(this);

        //技能测试使用
#if UNITY_EDITOR
        //NoticeBar.Instance.Notice("开启了技能测试功能");
        //_beAttacking = true;
        //GlobalEventLocator.Instance.dispatch(new GameEvent(GameEvent.BATTLE_STATE_CHANGED));
#endif
    }
    private void StartWave()
    {
        ResetWave();
        _wave = new Wave(GameModel.Instance.Wave, GameModel.Instance.Wave);
        _wave.addListener(WaveEvent.MONSTERS_ARRIVED, OnMonstersArrived);
        GlobalEventLocator.Instance.dispatch(new GameEvent(GameEvent.WAVE_START));
        //if(GameModel.Instance.Wave == 1)
        {
            CacheWaveMonster();
        }
    }
    private void CacheWaveMonster() {
        List<int> ids = _wave.GetMonsterIds();
        List<string> addeds = new List<string>();
        int count = 0;
        foreach (var monsterId in ids)
        {
            RoleTplData roleTpl = TemplateManager.GetRoleTplData(monsterId);
            if (addeds.IndexOf(roleTpl.Prefabname) == -1)
            {
                addeds.Add(roleTpl.Prefabname);

                GameObject go = (GameObject)Instantiate(Resources.Load("Prefabs/Roles/" + roleTpl.Prefabname));
                Monster monster = go.GetComponent<Monster>();
                monster.RoleId = monsterId;
                Vector2 pt = Vector2.zero;
                pt.x = 100;//Random.Range(MonsterBornPositions[0].position.x, MonsterBornPositions[1].position.x);
                pt.y = 100;//Random.Range(MonsterBornPositions[1].position.y, MonsterBornPositions[0].position.y);
                go.transform.position = pt;
                monster.IsModel = true;
            }
            count++;
            if (count > 2)
            {
                break;
            }
        }
    }
    private void ResetWave()
    {
        if (_wave != null)
        {
            _wave.removeListener(WaveEvent.MONSTERS_ARRIVED, OnMonstersArrived);
            _wave = null;
        }
        _battleEnd = false;
        _beAttacking = false;
        _creatingMonsterCount = 0;
    }
    public void NextWave()
    {
        if(GameModel.Instance.Wave > GameModel.Instance.MaxWave)
        {
            ResetWave();
            //GameModel.Instance.FinalWin = true;

            //NoticeBar.Instance.Notice(Language.Lang.GMAE_WIN_FINAL,NoticeBar.LEVEL_HIGH);
            FindObjectOfType<FullSceenDialogue>().ShowMsg(Language.Lang.GMAE_WIN_FINAL);
        }
        else
        {
           // GameModel.Instance.Wave++;//改为在胜利面板弹出时处理；
            StartWave();
        }
    }
    private void OnMonstersArrived(EventBase e)
    {
        //DataService.Instance.Safe = false;
        GameModel.Instance.OpenFunc(GameModel.FUNC_ID_SKILL);
        _beAttacking = true;
        GlobalEventLocator.Instance.dispatch(new GameEvent(GameEvent.BATTLE_STATE_CHANGED));
        WaveEvent waveEvent = e as WaveEvent;
        int[] monsters = waveEvent.Monsters;
        _creatingMonsterCount++;
        StartCoroutine(CreateMonsters(monsters, waveEvent.WaveConfigTpl));
        SoundManager.InstanceOfScene.PlayBattleMusic();
    }
    private IEnumerator CreateMonsters(int[] monsters, WaveConfigData configData)
    {
        Dictionary<string, ResourceRequest> loader = new Dictionary<string, ResourceRequest>();
        for (int i = 0; i < monsters.Length; i++)
        {
            int monsterId = monsters[i];
            if (monsterId != 0 && i < _map.MonsterBornPositions.Length)
            {
                RoleTplData roleTpl = TemplateManager.GetRoleTplData(monsterId);
                if (roleTpl != null && !loader.ContainsKey(roleTpl.Prefabname))
                {
                    ResourceRequest rr = Resources.LoadAsync("Prefabs/Roles/" + roleTpl.Prefabname);
                    loader.Add(roleTpl.Prefabname, rr);
                    yield return rr;
                }
            }
        }
        //List<int> points = new List<int>(new int[] {0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20 });
        for (int i = 0; i < monsters.Length; i++)
        {
            int monsterId = monsters[i];
            if (monsterId != 0 &&  configData.Param.Length > 0 && configData.Param[0] == Wave.PARAM_TYPE_RANDOM_SNEAK)
            {
                RoleTplData roleTpl = TemplateManager.GetRoleTplData(monsterId);
                if (roleTpl == null)
                {
                    Debug.LogError(string.Format("WaveConfig表中找不到怪物模板，monsterId={0}", monsterId));
                }
                else
                {
                    GameObject go = (GameObject)Instantiate(loader[roleTpl.Prefabname].asset);
                    Monster monster = go.GetComponent<Monster>();
                    monster.RoleId = monsterId;
                    monster.SetPosition(_map.GetRandomBornPosition(configData.Param.Length > 1? configData.Param[1]:-1));
                    _map.AddMonster(monster,0);

                    _map.addEffect("Prefabs/Effects/MonsterAppearEf", monster.transform.position);

                    yield return new WaitForSeconds(0.01f);
                }
            }
            else
            {
                if (monsterId != 0 && i < _map.MonsterBornPositions.Length)
                {
                    RoleTplData roleTpl = TemplateManager.GetRoleTplData(monsterId);
                    if (roleTpl == null)
                    {
                        Debug.LogError(string.Format("WaveConfig表中找不到怪物模板，monsterId={0}", monsterId));
                    }
                    else
                    {
                        int pos = i;
                        if (configData.Param.Length > 1 && configData.Param[0] == Wave.PARAM_TYPE_RANDOM_NUM)
                        {
                            pos = Random.Range(0, _map.MonsterBornPositions.Length);
                        }
                        GameObject go = (GameObject)Instantiate(loader[roleTpl.Prefabname].asset);
                        Monster monster = go.GetComponent<Monster>();
                        monster.RoleId = monsterId;
                        monster.SetPosition(_map.MonsterBornPositions[pos].position);
                        _map.AddMonster(monster, pos);
                        yield return new WaitForSeconds(0.01f);
                    }
                }
            }
        }
        _creatingMonsterCount--;
    }
   

    // Update is called once per frame
    protected virtual void Update()
    {
        groupAiMgr.Update();
        if (_battleEnd || _wave == null) return;
        _battleEnd = CheckBattleOver();
        if(!_battleEnd)
        {
            _wave.Update();
        }
    }
    protected bool CheckBattleOver()
    {
        if (!_hero.Alive || (_wall != null && !_wall.Alive))
        {
            OnBattleLose();
            return true;
        }
        if (_creatingMonsterCount > 0 || (_wave != null && !_wave.IsAllArrived)) return false;
        if(_map.GetMonsters().Count == 0)
        {
            OnBattleWin();
            return true;
        }

        return false;
    }
    protected virtual void OnBattleLose()
    {
        MainWindow mainWindow = FindObjectOfType<MainWindow>();
        if (mainWindow != null)
        {
            mainWindow.gameObject.SetActive(false);
        }
        if (!_hero.Alive) {
            if (_hero.GetRoleInfo().Level < 3)
            {
                ResetGameCtrl.Instance.ResetGame(false);
            }
            else
            {
                DropManager.DropWhenHeroDie();
            }
            GameModel.Instance.DieTime++;

#if !DISABLESTEAMWORKS
            SteamAchievementMgr.Instance.AchievementAboutNumChanged();
#endif
        }

        GlobalEventLocator.Instance.dispatch(new GameEvent(GameEvent.BATTLE_FAILED));
        GameObject faildPanel = (GameObject)Instantiate(Resources.Load("Prefabs/BattleResult/FailedPanel"), GameObject.Find("MainCanvas").transform);
    }
    protected virtual void OnBattleWin()
    {
        MainWindow mainWindow = FindObjectOfType<MainWindow>();
        if (mainWindow != null)
        {
            mainWindow.gameObject.SetActive(false);
        }

        GameObject victoryPanel = (GameObject)Instantiate(Resources.Load("Prefabs/BattleResult/VictoryPanel"), GameObject.Find("MainCanvas").transform);
        BattleVictory bv = victoryPanel.GetComponent<BattleVictory>();
        BattleResultData resultData = null;
        if (_map.IsWallMap)
        {
            WaveTplData waveTpl = TemplateManager.GetWaveTpl(_wave.TplId);
            resultData = DropManager.CreateBattleResult(waveTpl.Dropid);
            resultData.Exp = waveTpl.Exp;
        }
        else
        {
            resultData = DropManager.CreateBattleResult(_map.Tpl.Dropid, _map.Tpl);
            resultData.Exp = _map.Tpl.Exp;
        }
        resultData.Exp = (int)(resultData.Exp * (1 + Random.Range(-0.1f, 0.1f)));
        _hero.ObtainPastExp(resultData.Exp);

        resultData.BattleType = BattleType;
        bv.data = resultData;

        if(BattleType == BATTLE_TYPE_TREEHOLE)
        {
            TreeHoleCtrl.Instance.Win();
        }
        else if (BattleType == BATTLE_TYPE_CHAPTER)
        {
            WorldCtrl.Instance.Win();
        }

        _beAttacking = false;

        GameModel.Instance.WinTime++;

#if !DISABLESTEAMWORKS
        SteamAchievementMgr.Instance.AchievementAboutNumChanged();
        SteamAchievementMgr.Instance.WinABattle();
        if (BattleType == BATTLE_TYPE_TREEHOLE)
        {
            if(TreeHoleCtrl.Instance.CurrentLayerNum == 30)
            {
                SteamAchievementMgr.Instance.PastLastHole();
            }
        }
#endif

        GlobalEventLocator.Instance.dispatch(new GameEvent(GameEvent.BATTLE_STATE_CHANGED));
        _hero.ClearBuffs();
        UserSkillMgr.CancelSkill();

        if (BattleType == BATTLE_TYPE_GUARD)
        {
            GameModel.Instance.Wave++;
            SoundManager.InstanceOfScene.PlayPeaceBmg();
        }


        GameModel.Instance.OpenFunc(GameModel.FUNC_ID_BAG);
        if (!GameModel.Instance.IsFuncOpen(GameModel.FUNC_ID_INLAY))
        {
            foreach (var item in bv.data.Items)
            {
                if (item.Type == GoodsInfo.TYPE_GEM)
                {
                    GameModel.Instance.OpenFunc(GameModel.FUNC_ID_INLAY);
                }
            }
            if(!GameModel.Instance.IsFuncOpen(GameModel.FUNC_ID_INLAY) && GameModel.Instance.HeroInfo.Level >= SystemConsts.OPEN_INLAY_LEVEL)
            {
                GameModel.Instance.OpenFunc(GameModel.FUNC_ID_INLAY);
            }
        }

        DataService.Instance.Safe = true;

        AdManager.Instance.ShowInterstitial();
    }

    public bool AttackCounting
    {
        get
        {
            if (_wave != null)
            {
                return _wave.Counting;
            }
            return false;
        }
    }
    public void StopCounting()
    {
        if(_wave != null) _wave.Counting = false;
    }
    public void StartCounting()
    {
        if (_wave != null) _wave.Counting = true;
    }

    public void Pause()
    {

    }
    public void Resume()
    {

    }
    private void OnDestroy()
    {
        if(_wave != null) _wave.removeListener(WaveEvent.MONSTERS_ARRIVED, OnMonstersArrived);
    }
}
