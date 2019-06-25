// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-03-14 00:09:42
// 版 本：1.0
// ========================================================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public PathFinding pathFinding;

    protected MapTplData tpl;//城墙地图未使用此字段
    public Hero hero;
    public Wall wall;
    public Transform[] HeroBornPositions;
    public Transform[] MonsterBornPositions;
    private List<Role> monsters;
    private List<Role> myTeamMembers;
    public List<MapItem> MapItems;
    public MapItem ChapterPort;
    public bool IsWallMap = true;
    protected HeroInfo heroInfo;
    private List<Matrix> matrices;
    public List<Rect> BornZones;


    private SpawnActionMgr _spawnActionMgr;
    private MapEnvSfxMgr _envSoundMgr;

    public MapTplData Tpl { get => tpl;}


    protected virtual void Awake()
    {
        _spawnActionMgr = new SpawnActionMgr();
        hero.Map = this;
        GameManager.SetHero(hero);
        heroInfo = GameModel.Instance.HeroInfo;
        if (HeroBornPositions != null && HeroBornPositions.Length > 0)
        {
            if (HeroBornPositions.Length == 1)
            {
                hero.SetPosition(HeroBornPositions[0].position);
            }
            else
            {
                hero.SetPosition(HeroBornPositions[GameModel.Instance.HeroBornPointIdx].position);
                GameModel.Instance.ResetHeroBornPoint();
            }
            Camera.main.gameObject.GetComponent<CameraFollow>().LookAtHero();
        }
        monsters = new List<Role>(FindObjectsOfType<Monster>());
        foreach (var item in monsters)
        {
            item.Map = this;
        }
        myTeamMembers = new List<Role>();
        myTeamMembers.Add(hero);
        if(wall != null)
        {
            myTeamMembers.Add(wall);
        }
        ////预加载常用特效
        //Resources.Load("CommonSkillEff");
        //Resources.Load("Prefabs/Skills/Hero/Scalpel");
        //Resources.Load("Prefabs/Skills/ShouJi0");
        //Resources.Load("Prefabs/Skills/ShouJi1");
        //Resources.Load("Prefabs/Effects/LevelUp");
    }
    void Start()
    {
        InitMapItems();
        heroInfo.addListener(HeroInfoEvent.LEVELUP, OnFuncOpen);
        OnFuncOpen(null);
        matrices = new List<Matrix>();
        _envSoundMgr = new MapEnvSfxMgr(this);

        AdManager.Instance.RequestBanner();
        AdManager.Instance.RequestInterstitial();
        AdManager.Instance.RequestRewardedAd();
    }
    private void OnFuncOpen(EventBase e)
    {
        if (ChapterPort != null)
        {
            if(hero.GetRoleInfo().Level >= SystemConsts.OPEN_CHAPTER_LEVEL)//if (GameModel.Instance.IsFuncOpen(GameModel.FUNC_ID_CHAPTER))
            {
                ChapterPort.gameObject.SetActive(true);
            }
            else
            {
                ChapterPort.gameObject.SetActive(false);
            }
        }
    }
    protected void InitMapItems()
    {
        foreach (var item in MapItems)
        {
            if(item.CanPickup)
            {
                if (DataService.Instance.db.MapItems.Length - 1 < item.ID)
                {
                    Debug.LogError("配置的MapItemId超出范围, id = " + item.ID);
                }
                else if (DataService.Instance.db.MapItems[item.ID] == 1)
                {
                    item.gameObject.SetActive(true);
                }
            }
        }
    }
    public void PickUpMapItem(MapItem item)
    {
        if (SceneController.Transitioning())
        {
            return;
        }
        if (item.CanPickup) {
            item.gameObject.SetActive(false);
            if (item.ID == 0)
            {
                NoticeBar.Instance.Notice(Language.Lang.TIPS_PICKUP_GOLD_BAG, NoticeBar.LEVEL_HIGH);
                GoodsManager.Instance.AddGold(1000000);
            }

            DataService.Instance.db.MapItems[item.ID] = 0;
            DataService.Instance.save();
        }
        else
        {
            if (item.ID == 1)
            {
                if (GameManager.game.BeAttacking) {
                    NoticeBar.Instance.Notice(Language.Lang.TIPS_DONT_GO);
                    return;
                }
                TreeHoleCtrl.Instance.EnterHole();
            }
            else if (item.ID == 1000)//所有非城墙地图都使用同一个返回ID。
            {
                TreeHoleCtrl.Instance.GotoGuardScene();
            }
            else if (item.ID == 1001)
            {
                if (GameManager.game.BeAttacking)
                {
                    return;
                }
                WorldCtrl.Instance.OpenWorldWin();
            }
        }
    }
    public Vector2 GetMatrixGroupPosition()
    {
        return MonsterBornPositions[2].transform.position;
    }
    protected void CreateMonsters()
    {

        foreach (var monsterId in Tpl.Monsters)
        {
            RoleTplData roleTpl = TemplateManager.GetRoleTplData(monsterId);

            GameObject go = (GameObject)Instantiate(Resources.Load("Prefabs/Roles/" + roleTpl.Prefabname));
            Monster monster = go.GetComponent<Monster>();
            monster.RoleId = monsterId;
            Vector2 pt = Vector2.zero;
            pt.x = Random.Range(MonsterBornPositions[0].position.x, MonsterBornPositions[1].position.x);
            pt.y = Random.Range(MonsterBornPositions[1].position.y, MonsterBornPositions[0].position.y);
            monster.SetPosition(pt);
            AddMonster(monster);
        }
    }
    // Update is called once per frame
    void Update()
    {
        _spawnActionMgr.Update();
        foreach (var matrix in matrices)
        {
            matrix.Update();
        }
    }
    public void DimissMatrix(Matrix matrix)
    {
        matrices.Remove(matrix);
    }
    public void addAction(IAction action, bool immediately = false)
    {
        _spawnActionMgr.AddAction(action, immediately);
    }
    public List<Vector2> FindPath(Vector2 from, Vector2 to)
    {
        return pathFinding.findPath(from, to);
    }
    public bool CanMoveTo(Vector2 pt)
    {
        return pathFinding.CanMoveTo(pt);
    }
    public Vector2 GetNearstPoint(Vector2 start, Vector2 end)
    {
        bool found;
        Vector2 dest = pathFinding.getCloseEdgePoint(start, end, out found);
        if (found)
        {
            return dest;
        }
        return start;
    }
    private void buildScene()
    {
    }

    public GameObject addEffect(string res, Vector3 position, Role causer=null, bool scaleByOrientatio = false, float offsetX = 0.0f, bool speedAjustable=false)
    {
        GameObject effect = (GameObject)Instantiate(Resources.Load(res));
        if (scaleByOrientatio && causer != null)
        {
            var direction = causer.getOrientation() == Role.Orientation.RIGHT ? 1 : -1;
            position.x = position.x + offsetX * direction;
            effect.transform.localScale = new Vector3(direction* effect.transform.localScale.x, effect.transform.localScale.y, effect.transform.localScale.z);
            effect.transform.rotation = Quaternion.Euler(0f, 0f, effect.transform.rotation.eulerAngles.z * direction);
        }

        effect.transform.localPosition = position;
        if (speedAjustable && causer != null)
        {
            Animator animator = effect.GetComponent<Animator>();
            if(animator != null)
            {
                animator.speed = causer.ActionSpeed;
            }
        }
        return effect;
    }
    public List<Role> GetMonsters()
    {
        return monsters;
    }
    public void AddMonster(Monster monster, int pos = -1)
    {
        monsters.Add(monster);
        monster.Map = this;
        if (monster.aiTpl.Matrixid != 0)
        {
            AddMemberToMatrix(monster, pos);
        }
    }
    public void RemoveRole(Role role)
    {
        foreach (var item in monsters)
        {
            if (item == role)
            {
                monsters.Remove(item);
                if(item.Matrix != null)
                {
                    RemoveMemberFromMatrix(item);
                }
                return;
            }
        }
        myTeamMembers.Remove(role);
        if (hero == role)
        {
            hero = null;
        }
        else if (wall == role)
        {
            wall = null;
        }
    }
    protected void AddMemberToMatrix(Role member, int pos)
    {
        foreach (var matrix in matrices)
        {
            if(matrix.MatrixId == member.aiTpl.Matrixid)
            {
                matrix.AddMember(member, pos);
                return;
            }
        }
        Matrix matrixN = new Matrix(member.aiTpl.Matrixid, this);
        matrices.Add(matrixN);
        matrixN.AddMember(member, pos);
    }
    protected void RemoveMemberFromMatrix(Role member)
    {
        foreach (var matrix in matrices)
        {
            if (matrix.MatrixId == member.aiTpl.Matrixid)
            {
                matrix.RemoveMember(member);
                if (matrix.Dimiss())
                {
                    matrices.Remove(matrix);
                }
                break;
            }
        }
    }
    //返回的成员可能已死亡，但仍在返回列表中
    public List<Role> GetRolesBySide(int side)
    {
        if(side == Role.SIDE_LEFT)
        {
            return myTeamMembers;
        }
        else
        {
            return monsters;
        }
    }
    public List<Role> FindZonePartners(Role from, int distance, int count)
    {
        return FindZoneRoles(from.transform.position, from.Side, distance, count);
        //List<Role> partners = GetRolesBySide(from.Side);
        //List<Role> inZonePartners = new List<Role>();

        //foreach (var item in partners)
        //{
        //    if(item.Alive && Vector2.Distance(item.transform.position, from.transform.position) < distance)
        //    {
        //        inZonePartners.Add(item);
        //    }
        //}
        //if (inZonePartners.Count <= count)
        //{
        //    return inZonePartners;
        //}
        //List<Role> resultPartners = new List<Role>();
        //while(resultPartners.Count < count)
        //{
        //    Role item = inZonePartners[Random.Range(0, inZonePartners.Count - 1)];
        //    resultPartners.Add(item);
        //    inZonePartners.Remove(item);
        //}

        //return resultPartners;
    }
    public List<Role> FindZoneRoles(Vector2 centerPt,int side, int distance, int count, int roleTplId = -1)
    {
        List<Role> partners = GetRolesBySide(side);
        List<Role> inZonePartners = new List<Role>();

        foreach (var item in partners)
        {
            if (item.Alive &&(roleTplId == -1 || item.GetRoleInfo().Tpl.ID == roleTplId) && Vector2.Distance(item.transform.position, centerPt) < distance)
            {
                inZonePartners.Add(item);
            }
        }
        if (inZonePartners.Count <= count)
        {
            return inZonePartners;
        }
        List<Role> resultPartners = new List<Role>();
        while (resultPartners.Count < count)
        {
            Role item = inZonePartners[Random.Range(0, inZonePartners.Count - 1)];
            resultPartners.Add(item);
            inZonePartners.Remove(item);
        }

        return resultPartners;
    }
    void OnDisable()
    {
    }
    public void SetMonsterSelectable(bool value)
    {
        foreach (var item in monsters)
        {
            item.SetSelectable(value);
        }
    }
    public Vector2 GetRandomBornPosition(int zoneIndex = -1)
    {
        if (zoneIndex > BornZones.Count - 1) zoneIndex = -1;
        if (zoneIndex < 0)
        {
            zoneIndex = Random.Range(0, BornZones.Count);
        }
        Rect rect = BornZones[zoneIndex];
        return new Vector2(Random.Range(rect.x, rect.x + rect.width), Random.Range(rect.y - rect.height,rect.y));
    }
    public void StopCreateEnvSounds()
    {
        if(_envSoundMgr != null)
        {
            _envSoundMgr.Stop();
        }
    }
    private void OnDestroy()
    {
        heroInfo.removeListener(HeroInfoEvent.LEVELUP, OnFuncOpen);
    }
}
