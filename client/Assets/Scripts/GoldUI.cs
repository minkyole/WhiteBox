using UnityEngine;
using TMPro;

public class GoldUI : MonoBehaviour
{
    private TextMeshProUGUI textMesh;

    void Start()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        // 매 프레임마다 GameManager에 있는 진짜 골드 수치를 가져와서 텍스트로 갱신
        if (GameManager.Instance != null && textMesh != null)
        {
            textMesh.text = $"Gold: {GameManager.Instance.gold}";
        }
    }
}