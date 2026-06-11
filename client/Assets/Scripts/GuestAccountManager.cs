using UnityEngine;
using System.Text;

public static class GuestAccountManager
{
    // 기기에 데이터를 저장할 때 쓸 이름(Key)
    private const string PREFS_KEY = "GuestWalletAddress";

    public static string GetOrGenerateAddress()
    {
        // 1. 예전에 접속해서 저장된 주소가 있다면 그대로 가져옵니다.
        if (PlayerPrefs.HasKey(PREFS_KEY))
        {
            string savedAddress = PlayerPrefs.GetString(PREFS_KEY);
            Debug.Log($"[기존 유저 접속] 환영합니다! 주소: {savedAddress}");
            return savedAddress;
        }

        // 2. 처음 접속한 유저라면 새로 랜덤 주소를 발급합니다.
        string newAddress = GenerateRandomEthAddress();

        // 3. 발급된 주소를 기기에 영구 저장합니다.
        PlayerPrefs.SetString(PREFS_KEY, newAddress);
        PlayerPrefs.Save();

        Debug.Log($"[신규 유저 접속] 새로운 게스트 주소가 발급되었습니다: {newAddress}");
        return newAddress;
    }

    // 스마트 컨트랙트가 요구하는 이더리움 주소 형식(0x + 16진수 40자리)을 랜덤으로 생성하는 함수
    private static string GenerateRandomEthAddress()
    {
        StringBuilder sb = new StringBuilder("0x");
        char[] hexChars = "0123456789abcdef".ToCharArray();

        // 40번 반복해서 랜덤한 문자를 뽑아 붙입니다.
        for (int i = 0; i < 40; i++)
        {
            sb.Append(hexChars[Random.Range(0, hexChars.Length)]);
        }

        return sb.ToString();
    }
}