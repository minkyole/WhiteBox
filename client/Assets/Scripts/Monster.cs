using UnityEngine;
using TMPro;

public class Monster : MonoBehaviour
{
    public int maxHp = 1000;
    public int currentHp;
    public int rewardGold = 500;

    [Header("UI References")]
    public TextMeshPro hpText;

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
        currentHp = Mathf.Max(0, currentHp - damage);
        UpdateHpUI(); // 맞을 때마다 체력 UI 갱신!

        // 1. 풀에서 데미지 텍스트를 꺼내서 몬스터 위치보다 살짝 위쪽에 소환
        GameObject textObj = ObjectPoolManager.Instance.SpawnFromPool("DamageText", transform.position + new Vector3(0, 0.5f, 0), Quaternion.identity);

        if (textObj != null)
        {
            // 위치 살짝 분산 (여러 개 뜰 때 겹치지 않게)
            float randomX = Random.Range(-0.3f, 0.3f);
            textObj.transform.position += new Vector3(randomX, 0, 0);

            // 데미지 숫자 적용
            textObj.GetComponent<DamageText>().Setup(damage);
        }

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
    
    private void UpdateHpUI()
    {
        if (hpText != null)
        {
            hpText.text = $"{currentHp} / {maxHp}";
        }
    }
}