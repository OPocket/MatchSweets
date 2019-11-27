using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSweet : MonoBehaviour {

    private GameManager.SweetType type;
    public GameManager.SweetType Type
    {
        get
        {
            return type;
        }

        set
        {
            type = value;
        }
    }
    // 位置
    private int x;
    private int y;
    public int X
    {
        get
        {
            return x;
        }

        set
        {
            if (IsCanMove())
            {
                x = value;
            }          
        }
    }
    public int Y
    {
        get
        {
            return y;
        }

        set
        {
            if (IsCanMove())
            {
                y = value;
            }
        }
    }

    private MoveSweet moveSweet;
    public MoveSweet MoveSweet
    {
        get
        {
            return moveSweet;
        }
    }

    private ColorSweet colorSweet;
    public ColorSweet ColorSweet
    {
        get
        {
            return colorSweet;
        }
    }

    public ClearSweet ClearSweet
    {
        get
        {
            return clearSweet;
        }
    }
    private ClearSweet clearSweet;

    private GameManager gameManager;
    public GameManager GameManager
    {
        get
        {
            return gameManager;
        }
    }



    private void Awake()
    {
        moveSweet = GetComponent<MoveSweet>();
        colorSweet = GetComponent<ColorSweet>();
        clearSweet = GetComponent<ClearSweet>();
        if (clearSweet)
        {
            clearSweet.Sweet = this;
        }
    }

    public void Init(int x, int y, GameManager gameManager, GameManager.SweetType type)
    {
        this.x = x;
        this.y = y;
        this.gameManager = gameManager;
        this.type = type;
    }
    // 设置位置
    public Vector2 SetPos(int x, int y)
    {
        if (!gameManager)
        {
            gameManager = GameManager.Instance;
        }
        return gameManager.ResetPos(x, y);
    }
    // 判断是否为不可移动的甜品
    public bool IsCanMove()
    {
        return moveSweet !=null;
    }
    // 判断是否为有颜色的甜品
    public bool IsHaveColor()
    {
        return colorSweet != null;
    }
    // 判断是否可以清除
    public bool IsCanClear()
    {
        return clearSweet !=null;
    }
    private void OnMouseDown()
    {
        gameManager.FirstSweet(this);
    }

    private void OnMouseEnter()
    {
        gameManager.SecondSweet(this);
    }
  
    private void OnMouseUp()
    {
        gameManager.ReleaseSweet();
    }
}
