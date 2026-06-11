using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("추적 대상")]
    public Transform target;

    [Header("카메라 설정")]
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0, 0, -10f);

    [Header("자동 맵 경계선 (MapBounds 연결)")]
    public Transform mapBounds;

    private float minX, maxX, minY, maxY;
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;

        if (mapBounds != null)
        {
            CalculateCameraBounds();
        }
    }

    //벽들의 위치를 읽어와서 카메라의 한계선을 자동 계산하는 함수
    private void CalculateCameraBounds()
    {
        // 1. MapBounds 안에 있는 4개의 벽(Collider)을 모두 찾아서 사각형으로 합친다
        Collider2D[] colliders = mapBounds.GetComponentsInChildren<Collider2D>();
        if (colliders.Length == 0) return;

        Bounds totalBounds = colliders[0].bounds;
        foreach (Collider2D col in colliders)
        {
            totalBounds.Encapsulate(col.bounds);
        }

        // 2. 현재 카메라 화면의 절반 높이와 너비를 계산한다.
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        // 3. 전체 맵 크기에서 카메라 화면의 절반 크기만큼을 빼서, 카메라 중심점의 한계 좌표를 만든다.
        minX = totalBounds.min.x + camWidth;
        maxX = totalBounds.max.x - camWidth;
        minY = totalBounds.min.y + camHeight;
        maxY = totalBounds.max.y - camHeight;
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;

        // mapBounds가 할당되어 있다면 자동 계산된 구역 안에 카메라를 가둔다.
        if (mapBounds != null)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minX, maxX);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minY, maxY);
        }

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}