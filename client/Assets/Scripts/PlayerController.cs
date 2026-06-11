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

        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.playerTransform = this.transform;
        }
    }

    void Update()
    {
        // 1. 매 프레임마다 이동 방향을 일단 0으로 초기화합니다.
        movement.x = 0f;
        movement.y = 0f;

        // 2. 오직 화살표 키(Arrow)가 눌렸을 때만 movement 값을 변경합니다.
        if (Input.GetKey(KeyCode.RightArrow)) movement.x = 1f;
        else if (Input.GetKey(KeyCode.LeftArrow)) movement.x = -1f;

        if (Input.GetKey(KeyCode.UpArrow)) movement.y = 1f;
        else if (Input.GetKey(KeyCode.DownArrow)) movement.y = -1f;
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