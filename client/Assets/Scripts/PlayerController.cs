using System.Threading;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float attackRange = 1.5f; // 공격 사거리
    public LayerMask monsterLayer;   // 몬스터만 인식하기 위한 레이어

    private Rigidbody2D rb;
    private Vector2 movement;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // WASD 이동 입력 받기
        movement.x = Input.GetAxisRaw("Horizontal");
        movement.y = Input.GetAxisRaw("Vertical");
    }

    void FixedUpdate()
    {
        // 물리 엔진을 이용한 부드러운 이동
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    void OnEnable()
    {
        // InputManager의 공격 신호 구독
        InputManager.OnAttackPressed += Attack;
    }

    void OnDisable()

    {
        InputManager.OnAttackPressed -= Attack;
    }

    void Attack()
    {
        // 내 위치를 기준으로 동그란 범위(attackRange) 안의 몬스터(버섯) 찾기
        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackRange, monsterLayer);

        if (hit != null)
        {
            // 찾은 몬스터에게 데미지 주기
            Monster target = hit.GetComponent<Monster>();
            if (target != null)
            {
                target.TakeDamage(GameManager.Instance.tapDamage);
            }
        }
    }
}