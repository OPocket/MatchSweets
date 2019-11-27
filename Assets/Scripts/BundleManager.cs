using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class BundleManager : MonoBehaviour {

    public Text TextLog;

    public GameObject gameUI;
    private int loadCount = 0;
    private Dictionary<int, CsvBundleBaseData> bundleDic;
    private Dictionary<GameManager.SweetType, GameObject> sweetPrefabsDic;
    public Dictionary<GameManager.SweetType, GameObject> SweetPrefabsDic
    {
        get
        {
            return sweetPrefabsDic;
        }
    }
    private static BundleManager _instance;
    public static BundleManager Instance
    {
        get
        {
            if (_instance == null)
            {

                _instance = FindObjectOfType<BundleManager>();
                // 如果获取不到
                if (_instance == null)
                {
                    GameObject bundleObj = new GameObject();
                    bundleObj.hideFlags = HideFlags.HideAndDontSave;

                    _instance = bundleObj.AddComponent<BundleManager>();
                }
            }
            return _instance;
        }
    }
    private Dictionary<string, WWW> dicLoadingReq;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        if (_instance == null)
        {
            _instance = this;
        }
        // 保存Object
        sweetPrefabsDic = new Dictionary<GameManager.SweetType, GameObject>();
        // 保存www
        dicLoadingReq = new Dictionary<string, WWW>();

        StartCoroutine(GetCSV());


    }

    // 加载表格
    public IEnumerator GetCSV()
    {
        // 用WWW读取配置表
        WWW www = new WWW(GlobalSetting.ConvertToAssetBundleName("BundleData.csv"));
        while (!www.isDone)
        {
            yield return www;  
        }
        CsvBundleData bundleCsv = new CsvBundleData(www.text);
        bundleCsv.ReadCsvFile();
        bundleDic = bundleCsv.CsvDataDic;

        //for (int i = 0; i < bundleDic.Count; i++)
        //{
        //    StartCoroutine(LoadAssetBundles(bundleDic[i].Type, typeof(GameObject)));
        //}
        //TextLog.text = "开始加载AssetBundle：" + "    " + Application.streamingAssetsPath + "                  " + GlobalSetting.getplatformfolder();

        StartCoroutine(LoadAssetBundles(bundleDic));
    }


    private IEnumerator LoadAssetBundles(Dictionary<int , CsvBundleBaseData> temDic)
    {
        foreach (int key in temDic.Keys)
        {
            string assetBundleName = temDic[key].BundleName;
            string url = GlobalSetting.ConvertToAssetBundleName("streanmingassets/" + assetBundleName + ".bundle");
            
            Debug.Log("WWW AsyncLoad name =" + url + " versionNum = " + GlobalSetting.version);
            // 下载Bundle资源
            /*WWW www = WWW.LoadFromCacheOrDownload(url, GlobalSetting.version);

            dicLoadingReq.Add(bundleDic[key].BundleName, www);


            

            while (!www.isDone)
                yield return www;

            if (string.IsNullOrEmpty(www.error) == false)
            {
                Debug.Log(www.error);
                yield break;
            }

            TextLog.text = "正在加载AssetBundle：" + " 未完成  ";
            // 加载资源
            AssetBundleRequest req = www.assetBundle.LoadAssetAsync(bundleDic[key].PrefabName, typeof(GameObject));
            while (!req.isDone)
                yield return req;
               
            TextLog.text = "正在加载AssetBundle：" + " req  ";


            AssetBundle temAb = www.assetBundle;



            //GameObject temObj = (req.asset as GameObject);
            //DontDestroyOnLoad(temObj);
            GameObject temObj = temAb.LoadAsset(bundleDic[key].BundleName) as GameObject;
            sweetPrefabsDic.Add((GameManager.SweetType)key, temObj);
            dicLoadingReq.Remove(bundleDic[key].BundleName);
            www.assetBundle.Unload(false);
            www.Dispose();
            www = null;
            Debug.Log("加载AssetBundle完成：" + bundleDic[key].BundleName);

            loadCount++;*/

            //AssetBundle bundle;
            //bundle = AssetBundle.LoadFromFile(Application.dataPath + "!assets/texture");

            //UnityEngine.Object obj = bundle.LoadAsset("Texture") as UnityEngine.Object;
            //GameObject go = Instantiate(obj, transform.position, Quaternion.identity) as GameObject;
            //bundle.Unload(false);

            //在Android上同步方法AssetBundle.LoadFromFile 就得用 Application.dataPath+”!assets”这个路径
            AssetBundle temAb = AssetBundle.LoadFromFile(url);
            GameObject temPrefab = temAb.LoadAsset(bundleDic[key].PrefabName) as GameObject;
            GameObject temObj = Instantiate(temPrefab, transform.position, transform.rotation);
            temObj.SetActive(false);
            DontDestroyOnLoad(temObj);
            sweetPrefabsDic.Add((GameManager.SweetType)key, temObj);
            loadCount++;

        }
        TextLog.text = "加载AssetBundle完成";
        yield return null;

        if (loadCount == bundleDic.Count)
        {
            gameUI.SendMessage("ShowStartBtn");
        }
    }
}
/* 
    private IEnumerator LoadBundle()
    {
        //第四种加载AB方式 从服务器端下载 UnityWebRequest（新版Unity使用）
        //服务器路径 localhost为IP
        string uri = @"http://localhost/AssetBundles/cubewall.unity3d";
        UnityWebRequest request3 = UnityWebRequestAssetBundle.GetAssetBundle(uri);
        yield return request3.Send();

        //AssetBundle ab8 = ((DownloadHandlerAssetBundle)request3.downloadHandler).assetBundle;
        AssetBundle ab8 = DownloadHandlerAssetBundle.GetContent(request3);

        //使用里面的资源
        GameObject wallPrefab5 = ab8.LoadAsset("CubeWall") as GameObject;
        Instantiate(wallPrefab5);

        //加载cubewall.unity3d资源包所依赖的资源包
        AssetBundle manifestAB = AssetBundle.LoadFromFile("AssetBundles/AssetBundles");
        AssetBundleManifest manifest = manifestAB.LoadAsset("AssetBundleManifest") as AssetBundleManifest;

        //cubewall.unity3d资源包所依赖的资源包的名字
        string[] strs = manifest.GetAllDependencies("cubewall.unity3d");
        foreach (string name in strs)
        {
            AssetBundle.LoadFromFile("AssetBundles/" + name);
        }
    }
    */