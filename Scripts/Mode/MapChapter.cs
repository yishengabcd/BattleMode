// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-04-10 10:35:06
// 版 本：1.0
// ========================================================
using UnityEngine;
public class MapChapter : Map
{
    // Use this for initialization
    protected override void Awake()
    {
        if (MonsterBornPositions.Length != 2)
        {
            Debug.LogError("需配置左上、右下两个出生点");
        }
        tpl = WorldCtrl.Instance.CurrentMapTpl;

        base.Awake();
        CreateMonsters();
    }
}