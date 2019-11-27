using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Advertisements;
using System.IO;
using System;

public class GameManager : MonoBehaviour
{
    string gameAdsID =
#if UNITY_ANDROID && !UNITY_EDITOR
    "3191508";
#elif UNITY_IOS && !UNITY_EDITOR
    "3191509";
#else
    string.Empty;
#endif

    public enum SweetType
    {
        EMPTY = 0,
        NORMAL,
        BARRIER,                // 饼干障碍
        ROW_CLEAR,
        COLUM_CLEAR,
        RAINBOW_CANDY,
        COUNT
    }
    // 存储甜品、饼干、列消除甜品、行消除甜品、彩虹糖预设体
    private Dictionary<SweetType, GameObject> sweetPrefabsDic; 

    // 结构体
    [System.Serializable]
    public struct SweetPrefab
    {
        public SweetType sweetType;
        public GameObject prefab;
    }

    public SweetPrefab[] sweetPrefabs;
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            return _instance;
        }

        set
        {
            _instance = value;
        }
    }

    // 行列
    private int xColum;
    private int yRow;

    // Sweet数组
    public GameSweet[,] sweetArray;
    // 网格单元
    public GameObject gridPrefab;
    // 填充的时间
    public float fillTime = 0.1f;
    // 选中的两个甜品
    private GameSweet firstSweet;
    private GameSweet secondSweet;
    // 是否在判断消除阶段（便于此时禁止交换操作）
    private bool isRemoving = true;
    // 填充阶段禁止交换操作
    private bool isFillComplete = false;

    //UI显示
    // 时间
    public Text textLevel;
    public Text textAim;
    public Text textTime;
    // 分数
    public Text textScore;
    private int score;
    public int Score
    {
        get
        {
            return score;
        }

        set
        {
            score = value;
        }
    }
    // 游戏时间
    private float gameTime = 10.0f;
    // 登入游戏时是否加载完成
    private bool isFinishLoad = false;
    // 游戏是否结束
    private bool isGameOver = false;
    // 控制分数累加 
    private float addScoreTime = 0.0f;
    private int curScore = 0;

    // 结算弹窗
    public GameObject gameOverPanel;
    // 从头再玩
    public Button btnPass;
    // 关闭按钮
    public Button btnClose;
    // 重玩按钮
    public Button btnReplay;
    // 下一关按钮
    public Button btnNext;
    // 最终得分
    public Text textFinalScore;
    // 是否通关
    public Text passText;
    // 缓存CSV数据
    private Dictionary<int, CsvLevelBaseData> csvDataDic;
    // 当前关卡
    private int levelCount = 1;

    //private BundleManager bundleMgr;
    private void Awake()
    {
        _instance = this;

        //if (!bundleMgr)
        //{
        //    bundleMgr = BundleManager.Instance;
        //}
        textScore.text = 0.ToString();
        //设置屏幕正方向在Home键右边
        Screen.orientation = ScreenOrientation.LandscapeRight;
        // 获取缓存中的玩家关卡成就
        if (PlayerPrefs.HasKey("Level"))
        {
            levelCount = PlayerPrefs.GetInt("Level");
        }
        /// 读取配置数据
        StartCoroutine(LoadLevelCsv());
        
    }

    private IEnumerator StartSet()
    {
        //设置屏幕自动旋转， 并置支持的方向
        Screen.orientation = ScreenOrientation.AutoRotation;
        Screen.autorotateToLandscapeLeft = true;
        Screen.autorotateToLandscapeRight = true;
        Screen.autorotateToPortrait = false;
        Screen.autorotateToPortraitUpsideDown = false;

        // 配置行数列数
        xColum = csvDataDic[levelCount].XColumn;
        yRow = csvDataDic[levelCount].YRow;
        // 设置时间
        gameTime = csvDataDic[levelCount].TimeDe;
        textTime.text = gameTime.ToString();
        // 关卡显示
        textLevel.text = "第" + csvDataDic[levelCount].Level.ToString() + "关";
        // 得分目标显示
        textAim.text = csvDataDic[levelCount].AimLimit.ToString();
        // 加载广告
#if UNITY_ANDROID || UNITY_IOS
        if (gameAdsID != string.Empty)
        {
            Advertisement.Initialize(gameAdsID);
        }
#endif
        // 实例化sweetPrefabsDic
        /*
         测试Bundle加载
         
        sweetPrefabsDic = bundleMgr.SweetPrefabsDic;
        */
        sweetPrefabsDic = new Dictionary<SweetType, GameObject>();
        for (int k = 0; k < sweetPrefabs.Length; k++)
        {
            if (!sweetPrefabsDic.ContainsKey(sweetPrefabs[k].sweetType))
            {
                sweetPrefabsDic.Add(sweetPrefabs[k].sweetType, sweetPrefabs[k].prefab);
            }
        }

        // 生成大网格
        for (int i = 0; i < xColum; i++)
        {
            for (int j = 0; j < yRow; j++)
            {
                // 生成网格
                GameObject gridObj = Instantiate(gridPrefab, ResetPos(i, j), Quaternion.identity);
                gridObj.transform.SetParent(transform);
            }
        }
        // 生成Sweet
        sweetArray = new GameSweet[xColum, yRow];
        for (int i = 0; i < xColum; i++)
        {
            for (int j = 0; j < yRow; j++)
            {
                CreateSweet(i, j, SweetType.EMPTY);
            }
        }
        // 根据配置表删除空位置来创建饼干
        DelPosToCreateBiscuit(csvDataDic[levelCount].Biscuit_Pos);

        StartCoroutine(TotalFill());

        isRemoving = false;

        yield return null;

        isFinishLoad = true;
    }
    // 读取CSV文件数据

    // 针对安卓平台加载
    private IEnumerator LoadLevelCsv()
    {
        /* CSV文件路径 */
        string filePath = GlobalSetting.ConvertToAssetBundleName("LevelData.csv");

//#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
/* 读取CSV文件，一行行读取 */
        //string[] fileDataRs = File.ReadAllLines(filePath);
//#elif UNITY_ANDROID
        // 安卓平台只能用WWW加载
        WWW www = new WWW(filePath);
        yield return www;
        // 读取CSV文件，一行行读取
        
        string[] temFileData = www.text.Split('\r');

        // 把每一行的/n去掉
        string[] fileDataR = new string[temFileData.Length];
        for (int j=0; j< temFileData.Length;j++)
        {
            fileDataR[j] = temFileData[j].Replace("\n", "");
        }
//#endif
        ReadCsvFile(fileDataR);

        yield return null;
        StartCoroutine(StartSet());
    }


    void ReadCsvFile(string[] temData)
    {
        /* 把CSV文件按行存放，每一行的ID作为key值，内容作为value值 */
        csvDataDic = new Dictionary<int, CsvLevelBaseData>();

        /* CSV文件的第一行为Key字段，先读取key字段 */
        string[] keys = temData[0].Split(',');

        /* 第二行开始是数据 */
        for (int i = 1; i < temData.Length; i++)
        {
            /* 每一行的内容都是逗号分隔，读取每一列的值 */
            string[] lineData = temData[i].Split(new char[] { ',' }, keys.Length);

            /* CSVDemo类与CSVDemo.csv文件的key字段一一对应，用于保存每一行的数据内容 */
            CsvLevelBaseData csvDemo = new CsvLevelBaseData();
            for (int j = 0; j < lineData.Length; j++)
            {
                if (keys[j] == "Level")
                {
                    csvDemo.Level = Convert.ToInt32(lineData[j]);
                }
                else if (keys[j] == "TimeDe")
                {
                    csvDemo.TimeDe = Convert.ToInt32(lineData[j]);
                }
                else if (keys[j] == "XColumn")
                {
                    csvDemo.XColumn = Convert.ToInt32(lineData[j]);
                }
                else if (keys[j] == "YRow")
                {
                    csvDemo.YRow = Convert.ToInt32(lineData[j]);
                }
                else if (keys[j] == "AimLimit")
                {
                    csvDemo.AimLimit = Convert.ToInt32(lineData[j]);
                }
                else if (keys[j] == "Biscuit_Pos")
                {
                    csvDemo.SetBiscuitPos(lineData[j]);
                }
            }
            /* 保存每一行ID和数据对象的关系 */
            csvDataDic[csvDemo.Level] = csvDemo;
        }
    }

    // 根据配置表的位置创建饼干
    private void DelPosToCreateBiscuit(Dictionary<int, int> posDic)
    {
        foreach (int key in posDic.Keys)
        {
            if (key >= 0 && key < xColum && posDic[key] >= 0 && posDic[key] < yRow)
            {
                Destroy(sweetArray[key, posDic[key]].gameObject);
                CreateSweet(key, posDic[key], SweetType.BARRIER);
            }
        }
    }

    private void Update()
    {
        if (isGameOver || !isFinishLoad)
        {
            return;
        }
        gameTime -= Time.deltaTime;
        if (gameTime <= 0)
        {
            gameTime = 0;
            textTime.GetComponent<Animator>().speed = 0;
            // 避免时间结束时，分数未显示到实际得分
            textScore.text = score.ToString();
            // 判断是否通过当前关卡
            if (score >= csvDataDic[levelCount].AimLimit)
            {
                // 全部通关
                if (levelCount >= csvDataDic.Count)
                {
                    // 重置关卡数
                    PlayerPrefs.SetInt("Level",1);
                    passText.text = "恭喜您，全部通关！";
                    btnReplay.gameObject.SetActive(false);
                    btnPass.gameObject.SetActive(true);
                    btnClose.gameObject.SetActive(false);
                    btnNext.gameObject.SetActive(false);
                }
                // 未全部通关，仅通过关卡
                else
                {
                    // 保存关卡数
                    PlayerPrefs.SetInt("Level", levelCount + 1);
                    passText.text = "恭喜您，通关成功！";
                    btnReplay.gameObject.SetActive(false);
                    btnPass.gameObject.SetActive(false);
                    btnClose.gameObject.SetActive(true);
                    btnNext.gameObject.SetActive(true);
                }            
            }
            else
            {
                passText.text = "很遗憾，通关失败！";
                btnNext.gameObject.SetActive(false);
                btnPass.gameObject.SetActive(false);
                btnClose.gameObject.SetActive(true);
                btnReplay.gameObject.SetActive(true);
            }
            // 结算面板
            textFinalScore.text = score.ToString();
            gameOverPanel.SetActive(true);
            isGameOver = true;
        }
        textTime.text = gameTime.ToString("0");
        // 处理得分累加效果
        if (addScoreTime <= 0.05f)
        {
            addScoreTime += Time.deltaTime;
        }
        else
        {
            if (curScore < score)
            {
                curScore++;
                textScore.text = curScore.ToString();
                addScoreTime = 0;
            }
        }
    }

    // 设置甜品位置
    public Vector2 ResetPos(int x, int y)
    {
        return new Vector2(x - xColum / 2.0f + 0.5f, -y + yRow / 2.0f - 0.5f);
    }

    // 生成甜品
    private void CreateSweet(int x, int y, SweetType type)
    {
        GameObject sweetObj = Instantiate(sweetPrefabsDic[type], ResetPos(x, y), Quaternion.identity);
        sweetObj.transform.SetParent(transform);

        sweetArray[x, y] = sweetObj.GetComponent<GameSweet>();
        sweetArray[x, y].Init(x, y, this, type);
    }

    // 全部填充甜品
    private IEnumerator TotalFill()
    {
        isFillComplete = false;

        bool needBeFill = true;
        while (needBeFill)
        {
            yield return new WaitForSeconds(1.0f);
            while (ToFill())
            {
                // 控制填充速度
                yield return new WaitForSeconds(fillTime);
            }
            // 由于Tofill中只判断了非最后一行的情况，所以有可能出现最后一行非空位置旁边为空的情况
            ReviewLastLine();
            needBeFill = ClearSweetList();
        }

        isFillComplete = true;
    }
    // 部分填充甜品
    private bool ToFill()
    {
        // 是否完成填充
        bool isFinishFilled = false;
        // 从倒数第二行开始
        for (int y = yRow - 2; y >= 0; y--)
        {

            for (int x = 0; x < xColum; x++)
            {
                // 当前位置的GameSweet
                GameSweet sweet = sweetArray[x, y];

                if (sweet.IsCanMove())
                {
                    GameSweet downSweet = sweetArray[x, y + 1];
                    // 向正下填充
                    if (downSweet.Type == SweetType.EMPTY)
                    {
                        sweet.MoveSweet.ToMove(x, y + 1, fillTime);
                        sweetArray[x, y + 1] = sweet;
                        CreateSweet(x, y, SweetType.EMPTY);
                        isFinishFilled = true;
                    }
                    else
                    {
                        // 左下方 右下方 排除正下方的情况
                        for (int down = -1; down <= 1; down += 2)
                        {
                            int xPos = x + down;
                            if (xPos >= 0 && xPos < xColum)
                            {

                                // ①隔壁为空且隔壁的顶上存在障碍
                                // 获取隔壁的sweet
                                GameSweet sideSweet = sweetArray[xPos, y];
                                if (sideSweet.Type == SweetType.EMPTY)
                                {
                                    if (y - 1 >= 0)
                                    {

                                        int temY = GetFillPosY(xPos, y);

                                        if (temY > 0)
                                        {
                                            sweet.MoveSweet.ToMove(xPos, temY, fillTime);
                                            sweetArray[xPos, temY] = sweet;
                                            CreateSweet(x, y, SweetType.EMPTY);
                                            isFinishFilled = true;
                                            break;
                                        }
                                    }
                                }
                                // ②隔壁是障碍且隔壁的下方为空
                                else if (!sideSweet.IsCanMove() && sideSweet.Type == SweetType.BARRIER)
                                {
                                    if (y + 1 < yRow)
                                    {
                                        GameSweet sideDownSweet = sweetArray[xPos, y + 1];
                                        if (!sideDownSweet.IsCanMove() && sideDownSweet.Type == SweetType.EMPTY)
                                        {
                                            sweet.MoveSweet.ToMove(xPos, y + 1, fillTime);
                                            sweetArray[xPos, y + 1] = sweet;
                                            CreateSweet(x, y, SweetType.EMPTY);
                                            isFinishFilled = true;
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
            }
        }

        // 判断第一行的情况
        for (int x = 0; x < xColum; x++)
        {
            GameSweet sweet = sweetArray[x, 0];
            if (sweet.Type == SweetType.EMPTY)
            {
                // 在第一行上面新建一个甜品
                GameObject newSweet = Instantiate(sweetPrefabsDic[SweetType.NORMAL], ResetPos(x, -1), Quaternion.identity);
                newSweet.transform.SetParent(transform);
                // 设置属性
                sweetArray[x, 0] = newSweet.GetComponent<GameSweet>();
                sweetArray[x, 0].Init(x, -1, _instance, SweetType.NORMAL);
                sweetArray[x, 0].MoveSweet.ToMove(x, 0, fillTime);
                sweetArray[x, 0].ColorSweet.SetColor((ColorSweet.ColorType)UnityEngine.Random.Range(0, sweetArray[x, 0].ColorSweet.GetLength()));
                isFinishFilled = true;
            }
        }
        return isFinishFilled;
    }
    private void ReviewLastLine()
    {
        // 判断最后一行的情况
        for (int x = 0; x < xColum; x++)
        {
            GameSweet lastSweet = sweetArray[x, yRow - 1];
            if (lastSweet.Type == SweetType.EMPTY)
            {
                int temY = GetFillPosY(x, yRow - 1);
                Debug.Log("移动到Y：" + temY);
                // 进行左右边判断是否存在可移动的甜品
                for (int j = -1; j <= 1; j += 2)
                {
                    if (x + j >= 0 && x + j <= xColum)
                    {
                        GameSweet temMoveSweet = sweetArray[x + j, yRow - 1];
                        if (temMoveSweet.IsCanMove() && temMoveSweet.Type != SweetType.EMPTY)
                        {
                            temMoveSweet.MoveSweet.ToMove(x, temY, fillTime);
                            sweetArray[x, temY] = temMoveSweet;
                            CreateSweet(x + j, yRow - 1, SweetType.EMPTY);
                            if (temY > 0)
                            {
                                ToFill();
                            }
                            break;
                        }
                    }
                }
            }
        }
    }

    // 处理障碍间隔导致中间填充不到甜品
    private int GetFillPosY(int x, int y)
    {
        if (y < 0) return -1;
        GameSweet temSweet = sweetArray[x, y];
        if (temSweet.Type == SweetType.BARRIER)
        {
            return y + 1;
        }
        else if (temSweet.Type != SweetType.EMPTY)
        {
            return -1;
        }
        return GetFillPosY(x, y - 1); ;
    }

    // 判断是否相邻
    private bool IsAdjacent(GameSweet sweet1, GameSweet sweet2)
    {
        if (!sweet1 || !sweet2)
        {
            return false;
        }
        else if (sweet1.X == sweet2.X && Mathf.Abs(sweet1.Y - sweet2.Y) == 1) return true;
        else if (sweet1.Y == sweet2.Y && Mathf.Abs(sweet1.X - sweet2.X) == 1) return true;
        return false;
    }
    // 交换甜品并进行判断
    private void ExchangeSweet(GameSweet sweet1, GameSweet sweet2)
    {
        if (sweet1.IsCanMove() && sweet2.IsCanMove())
        {
            isRemoving = true;
            // 更换数组中的坐标，以便检测使用
            sweetArray[sweet1.X, sweet1.Y] = sweet2;
            sweetArray[sweet2.X, sweet2.Y] = sweet1;

            // 可消除
            if (RemoveSweet(sweet1, sweet2.X, sweet2.Y) != null || RemoveSweet(sweet2, sweet1.X, sweet1.Y) != null || (sweet1.Type == SweetType.RAINBOW_CANDY && sweet2.IsHaveColor()) || (sweet2.Type == SweetType.RAINBOW_CANDY && sweet1.IsHaveColor()))
            {
                int temX = sweet1.X;
                int temY = sweet1.Y;
                sweet1.MoveSweet.ToMove(sweet2.X, sweet2.Y, fillTime);
                sweet2.MoveSweet.ToMove(temX, temY, fillTime);
                // 彩虹糖判断
                if (sweet1.Type == SweetType.RAINBOW_CANDY && sweet2.IsCanClear() && sweet2.IsHaveColor())
                {
                    ClearColorsSweet clearColorSweet = (ClearColorsSweet)sweet1.ClearSweet;
                    clearColorSweet.ClearColor = sweet2.ColorSweet.CurColor;
                    ClearSweet(sweet1.X, sweet1.Y);
                    StartCoroutine(TotalFill());
                }
                else if (sweet2.Type == SweetType.RAINBOW_CANDY && sweet1.IsCanClear() && sweet1.IsHaveColor())
                {
                    ClearColorsSweet clearColorSweet = (ClearColorsSweet)sweet2.ClearSweet;
                    clearColorSweet.ClearColor = sweet1.ColorSweet.CurColor;
                    ClearSweet(sweet2.X, sweet2.Y);
                    StartCoroutine(TotalFill());
                }

                // 普通消除操作
                if (ClearSweetList())
                {
                    StartCoroutine(TotalFill());
                }
            }
            // 不可消除,还原更换
            else
            {
                sweetArray[sweet1.X, sweet1.Y] = sweet1;
                sweetArray[sweet2.X, sweet2.Y] = sweet2;
            }
        }
        isRemoving = false;
    }

    // 赋值选中的第一个甜品
    public void FirstSweet(GameSweet sweet)
    {
        if (isGameOver || isRemoving || !isFillComplete) return;
        firstSweet = sweet;

    }
    // 赋值选中的第二个甜品
    public void SecondSweet(GameSweet sweet)
    {
        if (isGameOver || isRemoving || !isFillComplete) return;
        if (firstSweet) secondSweet = sweet;
    }
    // 鼠标松开时
    public void ReleaseSweet()
    {
        if (isGameOver || isRemoving || !isFillComplete) return;

        if (IsAdjacent(firstSweet, secondSweet))
        {
            ExchangeSweet(firstSweet, secondSweet);
        }
    }

    // 进行消除判断,返回可消除的甜品列表    特殊情况：在开始时候直接消除可消除的甜品
    private List<GameSweet> RemoveSweet(GameSweet sweet, int sweetX, int sweetY)
    {
        if (sweet.IsHaveColor())
        {
            // 获取当前甜品的颜色类型
            ColorSweet.ColorType color = sweet.ColorSweet.CurColor;
            // 保存当前类型的甜品数组
            List<GameSweet> sweetList = new List<GameSweet>();
            // 行向
            List<GameSweet> sweetListRow = new List<GameSweet>();
            // 纵向
            List<GameSweet> sweetListColum = new List<GameSweet>();

            // 横向匹配
            // 向右匹配
            sweetListRow.Add(sweet);
            for (int dis = 1; dis < xColum - sweetX; dis++)
            {
                GameSweet temSweetR = sweetArray[sweetX + dis, sweetY];
                if (temSweetR.IsHaveColor() && temSweetR.ColorSweet.CurColor == color)
                {
                    sweetListRow.Add(temSweetR);
                }
                else
                {
                    break;
                }
            }
            // 向左匹配
            for (int dis = 1; dis <= sweetX; dis++)
            {
                GameSweet temSweetL = sweetArray[sweetX - dis, sweetY];
                if (temSweetL.IsHaveColor() && temSweetL.ColorSweet.CurColor == color)
                {
                    sweetListRow.Add(temSweetL);
                }
                else
                {
                    break;
                }
            }

            // L、T匹配
            if (sweetListRow.Count >= 3)
            {

                for (int j = 0; j < sweetListRow.Count; j++)
                {
                    // 将上面sweetListRow的元素存入总列表中
                    sweetList.Add(sweetListRow[j]);

                    // 对sweetListRow中每个甜品的上下进行遍历
                    GameSweet temSweetLR = sweetArray[sweetListRow[j].X, sweetListRow[j].Y];
                    // 对列表中的甜品进行上下遍历
                    // 相对当前甜品向上遍历
                    for (int k = 1; k <= temSweetLR.Y; k++)
                    {
                        if (sweetArray[temSweetLR.X, temSweetLR.Y - k].IsHaveColor() && sweetArray[temSweetLR.X, temSweetLR.Y - k].ColorSweet.CurColor == color)
                        {
                            sweetListColum.Add(sweetArray[temSweetLR.X, temSweetLR.Y - k]);
                        }
                        else
                        {
                            break;
                        }
                    }
                    // 相对当前甜品向下遍历
                    for (int k = 1; k < yRow - temSweetLR.Y; k++)
                    {
                        if (sweetArray[temSweetLR.X, temSweetLR.Y + k].IsHaveColor() && sweetArray[temSweetLR.X, temSweetLR.Y + k].ColorSweet.CurColor == color)
                        {
                            // 添加到纵向列表中
                            sweetListColum.Add(sweetArray[temSweetLR.X, temSweetLR.Y + k]);
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (sweetListColum.Count >= 2)
                    {
                        for (int l = 0; l < sweetListColum.Count; l++)
                        {
                            sweetList.Add(sweetListColum[l]);
                        }
                    }
                    else
                    {
                        sweetListColum.Clear();
                    }
                }

                Debug.Log("检测到数量满足可执行消除操作,数量为：" + sweetList.Count.ToString());
                return sweetList;
            }
            sweetListRow.Clear();
            sweetListColum.Clear();

            // 纵向匹配
            // 向下
            sweetListColum.Add(sweet);

            for (int dis = 1; dis < yRow - sweetY; dis++)
            {
                GameSweet temSweetD = sweetArray[sweetX, sweetY + dis];
                if (temSweetD.IsHaveColor() && temSweetD.ColorSweet.CurColor == color)
                {
                    sweetListColum.Add(temSweetD);
                }
                else
                {
                    break;
                }
            }
            // 向上
            for (int dis = 1; dis <= sweetY; dis++)
            {
                GameSweet temSweetU = sweetArray[sweetX, sweetY - dis];
                if (temSweetU.IsHaveColor() && temSweetU.ColorSweet.CurColor == color)
                {
                    sweetListColum.Add(temSweetU);
                }
                else
                {
                    break;
                }
            }
            // 只要一个方向达到3个同种甜品，即添加横向和纵向的相同类型甜品到总列表中
            if (sweetListColum.Count >= 3)
            {
                // 将上面sweetListRow的元素存入总列表中
                for (int j = 0; j < sweetListColum.Count; j++)
                {
                    sweetList.Add(sweetListColum[j]);

                    // 对sweetListColum中每个甜品的左右进行遍历
                    GameSweet temSweetUD = sweetArray[sweetListColum[j].X, sweetListColum[j].Y];
                    // 对列表中的甜品进行左右遍历    
                    // 相对当前甜品向左遍历
                    for (int k = 1; k <= temSweetUD.X; k++)
                    {
                        if (sweetArray[temSweetUD.X - k, temSweetUD.Y].IsHaveColor() && sweetArray[temSweetUD.X - k, temSweetUD.Y].ColorSweet.CurColor == color)
                        {
                            sweetListRow.Add(sweetArray[temSweetUD.X - k, temSweetUD.Y]);
                        }
                        else
                        {
                            break;
                        }
                    }
                    // 相对当前甜品向右遍历
                    for (int k = 1; k < xColum - temSweetUD.X; k++)
                    {
                        if (sweetArray[temSweetUD.X + k, temSweetUD.Y].IsHaveColor() && sweetArray[temSweetUD.X + k, temSweetUD.Y].ColorSweet.CurColor == color)
                        {
                            // 添加到横向列表中
                            sweetListRow.Add(sweetArray[temSweetUD.X + k, temSweetUD.Y]);
                        }
                        else
                        {
                            break;
                        }
                    }
                    if (sweetListRow.Count >= 2)
                    {
                        for (int l = 0; l < sweetListRow.Count; l++)
                        {
                            sweetList.Add(sweetListRow[l]);
                        }
                    }
                    else
                    {
                        sweetListRow.Clear();
                    }
                }

                Debug.Log("检测到数量满足可执行消除操作,数量为：" + sweetList.Count.ToString());
                return sweetList;
            }
            sweetListRow.Clear();
            sweetListColum.Clear();
        }
        return null;
    }
    // 清除单个甜品方法
    private bool ClearSweet(int x, int y)
    {
        if (sweetArray[x, y].IsCanClear() && !sweetArray[x, y].ClearSweet.IsClearing)
        {
            sweetArray[x, y].ClearSweet.Clear();

            CreateSweet(x, y, SweetType.EMPTY);
            // 判断当前消除的甜品周围是否存在可消除的饼干障碍
            ClearBiscuit(x, y);

            return true;
        }
        return false;
    }
    // 清除所有的可消除的甜品
    private bool ClearSweetList()
    {
        // 是否需要被填充，在消除阶段不允许填充的发生
        bool isCanFill = false;

        for (int x = 0; x < xColum; x++)
        {
            for (int y = 0; y < yRow; y++)
            {
                if (sweetArray[x, y].IsCanClear())
                {
                    List<GameSweet> temList = RemoveSweet(sweetArray[x, y], x, y);
                    if (temList != null)
                    {
                        GameSweet randSweet = null;
                        int randX = 0;
                        int randY = 0;
                        // ①生成行消除甜品或者列消除甜品，前部分先保存记录数据便于后面生成
                        // 设置randSweetType为默认SweetType.COUNT便于后面做判断
                        SweetType randSweetType = SweetType.COUNT;
                        ColorSweet.ColorType curColorType = ColorSweet.ColorType.COUNT;

                        if (temList.Count >= 4)
                        {
                            // 随机标记一个sweet位置
                            randSweet = temList[UnityEngine.Random.Range(0, temList.Count)];
                            randX = randSweet.X;
                            randY = randSweet.Y;
                        }
                        // 随机所要生成的甜品类型(行消除类型或者列消除类型)
                        if (temList.Count == 4)
                        {
                            randSweetType = (SweetType)UnityEngine.Random.Range((int)SweetType.ROW_CLEAR, (int)SweetType.COLUM_CLEAR + 1);
                            curColorType = randSweet.ColorSweet.CurColor;
                        }
                        // 生成彩虹糖
                        else if (temList.Count > 4)
                        {
                            randSweetType = SweetType.RAINBOW_CANDY;
                        }
                        // 先清除
                        for (int i = 0; i < temList.Count; i++)
                        {
                            if (ClearSweet(temList[i].X, temList[i].Y))
                            {
                                isCanFill = true;
                            }
                        }
                        // ②再生成行消除类型或者列消除类型甜品 或者 彩虹糖
                        if (randSweetType != SweetType.COUNT)
                        {
                            // 先隐藏 
                            sweetArray[randX, randY].gameObject.SetActive(false);
                            Destroy(sweetArray[randX, randY].gameObject);
                            CreateSweet(randX, randY, randSweetType);
                            //行消除类型或者列消除类型甜品
                            if (randSweetType == SweetType.ROW_CLEAR || randSweetType == SweetType.COLUM_CLEAR)
                            {
                                // 设置甜品颜色类型
                                if (curColorType != ColorSweet.ColorType.COUNT)
                                {
                                    sweetArray[randX, randY].ColorSweet.SetColor(curColorType);
                                }
                            }
                            else if (randSweetType == SweetType.RAINBOW_CANDY)
                            {

                            }
                        }
                    }
                }
            }
        }
        return isCanFill;
    }

    // 清除整行
    public void ClearRowSweets(int row)
    {
        for (int x = 0; x < xColum; x++)
        {
            // 包含饼干
            ClearSweet(x, row);
        }
    }

    // 清除整列
    public void ClearColumSweets(int colum)
    {
        for (int y = 0; y < yRow; y++)
        {
            // 包含饼干
            ClearSweet(colum, y);
        }
    }

    // 清除同一类型糖果
    public void ClearSameColorSweets(ColorSweet.ColorType sweetColor)
    {
        for (int x = 0; x < xColum; x++)
        {
            for (int y = 0; y < yRow; y++)
            {
                if (sweetArray[x, y].IsCanClear() && sweetArray[x, y].IsHaveColor() && sweetArray[x, y].ColorSweet.CurColor == sweetColor)
                {
                    ClearSweet(x, y);
                }
            }
        }
    }

    // 清除饼干
    private void ClearBiscuit(int x, int y)
    {
        // 左右遍历
        for (int i = x - 1; i <= x + 1; i += 2)
        {
            if (i >= 0 && i < xColum)
            {
                if (sweetArray[i, y].Type == SweetType.BARRIER && sweetArray[i, y].IsCanClear() && !sweetArray[i, y].ClearSweet.IsClearing)
                {
                    sweetArray[i, y].ClearSweet.Clear();
                    CreateSweet(i, y, SweetType.EMPTY);
                }
            }
        }
        // 上下遍历
        for (int j = y - 1; j <= y + 1; j += 2)
        {
            if (j > 0 && j < yRow)
            {
                if (sweetArray[x, j].Type == SweetType.BARRIER && sweetArray[x, j].IsCanClear() && !sweetArray[x, j].ClearSweet.IsClearing)
                {
                    sweetArray[x, j].ClearSweet.Clear();
                    CreateSweet(x, j, SweetType.EMPTY);
                }
            }
        }

    }
    
    // 点击进入下一关
    public void EnterToNext()
    {
        SceneManager.LoadScene(1);
    }
    
    // 点击重玩
    public void ReturnToReplay()
    {
        // 进入展示广告阶段
        showRewardedAd();
    }
    // 返回主菜单
    public void ReturnToMainGame()
    {
        SceneManager.LoadScene(0);
    }
    // 展示广告
    private void showRewardedAd()
    {
        // Win平台不展示广告
        if (gameAdsID == string.Empty)
        {
            SceneManager.LoadScene(1);
            return;
        }
#if UNITY_ANDROID || UNITY_IOS
        if (!Advertisement.IsReady())
        {
            HandleShowResult(ShowResult.Failed);
            return;
        }
        else
        {
            var options = new ShowOptions { resultCallback = HandleShowResult };
            Advertisement.Show(options);
        }
#endif
    }
#if UNITY_ANDROID || UNITY_IOS
    private void HandleShowResult(ShowResult result)
    {
        switch (result)
        {
            case ShowResult.Finished:
                SceneManager.LoadScene(1);
                //onAdSuccess.Invoke();
                break;
            case ShowResult.Skipped:
                SceneManager.LoadScene(1);
                //Debug.Log ("The ad was skipped before reaching the end.");
                //onAdFail.Invoke();
                break;
            case ShowResult.Failed:
                SceneManager.LoadScene(1);
                //Debug.LogError ("The ad failed to be shown.");
                //onAdFail.Invoke();
                break;
        }
    }
#endif
}
