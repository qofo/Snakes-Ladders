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
    public float restartLevelDelay = 1f;    // 다음 스테이지로 넘어가는 지연 시간
    public Text diceText;                   // diceText의 레퍼런스 저장하는 변수

    // Player의 오디오 클립들
    public AudioClip moveSound1;
    public AudioClip moveSound2;
    public AudioClip gameOverSound;

    /* private 변수 */
    private Animator animator;              // Animator 레퍼런스를 저장할 변수
    private SpriteRenderer spriteRenderer;  // 플레이어의 SpriteRenderer 컴포넌트

    private int dice = 0;


    /* 유니티 API 함수들 */
    protected override void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        diceText = GameObject.Find("FoodText").GetComponent<Text>();

        diceText.text = "DICE: " + dice;

        base.Start();                                   // 부모 클래스의 Start함수 호출
    }

    // 오브젝트가 비활성화될 때 호출되는 함수
    /*private void OnDisable()
    {
        // 나중에 맵 넘어갈 때 실행할 거 있으면 여기서 실행
        return;
    }*/

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

        if (Input.GetKeyDown(KeyCode.Space))
        {
//
// 주사위 랜덤 설정
//
            dice = Random.Range(1, 7);
            diceText.text = "DICE: " + dice;
        }

        if (dice > 0)
        {
            dice--;
            GameObject nextTile = GameManager.instance.GetNextMove(1);
            if (nextTile == GameManager.instance.boardScript.bossTile)
            {
                dice = 0;

            }
            else if (nextTile == GameManager.instance.boardScript.shopTile)
            {
                dice = 0;
            }

            movement = nextTile.transform.position - transform.position;
            horizontal = (int)movement.x;
            vertical = (int)movement.y;
            spriteRenderer.flipX = horizontal < 0;
        }
        else if (dice <= 0)
        {
            int nextPos = GameManager.instance.CheckLadder(transform.position);
            if (nextPos != -1)      // 만약 주사위만큼 이동 끝난 위치가 사다리 or 뱀일 때
            {
                GameManager.instance.SetNextMove(nextPos);
                GameManager.instance.GetNextMove();
                movement = GameManager.instance.GetNextMove(1).transform.position - transform.position;
                horizontal = (int)movement.x;
                vertical = (int)movement.y;
            }
        }

        if (horizontal != 0 || vertical != 0)                         // 만약 움직였다면
            AttemptMove<MonoBehaviour>(horizontal, vertical);         // 상호작용하는 오브젝트를 Wall로 주어서 이동 시도하기

    }   

// 안 쓰는 코드인데 나중에 쓰일까봐 남겨둠
// Exit(가칭)과의 충돌을 확인하는 함수
/*  private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.tag == "Exit")
        {                                         // 만약 Exit와 충돌했다면
            Invoke("Restart", restartLevelDelay); // 지연 시간 후 다음 스테이지로 이동
            enabled = false;
        }
    }*/
    /* 오버라이딩한 함수들 */
    // 이동할 수 있으면 이동하고, 이동할 수 없으면 OnCantMove를 실행하는 함수
    protected override bool AttemptMove<T>(int xDir, int yDir)
    {

        // 이동시 애니메이션
        bool isMovable = base.AttemptMove<T>(xDir, yDir);               // 부모 클래스의 함수 호출
        if (isMovable)
            animator.SetTrigger("playerWalk");                          

        // Wall을 부쉈을 때는 바로 이동하고 소리 재생하기
        RaycastHit2D hit;                                               // Move함수에서 충돌확인 결과를 가져올 변수
        if (Move(xDir, yDir, out hit))                                  // 이동 가능 여부 확인하고 이동하기
            SoundManager.instance.RandomizeSfx(moveSound1, moveSound2); // 이동 했으면 소리 재생하기


        //CheckIfGameOver();

        GameManager.instance.playersTurn = false;

        return isMovable;
    }

    // 이동하려는 위치에 상호작용할 수 있는 오브젝트가 있을 때 실행되는 함수
    protected override void OnCantMove<T>(T component)
    {
        return;
    }

    /* private 함수들 */

    private void CheckIfGameOver()
    {
        SoundManager.instance.PlaySingle(gameOverSound);
        SoundManager.instance.musicSource.Stop();

        enabled = false;
        GameManager.instance.GameOver();
    }

    /* public 함수들 */


}
