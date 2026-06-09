using UnityEngine;
using System; // Action(이벤트)을 사용하기 위해 필요

public class InputManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static InputManager Instance { get; private set; }

    // 공격 신호 (다른 스크립트에서 구독하는 이벤트)
    public static event Action OnAttackPressed;

    void Awake()
    {
        // 1. 내가 유일한 InputManager인지 확인
        if (Instance == null)
        {
            Instance = this;
            // 2. 씬이 넘어가도 날 파괴하지 마라! (핵심)
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // 3. 이미 씬에 다른 InputManager가 있다면 자살하여 중복 방지
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // 스페이스바를 누르면 구독자들에게 공격 신호 발송!
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnAttackPressed?.Invoke();
        }
    }
}