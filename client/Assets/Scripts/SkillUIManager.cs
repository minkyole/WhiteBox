using UnityEngine;
using System.Collections.Generic;

public class SkillUIManager : MonoBehaviour
{
    public static SkillUIManager Instance;

    [Header("UI References")]
    public Transform skillSlotContainer; // 아까 만든 Horizontal Layout Group이 있는 SkillPanel
    public GameObject skillSlotPrefab;   // 아까 만든 SkillSlot 프리팹

    // 10개의 슬롯을 관리할 리스트 (메모리 효율을 위한 캐싱)
    private List<SkillSlotUI> slotUIList = new List<SkillSlotUI>();

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        InitializeSlots();
    }

    private void InitializeSlots()
    {
        // 시작할 때 1~10단계 슬롯을 미리 생성해 컨테이너 안에 넣습니다.
        for (int i = 1; i <= 10; i++)
        {
            GameObject slotObj = Instantiate(skillSlotPrefab, skillSlotContainer);
            SkillSlotUI slotUI = slotObj.GetComponent<SkillSlotUI>();
            slotUIList.Add(slotUI);
        }

        // 초기 상태(전부 자물쇠) 반영
        RefreshAllSlots();
    }

    // 가챠를 뽑고 나서 호출하면 UI가 한 번에 싹 바뀝니다.
    public void RefreshAllSlots()
    {
        for (int i = 0; i < 10; i++)
        {
            int grade = i + 1;
            if (SkillManager.Instance.skillDatabase.ContainsKey(grade))
            {
                SkillData data = SkillManager.Instance.skillDatabase[grade];
                slotUIList[i].UpdateSlot(data);
            }
        }
    }
}