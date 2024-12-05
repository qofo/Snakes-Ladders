using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    /* public 변수 */
    public static GameManager instance = null;          // GameManager를 싱글톤 패턴으로 만들 때 사용할 변수
    public BoardManager boardScript;                    // 스테이지를 만드는 오브젝트
    public UIManager uiManager;                         // UI 담당하는 클래스
    public ScoreManager scoreManager;                   // 점수를 담당하는 클래스
    public float levelStartDelay = 0.2f;                  // 레벨이 시작되기 전에 초단위로 대기할 시간
    // Player용 변수들
    public int playerFoodPoints = 100;                  // 플레이어 포만감
    [HideInInspector] public bool playersTurn = true;   // 
    [HideInInspector] public bool shopping = false;
    public int nowPlayer = 1;
    // Enemy용 변수들
    public float turnDelay = 0.5f;                      // 각 턴 사이 대기시간

    /* private 변수 */
    private int level = 0;                      // 
    private bool doingCoroutine;                 //
    private bool isInitialized = false;         // InitGame() 함수가 호출되었는지 확인하는 함수
    private bool doingSetup = false;                    // 게임 보드를 만드는 중인지 확인하는 변수

    private HashSet<Vector2> occupiedPositions = new HashSet<Vector2>(); // 적들이 현재 점유하고 있는 위치를 추적하는 HashSet

    /* 유니티 API 함수들 */
    void Awake()
    {
        // 두 개의 GameManager가 생기지 않게 하기
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);                              // 다음 Scene으로 넘어가도 GameManager가 삭제되지 않게 하기

        boardScript = GetComponent<BoardManager>();
        uiManager = GetComponent<UIManager>();
        scoreManager = GetComponent<ScoreManager>();

        scoreManager.LoadScores();                                  // scores 변수에 이전 점수 저장하기

        playerFoodPoints = 100;
        if (SceneManager.GetActiveScene().name == "MainScene")      // TitleScene을 거치지 않고 실행한다면
        {
            instance.level = 1;
            InitGame();                                             // 스테이지 생성
        }
    }

    /*// 새 레벨이 로드될 때마다 InitGame 함수 호출하는 함수
    private void OnLevelWasLoaded(int index)
    {
        level++;

        InitGame();

    }*/
    void Update()
    {
        if (playersTurn || doingCoroutine || doingSetup || shopping)
            return;

        StartCoroutine(CoroutineExample());
    }

    // Scene 넘어가는 데 쓰이는 함수들
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static public void CallbackInitialization()
    {
        //register the callback to be called everytime the scene is loaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    //This is called each time a scene is loaded.
    static private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (instance.isInitialized)         // 만약 이미 InitGame()이 실행되었다면
            return;                             // InitGame() 실행하지 않고 넘어가기
        if (instance.level == 0)            // 만약 재시작한 것이라면
            instance.playerFoodPoints = 100;    // 플레이어 체력을 100으로 만들기

        instance.level++;
        instance.InitGame();
    }

    /* private 함수들 */
    // BoardManager를 통해 스테이지를 생성하는 함수
    void InitGame()
    {
        doingSetup = true;                                              // 플레이어가 맵 로드될 동안 못 움직이게 하기
        Time.timeScale = 1;
        // UI 띄우기
        uiManager.InitMainUI(level);
        Invoke("HideLevelImage", levelStartDelay);                      // levelStartDelay만큼 기다리고 다음 레벨 시작

        boardScript.SetupScene(level);
        isInitialized = true;
    }

    // 레벨이 다 로드되면 LevelImage UI 끄는 함수
    private void HideLevelImage()
    {
        uiManager.HideLevelImage();
        //levelImage.SetActive(false);
        doingSetup = false;
    }

    // Player가 다음 이동할 타일 확인하기
    public GameObject GetNextMove(int n = 1)
    {
        nowPlayer += n;
        return boardScript.floors[nowPlayer];
    }
    public void SetNextMove(int n)
    {
        nowPlayer = n;
    }

    public int CheckLadder(Vector3 pos)
    {
        int index = -1;
        Vector3 nextPos = Vector3.zero;
        index = boardScript.ladderStartPos.IndexOf(pos);
        if (index != -1)
            nextPos = boardScript.ladderEndPos[index];
        else
        {
            index = boardScript.snakeStartPos.IndexOf(pos);
            if (index != -1)
                nextPos = boardScript.snakeEndPos[index];
        }

        if (index == -1)
            return -1;
        int floorCount = (int)(boardScript.column * nextPos.y);
        if ((nextPos.y % 2 == 1))
            floorCount += (int)(boardScript.column - nextPos.x);
        else
            floorCount += (int)(nextPos.x + 1);

        return floorCount - 2;
    }

    // 게임 오버시 Player에 의해서 호출되어 GameManager를 비활성화시키는 함수
    public void GameOver()
    {
        uiManager.ShowGameOver(level);
        scoreManager.AddScore(level);

        enabled = false;
    }
    // Player의 다음 움직임 확인

     // 모든 Enemy가 한번에 이동하도록 하는 코루틴 함수
    IEnumerator CoroutineExample()
    {
        doingCoroutine = true;
        yield return new WaitForSeconds(turnDelay);
        yield return new WaitForSeconds(turnDelay);

        playersTurn = true;
        doingCoroutine = false;
    }
    
    // 특정 위치가 적에 의해 점유되었는지 확인하는 메서드
    public bool IsPositionOccupied(Vector2 position)
    {
        return occupiedPositions.Contains(position);
    }

    // 다음 레벨로 넘어가는 함수, Player가 호출함
    public void NextLevel()
    {
        isInitialized = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single); // 마지막으로 로드된 Scene을 다시 로드함.
    }

    // 레벨 1부터 재시작하는 함수
    public void RestartGame()
    {
        enabled = true;     // 꺼진 GameManager 오브젝트 다시 활성화
        instance.level = 0; // 레벨 초기화

        NextLevel();        // Scene 새로 불러오기
    }

    public void Pause()
    {
        if (uiManager.IsPaused())
        {
            enabled = true;
            Time.timeScale = 1;
        }
        else
        {  
            enabled = false;
            Time.timeScale = 0;
        }
    }

    // ShoppingButton이 실행해서 상점창을 여는 함수
    public void OpenShop()
    {
        if (uiManager.ToggleShop())
        {
            shopping = false;
        }
        else
        { 
            shopping = true; 
        }
    }
}
