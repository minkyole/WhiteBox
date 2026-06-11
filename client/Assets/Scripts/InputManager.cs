using UnityEngine;
using System; // Action(이벤트)을 사용하기 위해 필요

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    public static event Action OnAttackPressed;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnAttackPressed?.Invoke();
        }
    }
}