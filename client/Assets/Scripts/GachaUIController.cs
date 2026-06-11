using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GachaUIController : MonoBehaviour
{
    public static GachaUIController Instance;

    [Header("UI 연결")]
    public GameObject gachaUIPanel;
    public Image eggImage;
    public Button eggButton;
    public GameObject resultPanel;

    [Header("시각적 결과창 연결")]
    public Transform slotContainer;     // 🌟 그리드 레이아웃이 있는 컨테이너
    public GameObject resultSlotPrefab; // 🌟 방금 만든 ResultSlot 프리팹
    public Button closeButton;          // 🌟 창을 닫을 확인 버튼

    [Header("스프라이트 프레임")]
    public Sprite[] eggFrames;

    private bool isReadyToOpen = false;
    private int[] pendingGrades;

    void Awake()
    {
        if (Instance == null) Instance = this;

        gachaUIPanel.SetActive(false);
        eggButton.onClick.AddListener(OnClickEgg);

        // 🌟 닫기 버튼에 창 닫는 함수 연결
        closeButton.onClick.AddListener(CloseGachaUI);
    }

    public void StartGachaAnimation()
    {
        gachaUIPanel.SetActive(true);
        resultPanel.SetActive(false);
        eggImage.gameObject.SetActive(true);

        isReadyToOpen = false;
        eggButton.interactable = false;

        StopAllCoroutines();
        StartCoroutine(IdleAnimation());
    }

    public void OnTransactionComplete(int[] grades)
    {
        pendingGrades = grades;
        isReadyToOpen = true;
        eggButton.interactable = true;

        StopAllCoroutines();
        StartCoroutine(ReadyAnimation());
    }

    private void OnClickEgg()
    {
        if (!isReadyToOpen) return;

        isReadyToOpen = false;
        eggButton.interactable = false;

        StopAllCoroutines();
        StartCoroutine(ExplodeAnimation());
    }

    // --- 애니메이션 로직 ---
    private IEnumerator IdleAnimation()
    {
        int frameIndex = 0;
        while (!isReadyToOpen)
        {
            eggImage.sprite = eggFrames[frameIndex];
            frameIndex = (frameIndex + 1) % 4;
            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator ReadyAnimation()
    {
        int frameIndex = 4;
        while (isReadyToOpen)
        {
            eggImage.sprite = eggFrames[frameIndex];
            frameIndex = (frameIndex == 4) ? 5 : 4;
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator ExplodeAnimation()
    {
        eggImage.sprite = eggFrames[6];
        yield return new WaitForSeconds(0.15f);

        eggImage.sprite = eggFrames[7];
        yield return new WaitForSeconds(0.3f);

        // 연출 종료 후 결과창 세팅
        eggImage.gameObject.SetActive(false);
        resultPanel.SetActive(true);

        // 🌟 1. 기존에 생성된 슬롯이 있다면 싹 지워주기 (안 그러면 뽑을 때마다 무한 증식함)
        foreach (Transform child in slotContainer)
        {
            Destroy(child.gameObject);
        }

        // 🌟 2. 10연뽑 배열을 돌면서 슬롯 프리팹 생성 및 이미지 입히기
        foreach (int grade in pendingGrades)
        {
            if (SkillManager.Instance.skillDatabase.TryGetValue(grade, out SkillData skillData))
            {
                // 프리팹 생성 후 컨테이너 안에 넣기
                GameObject slot = Instantiate(resultSlotPrefab, slotContainer);

                // 자식 오브젝트 찾아서 이미지와 이름 교체
                Image iconImg = slot.transform.Find("Icon").GetComponent<Image>();
                TextMeshProUGUI nameTxt = slot.transform.Find("NameText").GetComponent<TextMeshProUGUI>();

                iconImg.sprite = skillData.skillIcon;
                nameTxt.text = skillData.skillName;
            }
        }

        // 🌟 3. 스펙업을 적용하고 총합 요약 텍스트만 상단에 띄우기
        SkillManager.Instance.UnlockOrUpgradeSkills(pendingGrades);
    }

    // 🌟 확인 버튼을 누르면 완전히 깔끔하게 꺼지는 로직
    public void CloseGachaUI()
    {
        gachaUIPanel.SetActive(false);
        resultPanel.SetActive(false);
    }
}