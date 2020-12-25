using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;
using System.IO;
using EasyMobile;
#pragma warning disable 0414

public class GameManager : MonoBehaviour
{

    public static GameManager instance = null;
    [HideInInspector]
    public GameObject board;
    [HideInInspector]
    public int boardGridWidth = 4, boardGridHeight = 4;

    //This used only for current level
    [HideInInspector]
    public int stars;


    [HideInInspector]
    public bool goalAchieved = false;
    public int maxLevels = 40;
    public int maxPacks = 4;
    [HideInInspector]
    public GameObject genericDialog;
    [HideInInspector]
    public static int rewardsCount = 0, rewardsLimit = 3;
    public GameObject genericDialogprefab;
    public static float boardBorderWidth = 0.15f;
    public static Vector2 tileSize;
    public static int currentPack = 1;
    public static int currentLevel = 1;
    public GameObject ballPrefab;
    public GameObject levelCompletedDialog_prefab;
    private GameObject levelCompletedDialog;
    private float boardInitX, boardInitY;
    private Vector3 mouseHitPos;
    private bool startSwipe;
    private GameObject targetTile;
    private BoxCollider2D boardCollider;
    private int tileX, tileY, moveToX, moveToY;
    private float targetX, targetY;
    private const int NO_MOVE = 0, MOV_RIGHT = 1, MOV_LEFT = 2, MOV_UP = 3, MOV_DOWN = 4;
    private int boardState;
    //How fast the tile will be moved
    private float speed = 15f;
    private GameObject ball;
    private GameObject startTile, goalTile;

    void Awake()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        //Use singleton pattern, which allows only one
        //GameManager's instance between all the scenes
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (SoundManager.instance != null)
            SoundManager.instance.PlaySound(SoundManager.instance.music);

        Prefs.onHintCountChanged += SetHintCountText;
    }

    public void LoadLevel()
    {
        goalAchieved = false;
        stars = 0;

        GameObject.Find("LevelPackText").GetComponent<Text>().text = "Level Pack " + currentPack;
        GameObject.Find("LevelText").GetComponent<Text>().text = "Level " + currentLevel;

        GameObject brush = GameObject.Find("Brush");
        if (brush != null)
            DestroyImmediate(brush);

        //Get board GameObject
        boardState = NO_MOVE;

        var hint = GameObject.FindWithTag("Hint");
        board = GameObject.FindWithTag("Board");

        if (GameConfig.instance != null)
        {
            if (board != null) DestroyImmediate(board);
            if (hint != null) DestroyImmediate(hint);

            string folder = "LevelPack_" + currentPack + "/Level_" + currentLevel;
            hint = Instantiate(Resources.Load(folder + "/Hint")) as GameObject;
            board = Instantiate(Resources.Load(folder + "/Board")) as GameObject;
        }

        GameObject puzzle = board.transform.Find("Tiles").gameObject;

        Board puzzleScript = puzzle.GetComponent<Board>();
        tileSize = new Vector2(puzzleScript.tileSize.x / puzzleScript.pixelsToUnits, puzzleScript.tileSize.y / puzzleScript.pixelsToUnits);
        boardInitX = puzzle.transform.position.x;
        boardInitY = puzzle.transform.position.y;
        for (int i = 0; i < boardGridWidth; i++)
        {
            for (int k = 0; k < boardGridHeight; k++)
            {
                GameObject boardRect = new GameObject("boardRect");
                boardRect.transform.SetParent(board.transform);
                boardCollider = boardRect.AddComponent<BoxCollider2D>();
                boardCollider.size = new Vector3(tileSize.x - 0.1f, tileSize.y - 0.1f, 0);
                boardCollider.tag = "BoardCollider";
                boardCollider.isTrigger = true;
                boardRect.transform.position = new Vector3(boardInitX + tileSize.x / 2f + tileSize.x * i, boardInitY - tileSize.y / 2f - tileSize.y * k, 1);
            }
        }

        GameObject[] tiles = GameObject.FindGameObjectsWithTag("Tile");
        for (int i = 0; i < tiles.Length; i++)
        {
            if (tiles[i].GetComponent<Tile>().isStartTile)
                startTile = tiles[i];
            if (tiles[i].GetComponent<Tile>().isGoalTile)
                goalTile = tiles[i];
        }
        ball = Instantiate(ballPrefab);
        ball.transform.position = startTile.transform.position;

        GameObject canvas = GameObject.Find("Canvas");
        levelCompletedDialog = Instantiate(levelCompletedDialog_prefab);
        levelCompletedDialog.transform.SetParent(canvas.transform, false);
        levelCompletedDialog.SetActive(false);

        SetHintCountText();
        if (genericDialog == null)
        {
            genericDialog = Instantiate(genericDialogprefab);
            canvas = GameObject.Find("Canvas");
            genericDialog.transform.SetParent(canvas.transform, false);
        }
    }

    public void FinishLevel()
    {
        //Update stars for current level
        string currentLevelStr = "LP" + currentPack + "_" + "level-" + currentLevel;
        string levelStars = currentLevelStr + "stars";
        int s = PlayerPrefs.GetInt(levelStars);
        string packNameStr = "level_pack_" + currentPack;
        string starsCollectedStr = packNameStr + "stars";
        string packCompletedStr = packNameStr + "completed";
        //If current collected stars amount is greater then last record, then
        //update stars amount for current level and for current pack
        if (stars > s)
        {
            PlayerPrefs.SetInt(levelStars, stars);
            int oldStars = PlayerPrefs.GetInt(starsCollectedStr);
            PlayerPrefs.SetInt(starsCollectedStr, oldStars + (stars - s));
        }
        if (currentLevel + 1 <= maxLevels)
        {
            //Unlock next level
            string nextLevelStr = "LP" + currentPack + "_" + "level-" + (currentLevel + 1);
            PlayerPrefs.SetInt(nextLevelStr, 1);
        }
        else
        {
            PlayerPrefs.SetInt(packCompletedStr, 1);
            if (currentPack + 1 <= maxPacks)
            {
                PlayerPrefs.SetInt("level_pack_" + (currentPack + 1), 1);
            }
        }
    }

    public void UpdateLevel()
    {
        if (Input.GetMouseButtonDown(0) && boardState == NO_MOVE && !ball.GetComponent<Ball>().move
            && !goalAchieved)
        {
            startSwipe = true;
            targetTile = GetTileClicked();
            if (targetTile != null && targetTile.GetComponent<Tile>().isFixed)
                targetTile = null;
        }
        if (Input.GetMouseButtonUp(0))
        {
            startSwipe = false;
        }
        if (startSwipe && targetTile != null)
            CheckClickPosition();

        if (boardState != NO_MOVE && targetTile != null)
        {
            MoveTile(targetTile);
        }
    }

    GameObject GetTileClicked()
    {
        GameObject b = null;
        float mouseX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
        float mouseY = Camera.main.ScreenToWorldPoint(Input.mousePosition).y;
        RaycastHit2D hitInfo = Physics2D.Raycast(new Vector2(mouseX, mouseY), Vector2.zero, 0);
        if (hitInfo)
        {
            if (hitInfo.collider.gameObject.tag == "Tile")
            {
                b = hitInfo.collider.gameObject;
                tileX = Mathf.FloorToInt((b.transform.position.x - boardInitX) / tileSize.x);
                tileY = Mathf.FloorToInt((b.transform.position.y + boardInitY) / tileSize.y);
            }
            if (hitInfo.collider.gameObject.tag == "star-collectable" ||
                hitInfo.collider.gameObject.tag == "connection_point_1" ||
                hitInfo.collider.gameObject.tag == "connection_point_2")
            {
                b = hitInfo.collider.transform.parent.gameObject;
                tileX = Mathf.FloorToInt((b.transform.position.x - boardInitX) / tileSize.x);
                tileY = Mathf.FloorToInt((b.transform.position.y + boardInitY) / tileSize.y);
            }
        }
        return b;
    }

    public void SetHintCountText()
    {
        GameObject hintObject = GameObject.Find("Hint-Button");
        if (hintObject != null)
        {
            GameObject txt = hintObject.transform.Find("Text").gameObject;
            txt.GetComponent<Text>().text = "Hint (" + Prefs.hintCount + ")";
        }
    }

    void FinishMove()
    {
        boardState = NO_MOVE;
        targetTile = null;
        SoundManager.instance.PlaySound(SoundManager.instance.moveTileSound);
    }

    void MoveTile(GameObject tile)
    {
        if (boardState == MOV_RIGHT)
        {
            if (tile.transform.position.x + speed * Time.deltaTime < targetX)
                tile.transform.Translate(speed * Time.deltaTime, 0, 0);
            else
            {
                tile.transform.position = new Vector3(targetX, tile.transform.position.y, tile.transform.position.z);
                FinishMove();
            }
        }
        if (boardState == MOV_LEFT)
        {
            if (tile.transform.position.x - speed * Time.deltaTime > targetX)
                tile.transform.Translate(-speed * Time.deltaTime, 0, 0);
            else
            {
                tile.transform.position = new Vector3(targetX, tile.transform.position.y, tile.transform.position.z);
                FinishMove();
            }
        }
        if (boardState == MOV_UP)
        {
            if (tile.transform.position.y + speed * Time.deltaTime < targetY)
                tile.transform.Translate(0, speed * Time.deltaTime, 0);
            else
            {
                tile.transform.position = new Vector3(tile.transform.position.x, targetY, tile.transform.position.z);
                FinishMove();
            }
        }
        if (boardState == MOV_DOWN)
        {
            if (tile.transform.position.y - speed * Time.deltaTime > targetY)
                tile.transform.Translate(0, -speed * Time.deltaTime, 0);
            else
            {
                tile.transform.position = new Vector3(tile.transform.position.x, targetY, tile.transform.position.z);
                FinishMove();
            }
        }
    }

    public void CheckPathCompleted()
    {
        if (!goalAchieved)
        {
            GameObject nextTile = startTile.transform.Find("connection_point_1").GetComponent<ConnectionPoint>().connectedTile;
            if (nextTile != null)
                CheckNextPath(startTile, nextTile);
            else
                ball.GetComponent<Ball>().ClearWaypoints();
        }
    }

    void CheckNextPath(GameObject previousTile, GameObject currentTile)
    {
        GameObject nextTile = null;
        if (currentTile.GetComponent<Tile>().IsConnected())
        {
            GameObject connectedTile2 = null;
            GameObject connectedTile1 = currentTile.transform.Find("connection_point_1").GetComponent<ConnectionPoint>().connectedTile;
            if (!currentTile.GetComponent<Tile>().isStartTile && !currentTile.GetComponent<Tile>().isGoalTile)
                connectedTile2 = currentTile.transform.Find("connection_point_2").GetComponent<ConnectionPoint>().connectedTile;
            if (connectedTile1 != previousTile)
                nextTile = connectedTile1;
            if (connectedTile2 != previousTile && connectedTile2 != null)
                nextTile = connectedTile2;

            if (nextTile != null)
            {
                ball.GetComponent<Ball>().AddWaypoint(currentTile.transform);
                CheckNextPath(currentTile, nextTile);
            }
            else
            {
                ball.GetComponent<Ball>().AddWaypoint(currentTile.transform);
                if (!ball.GetComponent<Ball>().move)
                {
                    ball.GetComponent<Ball>().move = true;
                    SoundManager.instance.PlaySound(SoundManager.instance.pathCompletedSound);
                }
            }

        }
        else
            ball.GetComponent<Ball>().ClearWaypoints();
    }

    void CheckClickPosition()
    {
        float mouseX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
        float mouseY = Camera.main.ScreenToWorldPoint(Input.mousePosition).y;

        RaycastHit2D hitInfo = Physics2D.Raycast(new Vector2(mouseX, mouseY), Vector2.zero, 0);
        if (hitInfo)
        {
            if (hitInfo.collider.gameObject.tag == "BoardCollider")
            {
                GameObject c = hitInfo.collider.gameObject;
                moveToX = Mathf.FloorToInt((c.transform.position.x - boardInitX) / tileSize.x);
                moveToY = Mathf.FloorToInt((c.transform.position.y + boardInitY) / tileSize.y);
                if (targetTile != null && boardState == NO_MOVE)
                {
                    if (tileY == moveToY)
                    {
                        if (tileX == moveToX - 1)
                        {
                            targetX = c.transform.position.x;
                            boardState = MOV_RIGHT;
                        }
                        if (tileX == moveToX + 1)
                        {
                            targetX = c.transform.position.x;
                            boardState = MOV_LEFT;
                        }
                    }
                    if (tileX == moveToX)
                    {
                        if (tileY == moveToY - 1)
                        {
                            targetY = c.transform.position.y;
                            boardState = MOV_UP;
                        }
                        if (tileY == moveToY + 1)
                        {
                            targetY = c.transform.position.y;
                            boardState = MOV_DOWN;
                        }
                    }
                }
            }
        }
    }

    //UI Management section

    public void ShowLevelCompletedDialog(int stars)
    {
        if (!levelCompletedDialog.activeSelf)
        {
            levelCompletedDialog.GetComponent<RectTransform>().offsetMax = new Vector2(0, 0);
            levelCompletedDialog.GetComponent<RectTransform>().offsetMin = new Vector2(0, 0);
            levelCompletedDialog.SetActive(true);
            levelCompletedDialog.GetComponent<LevelCompletedDialog>().ShowStars(stars);

            CUtils.ShowInterstitialAd();
        }
    }

    public void LoadHomeScene()
    {
        SceneManager.LoadScene("Home");
    }

    public void LoadSelectLevelScene()
    {
        SceneManager.LoadScene("SelectLevel");
    }

    public void LoadSelectPackScene()
    {
        SceneManager.LoadScene("SelectPack");
    }

    public void RestartLevelW()
    {
        instance.RestartLevel();
    }

    void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }

    public void PlayNextLevelW()
    {
        instance.PlayNextLevel();
    }

    public void ShowExitDialog()
    {
        instance.ExitDialog();
    }

    void ExitDialog()
    {
        if (genericDialog != null)
        {
            genericDialog.GetComponent<GenericDialog>().SetActiveDialog("ExitDialog");
            genericDialog.SetActive(true);
            CUtils.ShowInterstitialAd();
        }
    }

    public void ShowEarnHintsDialog()
    {
        instance.EarnHintsDialog();
    }

    void EarnHintsDialog()
    {
        if (genericDialog != null)
        {
            genericDialog.GetComponent<GenericDialog>().SetActiveDialog("EarnHintsDialog");
            genericDialog.SetActive(true);
        }
    }

    public void ShowRewardLimitDialog()
    {
        if (genericDialog != null)
        {
            genericDialog.GetComponent<GenericDialog>().SetActiveDialog("RewardLimitMetDialog");
            genericDialog.SetActive(true);
        }
    }

    public void ShowVideoUnavailableDialog()
    {
        if (genericDialog != null)
        {
            genericDialog.GetComponent<GenericDialog>().SetActiveDialog("VideoUnavailableDialog");
            genericDialog.SetActive(true);
        }
    }

    public void ShowRewardedVideo()
    {
        instance.PlayRewardedVideo();
    }

    void PlayRewardedVideo()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            genericDialog.GetComponent<GenericDialog>().SetActiveDialog("InternetErrorDialog");
            genericDialog.SetActive(true);
        }
        else
        {
            genericDialog.SetActive(false);
            OnRewardedVideoClick();
        }
    }

    private const string ACTION_NAME = "rewarded_video";
    public void OnRewardedVideoClick()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (IsAvailableToShow())
            AdmobController.instance.ShowRewardBasedVideo();
        else if (!IsAdAvailable())
            Toast.instance.ShowMessage("Ad is not available now");
        else
        {
            int remainTime = (int)(GameConfig.instance.rewardedVideoPeriod - CUtils.GetActionDeltaTime(ACTION_NAME));
            Toast.instance.ShowMessage("Please wait " + remainTime + " seconds for the next ad");
        }
#endif
    }

    public bool IsAvailableToShow()
    {
        return IsActionAvailable() && IsAdAvailable();
    }

    private bool IsActionAvailable()
    {
        return CUtils.IsActionAvailable(ACTION_NAME, GameConfig.instance.rewardedVideoPeriod);
    }

    private bool IsAdAvailable()
    {
        if (AdmobController.instance.rewardBasedVideo == null) return false;
        bool isLoaded = AdmobController.instance.rewardBasedVideo.IsLoaded();
        return isLoaded;
    }

    public void ShowCongratsDialog()
    {
        if (genericDialog != null)
        {
            genericDialog.GetComponent<GenericDialog>().SetActiveDialog("CongratsDialog");
            genericDialog.SetActive(true);
            CUtils.ShowInterstitialAd();
        }
    }

    public void CancelDialog()
    {
        if (instance.genericDialog != null)
            instance.genericDialog.SetActive(false);
    }

    public void Exit()
    {
        Application.Quit();
    }

    void PlayNextLevel()
    {
        //string packNameStr = "level_pack_" + currentPack;
        //string packCompletedStr = packNameStr + "completed";
        if (currentLevel + 1 <= maxLevels)
        {
            currentLevel += 1;
            SceneManager.LoadScene("Main");
        }
        else
            SceneManager.LoadScene("SelectPack", LoadSceneMode.Single);
    }

    public void RateUs()
    {
#if UNITY_ANDROID
        Application.OpenURL("market://details?id=" + GameConfig.instance.androidPackageName);
#elif UNITY_IOS
        Application.OpenURL("https://itunes.apple.com/app/id" + GameConfig.instance.iosAppID);
#endif
    }

    public void ShareIt()
    {
        instance.ShareLink();
    }

    void ShareLink()
    {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        MobileNativeShare.ShareScreenshot("screenshot", "");
#endif
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SceneManager.GetActiveScene().name == "Home")
                instance.ExitDialog();
        }
    }

    void OnApplicationQuit()
    {
        GameManager.instance = null;
    }
}
