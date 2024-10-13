using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Player�� Enemy�� ��ӹ޴� �߻� Ŭ����
public abstract class MovingObject : MonoBehaviour
{
    /* public ���� */
    public float moveTime = 0.1f;       // ������Ʈ�� �����̴� �ð� ����
    public LayerMask blockingLayer;     // �����̴� �������� �浹�� �Ͼ���� üũ�� ���
    public bool canMove;                // Enemy���� �ؽ�Ʈ�� �����̱� ���� public���� ������

    /* private ���� */
    private BoxCollider2D boxCollider;  // ������ ������ BoxCollider2D ������Ʈ
    private Rigidbody2D rb2D;           // ������ ������ RigidBody2D ������Ʈ
    private float inverseMoveTime;      // ������ ���� ������ ���̱� ���ؼ� moveTime�� ������ ������
    private bool isMoving;

    protected virtual void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        inverseMoveTime = 1f / moveTime;
    }

    // �̵��� �� ������ �̵��ϰ� true�� ��ȯ, �̵��� �� ������ false�� ��ȯ�ϴ� �Լ�
    protected bool Move(int xDir, int yDir, out RaycastHit2D hit)
    {
        Vector2 start = transform.position;                     // ���� ��ġ�� 2���� ���ͷ� ��ȯ
        Vector2 end = start + new Vector2(xDir, yDir);          // ���� ��ġ

        boxCollider.enabled = false;                            // Ray�� ����� �� �ڽſ��� �ε����� �ʵ��� ��� BoxCollider2D ��Ȱ��ȭ
        hit = Physics2D.Linecast(start, end, blockingLayer);    // �������� ������ ���̿� BlockingLayer ���̾ �ִ��� Ȯ���ؼ� ����
        boxCollider.enabled = true;

        if (hit.transform == null && !isMoving)                 // �̵��� �� �ִٸ�
        {
            StartCoroutine(SmoothMovement(end));                // �ڷ�ƾ���� ���� �̵���Ű��
            return true;                                        // �̵� ����
        }
        else
            return false;                                       // �̵� ����
    }

    // ������ �� �������� end�� �̵���Ű�� �ڷ�ƾ �Լ�
    protected IEnumerator SmoothMovement(Vector3 end)
    {
        isMoving = true;
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;   // �̵��� �Ÿ� ���(Vector3.sqrMagintude : ���� ������ ����)

        while (sqrRemainingDistance > float.Epsilon)                            // �������� ������ ���� �Ÿ��� ������ 0���� Ŭ ��
        {
            // ���� ��ġ���� end���� moveTime��ŭ �������� ��ġ�� �̵���Ű��
            Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
            rb2D.MovePosition(newPosition);

            sqrRemainingDistance = (transform.position - end).sqrMagnitude;     // �Ÿ� �ٽ� ����ϱ�

            yield return null;                                                  // ���� �����ӿ��� ���� �����ϱ�
        }
        rb2D.MovePosition(end);
        isMoving = false;
    }

    // �̵��� �� ������ �̵��ϰ�, �̵��� �� ������ OnCantMove�� �����ϴ� �Լ�
    protected virtual bool AttemptMove<T>(int xDir, int yDir)
        where T : Component
    {
        RaycastHit2D hit;                                   // ���� ��ġ���� (xDir, yDir)��ŭ �̵��� ���� �浹�� ����
        canMove = Move(xDir, yDir, out hit);                // Move�Լ��� ����� hit���� �����
        if (hit.transform == null)                          // �̵��� �������� ��(�浹���� �ʾ��� ��)
            return true;

        T hitComponent = hit.transform.GetComponent<T>();
        if (!canMove && hitComponent != null)               // �̵��� �� ����, �̵��Ϸ��� ��ġ�� ��ȣ�ۿ��� �� �ִ� ������Ʈ�� ���� ��
            OnCantMove<T>(hitComponent);
        return false;
    }

    // �̵��Ϸ��� ��ġ�� ��ȣ�ۿ��� �� �ִ� ������Ʈ�� ���� �� ����Ǵ� �߻� �Լ�
    protected abstract void OnCantMove<T>(T component)
        where T : Component;
}
