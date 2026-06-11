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
    public Sprite[] portalFrames;
    public float frameRate = 0.1f;

    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (portalFrames != null && portalFrames.Length > 0)
        {
            StartCoroutine(PlayPortalAnimation());
        }
    }

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