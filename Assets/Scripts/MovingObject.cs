using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Player와 Enemy가 상속받는 추상 클래스
public abstract class MovingObject : MonoBehaviour
{
    /* public 변수 */
    public float moveTime = 0.1f;       // 오브젝트가 움직이는 시간 단위
    public LayerMask blockingLayer;     // 움직이는 공간에서 충돌이 일어나는지 체크할 장소
    public bool canMove;                // Enemy에서 텍스트를 움직이기 위해 public으로 선언함

    /* private 변수 */
    private BoxCollider2D boxCollider;  // 움직일 유닛의 BoxCollider2D 컴포넌트
    private Rigidbody2D rb2D;           // 움직일 유닛의 RigidBody2D 컴포넌트
    private float inverseMoveTime;      // 움직임 계산시 연산을 줄이기 위해서 moveTime의 역수를 저장함
    private bool isMoving;

    protected virtual void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        inverseMoveTime = 1f / moveTime;
    }

    // 이동할 수 있으면 이동하고 true를 반환, 이동할 수 없으면 false를 반환하는 함수
    protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
    {
        Vector2 start = transform.position;                     // 현재 위치를 2차원 벡터로 변환
        Vector2 end = start + new Vector2(xDir, yDir);          // 도착 위치

        boxCollider.enabled = false;                            // Ray를 사용할 때 자신에게 부딪히지 않도록 잠깐만 BoxCollider2D 비활성화
        hit = Physics2D.Linecast(start, end, blockingLayer);    // 시작점과 도착점 사이에 BlockingLayer 레이어가 있는지 확인해서 저장
        boxCollider.enabled = true;

        if (hit.transform == null && !isMoving)                 // 이동할 수 있다면
        {
            StartCoroutine(SmoothMovement(end));                // 코루틴으로 유닛 이동시키기
            return true;                                        // 이동 성공
        }
        else
            return false;                                       // 이동 실패
    }

    // 유닛을 한 공간에서 end로 이동시키는 코루틴 함수
    protected IEnumerator SmoothMovement(Vector3 end)
    {
        isMoving = true;
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;   // 이동할 거리 계산(Vector3.sqrMagintude : 벡터 길이의 제곱)

        while (sqrRemainingDistance > float.Epsilon)                            // 시작점과 도착점 사이 거리의 제곱이 0보다 클 때
        {
            // 현재 위치에서 end까지 moveTime만큼 가까위진 위치로 이동시키기
            Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
            rb2D.MovePosition(newPosition);

            sqrRemainingDistance = (transform.position - end).sqrMagnitude;     // 거리 다시 계산하기

            yield return null;                                                  // 다음 프레임에서 루프 실행하기
        }
        rb2D.MovePosition(end);
        isMoving = false;
    }

    // 이동할 수 있으면 이동하고, 이동할 수 없으면 OnCantMove를 실행하는 함수
    protected virtual bool AttemptMove<T>(int xDir, int yDir)
        where T : Component
    {
        RaycastHit2D hit;                                   // 현재 위치에서 (xDir, yDir)만큼 이동할 동안 충돌을 판정
        canMove = Move(xDir, yDir, out hit);                // Move함수의 결과가 hit에도 저장됨
        if (hit.transform == null)                          // 이동에 성공했을 때(충돌하지 않았을 때)
            return true;

        T hitComponent = hit.transform.GetComponent<T>();
        if (!canMove && hitComponent != null)               // 이동할 수 없고, 이동하려는 위치에 상호작용할 수 있는 오브젝트가 있을 때
            OnCantMove<T>(hitComponent);
        return false;
    }

    // 이동하려는 위치에 상호작용할 수 있는 오브젝트가 있을 때 실행되는 추상 함수
    protected abstract void OnCantMove<T>(T component)
        where T : Component;
}
