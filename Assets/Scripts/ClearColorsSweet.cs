using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearColorsSweet : ClearSweet {
    // 要删除的颜色类型
    private ColorSweet.ColorType clearColor;
    public ColorSweet.ColorType ClearColor
    {
        get
        {
            return clearColor;
        }

        set
        {
            clearColor = value;
        }
    }

    public override void Clear()
    {
        base.Clear();
        Sweet.GameManager.ClearSameColorSweets(clearColor);
    }
}
