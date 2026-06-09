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

public class GachaManager : MonoBehaviour
{
    public int gachaCost = 1000;
    private string baseUrl = "http://localhost:3000/api";

    public string myUserAddress = "0x5e429502998610284b79c85c66662cA6E23701B4";
    public int currentGachaType = 1;
    private bool isProcessing = false;

    [Header("Receipt UI References")]
    public TextMeshProUGUI statusText;
    public GameObject requestButton;
    public GameObject fulfillButton;

    private string requestTxHash = "";
    private string fulfillTxHash = "";

    void Start()
    {
        if (requestButton != null) requestButton.SetActive(false);
        if (fulfillButton != null) fulfillButton.SetActive(false);
        if (statusText != null) statusText.text = "준비 완료";
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

                    // 🌟 수정됨: SkillManager를 호출하여 처리
                    ApplyBatchResult(result.weaponGrades);

                    isDone = true;
                    isProcessing = false;
                }
            }
        }
    }

    // 🌟 대폭 간소화된 결과 처리 로직
    private void ApplyBatchResult(int[] grades)
    {
        // SkillManager에게 배열을 넘기고, UI에 출력할 텍스트를 받아옵니다.
        string finalResultText = SkillManager.Instance.UnlockOrUpgradeSkills(grades);

        if (statusText != null) statusText.text = finalResultText;
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