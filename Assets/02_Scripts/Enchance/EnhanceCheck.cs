using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnhanceCheck : MonoBehaviour
{
    private bool isPlayerNearby = false;

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (EnhanceManager.Instance != null)
            {
                EnhanceManager.Instance.ToggleEnhance();
                Debug.Log("강화소 열기/닫기");
            }
        }
    }

    // 플레이어가 영역 안으로 들어왔을 때
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;
            Debug.Log("플레이어 진입: 이제 강화 이용 가능");
        }
    }

    // 플레이어가 영역 밖으로 나갔을 때
    private void OnTriggerExit2D(Collider2D collision)
    {
        // [수정] 이미 파괴된 오브젝트거나 null인 경우 로직을 타지 않도록 방어
        if (collision == null || !collision.gameObject) return;

        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (EnhanceManager.Instance != null && EnhanceManager.Instance.enhancePanel != null)
            {
                if (EnhanceManager.Instance.enhancePanel.activeSelf)
                {
                    EnhanceManager.Instance.ToggleEnhance();
                    Debug.Log("강화 영역을 벗어나 창을 닫습니다.");
                }
            }
        }
    }
}
