using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CsvLevelBaseData {

    public int Level { get; set; }
    public int TimeDe { get; set; }
    public int XColumn { get; set; }
    public int YRow { get; set; }
    public int AimLimit { get; set; }

    public Dictionary<int, int> Biscuit_Pos
    {
        get
        {
            return biscuit_Pos;
        }
    }
    private Dictionary<int, int> biscuit_Pos;

    // 转换饼干位置存储于字典
    public void SetBiscuitPos(string dic)
    {
        biscuit_Pos = new Dictionary<int, int>();
        // 注意：分割/时候应写为'\"'
        string[] str = dic.Split(new char[]{ ',','{','}','\"'},StringSplitOptions.RemoveEmptyEntries);
        for (int j = 0; j < str.Length; j+=2)
        {
            Biscuit_Pos[int.Parse(str[j])] = Convert.ToInt32(str[j+1]);
        }
    }
}
