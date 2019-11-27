using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUI : MonoBehaviour {

    public Button startBtn;


    // AssetBundle处理相关
    //private BundleManager bundleMgr = null;

    private void Awake()
    {
        //if (!bundleMgr)
        //{
        //    bundleMgr = BundleManager.Instance;
        //}
        //else
        //{
           ShowStartBtn();
        //}
    }
    // 加载完成后显示开始按钮
    public void ShowStartBtn()
    {
        startBtn.gameObject.SetActive(true);
    }

    public void StartBtnClick()
    {
        SceneManager.LoadScene(1);
    }
    public void ExitBtnClick()
    {
        Application.Quit();
    }
}
