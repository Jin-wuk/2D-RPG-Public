using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillUpgradeCheck : MonoBehaviour
{
    private bool isPlayerNearby = false; // 플레이어가 범위 안에 있는지 확인

    private void Update()
    {
        // 1. 키 입력이 감지되는지 확인
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (isPlayerNearby)
            {
                Debug.Log("스킬 영역 안에서 윗키 누름!");
                if (SkillUpgradeUI.Instance != null)
                {
                    SkillUpgradeUI.Instance.ToggleShop();
                }
                else
                {
                    Debug.LogError("SkillUpgradeUI 인스턴스를 찾을 수 없습니다!");
                }
            }
            else
            {
                // 영역 밖인데 키만 누른 경우 (로그가 찍히면 감지 범위 문제)
                Debug.Log("스킬업글 영역 밖에서 윗키 누름.");
            }
        }
    }

    // 플레이어가 영역 안으로 들어왔을 때
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = true;
            Debug.Log("플레이어 진입: 이제 업글 이용 가능");
        }
    }

    // 플레이어가 영역 밖으로 나갔을 때
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (SkillUpgradeUI.Instance != null)
            {
                SkillUpgradeUI.Instance.CloseShop();
            }
            Debug.Log("플레이어 나감: 업글 이용 불가");
        }
    }
}
