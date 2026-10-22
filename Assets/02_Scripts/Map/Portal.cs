using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour
{
    [Header("포탈 이동 설정")]
    [SerializeField] private Transform destination; //이동할 좌표
    [SerializeField] private string targetMapName;  //이동할 맵의 이름

    private bool isPlayerIn = false;
    private GameObject playerObj;

    private void Update()
    {
        // 로딩 중에는 포탈 입력을 받지 않음
        if (LoadingManager.Instance != null && LoadingManager.Instance.IsLoading) return;

        if (isPlayerIn && Input.GetKeyDown(KeyCode.UpArrow))
        {
            Teleport();
        }
    }
    private void Teleport()
    {
        SoundManager.Instance.PlaySFX(SoundManager.SoundType.Teleport);
        // LoadingManager를 통해 페이드 인 -> 이동 -> 페이드 아웃 실행
        LoadingManager.Instance.StartCoroutine(LoadingManager.Instance.FadeAndAction(() =>
        {

            // 1. 목적지 좌표 캐싱 (저장)
            Vector3 targetPos = destination.position;

            // 2. 안전장치: 플레이어와 리지드바디 확인
            if (playerObj == null) playerObj = GameObject.FindWithTag("Player");
            Rigidbody2D rb = playerObj.GetComponent<Rigidbody2D>();

            // 3. 플레이어 위치 이동
            playerObj.transform.position = targetPos;

            // 4. [추가] 시네머신 카메라 즉시 워프 (카메라가 휙 지나가는 소리/현상 방지)
            // 가상 카메라가 플레이어를 즉시 따라가도록 정보를 갱신합니다.
            var vcam = FindObjectOfType<Cinemachine.CinemachineVirtualCamera>();
            if (vcam != null) vcam.OnTargetObjectWarped(playerObj.transform, targetPos - playerObj.transform.position);

            // 5. 물리 연산 초기화 (추락 및 밀림 방지)
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }

            // 6. 맵 교체 (새로운 맵 활성화 + 카메라 컨파이너 갱신)
            MapManager.Instance.ChangeMap(targetMapName);

            Debug.Log($"[이동 완료] 목적지: {targetMapName}, 좌표: {targetPos}");
        })); // 닫는 괄호 추가 완료
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 플레이어 레이어 또는 태그 확인
        if (collision.CompareTag("Player"))
        {
            isPlayerIn = true;
            playerObj = collision.gameObject;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerIn = false;
            playerObj = null;
        }
    }
}
