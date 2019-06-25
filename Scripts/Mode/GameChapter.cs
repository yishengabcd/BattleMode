// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-04-10 10:35:06
// 版 本：1.0
// ========================================================
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameChapter : Game
{
    public override int BattleType => BATTLE_TYPE_CHAPTER;
    private bool _noticed;
    protected override void Awake()
    {
        //DataService.Instance.Safe = false;
        GameManager.SetGame(this);
    }
    protected override void Update()
    {
        if (!_noticed)
        {
            _beAttacking = true;
            _noticed = true;
            GlobalEventLocator.Instance.dispatch(new GameEvent(GameEvent.BATTLE_STATE_CHANGED));
        }
        if (_battleEnd) return;
        if (SceneController.Phase != SceneController.PHASE_LOADING_NEXT)
        {
            _battleEnd = CheckBattleOver();
        }
    }
}