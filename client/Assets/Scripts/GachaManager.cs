using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Text;
using TMPro;

[System.Serializable]
public class GachaRequestData { public string userAddress; public int gachaType; public int amount; }

[System.Serializable]
public class GachaResponseData { public bool success; public string txHash; public string requestId; }

[System.Serializable]
public class GachaResultData { public string status; public int[] weaponGrades; public string fulfillTxHash; }

[System.Serializable]
public class GachaCountResponse { public bool success; public int beginnerCount; public int midCount; }

[System.Serializable]
public class GachaHistoryItem
{
    public string requestId;
    public int gachaType;
    public int[] weaponGrades;
    public string txHash;
}

[System.Serializable]
public class GachaHistoryResponse
{
    public bool success;
    public GachaHistoryItem[] history;
}

public class GachaManager : MonoBehaviour
{
    public int gachaCost = 1000;
    private string baseUrl = "http://localhost:3000/api";

    public string myUserAddress = "";
    public int currentGachaType = 1;
    private bool isProcessing = false;

    [Header("Receipt UI References")]
    public TextMeshProUGUI statusText;
    public GameObject requestButton;
    public GameObject fulfillButton;

    private string requestTxHash = "";
    private string fulfillTxHash = "";

    public int currentStage = 1;

    [Header("Gacha Counts")]
    public int currentBeginnerCount = 0;
    public int currentMidCount = 0;


    void Start()
    {
        myUserAddress = GuestAccountManager.GetOrGenerateAddress();

        if (requestButton != null) requestButton.SetActive(false);
        if (fulfillButton != null) fulfillButton.SetActive(false);
        if (statusText != null) statusText.text = "준비 완료";

        UpdateGachaCounts();
    }


    // GachaManager.cs 내부에 추가/수정
    public void RunGacha(int tierLevel, int amount)
    {
        // 1. 선택한 티어에 따라 gachaType을 바꿉니다.
        currentGachaType = tierLevel;

        // 2. 기존의 골드 확인 및 요청 로직을 수행합니다.
        RequestGacha(amount);
    }

    public void RequestGacha(int amount)
    {
        if (isProcessing) return;

        int totalCost = gachaCost * amount;

        if (GameManager.Instance.gold >= totalCost)
        {
            isProcessing = true;
            GameManager.Instance.AddGold(-totalCost);

            if (requestButton != null) requestButton.SetActive(false);
            if (fulfillButton != null) fulfillButton.SetActive(false);

            if (statusText != null) statusText.text = $"블록체인 {amount}연뽑 요청 중...";
            GachaUIController.Instance.StartGachaAnimation();
            StartCoroutine(SendGachaRequest(amount));
        }
        else
        {
            if (statusText != null) statusText.text = "골드가 부족합니다!";
        }
    }

    private IEnumerator SendGachaRequest(int amount)
    {
        GachaRequestData requestData = new GachaRequestData
        {
            userAddress = myUserAddress,
            gachaType = currentGachaType,
            amount = amount
        };
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

            requestTxHash = res.txHash;
            if (statusText != null) statusText.text = "1단계 트랜잭션 완료!\n체인링크 난수 배달 대기 중...";
            if (requestButton != null) requestButton.SetActive(true);

            StartCoroutine(PollGachaResult(res.requestId));
        }
        else
        {
            if (statusText != null) statusText.text = "가챠 요청 실패!";
            GameManager.Instance.AddGold(gachaCost * amount);
            isProcessing = false;
        }
    }

    private IEnumerator PollGachaResult(string requestId)
    {
        bool isDone = false;
        while (!isDone)
        {
            yield return new WaitForSeconds(3.0f);

            UnityWebRequest request = UnityWebRequest.Get($"{baseUrl}/gacha-result?requestId={requestId}");
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                GachaResultData result = JsonUtility.FromJson<GachaResultData>(request.downloadHandler.text);

                if (result.status == "success")
                {
                    fulfillTxHash = result.fulfillTxHash;
                    if (fulfillButton != null) fulfillButton.SetActive(true);

                    GachaUIController.Instance.OnTransactionComplete(result.weaponGrades);

                    isDone = true;
                    isProcessing = false;

                    UpdateGachaCounts();

                }
            }
        }
    }

    public void FetchGachaHistory(System.Action<GachaHistoryItem[]> onComplete)
    {
        StartCoroutine(FetchHistoryCoroutine(onComplete));
    }

    private IEnumerator FetchHistoryCoroutine(System.Action<GachaHistoryItem[]> onComplete)
    {
        // GuestAccountManager에서 만든 내 고유 주소를 담아서 요청
        string requestUrl = $"{baseUrl}/history?address={myUserAddress}";
        UnityWebRequest request = UnityWebRequest.Get(requestUrl);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            GachaHistoryResponse res = JsonUtility.FromJson<GachaHistoryResponse>(request.downloadHandler.text);
            if (res.success && res.history != null)
            {
                // 성공적으로 배열을 가져오면 콜백(onComplete)으로 넘겨줍니다.
                onComplete?.Invoke(res.history);
            }
            else
            {
                Debug.LogWarning("[기록 조회] 성공했지만 데이터가 비어있습니다.");
                onComplete?.Invoke(new GachaHistoryItem[0]); // 빈 배열 전달
            }
        }
        else
        {
            Debug.LogError($"[기록 조회 실패] {request.error}");
            onComplete?.Invoke(new GachaHistoryItem[0]);
        }
    }
    public void UpdateGachaCounts()
    {
        StartCoroutine(FetchGachaCountsCoroutine());
    }

    private IEnumerator FetchGachaCountsCoroutine()
    {
        // 서버의 /api/gacha-counts 주소로 내 지갑 주소를 담아 GET 요청
        UnityWebRequest request = UnityWebRequest.Get($"{baseUrl}/gacha-counts?address={myUserAddress}");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            // 성공하면 데이터를 파싱해서 변수에 쏙 넣어줍니다.
            GachaCountResponse res = JsonUtility.FromJson<GachaCountResponse>(request.downloadHandler.text);
            if (res.success)
            {
                currentBeginnerCount = res.beginnerCount;
                currentMidCount = res.midCount;
                Debug.Log($"[가챠 횟수 갱신] 초보자: {currentBeginnerCount}회, 중급: {currentMidCount}회");
            }
        }
        else
        {
            Debug.LogError("가챠 횟수 조회 실패: " + request.error);
        }
    }

    public void OpenRequestLink()
    {
        if (!string.IsNullOrEmpty(requestTxHash))
            Application.OpenURL($"https://sepolia.etherscan.io/tx/{requestTxHash}");
    }

    public void OpenFulfillLink()
    {
        if (!string.IsNullOrEmpty(fulfillTxHash))
            Application.OpenURL($"https://sepolia.etherscan.io/tx/{fulfillTxHash}");
    }
}