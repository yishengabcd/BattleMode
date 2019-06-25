// ========================================================
// 描述：
// 作者：Yisheng 
// 创建时间：2019-03-21 15:04:08
// 版 本：1.0
// ========================================================
using System;
using UnityEngine;
public class FlipEffect
{
    private Vector2 _offset;
    private GameObject _effect;
    private bool _flipX;
    private Vector3 _defScale;
    private Quaternion _defRotation;
    public FlipEffect(string res, Vector2 offset, Transform parent, bool flip)
    {
        _offset = offset;
        _effect = (GameObject)UnityEngine.Object.Instantiate(Resources.Load(res), parent);
        _defScale = _effect.transform.localScale;
        _defRotation = _effect.transform.rotation;
        _flipX = flip;
        UpdatePosition();
    }
    private void UpdatePosition()
    {
        int direction = _flipX ? -1 : 1;
        Vector2 pt = _offset;
        pt.x = pt.x * direction;

        _effect.transform.localScale = new Vector3(direction * _defScale.x, _defScale.y, _defScale.z);
        _effect.transform.rotation = Quaternion.Euler(0f, 0f, _defRotation.eulerAngles.z * direction);

        _effect.transform.localPosition = pt;
    }
    public bool Flip {
        set
        {
            if(_flipX != value)
            {
                _flipX = value;
                UpdatePosition();
            }
        }
    }
    public void SetVisible(bool value)
    {
        if(_effect != null)
        {
            _effect.SetActive(value);
        }
    }
    public void Destroy()
    {
        UnityEngine.Object.Destroy(_effect);
    }
}
