using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 전역에서 쉽게 접근할 수 있도록 static Instance 선언 (싱글톤 패턴)
    public static GameManager Instance;

    [Header("유저 데이터 (씬이동 시 유지됨)")]
    public int gold = 0;
    public int tapDamage = 1;
    public int currentStage = 1; // 포탈 탈 때 저장할 스테이지 정보

    void Awake()
    {
        // 1. 만약 Instance가 비어있다면, 나 자신을 Instance로 지정
        if (Instance == null)
        {
            Instance = this;
            // 2. 씬이 넘어가도 이 오브젝트를 파괴하지 마라! (가장 핵심)
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 3. 로비로 다시 돌아왔을 때, 새로운 GameManager가 생기는 것을 막기 위해 자살
            Destroy(gameObject);
        }
    }

    public void AddGold(int amount)
    {
        gold += amount;
    }

    public void AddTapDamage(int amount)
    {
        tapDamage += amount;
        Debug.Log($"[GameManager] 무기 획득/장착! 현재 탭 데미지: {tapDamage}");
    }
}