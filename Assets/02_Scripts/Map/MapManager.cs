using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance;

    [Header("카메라 줌 설정")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    [SerializeField] private float normalSize = 4f;      // 평소 카메라 크기
    //[SerializeField] private float bossSize = 5.8f;     // 보스전 카메라 크기
    [SerializeField] private float zoomDuration = 2.0f; // 줌 속도

    [Header("맵 설정")]
    [SerializeField] private CinemachineConfiner2D confiner;
    [SerializeField] private List<GameObject> maps; //등록된 모든 맵리스트
    [SerializeField] private string startingMapName = "Map_Town";

    private Coroutine zoomCoroutine; // [추가] 줌 전환을 관리할 변수
    // 현재 맵의 경계 정보를 저장할 변수
    private Bounds currentMapBounds;

    // 외부에서 읽을 수 있는 프로퍼티
    public float MinX => currentMapBounds.min.x;
    public float MaxX => currentMapBounds.max.x;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 시작할 때 지정된 맵만 켜기
        ChangeMap(startingMapName);
    }

    // 보스전용 줌 아웃 함수
    public void ZoomToBoss()
    {
        //SetCameraSize(bossSize, zoomDuration);
    }

    // 보스 처치 후 원래대로 돌릴 때 호출할 함수
    public void ZoomToNormal()
    {
        SetCameraSize(normalSize, zoomDuration);
    }

    public void SetCameraSize(float targetSize, float duration = 1.0f)
    {
        if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
        zoomCoroutine = StartCoroutine(SmoothZoom(targetSize, duration));
    }
    private IEnumerator SmoothZoom(float targetSize, float duration)
    {
        float startSize = virtualCamera.m_Lens.OrthographicSize;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // 부드럽게 사이즈 변경
            virtualCamera.m_Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, elapsed / duration);
            yield return null;
        }

        virtualCamera.m_Lens.OrthographicSize = targetSize;
    }

    public void ChangeMap(string targetMapName)
    {
        foreach (GameObject map in maps)
        {
            if (map.name == targetMapName)
            {
                map.SetActive(true);
                PolygonCollider2D newBoundary = map.GetComponentInChildren<PolygonCollider2D>();

                if (newBoundary != null)
                {
                    confiner.m_BoundingShape2D = newBoundary;
                    // [필수 추가] 이 코드가 있어야 시네머신이 바뀐 경계를 즉시 인식합니다.
                    confiner.InvalidateCache();

                    // [추가] 현재 맵의 Bounds 정보를 업데이트합니다.
                    currentMapBounds = newBoundary.bounds;
                }
            }
            else
            {
                // 이 줄이 실행될 때 LoadingManager가 꺼지고 있는지 확인이 필요합니다.
                map.SetActive(false);
            }
        }
    }
}