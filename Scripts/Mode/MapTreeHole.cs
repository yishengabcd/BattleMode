// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-04-08 22:57:32
// 版 本：1.0
// ========================================================
using UnityEngine;
using System.Collections;

public class MapTreeHole : Map
{
    // Use this for initialization
    protected override void Awake()
    {
        if(MonsterBornPositions.Length != 2) {
            Debug.LogError("需配置左上、右下两个出生点");
        }
        tpl = TreeHoleCtrl.Instance.CurrentMapTpl;

        base.Awake();
        CreateMonsters();
    }
}
