using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance;

    // 인스펙터에서 풀(Pool)을 여러 개 관리하기 위한 클래스
    [System.Serializable]
    public class Pool
    {
        public string tag;        // 풀의 이름 (예: "DamageText", "Fireball")
        public GameObject prefab; // 복사할 원본 프리팹
        public int size;          // 미리 만들어둘 개수 (예: 50)
    }

    [Header("오브젝트 풀 설정")]
    public List<Pool> pools;

    // 실제 오브젝트들을 담아둘 딕셔너리
    public Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Start()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                // 생성 후 ObjectPoolManager의 자식으로 묶어서 하이어라키를 깔끔하게 유지한다.
                GameObject obj = Instantiate(pool.prefab, transform);
                obj.SetActive(false); // 일단 숨겨둠
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    // 필요할 때 오브젝트를 꺼내 쓰는 함수
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning($"[ObjectPoolManager] '{tag}' 이름을 가진 풀이 없습니다!");
            return null;
        }

        // 1. 큐의 맨 앞에서 오브젝트를 하나 꺼낸다.
        GameObject obj = poolDictionary[tag].Dequeue();

        // 2. 위치와 회전값을 설정하고 활성화합니다.
        obj.SetActive(true);
        obj.transform.position = position;
        obj.transform.rotation = rotation;

        // 3. 다 쓴 오브젝트는 다시 큐의 맨 뒤로 줄을 세운다. 
        poolDictionary[tag].Enqueue(obj);

        return obj;
    }
}
