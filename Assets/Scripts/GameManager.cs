using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    /* public ���� */
    public static GameManager instance = null;          // GameManager�� �̱��� �������� ���� �� ����� ����
    public BoardManager boardScript;                    // ���������� ����� ������Ʈ
    public UIManager uiManager;                         // UI ����ϴ� Ŭ����
    public ScoreManager scoreManager;                   // ������ ����ϴ� Ŭ����
    public float levelStartDelay = 0.2f;                  // ������ ���۵Ǳ� ���� �ʴ����� ����� �ð�
    // Player�� ������
    public int playerFoodPoints = 100;                  // �÷��̾� ������
    [HideInInspector] public bool playersTurn = true;   // 
    public int nowPlayer = 1;
    // Enemy�� ������
    public float turnDelay = 0.1f;                      // �� �� ���� ���ð�

    /* private ���� */
    private int level = 0;                      // 
    private List<Enemy> enemies;                // ���������� ��� Enemy ������Ʈ�� ������ ����
    private bool enemiesMoving;                 //
    private bool isInitialized = false;         // InitGame() �Լ��� ȣ��Ǿ����� Ȯ���ϴ� �Լ�
    private bool doingSetup;                    // ���� ���带 ����� ������ Ȯ���ϴ� ����

    private HashSet<Vector2> occupiedPositions = new HashSet<Vector2>(); // ������ ���� �����ϰ� �ִ� ��ġ�� �����ϴ� HashSet

    /* ����Ƽ API �Լ��� */
    void Awake()
    {
        // �� ���� GameManager�� ������ �ʰ� �ϱ�
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);                              // ���� Scene���� �Ѿ�� GameManager�� �������� �ʰ� �ϱ�

        enemies = new List<Enemy>();
        boardScript = GetComponent<BoardManager>();
        uiManager = GetComponent<UIManager>();
        scoreManager = GetComponent<ScoreManager>();

        scoreManager.LoadScores();                                  // scores ������ ���� ���� �����ϱ�

        playerFoodPoints = 100;
        if (SceneManager.GetActiveScene().name == "MainScene")      // TitleScene�� ��ġ�� �ʰ� �����Ѵٸ�
        {
            instance.level = 1;
            InitGame();                                             // �������� ����
        }
    }

    /*// �� ������ �ε�� ������ InitGame �Լ� ȣ���ϴ� �Լ�
    private void OnLevelWasLoaded(int index)
    {
        level++;

        InitGame();

    }*/
    void Update()
    {
        if (playersTurn || enemiesMoving || doingSetup)
            return;

        StartCoroutine(MoveEnemies());
    }

    // Scene �Ѿ�� �� ���̴� �Լ���
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static public void CallbackInitialization()
    {
        //register the callback to be called everytime the scene is loaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    //This is called each time a scene is loaded.
    static private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        if (instance.isInitialized)         // ���� �̹� InitGame()�� ����Ǿ��ٸ�
            return;                             // InitGame() �������� �ʰ� �Ѿ��
        if (instance.level == 0)            // ���� ������� ���̶��
            instance.playerFoodPoints = 100;    // �÷��̾� ü���� 100���� �����

        instance.level++;
        instance.InitGame();
    }

    /* private �Լ��� */
    // BoardManager�� ���� ���������� �����ϴ� �Լ�
    void InitGame()
    {
        doingSetup = true;                                              // �÷��̾ �� �ε�� ���� �� �����̰� �ϱ�
        Time.timeScale = 1;
        // UI ����
        uiManager.InitMainUI(level);
        Invoke("HideLevelImage", levelStartDelay);                      // levelStartDelay��ŭ ��ٸ��� ���� ���� ����

        enemies.Clear();
        boardScript.SetupScene(level);
        isInitialized = true;
    }

    // ������ �� �ε�Ǹ� LevelImage UI ���� �Լ�
    private void HideLevelImage()
    {
        uiManager.HideLevelImage();
        //levelImage.SetActive(false);
        doingSetup = false;
    }

    // Player�� ���� �̵��� Ÿ�� Ȯ���ϱ�
    public GameObject GetNextMove(int n = 1)
    {
        nowPlayer += n;
        return boardScript.floors[nowPlayer];
    }
    public void AddEnemyToList(Enemy script)
    {
        enemies.Add(script);
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

        return floorCount;
    }

    // ���� ������ Player�� ���ؼ� ȣ��Ǿ� GameManager�� ��Ȱ��ȭ��Ű�� �Լ�
    public void GameOver()
    {
        uiManager.ShowGameOver(level);
        scoreManager.AddScore(level);

        enabled = false;
    }
    // Player�� ���� ������ Ȯ��

     // ��� Enemy�� �ѹ��� �̵��ϵ��� �ϴ� �ڷ�ƾ �Լ�
    IEnumerator MoveEnemies()
    {
        enemiesMoving = true;
        occupiedPositions.Clear();  // �� �ϸ��� ������ ��ġ �ʱ�ȭ

        yield return new WaitForSeconds(turnDelay);
        if (enemies.Count == 0)
            yield return new WaitForSeconds(turnDelay);

        for (int i = 0; i < enemies.Count; i++)
        {
            // ���� ���� ��ġ�� ���� �̵��� ��ġ ���
            Vector2 currentPosition = enemies[i].transform.position;
            Vector2 newPosition = enemies[i].GetNextMove();

            // �� ��ġ�� �ٸ� ���� ���� �������� �ʾҴٸ� �̵� ����
            if (!occupiedPositions.Contains(newPosition))
            {
                occupiedPositions.Remove(currentPosition);  // ���� ��ġ���� ����
                occupiedPositions.Add(newPosition);         // �� ��ġ �߰�
                enemies[i].MoveEnemy();                     // �� �̵� ����
            }
            // �׷��� ������ �̵����� ���� (���� ��ġ ����)

            yield return new WaitForSeconds(enemies[i].moveTime);
        }

        playersTurn = true;
        enemiesMoving = false;
    }
    
    // Ư�� ��ġ�� ���� ���� �����Ǿ����� Ȯ���ϴ� �޼���
    public bool IsPositionOccupied(Vector2 position)
    {
        return occupiedPositions.Contains(position);
    }

    // ���� ������ �Ѿ�� �Լ�, Player�� ȣ����
    public void NextLevel()
    {
        isInitialized = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single); // ���������� �ε�� Scene�� �ٽ� �ε���.
    }

    // ���� 1���� ������ϴ� �Լ�
    public void RestartGame()
    {
        enabled = true;     // ���� GameManager ������Ʈ �ٽ� Ȱ��ȭ
        instance.level = 0; // ���� �ʱ�ȭ

        NextLevel();        // Scene ���� �ҷ�����
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
}
