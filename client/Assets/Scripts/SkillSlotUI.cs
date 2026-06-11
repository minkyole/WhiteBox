using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillSlotUI : MonoBehaviour
{
    public Image skillIcon;
    public GameObject lockOverlay;
    public TextMeshProUGUI levelText;

    public TextMeshProUGUI hotkeyText;
    public Image cooldownOverlay;

    // 현재 이 슬롯이 담당하고 있는 스킬 데이터를 기억할 변수
    private SkillData mySkillData;

    public void UpdateSlot(SkillData data)
    {
        mySkillData = data; // 데이터 저장

        // 데이터에서 아이콘을 가져와서 UI 이미지 교체
        if (skillIcon != null && data.skillIcon != null)
        {
            skillIcon.sprite = data.skillIcon;
        }

        if (hotkeyText != null)
        {
            hotkeyText.text = data.activeKey.ToString();
        }
        if (data.isUnlocked)
        {
            lockOverlay.SetActive(false);
            levelText.text = $"Lv.{data.level}";
            levelText.gameObject.SetActive(true);
        }
        else
        {
            lockOverlay.SetActive(true);
            levelText.gameObject.SetActive(false);
            // 미해금 상태일 땐 쿨타임 게이지 숨기기
            if (cooldownOverlay != null) cooldownOverlay.fillAmount = 0;
        }
    }

    void Update()
    {
        if (mySkillData != null && mySkillData.isUnlocked && cooldownOverlay != null)
        {
            // 쿨타임 게이지 업데이트 (0 이하로 내려가면 0으로 고정)
            float fillRatio = Mathf.Clamp01(mySkillData.currentCooldown / mySkillData.maxCooldown);
            cooldownOverlay.fillAmount = fillRatio;
        }
    }
}