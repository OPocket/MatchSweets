using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CsvBundleData
{
    private string csvData="";

    private Dictionary<int, CsvBundleBaseData> csvDataDic = null;

    public Dictionary<int, CsvBundleBaseData> CsvDataDic
    {
        get
        {
            if (csvDataDic == null)
            {
                ReadCsvFile();
            }
            return csvDataDic;
        }
    }

    public CsvBundleData(string csvStr)
    {
        csvData = csvStr;
    }
    /*
    private static CsvBundleData _instance = null;

    public static CsvBundleData Instance() 
    {
        if(_instance==null)
        {
            _instance = new CsvBundleData();
            _instance.ReadCsvFile(csvStr);
        }
        return _instance;
    }
*/


    // 读取CSV文件数据
    public void ReadCsvFile()
    {
        /* 读取CSV文件，一行行读取 */
        string[] temFileData = csvData.Split('\r');
        /* 把每一行的/n去掉 */
        string[] fileData = new string[temFileData.Length];
        for (int j=0; j< temFileData.Length;j++)
        {
            fileData[j] = temFileData[j].Replace("\n", "");
        }

        /* 把CSV文件按行存放，每一行的ID作为key值，内容作为value值 */
        csvDataDic = new Dictionary<int, CsvBundleBaseData>();

        /* CSV文件的第一行为Key字段，先读取key字段 */
        string[] keys = fileData[0].Split(',');

        /* 第二行开始是数据 */
        for (int i = 1; i < fileData.Length; i++)
        {
            /* 每一行的内容都是逗号分隔，读取每一列的值 */
            string[] lineData = fileData[i].Split(new char[] { ',' }, keys.Length);

            /* CSVDemo类与CSVDemo.csv文件的key字段一一对应，用于保存每一行的数据内容 */
            CsvBundleBaseData csvDemo = new CsvBundleBaseData();
            for (int j = 0; j < lineData.Length; j++)
            {
                if (keys[j] == "Type")
                {
                    //csvDemo.Type = int.Parse(lineData[j]);
                    int iType;
                    int.TryParse(lineData[j], out iType);
                    csvDemo.Type = iType;
                }
                else if (keys[j] == "Bundle_Name")
                {
                    csvDemo.BundleName = lineData[j];
                }
                else if (keys[j] == "Prefab_Name")
                {
                    csvDemo.PrefabName = lineData[j];
                }
            }
            /* 保存每一行ID和数据对象的关系 */
            csvDataDic[csvDemo.Type] = csvDemo;
        }
    }
}
