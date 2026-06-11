using UnityEngine;
using UnityEngine.SceneManagement; // 씬 이동을 위해 필수!

public class SceneController : MonoBehaviour
{
    // 로비에서 [스테이지 입장] 버튼을 누를 때 실행
    public void GoToDungeon(int stageNumber)
    {
        GameManager.Instance.currentStage = stageNumber;

        // 던전 씬으로 로딩
        SceneManager.LoadScene("DungeonScene");
    }

    public void GoToLobby()
    {
        // 로비 씬으로 로딩
        SceneManager.LoadScene("LobbyScene");
    }
}