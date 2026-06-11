using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Portal : MonoBehaviour
{
    [Header("포탈 설정")]
    public string targetSceneName = "DungeonScene";
    public bool isReturnToLobby = false;
    public int stageLevel = 1;

    [Header("포탈 애니메이션")]
    public Sprite[] portalFrames;   // 잘라낸 8개의 포탈 이미지
    public float frameRate = 0.1f;  // 프레임 넘어가는 속도

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 애니메이션 프레임이 등록되어 있다면 코루틴 시작
        if (portalFrames != null && portalFrames.Length > 0)
        {
            StartCoroutine(PlayPortalAnimation());
        }
    }

    // 🌟 무한 반복되는 애니메이션 코루틴
    private IEnumerator PlayPortalAnimation()
    {
        int currentFrame = 0;
        while (true)
        {
            spriteRenderer.sprite = portalFrames[currentFrame];
            currentFrame = (currentFrame + 1) % portalFrames.Length;

            yield return new WaitForSeconds(frameRate);
        }
    }

    // 기존의 이동 로직 (변경 없음)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (!isReturnToLobby)
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.currentStage = stageLevel;
                }
            }

            Debug.Log($"{targetSceneName} 씬으로 이동합니다! (스테이지: {stageLevel})");
            SceneManager.LoadScene(targetSceneName);
        }
    }
}