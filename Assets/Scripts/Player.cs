using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Player : MovingObject
{
    /* public 변수 */
    public int wallDamage = 1;              // 1회 공격당 Wall에 가하는 데미지
    public int pointsPerFood = 10;          // 음식을 먹었을 때 오르는 포만감
    public int pointsPerSoda = 15;          // 소다를 먹었을 때 오르는 포만감
    public int pointsPerPoison = -3;        // 독에 중독되었을 때 감소하는 포만감

    public float restartLevelDelay = 1f;    // 다음 스테이지로 넘어가는 지연 시간
    public Text foodText;                   // FoodText의 레퍼런스 저장하는 변수
    public Color poisonColor = new Color(0.5f, 0, 0.5f, 1f); // 독 중독 상태일 때의 플레이어 색상 (보라색)

    // Player의 오디오 클립들
    public AudioClip moveSound1;
    public AudioClip moveSound2;
    public AudioClip eatSound1;
    public AudioClip eatSound2;
    public AudioClip drinkSound1;
    public AudioClip drinkSound2;
    public AudioClip gameOverSound;

    /* private 변수 */
    private Animator animator;              // Animator 레퍼런스를 저장할 변수
    private int food;                       // 해당 스테이지 동안의 플레이어 배고픔 수치
    private bool isPoisoned = false;        // 플레이어의 독 중독 상태를 나타내는 변수
    private int poisonedMovesLeft = 0;      // 독 효과가 남은 이동 횟수
    private SpriteRenderer spriteRenderer;  // 플레이어의 SpriteRenderer 컴포넌트
    private Color originalColor;            // 플레이어의 원래 색상

    private int dice = 0;


    /* 유니티 API 함수들 */
    protected override void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        food = GameManager.instance.playerFoodPoints;

        foodText.text = "DICE: " + dice;

        base.Start();                                   // 부모 클래스의 Start함수 호출
    }

    // 오브젝트가 비활성화될 때 호출되는 함수
    private void OnDisable()
    {
        GameManager.instance.playerFoodPoints = food;   // 이번 스테이지의 최종 배고픔 수치를 GameManager에 저장함
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //GameManager.instance.uiManager.IsPaused();
            GameManager.instance.Pause();
            return;
        }
        if (!GameManager.instance.playersTurn || !GameManager.instance.enabled) return;

        int horizontal = 0;
        int vertical = 0;
        
        Vector2 movement = Vector2.zero;
        //horizontal = (int)Input.GetAxisRaw("Horizontal");
        //vertical = (int)Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            dice = Random.Range(1, 6);
            foodText.text = "DICE: " + dice;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
        {
            horizontal = -1;
            spriteRenderer.flipX = true;
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
        {
            horizontal = 1;
            spriteRenderer.flipX = false;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            vertical = 1;
        else if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            vertical = -1;

        if (dice > 0)
        {
            dice--;
            movement = GameManager.instance.GetNextMove(1).transform.position - transform.position;
            horizontal = (int)movement.x;
            vertical = (int)movement.y;
        }

        //if (horizontal != 0)                                // 만약 수평으로 움직였다면
        //    vertical = 0;                                   // 수직 움직임을 0으로 정하기(대각선으로 움직이지 않게 하기 위함)

        if (horizontal != 0 || vertical != 0)               // 만약 움직였다면
            AttemptMove<Wall>(horizontal, vertical);         // 상호작용하는 오브젝트를 Wall로 주어서 이동 시도하기

    }   

// Soda, Food, Exit과의 충돌을 확인하는 함수
private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.tag == "Exit")
        {                                                                       // 만약 Exit와 충돌했다면
            if (food > 0)
            {
            Invoke("Restart", restartLevelDelay); // 지연 시간 후 다음 스테이지로 이동
            enabled = false;
            }
        }
    }
    /* 오버라이딩한 함수들 */
    // 이동할 수 있으면 이동하고, 이동할 수 없으면 OnCantMove를 실행하는 함수
    protected override bool AttemptMove<T>(int xDir, int yDir)
    {
        //food--;                                                         // 1회 이동 시도시 포만감 1 감소
        //foodText.text = "DICE: " + dice;                                // FoodText UI 최신화

        // 이동시 애니메이션
        bool isMovable = base.AttemptMove<T>(xDir, yDir);               // 부모 클래스의 함수 호출
        if (isMovable)
            animator.SetTrigger("playerWalk");                          

        // Wall을 부쉈을 때는 바로 이동하고 소리 재생하기
        RaycastHit2D hit;                                               // Move함수에서 충돌확인 결과를 가져올 변수
        if (Move(xDir, yDir, out hit))                                  // 이동 가능 여부 확인하고 이동하기
            SoundManager.instance.RandomizeSfx(moveSound1, moveSound2); // 이동 했으면 소리 재생하기


        CheckIfGameOver();

        GameManager.instance.playersTurn = false;

        return isMovable;
    }

    // 이동하려는 위치에 상호작용할 수 있는 오브젝트가 있을 때 실행되는 함수
    protected override void OnCantMove<T>(T component)
    {
        Wall hitWall = component as Wall;           // 입력받은 T형 변수를 Wall 타입으로 타입캐스팅하기
        hitWall.DamageWall(wallDamage);             // 충돌한 벽에 wallDamage만큼 피해 주기

        animator.SetTrigger("playerChop");          // 플레이어에게 PlayerChop 애니메이션 실행시키기
    }

    /* private 함수들 */
    // Player가 Exit랑 충돌했을 때 호출되어 현재 Scene을 재시작하는 함수
    private void Restart()
    {
        //Application.LoadLevel(Application.loadedLevel); 
        // 위 코드는 버전 문제로 실행이 안되어서 아래 코드로 대체함
        GameManager.instance.NextLevel();
    }

    private void CheckIfGameOver()
    {
        if (food <= 0)
        {
            SoundManager.instance.PlaySingle(gameOverSound);
            SoundManager.instance.musicSource.Stop();

            enabled = false;
            GameManager.instance.GameOver();
        }
    }

    /* public 함수들 */
    // Enemy가 Player와 충돌했을 때 실행되는 함수
    public void LoseFood(int loss)
    {
        animator.SetTrigger("playerHit");               // PlayerHit 애니메이션 실행시키기
        food -= loss;                                   // loss만큼 food 감수
        foodText.text = "-" + loss + " HP: " + food;   // FoodText UI 최신화

        CheckIfGameOver();

    }

    // 체력을 회복하는 메서드
    public void RecoverHealth(int amount)
    {
        food += amount;
        foodText.text = "+" + amount + " HP: " + food; // 체력 회복 UI에 표시
    }


}
