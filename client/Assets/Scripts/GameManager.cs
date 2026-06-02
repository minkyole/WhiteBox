using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Stats")]
    public int gold = 0;
    public int tapDamage = 10; // 초기 기본 탭 데미지

    [Header("UI References")]
    public TextMeshProUGUI goldText;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateGoldUI();
    }

    // 골드 획득 및 차감 함수
    public void AddGold(int amount)
    {
        gold += amount;
        UpdateGoldUI();
    }

    // 무기 뽑았을 때 데미지 올려주는 함수
    public void AddTapDamage(int amount)
    {
        tapDamage += amount;
        Debug.Log($"[GameManager] 무기 장착! 현재 탭 데미지: {tapDamage}");
    }

    private void UpdateGoldUI()
    {
        if (goldText != null)
        {
            goldText.text = $"Gold: {gold}";
        }
    }
}