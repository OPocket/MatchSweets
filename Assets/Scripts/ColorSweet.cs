using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * 部分甜品拥有该功能，所以独立脚本,代表有颜色属性
 */
public class ColorSweet : MonoBehaviour {

    public enum ColorType
    {
        BLUE = 0, 
        GREEN,
        PINK,
        PURPLE,
        RED,
        YELLOW,
        COUNT
    }
    
    [System.Serializable]
    public struct ColorSprite
    {
        public ColorType color;
        public Sprite sprite;
    }
    public ColorSprite[] colorSpriteArray;

    private Dictionary<ColorType, Sprite> colorSpriteDic;

    private SpriteRenderer spriteRenderer;

    // 当前甜品颜色
    private ColorType curColor;
    public ColorType CurColor
    {
        get
        {
            return curColor;
        }

        set
        {
            curColor = value;
        }
    }

    private void Awake()
    {
        spriteRenderer = transform.Find("Sweet").GetComponent<SpriteRenderer>();
        colorSpriteDic = new Dictionary<ColorType, Sprite>();
        for (int i=0; i< colorSpriteArray.Length; i++)
        {
            if (!colorSpriteDic.ContainsKey(colorSpriteArray[i].color))
            {
                colorSpriteDic.Add(colorSpriteArray[i].color, colorSpriteArray[i].sprite);
            }
        }
    }
    // 设置甜品类型
    public void SetColor(ColorType newColor)
    {
        curColor = newColor;
        // 安全校验
        if (colorSpriteDic.ContainsKey(newColor))
        {
            spriteRenderer.sprite = colorSpriteDic[newColor];
        }
    }
    // 获取类型长度
    public int GetLength()
    {
        return colorSpriteArray.Length;
    }
}
