using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    [Header("이동 및 페이드 설정")]
    public float moveSpeed = 2.0f;  // 위로 올라가는 속도
    public float fadeSpeed = 2.0f;  // 투명해지는 속도
    public float lifeTime = 1.0f;   // 화면에 머무는 시간

    private TextMeshPro textMesh;
    private Color textColor;
    private float timer;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
    }

    // 🌟 풀에서 꺼내져서 활성화될 때마다 초기화됩니다.
    void OnEnable()
    {
        if (textMesh != null)
        {
            textColor = textMesh.color;
            textColor.a = 1f; // 알파값(투명도)을 다시 100%로 복구
            textMesh.color = textColor;
        }
        timer = lifeTime;
    }

    void Update()
    {
        // 1. 위로 이동
        transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);

        // 2. 서서히 투명해짐
        textColor.a = Mathf.Lerp(textColor.a, 0, Time.deltaTime * fadeSpeed);
        textMesh.color = textColor;

        // 3. 수명이 다하면 스스로를 끔 (ObjectPoolManager가 알아서 재사용함)
        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    // 🌟 외부(몬스터)에서 데미지 숫자를 입력해 주는 함수
    public void Setup(int damage)
    {
        textMesh.text = damage.ToString();
    }
}