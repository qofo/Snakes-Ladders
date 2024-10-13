using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public Sprite dmgSprite;
    public Sprite dmgSprite2;
    public int hp = 3;
    public int recoverAmount = 5;  // 회복량

    private SpriteRenderer spriteRenderer;
    private Player player;  // 플레이어 참조

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();  // 플레이어 찾기
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

            // 50% 확률로 체력 회복
            if (Random.value < 0.5f)
            {
                player.RecoverHealth(recoverAmount);
            }
        }
    }
}