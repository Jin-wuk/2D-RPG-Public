using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossIdleState : BossBaseState
{
    private float idleTimer;
    private float idleDuration = 2.0f; // 다음 패턴까지 대기할 시간 (초)

    public BossIdleState(BossController owner) : base(owner) { }

    public override void Enter()
    {
        idleTimer = 0;
        // 보스 속도 초기화 (이전 패턴의 속도가 남아있을 수 있음)
        owner.rb.velocity = Vector2.zero;
        // Idle 애니메이션으로 전환 (기존 파라미터가 있다면 활용)
        Debug.Log("보스: 대기 및 탐색 중...");
    }

    public override void Update()
    {
        // 1. 플레이어를 항상 바라보도록 회전 (방향 유지)
        LookAtPlayer();

        // 2. 시간 측정
        idleTimer += Time.deltaTime;

        // 3. 설정한 대기 시간이 지나면 다음 패턴으로 전환
        if (idleTimer >= idleDuration)
        {
            // 상태 내부에서 복잡하게 계산하지 않고, 컨트롤러의 중앙 집중식 판단 함수를 호출합니다.
            owner.ChooseNextPattern();
        }
    }

    public override void PhysicsUpdate() { }

    public override void Exit()
    {
        // 상태 탈출 시 로그 기록 (디버깅용)
        Debug.Log("보스: 대기 종료, 패턴 시작");
    }

    // 플레이어의 위치에 따라 보스의 좌우 방향을 바꿉니다.
    private void LookAtPlayer()
    {
        if (owner.player == null) return;

        float dir = (owner.player.position.x > owner.transform.position.x) ? 1f : -1f;

        // owner.baseScale을 사용하여 기존 보스 크기를 유지하면서 방향만 전환
        owner.transform.localScale = new Vector3(dir * owner.baseScale, owner.baseScale, 1f);
    }  
}
