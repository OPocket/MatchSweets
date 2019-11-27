using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSetting
{
    // 版本号
    public static int version = 1;

    public static string ConvertToAssetBundleName(string name)
    {
        if (getplatformfolder() == string.Empty)
            return string.Empty;
        return getplatformfolder()+name;
    }

    public static string getplatformfolder()
    {
        //"jar:file://"+
        /* 路径 */
        string filePath =
#if UNITY_EDITOR_WIN
        Application.streamingAssetsPath + "/";
#elif UNITY_ANDROID && !UNITY_EDITOR
        "jar:file://"+Application.dataPath + "!/assets/";
#elif UNITY_STANDALONE_WIN
        Application.streamingAssetsPath+ "/";
#else
        string.Empty;
#endif
        return filePath;
    }

}
