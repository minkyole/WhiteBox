using UnityEngine;
using UnityEngine.SceneManagement; // 씬 이동을 위해 필수!

public class SceneController : MonoBehaviour
{
    // 로비에서 [스테이지 입장] 버튼을 누를 때 실행
    public void GoToDungeon(int stageNumber)
    {
        // GameManager에 "나 몇 스테이지 간다!" 라고 메모해두기
        GameManager.Instance.currentStage = stageNumber;

        // 던전 씬으로 로딩
        SceneManager.LoadScene("DungeonScene");
    }

    // 던전에서 [로비로 귀환] 버튼을 누를 때 실행
    public void GoToLobby()
    {
        // 로비 씬으로 로딩
        SceneManager.LoadScene("LobbyScene");
    }
}