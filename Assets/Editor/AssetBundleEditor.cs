using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetBundleEditor : MonoBehaviour {
    [MenuItem("AssetBundle/package(Default)")]
    private static void PackageBuddle()
    {
        Debug.Log("Packaging AssetBundle...");
        string packagePath = UnityEditor.EditorUtility.OpenFolderPanel("Select Package Path", "E:/C#Project/MatchSweets/Assets/StreamingAssets", "");
        if (packagePath.Length <= 0 || !Directory.Exists(packagePath))
            return;
        Debug.Log("Output Path: " + packagePath);
        BuildPipeline.BuildAssetBundles(packagePath, BuildAssetBundleOptions.None, BuildTarget.Android);
        AssetDatabase.Refresh();
        
    }

    [MenuItem("AssetBundle/Buil (All)")]
    private static void _packageBuddlesInOne()
    {
        string _packagePath = UnityEditor.EditorUtility.OpenFolderPanel("Select Package Path", "E:/C#Project/MatchSweets/Assets/StreamingAssets", "");
        if (_packagePath.Length <= 0 || !Directory.Exists(_packagePath))
            return;
        
        //将选中对象一起打包
        AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
        buildMap[0].assetBundleName = "myBnndle";
        GameObject[] objs = Selection.gameObjects; //获取当前选中的所有对象
        string[] itemAssets = new string[objs.Length];
        for (int i = 0; i < objs.Length; i++)
        {
            itemAssets[i] = AssetDatabase.GetAssetPath(objs[i]); //获取对象在工程目录下的相对路径
            
        }
        buildMap[0].assetNames = itemAssets;
        
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(_packagePath, buildMap, BuildAssetBundleOptions.None, BuildTarget.Android);
        AssetDatabase.Refresh(); //刷新
    
        if (manifest == null)
            Debug.LogError("Package AssetBundles Faild.");
        else
            Debug.Log("Package AssetBundles Success.");
    }


    [MenuItem("AssetBundle/Build (Selected)")]
    private static void _packageBuddleSelected()
    {
        string _packagePath = UnityEditor.EditorUtility.OpenFolderPanel("Select Package Path", "E:/C#Project/MatchSweets/Assets/StreamingAssets", "");
        if (_packagePath.Length <= 0 || !Directory.Exists(_packagePath))
            return;
        
        GameObject[] objs = Selection.gameObjects;
        AssetBundleBuild[] buildMap = new AssetBundleBuild[objs.Length];
        for (int i = 0; i < objs.Length; i++)
        {
            string[] itemAsset = new string[] { AssetDatabase.GetAssetPath(objs[i]) };
            buildMap[i].assetBundleName = objs[i].name;
            buildMap[i].assetNames = itemAsset;
        }
        AssetBundleManifest manifest = BuildPipeline.BuildAssetBundles(_packagePath, buildMap, BuildAssetBundleOptions.None, BuildTarget.Android);
        AssetDatabase.Refresh();
        if (manifest == null)
            Debug.LogError("Error:Package Failed");
        else
            Debug.Log("Package Success.");

        
    }
}



