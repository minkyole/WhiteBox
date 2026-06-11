using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GachaHistoryUI : MonoBehaviour
{
    [Header("연결")]
    public GachaManager gachaManager;
    public Transform contentContainer;    // Scroll View 안의 Content 오브젝트
    public GameObject historySlotPrefab;  // 기록 한 줄을 표시할 UI 프리팹 (Text와 Button이 포함됨)

    public GameObject mainMenuPanel;

    // 가챠 하우스에서 '기록 보기' 버튼을 누르면 이 함수가 실행되게 연결하세요.
    public void OpenHistoryPanel()
    {
        gameObject.SetActive(true);

        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);

        foreach (Transform child in contentContainer)
        {
            Destroy(child.gameObject);
        }

        Debug.Log("서버에 기록을 요청합니다...");

        gachaManager.FetchGachaHistory(OnHistoryReceived);
    }

    // 서버 통신이 끝나면 자동으로 실행되는 함수
    private void OnHistoryReceived(GachaHistoryItem[] historyData)
    {
        if (historyData.Length == 0)
        {
            Debug.Log("가챠 기록이 없습니다.");
            return;
        }

        // 최신 기록이 위로 오도록 배열을 뒤집어서(역순으로) UI를 생성합니다.
        for (int i = historyData.Length - 1; i >= 0; i--)
        {
            GachaHistoryItem item = historyData[i];

            // 프리팹 생성
            GameObject slot = Instantiate(historySlotPrefab, contentContainer);

            // 프리팹 내부의 Text 찾아서 내용 채우기
            TextMeshProUGUI logText = slot.transform.Find("LogText").GetComponent<TextMeshProUGUI>();

            string typeName = item.gachaType == 0 ? "초보자" : "중급";
            int pullCount = item.weaponGrades.Length; // 1연뽑인지 10연뽑인지 배열 길이로 확인

            // 간단하게 텍스트로 요약 (예: [초보자] 10연뽑 완료! (결과: 1, 2, 1...))
            logText.text = $"[{typeName}] {pullCount}연뽑 완료!\n결과: {string.Join(", ", item.weaponGrades)}";

            // 프리팹 내부의 '이더스캔 링크 버튼' 연결
            Button linkButton = slot.transform.Find("EtherscanButton").GetComponent<Button>();

            // 버튼을 누르면 해당 txHash를 가진 이더스캔 페이지를 엽니다.
            string hashToOpen = item.txHash;
            linkButton.onClick.AddListener(() =>
            {
                Application.OpenURL($"https://sepolia.etherscan.io/tx/{hashToOpen}");
            });
        }
    }
    public void CloseHistoryPanel()
    {
        gameObject.SetActive(false); // 기록 창 끄기

        // 다시 메인 메뉴 켜기
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }
}