using UnityEngine;
using System.Collections.Generic;

// 🌟 스킬 상태를 관리할 데이터 클래스
[System.Serializable]
public class SkillData
{
    public int grade;           // 스킬 등급 (1~10)
    public string skillName;    // 스킬 이름
    public string description;  // 공격 연출 설명
    public int damage;          // 스킬 데미지
    public bool isUnlocked;     // 해금 여부
    public int level;           // 중복 획득 시 레벨업
    public float maxCooldown;     // 스킬의 기본 쿨타임 (예: 3초)
    public float currentCooldown; // 현재 남은 쿨타임
    public KeyCode activeKey;
    public SkillType type;
    public GameObject prefab;
    public Sprite skillIcon;
}

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    // 전체 스킬 목록을 관리하는 딕셔너리
    public Dictionary<int, SkillData> skillDatabase = new Dictionary<int, SkillData>();

    [Header("Debug Settings")]
    public bool unlockAllForTest = true;

    [Header("Skill Summon Settings")]
    public Transform playerTransform;
    public GameObject[] skillPrefabs = new GameObject[10];
    public Sprite[] skillIcons = new Sprite[10];

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        InitializeSkills();

        // 테스트 모드가 켜져 있다면 시작하자마자 전부 해금
        if (unlockAllForTest)
        {
            UnlockAllSkillsForDebug();
        }
    }

    // 3. 매 프레임마다 쿨타임을 계산하고 스킬을 발사하는 로직
    void Update()
    {
        foreach (var kvp in skillDatabase)
        {
            SkillData skill = kvp.Value;

            if (skill.isUnlocked)
            {
                // 1. 쿨타임은 매 프레임마다 무조건 감소시킵니다.
                if (skill.currentCooldown > 0)
                {
                    skill.currentCooldown -= Time.deltaTime;
                }

                // 2. 할당된 키보드 입력 감지
                if (Input.GetKeyDown(skill.activeKey))
                {
                    TryUseSkill(skill);
                }
            }
        }
    }

    // 전체 스킬 강제 해금 로직
    private void UnlockAllSkillsForDebug()
    {
        foreach (var kvp in skillDatabase)
        {
            kvp.Value.isUnlocked = true;
            kvp.Value.level = 1; // 기본 레벨 1로 세팅
            // 게임 시작 즉시 스킬을 쏠 수 있게 쿨타임 0으로 초기화
            kvp.Value.currentCooldown = 0f;
        }
        Debug.Log("[디버그] 테스트 모드 작동: 모든 스킬이 강제 해금되었습니다!");
    }

    private void InitializeSkills()
    {
        skillDatabase.Clear();

        // 🌟 수정: 끝부분에 skillIcon = skillIcons[0] 등을 각각 추가해 줍니다.
        skillDatabase.Add(1, new SkillData { grade = 1, skillName = "몽둥이치기", type = SkillType.Melee, prefab = skillPrefabs[0], skillIcon = skillIcons[0], damage = 10, maxCooldown = 2f, currentCooldown = 0f, activeKey = KeyCode.A });
        skillDatabase.Add(2, new SkillData { grade = 2, skillName = "광석캐기", type = SkillType.Melee, prefab = skillPrefabs[1], skillIcon = skillIcons[1], damage = 30, maxCooldown = 3f, currentCooldown = 0f, activeKey = KeyCode.S });
        skillDatabase.Add(3, new SkillData { grade = 3, skillName = "상단베기", type = SkillType.Melee, prefab = skillPrefabs[2], skillIcon = skillIcons[2], damage = 70, maxCooldown = 4f, currentCooldown = 0f, activeKey = KeyCode.D });

        skillDatabase.Add(4, new SkillData { grade = 4, skillName = "K2소총", type = SkillType.Projectile, prefab = skillPrefabs[3], skillIcon = skillIcons[3], damage = 150, maxCooldown = 5f, currentCooldown = 0f, activeKey = KeyCode.F });
        skillDatabase.Add(5, new SkillData { grade = 5, skillName = "RPG", type = SkillType.AoE, prefab = skillPrefabs[4], skillIcon = skillIcons[4], damage = 300, maxCooldown = 6f, currentCooldown = 0f, activeKey = KeyCode.G });

        skillDatabase.Add(6, new SkillData { grade = 6, skillName = "파이어볼", type = SkillType.Projectile, prefab = skillPrefabs[5], skillIcon = skillIcons[5], damage = 600, maxCooldown = 7f, currentCooldown = 0f, activeKey = KeyCode.Z });
        skillDatabase.Add(7, new SkillData { grade = 7, skillName = "천벌", type = SkillType.AoE, prefab = skillPrefabs[6], skillIcon = skillIcons[6], damage = 1200, maxCooldown = 8f, currentCooldown = 0f, activeKey = KeyCode.X });
        skillDatabase.Add(8, new SkillData { grade = 8, skillName = "에너지파", type = SkillType.Projectile, prefab = skillPrefabs[7], skillIcon = skillIcons[7], damage = 2500, maxCooldown = 9f, currentCooldown = 0f, activeKey = KeyCode.C });

        skillDatabase.Add(9, new SkillData { grade = 9, skillName = "검강", type = SkillType.Melee, prefab = skillPrefabs[8], skillIcon = skillIcons[8], damage = 5000, maxCooldown = 10f, currentCooldown = 0f, activeKey = KeyCode.V });
        skillDatabase.Add(10, new SkillData { grade = 10, skillName = "종말의 메테오", type = SkillType.AoE, prefab = skillPrefabs[9], skillIcon = skillIcons[9], damage = 10000, maxCooldown = 20f, currentCooldown = 0f, activeKey = KeyCode.B });
    }



    // 입력 신호가 들어왔을 때 발사 가능 여부를 검증하는 함수
    public void TryUseSkill(SkillData skill)
    {
        if (skill.currentCooldown <= 0)
        {
            FireSkill(skill);
            skill.currentCooldown = skill.maxCooldown; // 사용 성공 시 쿨타임 리셋
        }
        else
        {
            Debug.Log($"{skill.skillName} 쿨타임 중! ({skill.currentCooldown:F1}초 남음)");
        }
    }

    private void FireSkill(SkillData skill)
    {
        if (skill.prefab == null || playerTransform == null)
        {
            Debug.LogWarning($"{skill.skillName} - 프리팹이나 플레이어 위치가 할당되지 않았습니다!");
            return;
        }

        //플레이어의 애니메이터를 찾아서 공격 애니메이션 실행!
        Animator anim = playerTransform.GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        // 플레이어보다 약간 오른쪽(또는 앞쪽)에서 소환되도록 위치 조정
        Vector3 spawnPos = playerTransform.position + new Vector3(1f, 0f, 0f);

        GameObject skillObj = Instantiate(skill.prefab, spawnPos, Quaternion.identity);
        SkillController controller = skillObj.GetComponent<SkillController>();

        if (controller != null)
        {
            controller.Init(skill.damage, skill.type, Vector2.right);
        }
    }

    public void UnlockOrUpgradeSkills(int[] grades)
    {
        int totalDamageAdded = 0;

        foreach (int grade in grades)
        {
            if (skillDatabase.ContainsKey(grade))
            {
                SkillData skill = skillDatabase[grade];

                if (!skill.isUnlocked)
                {
                    skill.isUnlocked = true;
                    skill.level = 1;
                    totalDamageAdded += skill.damage;
                }
                else
                {
                    skill.level++;
                    int bonusDamage = skill.damage / 10;
                    skill.damage += bonusDamage;
                    totalDamageAdded += bonusDamage;
                }

            }
        }

        GameManager.Instance.AddTapDamage(totalDamageAdded);

        if (SkillUIManager.Instance != null)
        {
            SkillUIManager.Instance.RefreshAllSlots();
        }
    }
}