using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public Sprite dmgSprite;
    public Sprite dmgSprite2;
    public int hp = 3;
    public int recoverAmount = 5;  // ȸ����

    private SpriteRenderer spriteRenderer;
    private Player player;  // �÷��̾� ����

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();  // �÷��̾� ã��
    }

    public void DamageWall(int loss)
    {
        if (spriteRenderer.sprite != dmgSprite)
            spriteRenderer.sprite = dmgSprite;
        else
            spriteRenderer.sprite = dmgSprite2;
        hp -= loss;
        if (hp <= 0)
        {
            gameObject.SetActive(false);

            // 50% Ȯ���� ü�� ȸ��
            if (Random.value < 0.5f)
            {
                player.RecoverHealth(recoverAmount);
            }
        }
    }
}