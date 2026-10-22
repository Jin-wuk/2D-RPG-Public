using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerMovement movement;
    private PlayerCombat combat;
    private PlayerHealth playerHealth;
    private void Awake()
    {
        movement = GetComponent<PlayerMovement>();
        combat = GetComponent<PlayerCombat>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void Update()
    {

        if (LoadingManager.Instance != null && LoadingManager.Instance.IsLoading)
        {
            movement.Move(0);
            return;
        }
        // 1. 넉백 중이면 물리 엔진이 힘을 쓸 수 있도록 모든 조작 로직을 즉시 중단합니다.
        if (playerHealth != null && playerHealth.IsKnockback)
        {
            return; // 아무것도 호출하지 않고 리턴!
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        movement.Move(h);


        // 점프 관련 입력 처리
        if (Input.GetButtonDown("Jump"))
        {
            if (v > 0)
            {
                // 윗 방향키 + 점프 = 위 점프
                movement.UpJump();
            }
            else if (v < 0)
            {
                // 아래 방향키 + 점프 = 아래 점프
                movement.DownJump();
            }
            else
            {
                // 일반 점프 (이동 중이면 앞점프 효과)
                movement.Jump(h);
            }
        }
        
        //공격 입력 처리
        if (Input.GetKeyDown(KeyCode.Z))
        {
            combat.Attack();
        }

        movement.GroundCheck();
    }
}
