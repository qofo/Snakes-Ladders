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
    /* public ���� */
    public float restartLevelDelay = 1f;    // ���� ���������� �Ѿ�� ���� �ð�
    public Text diceText;                   // diceText�� ���۷��� �����ϴ� ����

    // Player�� ����� Ŭ����
    public AudioClip moveSound1;
    public AudioClip moveSound2;
    public AudioClip gameOverSound;

    /* private ���� */
    private Animator animator;              // Animator ���۷����� ������ ����
    private SpriteRenderer spriteRenderer;  // �÷��̾��� SpriteRenderer ������Ʈ

    private int dice = 0;


    /* ����Ƽ API �Լ��� */
    protected override void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        diceText = GameObject.Find("FoodText").GetComponent<Text>();

        diceText.text = "DICE: " + dice;

        base.Start();                                   // �θ� Ŭ������ Start�Լ� ȣ��
    }

    // ������Ʈ�� ��Ȱ��ȭ�� �� ȣ��Ǵ� �Լ�
    /*private void OnDisable()
    {
        // ���߿� �� �Ѿ �� ������ �� ������ ���⼭ ����
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
// �ֻ��� ���� ����
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
            if (nextPos != -1)      // ���� �ֻ�����ŭ �̵� ���� ��ġ�� ��ٸ� or ���� ��
            {
                GameManager.instance.SetNextMove(nextPos);
                GameManager.instance.GetNextMove();
                movement = GameManager.instance.GetNextMove(1).transform.position - transform.position;
                horizontal = (int)movement.x;
                vertical = (int)movement.y;
            }
        }

        if (horizontal != 0 || vertical != 0)                         // ���� �������ٸ�
            AttemptMove<MonoBehaviour>(horizontal, vertical);         // ��ȣ�ۿ��ϴ� ������Ʈ�� Wall�� �־ �̵� �õ��ϱ�

    }   

// �� ���� �ڵ��ε� ���߿� ���ϱ�� ���ܵ�
// Exit(��Ī)���� �浹�� Ȯ���ϴ� �Լ�
/*  private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.tag == "Exit")
        {                                         // ���� Exit�� �浹�ߴٸ�
            Invoke("Restart", restartLevelDelay); // ���� �ð� �� ���� ���������� �̵�
            enabled = false;
        }
    }*/
    /* �������̵��� �Լ��� */
    // �̵��� �� ������ �̵��ϰ�, �̵��� �� ������ OnCantMove�� �����ϴ� �Լ�
    protected override bool AttemptMove<T>(int xDir, int yDir)
    {

        // �̵��� �ִϸ��̼�
        bool isMovable = base.AttemptMove<T>(xDir, yDir);               // �θ� Ŭ������ �Լ� ȣ��
        if (isMovable)
            animator.SetTrigger("playerWalk");                          

        // Wall�� �ν��� ���� �ٷ� �̵��ϰ� �Ҹ� ����ϱ�
        RaycastHit2D hit;                                               // Move�Լ����� �浹Ȯ�� ����� ������ ����
        if (Move(xDir, yDir, out hit))                                  // �̵� ���� ���� Ȯ���ϰ� �̵��ϱ�
            SoundManager.instance.RandomizeSfx(moveSound1, moveSound2); // �̵� ������ �Ҹ� ����ϱ�


        //CheckIfGameOver();

        GameManager.instance.playersTurn = false;

        return isMovable;
    }

    // �̵��Ϸ��� ��ġ�� ��ȣ�ۿ��� �� �ִ� ������Ʈ�� ���� �� ����Ǵ� �Լ�
    protected override void OnCantMove<T>(T component)
    {
        return;
    }

    /* private �Լ��� */

    private void CheckIfGameOver()
    {
        SoundManager.instance.PlaySingle(gameOverSound);
        SoundManager.instance.musicSource.Stop();

        enabled = false;
        GameManager.instance.GameOver();
    }

    /* public �Լ��� */


}
