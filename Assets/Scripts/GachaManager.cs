using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using TMPro;

[System.Serializable]
public class GachaRequestData { public int userId; }

[System.Serializable]
public class GachaResponseData { public bool success; public string txHash; public string requestId; }

// 🌟 서버 응답 구조체에 fulfillTxHash 필드 추가
[System.Serializable]
public class GachaResultData { public string status; public int rarity; public string fulfillTxHash; }

public class GachaManager : MonoBehaviour
{
    public int gachaCost = 1000;
    private string baseUrl = "http://localhost:3000/api";
    private int myUserId = 1234;
    private bool isProcessing = false;

    [Header("Receipt UI References")]
    public TextMeshProUGUI statusText;
    public GameObject requestButton;     // 1단계: 요청 영수증 버튼
    public GameObject fulfillButton;     // 2단계: 배달 영수증 버튼 (신규)

    private string requestTxHash = "";     // 1단계 요청 해시 저장
    private string fulfillTxHash = "";     // 2단계 배달 해시 저장 (신규)

    void Start()
    {
        // 게임 시작 시 두 영수증 버튼 모두 숨김
        if (requestButton != null) requestButton.SetActive(false);
        if (fulfillButton != null) fulfillButton.SetActive(false);
        if (statusText != null) statusText.text = "준비 완료";
    }

    public void RequestGacha()
    {
        if (isProcessing) return;

        if (GameManager.Instance.gold >= gachaCost)
        {
            isProcessing = true;
            GameManager.Instance.AddGold(-gachaCost);

            // 새 뽑기 시작 시 이전 버튼들 초기화
            if (requestButton != null) requestButton.SetActive(false);
            if (fulfillButton != null) fulfillButton.SetActive(false);

            if (statusText != null) statusText.text = "블록체인 트랜잭션 요청 중...";
            StartCoroutine(SendGachaRequest());
        }
    }

    private IEnumerator SendGachaRequest()
    {
        GachaRequestData requestData = new GachaRequestData { userId = myUserId };
        string jsonData = JsonUtility.ToJson(requestData);

        UnityWebRequest request = new UnityWebRequest($"{baseUrl}/gacha", "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            GachaResponseData res = JsonUtility.FromJson<GachaResponseData>(request.downloadHandler.text);

            // 🌟 1단계: 요청 영수증 활성화
            requestTxHash = res.txHash;
            if (statusText != null) statusText.text = "1단계 트랜잭션 완료!\n체인링크 난수 배달 대기 중...";
            if (requestButton != null) requestButton.SetActive(true);

            StartCoroutine(PollGachaResult(res.requestId));
        }
        else
        {
            if (statusText != null) statusText.text = "가챠 요청 실패!";
            GameManager.Instance.AddGold(gachaCost);
            isProcessing = false;
        }
    }

    private IEnumerator PollGachaResult(string requestId)
    {
        bool isDone = false;
        while (!isDone)
        {
            yield return new WaitForSeconds(2.0f);

            UnityWebRequest request = UnityWebRequest.Get($"{baseUrl}/gacha-result?requestId={requestId}");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                GachaResultData result = JsonUtility.FromJson<GachaResultData>(request.downloadHandler.text);

                if (result.status == "success")
                {
                    // 🌟 2단계: 배달 영수증 해시 저장 및 버튼 활성화
                    fulfillTxHash = result.fulfillTxHash;
                    if (fulfillButton != null) fulfillButton.SetActive(true);

                    ApplyOnChainResult(result.rarity);
                    isDone = true;
                    isProcessing = false;
                }
            }
        }
    }

    private void ApplyOnChainResult(int rarity)
    {
        int damageBuff = 0;
        string weaponName = "";

        switch (rarity)
        {
            case 0: damageBuff = 5; weaponName = "[일반] 나무 몽둥이"; break; // ⬜ 대신 [일반]
            case 1: damageBuff = 20; weaponName = "[희귀] 철검"; break;       // 🟦 대신 [희귀]
            case 2: damageBuff = 50; weaponName = "[에픽] 마법 지팡이"; break; // 🟪 대신 [에픽]
            case 3: damageBuff = 100; weaponName = "[전설] 엑스칼리버"; break; // 🟨 대신 [전설]
        }

        GameManager.Instance.AddTapDamage(damageBuff);
        if (statusText != null) statusText.text = $"[온체인 결과]\n{weaponName} 획득! (+{damageBuff})"; // '추첨' 대신 '결과'로 변경하여 에러 원천 차단 가능!
    }

    // 1단계 버튼 기능
    public void OpenRequestLink()
    {
        if (!string.IsNullOrEmpty(requestTxHash))
            Application.OpenURL($"https://sepolia.etherscan.io/tx/{requestTxHash}");
    }

    // 2단계 버튼 기능 (신규)
    public void OpenFulfillLink()
    {
        if (!string.IsNullOrEmpty(fulfillTxHash))
            Application.OpenURL($"https://sepolia.etherscan.io/tx/{fulfillTxHash}");
    }
}