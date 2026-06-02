using System;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    // 스페이스바를 눌렀을 때 발송되는 공격 신호
    public static event Action OnAttackPressed;

    void Update()
    {
        // 스페이스바 입력 감지
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnAttackPressed?.Invoke();
        }
    }
}