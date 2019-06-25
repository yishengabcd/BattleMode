// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-04-08 18:33:29
// 版 本：1.0
// ========================================================
using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;


public class MapItem : MonoBehaviour, IPointerDownHandler
{
    public int ID;
    public bool CanPickup = true;//为false时表示是常驻的交互物体

    private void Start()
    {
        if (GameManager.map.IsWallMap)
        {
            GlobalEventLocator.Instance.addListener(GameEvent.BATTLE_STATE_CHANGED, OnBattleStateChanged);
        }
        OnBattleStateChanged(null);
    }
    private void OnBattleStateChanged(EventBase e)
    {
        if(GameManager.game.BeAttacking)
        {
            BoxCollider2D coll = GetComponent<BoxCollider2D>();
            if (coll != null)
            {
                coll.enabled = false;
            }
        }
        else
        {
            BoxCollider2D coll = GetComponent<BoxCollider2D>();
            if (coll != null)
            {
                coll.enabled = true;
            }
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        var worldPt = transform.position;//Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (!GameManager.map.hero.Alive) return;
        if (Vector2.Distance(GameManager.map.hero.transform.position, worldPt) < 0.2) {
            GameManager.map.PickUpMapItem(this);
            return;
        }

        List<Vector2> path = null;
        path = GameManager.map.FindPath(GameManager.hero.transform.position, worldPt);

        if (path.Count > 1)//至少有两个点，其中第一个点是当前位置
        {
            GameManager.hero.ExecuteCommand(new PickupItemCommand(GameManager.hero, path, this));
        }
    }

    public void SetEnable(bool value)
    {

    }
    private void OnDestroy()
    {
        GlobalEventLocator.Instance.removeListener(GameEvent.BATTLE_STATE_CHANGED, OnBattleStateChanged);
    }

}
