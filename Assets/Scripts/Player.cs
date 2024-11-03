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
    public int wallDamage = 1;              // 1ȸ ���ݴ� Wall�� ���ϴ� ������
    public int pointsPerFood = 10;          // ������ �Ծ��� �� ������ ������
    public int pointsPerSoda = 15;          // �Ҵٸ� �Ծ��� �� ������ ������
    public int pointsPerPoison = -3;        // ���� �ߵ��Ǿ��� �� �����ϴ� ������

    public float restartLevelDelay = 1f;    // ���� ���������� �Ѿ�� ���� �ð�
    public Text foodText;                   // FoodText�� ���۷��� �����ϴ� ����
    public Color poisonColor = new Color(0.5f, 0, 0.5f, 1f); // �� �ߵ� ������ ���� �÷��̾� ���� (�����)

    // Player�� ����� Ŭ����
    public AudioClip moveSound1;
    public AudioClip moveSound2;
    public AudioClip eatSound1;
    public AudioClip eatSound2;
    public AudioClip drinkSound1;
    public AudioClip drinkSound2;
    public AudioClip gameOverSound;

    /* private ���� */
    private Animator animator;              // Animator ���۷����� ������ ����
    private int food;                       // �ش� �������� ������ �÷��̾� ����� ��ġ
    private bool isPoisoned = false;        // �÷��̾��� �� �ߵ� ���¸� ��Ÿ���� ����
    private int poisonedMovesLeft = 0;      // �� ȿ���� ���� �̵� Ƚ��
    private SpriteRenderer spriteRenderer;  // �÷��̾��� SpriteRenderer ������Ʈ
    private Color originalColor;            // �÷��̾��� ���� ����

    private int dice = 0;


    /* ����Ƽ API �Լ��� */
    protected override void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        food = GameManager.instance.playerFoodPoints;

        foodText.text = "DICE: " + dice;

        base.Start();                                   // �θ� Ŭ������ Start�Լ� ȣ��
    }

    // ������Ʈ�� ��Ȱ��ȭ�� �� ȣ��Ǵ� �Լ�
    private void OnDisable()
    {
        GameManager.instance.playerFoodPoints = food;   // �̹� ���������� ���� ����� ��ġ�� GameManager�� ������
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

        //if (horizontal != 0)                                // ���� �������� �������ٸ�
        //    vertical = 0;                                   // ���� �������� 0���� ���ϱ�(�밢������ �������� �ʰ� �ϱ� ����)

        if (horizontal != 0 || vertical != 0)               // ���� �������ٸ�
            AttemptMove<Wall>(horizontal, vertical);         // ��ȣ�ۿ��ϴ� ������Ʈ�� Wall�� �־ �̵� �õ��ϱ�

    }   

// Soda, Food, Exit���� �浹�� Ȯ���ϴ� �Լ�
private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.tag == "Exit")
        {                                                                       // ���� Exit�� �浹�ߴٸ�
            if (food > 0)
            {
            Invoke("Restart", restartLevelDelay); // ���� �ð� �� ���� ���������� �̵�
            enabled = false;
            }
        }
    }
    /* �������̵��� �Լ��� */
    // �̵��� �� ������ �̵��ϰ�, �̵��� �� ������ OnCantMove�� �����ϴ� �Լ�
    protected override bool AttemptMove<T>(int xDir, int yDir)
    {
        //food--;                                                         // 1ȸ �̵� �õ��� ������ 1 ����
        //foodText.text = "DICE: " + dice;                                // FoodText UI �ֽ�ȭ

        // �̵��� �ִϸ��̼�
        bool isMovable = base.AttemptMove<T>(xDir, yDir);               // �θ� Ŭ������ �Լ� ȣ��
        if (isMovable)
            animator.SetTrigger("playerWalk");                          

        // Wall�� �ν��� ���� �ٷ� �̵��ϰ� �Ҹ� ����ϱ�
        RaycastHit2D hit;                                               // Move�Լ����� �浹Ȯ�� ����� ������ ����
        if (Move(xDir, yDir, out hit))                                  // �̵� ���� ���� Ȯ���ϰ� �̵��ϱ�
            SoundManager.instance.RandomizeSfx(moveSound1, moveSound2); // �̵� ������ �Ҹ� ����ϱ�


        CheckIfGameOver();

        GameManager.instance.playersTurn = false;

        return isMovable;
    }

    // �̵��Ϸ��� ��ġ�� ��ȣ�ۿ��� �� �ִ� ������Ʈ�� ���� �� ����Ǵ� �Լ�
    protected override void OnCantMove<T>(T component)
    {
        Wall hitWall = component as Wall;           // �Է¹��� T�� ������ Wall Ÿ������ Ÿ��ĳ�����ϱ�
        hitWall.DamageWall(wallDamage);             // �浹�� ���� wallDamage��ŭ ���� �ֱ�

        animator.SetTrigger("playerChop");          // �÷��̾�� PlayerChop �ִϸ��̼� �����Ű��
    }

    /* private �Լ��� */
    // Player�� Exit�� �浹���� �� ȣ��Ǿ� ���� Scene�� ������ϴ� �Լ�
    private void Restart()
    {
        //Application.LoadLevel(Application.loadedLevel); 
        // �� �ڵ�� ���� ������ ������ �ȵǾ �Ʒ� �ڵ�� ��ü��
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

    /* public �Լ��� */
    // Enemy�� Player�� �浹���� �� ����Ǵ� �Լ�
    public void LoseFood(int loss)
    {
        animator.SetTrigger("playerHit");               // PlayerHit �ִϸ��̼� �����Ű��
        food -= loss;                                   // loss��ŭ food ����
        foodText.text = "-" + loss + " HP: " + food;   // FoodText UI �ֽ�ȭ

        CheckIfGameOver();

    }

    // ü���� ȸ���ϴ� �޼���
    public void RecoverHealth(int amount)
    {
        food += amount;
        foodText.text = "+" + amount + " HP: " + food; // ü�� ȸ�� UI�� ǥ��
    }


}
