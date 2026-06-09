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
}

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance;

    // 전체 스킬 목록을 관리하는 딕셔너리
    public Dictionary<int, SkillData> skillDatabase = new Dictionary<int, SkillData>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        InitializeSkills();
    }

    // 🌟 기획하신 1~10단계 스킬 초기화
    private void InitializeSkills()
    {
        // 0단계는 기본 공격이므로 가챠 해금 대상에서 제외하거나 별도 관리합니다.

        skillDatabase.Add(1, new SkillData { grade = 1, skillName = "밭갈기", description = "호미로 때림", damage = 10, isUnlocked = false, level = 0 });
        skillDatabase.Add(2, new SkillData { grade = 2, skillName = "광석캐기", description = "곡괭이로 때림", damage = 30, isUnlocked = false, level = 0 });
        skillDatabase.Add(3, new SkillData { grade = 3, skillName = "상단베기", description = "검으로 때림", damage = 70, isUnlocked = false, level = 0 });
        skillDatabase.Add(4, new SkillData { grade = 4, skillName = "K2소총", description = "총으로 공격함", damage = 150, isUnlocked = false, level = 0 });
        skillDatabase.Add(5, new SkillData { grade = 5, skillName = "RPG", description = "유탄을 발사함", damage = 300, isUnlocked = false, level = 0 });
        skillDatabase.Add(6, new SkillData { grade = 6, skillName = "파이어볼", description = "파이어볼을 발사함", damage = 600, isUnlocked = false, level = 0 });
        skillDatabase.Add(7, new SkillData { grade = 7, skillName = "천벌", description = "번개를 소환함", damage = 1200, isUnlocked = false, level = 0 });
        skillDatabase.Add(8, new SkillData { grade = 8, skillName = "에너지파", description = "에너지파를 쏨", damage = 2500, isUnlocked = false, level = 0 });
        skillDatabase.Add(9, new SkillData { grade = 9, skillName = "검강", description = "검기로 강력한 근접 공격을 함", damage = 5000, isUnlocked = false, level = 0 });
        skillDatabase.Add(10, new SkillData { grade = 10, skillName = "종말의 메테오", description = "메테오를 추락시킴", damage = 10000, isUnlocked = false, level = 0 });
    }

    // 🌟 GachaManager에게 텍스트를 리턴하도록 string 반환형으로 변경
    public string UnlockOrUpgradeSkills(int[] grades)
    {
        string resultLog = "[온체인 스킬 획득 결과]\n";
        int totalDamageAdded = 0;

        // 결과 요약을 위해 임시로 카운팅할 딕셔너리
        Dictionary<string, int> acquiredSummary = new Dictionary<string, int>();

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
                    resultLog += $"[신규 해금] {skill.skillName}\n";
                }
                else
                {
                    skill.level++;
                    int bonusDamage = skill.damage / 10;
                    skill.damage += bonusDamage;
                    totalDamageAdded += bonusDamage;
                }

                // 획득 내역 요약용
                if (acquiredSummary.ContainsKey(skill.skillName))
                    acquiredSummary[skill.skillName]++;
                else
                    acquiredSummary.Add(skill.skillName, 1);
            }
        }

        // 10연뽑일 경우 깔끔하게 요약 텍스트 추가
        if (grades.Length > 1)
        {
            resultLog += "\n<요약>\n";
            foreach (var kvp in acquiredSummary)
            {
                resultLog += $"{kvp.Key} x{kvp.Value}\n";
            }
        }

        resultLog += $"\n총 합산 공격력 증가: +{totalDamageAdded}";

        // 데미지 증가 적용
        GameManager.Instance.AddTapDamage(totalDamageAdded);

        return resultLog;
    }
}