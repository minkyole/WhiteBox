using UnityEngine;
using TMPro;

public class Monster : MonoBehaviour
{
    public int maxHp = 1000;
    public int currentHp;
    public int rewardGold = 500;

    [Header("UI References")]
    public TextMeshPro hpText; // 몬스터 머리 위 3D Text 연결용 변수

    void Start()
    {
        Spawn();
    }

    void Spawn()
    {
        currentHp = maxHp;
        UpdateHpUI(); // 스폰될 때 꽉 찬 체력으로 UI 초기화
    }

    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        UpdateHpUI(); // 맞을 때마다 체력 UI 갱신!

        if (currentHp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        GameManager.Instance.AddGold(rewardGold);
        Spawn(); // 죽으면 다시 부활
    }

    // 몬스터 체력 텍스트를 갱신해 주는 함수
    private void UpdateHpUI()
    {
        if (hpText != null)
        {
            hpText.text = $"{currentHp} / {maxHp}";
        }
    }
}