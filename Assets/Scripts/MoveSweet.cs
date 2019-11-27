using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * 部分甜品拥有该功能，所以独立脚本,代表可移动属性
 */
public class MoveSweet : MonoBehaviour {

    private GameSweet sweet;

    private IEnumerator moveIenumerator;

    private void Awake()
    {
        sweet = GetComponent<GameSweet>();
    }

    public void ToMove(int x, int y, float time)
    {
        // 避免协程未结束又开始新的协程
        if (moveIenumerator != null)
        {
            StopCoroutine(moveIenumerator);
        }
        moveIenumerator = MoveCoroutine(x, y, time);
        StartCoroutine(moveIenumerator);
    }
    // 开始移动
    private IEnumerator MoveCoroutine(int x, int y, float time)
    {
        sweet.X = x;
        sweet.Y = y;
        Vector2 startPos = sweet.transform.position;
        Vector2 endPos = sweet.SetPos(x, y);
        for (float t = 0; t < time; t += Time.deltaTime)
        {
            sweet.transform.position = Vector2.Lerp(startPos, endPos, t/time);
            yield return null;
        }
        
        // 纠正位置
        sweet.transform.position = endPos;
        StopCoroutine(moveIenumerator);
        moveIenumerator = null;
    }
}
