using UnityEngine;

public class GachaHouse : MonoBehaviour
{
    [Header("UI 패널 연결")]
    public GameObject mainGachaPanel;   // 전체 가챠 화면 (집에 닿으면 켜질 최상위 부모)
    public GameObject step1_MainMenu;   // 1회/10회/기록 선택 화면
    public GameObject step2_TierMenu;   // 3가지 단계(유형) 선택 화면
    public GachaManager gachaManager; // 인스펙터에서 GachaManager 오브젝트를 드래그해서 연결

    private bool isTenPull = false;     // 현재 10회 소환을 선택했는지 기억하는 변수

    public static bool isUIOpen = false;

    // 🌟 플레이어가 집에 닿았을 때
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (gachaManager != null) gachaManager.UpdateGachaCounts();

        if (collision.CompareTag("Player"))
        {
            OpenMainMenu();

            // 🌟 플레이어가 걸어오던 관성 때문에 미끄러지지 않도록 속도를 0으로 강제 정지
            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            CloseAllMenus(); // UI 끄기
        }
    }

    // --- UI 버튼에서 호출할 함수들 ---

    public void OpenMainMenu()
    {
        isUIOpen=true;
        mainGachaPanel.SetActive(true);
        step1_MainMenu.SetActive(true);
        step2_TierMenu.SetActive(false);
    }

    public void CloseAllMenus()
    {
        isUIOpen = false; // 상태 해제

        // 🌟 '?'를 사용하거나 if 문으로 null 체크를 추가합니다.
        // mainGachaPanel이 파괴되었는지 확인하고, 살아있을 때만 끕니다.
        if (mainGachaPanel != null)
        {
            mainGachaPanel.SetActive(false);
        }
    }

    // 버튼: 1회 소환 누름
    public void OnClickSingleSummon()
    {
        isTenPull = false;
        step1_MainMenu.SetActive(false);
        step2_TierMenu.SetActive(true); // 단계 선택창으로 이동
    }

    // 버튼: 10회 소환 누름
    public void OnClickTenSummon()
    {
        isTenPull = true;
        step1_MainMenu.SetActive(false);
        step2_TierMenu.SetActive(true); // 단계 선택창으로 이동
    }

    // 버튼: 단계(티어) 선택 완료 후 진짜 소환 실행!
    // 버튼의 OnClick 이벤트에서 매개변수로 1, 2, 3을 넣어주면 됩니다.
    // GachaHouse.cs 상단에 참조 추가
    

    public void OnClickExecuteSummon(int tierLevel)
    {
        // 현재 서버에서 받아온 횟수라고 가정 (GachaManager 등에 변수로 저장해 둠)
        int myBeginnerCount = gachaManager.currentBeginnerCount;
        int myMidCount = gachaManager.currentMidCount;

        // 🌟 1. 중급 소환(1)을 눌렀는데 초급 횟수가 부족할 때
        if (tierLevel == 1 && myBeginnerCount < 100)
        {
            Debug.Log($"[안내] 초급 소환 100회를 먼저 완료하세요! (현재: {myBeginnerCount}/100)");
            // TODO: 플레이어에게 보여줄 팝업창이나 경고 텍스트 띄우기
            return; // 여기서 함수를 끝내서 서버로 요청을 안 보냄!
        }

        // 🌟 2. 고급 소환(2)을 눌렀는데 중급 횟수가 부족할 때
        if (tierLevel == 2 && myMidCount < 100)
        {
            Debug.Log($"[안내] 중급 소환 100회를 먼저 완료하세요! (현재: {myMidCount}/100)");
            return;
        }

        // 조건이 다 맞으면 그제서야 정상적으로 소환 실행!
        step2_TierMenu.SetActive(false);
        int amount = isTenPull ? 10 : 1;
        gachaManager.RunGacha(tierLevel, amount);
        CloseAllMenus();
    }
}