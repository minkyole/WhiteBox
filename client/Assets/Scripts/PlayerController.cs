using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float attackRange = 1.5f;
    public LayerMask monsterLayer;

    private Rigidbody2D rb;
    private Vector2 movement;
    
    // 🌟 추가된 컴포넌트
    private Animator anim;
    private SpriteRenderer sr;
    private Vector2 lastMoveDir = new Vector2(0, -1); // 캐릭터가 멈췄을 때 바라볼 기본 방향 (아래)

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();         // 애니메이터 가져오기
        sr = GetComponent<SpriteRenderer>();     // 스프라이트 렌더러 가져오기

        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.playerTransform = this.transform;
        }
    }

    void Update()
    {
        if (GachaHouse.isUIOpen)
        {
            movement = Vector2.zero;
            anim.SetFloat("Speed", 0f); // UI 열리면 걷기 애니메이션 강제 정지
            return;
        }

        movement.x = 0f;
        movement.y = 0f;

        if (Input.GetKey(KeyCode.RightArrow)) movement.x = 1f;
        else if (Input.GetKey(KeyCode.LeftArrow)) movement.x = -1f;

        if (Input.GetKey(KeyCode.UpArrow)) movement.y = 1f;
        else if (Input.GetKey(KeyCode.DownArrow)) movement.y = -1f;

        // 1. 애니메이션 방향 기억하기
        if (movement.x != 0 || movement.y != 0)
        {
            lastMoveDir = movement;
        }

        // 2. 애니메이터에 파라미터 값 전달하기
        anim.SetFloat("DirX", lastMoveDir.x);
        anim.SetFloat("DirY", lastMoveDir.y);
        anim.SetFloat("Speed", movement.magnitude); // 이동 중이면 0보다 커서 Walk로 전환됨

        // 3. 왼쪽으로 이동할 때 이미지 좌우 반전시키기
        if (movement.x < 0)
        {
            sr.flipX = true;
        }
        else if (movement.x > 0)
        {
            sr.flipX = false;
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movement.normalized * moveSpeed * Time.fixedDeltaTime);
    }

    void OnEnable()
    {
        InputManager.OnAttackPressed += Attack;
    }

    void OnDisable()
    {
        InputManager.OnAttackPressed -= Attack;
    }

    void Attack()
    {
        // 공격 애니메이션 실행
        anim.SetTrigger("Attack");

        Collider2D hit = Physics2D.OverlapCircle(transform.position, attackRange, monsterLayer);
        if (hit != null)
        {
            Monster target = hit.GetComponent<Monster>();
            if (target != null)
            {
                target.TakeDamage(GameManager.Instance.tapDamage);
            }
        }
    }
}