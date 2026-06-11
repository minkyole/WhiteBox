using UnityEngine;

// 🌟 스킬 타입을 정의합니다. (근접 타격, 투사체, 광역 폭발)
public enum SkillType { Melee, Projectile, AoE }

public class SkillController : MonoBehaviour
{
    public float speed = 10f;       // 투사체 속도
    public float lifeTime = 2f;     // 유지 시간 (자동 파괴 타이머)
    public float aoeRadius = 3f;    // 광역 범위 크기
    public LayerMask monsterLayer;  // 몬스터 레이어 지정

    private int damage;
    private SkillType skillType;
    private Vector2 moveDirection;
    private bool isInitialized = false;

    // SkillManager에서 프리팹을 생성할 때 초기 데이터(데미지, 방향 등)를 넘겨주는 함수
    public void Init(int skillDamage, SkillType type, Vector2 dir)
    {
        damage = skillDamage;
        skillType = type;
        moveDirection = dir;
        isInitialized = true;

        // 모든 스킬은 생성되고 'lifeTime' 초 뒤에 메모리에서 자동 파괴됩니다.
        Destroy(gameObject, lifeTime);

        // 광역기(AoE)의 경우: 투사체가 날아가지 않고, 생성 즉시 지정된 범위 안의 적들에게 데미지를 줍니다.
        if (skillType == SkillType.AoE)
        {
            ApplyAoEDamage();
        }
    }

    void Update()
    {
        if (!isInitialized) return;

        // 투사체(Projectile)일 때만 앞으로 날아갑니다.
        if (skillType == SkillType.Projectile)
        {
            transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
        }
        // Melee(근접)나 AoE(광역)는 날아가지 않고 생성된 자리에 가만히 둡니다. (애니메이션만 재생)
    }

    // 트리거 충돌 감지 (근접 타격이나 투사체가 몬스터와 부딪혔을 때)
    void OnTriggerEnter2D(Collider2D collision)
    {
        // 광역기는 이미 생성될 때 ApplyAoEDamage()로 데미지를 줬으므로 트리거 충돌은 무시합니다.
        if (skillType == SkillType.AoE) return;

        Monster target = collision.GetComponent<Monster>();
        if (target != null)
        {
            target.TakeDamage(damage);

            // 투사체라면 적을 맞춘 후 소멸시킵니다. (관통하려면 이 줄을 지우면 됩니다!)
            if (skillType == SkillType.Projectile)
            {
                Destroy(gameObject);
            }
        }
    }

    private void ApplyAoEDamage()
    {
        // 내 위치를 기준으로 동그란 범위(aoeRadius) 안의 몬스터들을 싹 다 찾습니다.
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, aoeRadius, monsterLayer);
        foreach (Collider2D hit in hits)
        {
            Monster target = hit.GetComponent<Monster>();
            if (target != null)
            {
                target.TakeDamage(damage);
            }
        }
    }
}