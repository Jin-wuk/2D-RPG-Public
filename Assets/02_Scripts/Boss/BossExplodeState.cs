using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossExplodeState : BossBaseState
{
    private float explodeRadius = 6.5f;    // 폭발 타격 범위
    private float chargeTime = 1.5f;       // 폭발 전 예고 시간
    private GameObject currentIndicator;   // 생성된 예고 범위 오브젝트
    private Coroutine explodeCoroutine; // [추가] 코루틴 제어를 위한 변수

    public BossExplodeState(BossController owner) : base(owner) { }

    public override void Enter()
    {
        // 1. 폭발 쿨타임 기록
        owner.RecordExplodeTime();

        explodeCoroutine = owner.StartCoroutine(ExplodeRoutine());
    }

    IEnumerator ExplodeRoutine()
    {
        if (owner.explodeIndicatorPrefab != null)
        {
            // 부모를 owner.transform으로 설정하여 보스를 따라다니게 함
            currentIndicator = Object.Instantiate(owner.explodeIndicatorPrefab, owner.transform.position, Quaternion.identity, owner.transform);

            float diameter = explodeRadius * 2f;
            currentIndicator.transform.localScale = new Vector3(diameter, diameter, 1f);

            AttackIndicator indicatorScript = currentIndicator.GetComponent<AttackIndicator>();
            if (indicatorScript != null) indicatorScript.SetIndicator(chargeTime);
        }

        // 예고 시간 동안 대기
        yield return new WaitForSeconds(chargeTime);

        // [핵심] 보스의 월드 좌표를 명확하게 가져옴
        Vector2 bossWorldPos = owner.transform.position;

        // 월드 좌표 기준 범위 내 플레이어 검출
        Collider2D hit = Physics2D.OverlapCircle(bossWorldPos, explodeRadius, LayerMask.GetMask("Player"));


        if (hit != null)
        {
            PlayerHealth pHealth = hit.GetComponent<PlayerHealth>();
            if (pHealth != null)
            {
                // 플레이어의 월드 좌표와 보스의 월드 좌표 직접 비교
                float playerX = hit.transform.position.x;
                float bossX = bossWorldPos.x;

                /// 순수하게 월드 공간에서 누가 더 오른쪽에 있는지 판단
                float directionX = (playerX > bossX) ? 1f : -1f;

                HitData data = new HitData
                {
                    damage = 40, // 폭발 데미지
                    hitPosition = bossWorldPos,
                    useKnockback = true,
                    forceX = directionX * 22f, // 레이지 모드이므로 강력하게 밀쳐냄
                    forceY = 12f
                };
                pHealth.ProcessHit(data);
            }
        }

        // 6. 예고 지시기 제거 및 이펙트 처리
        if (currentIndicator != null) Object.Destroy(currentIndicator);

        // 후딜레이 후 Idle 상태로 복귀
        yield return new WaitForSeconds(0.8f);
        owner.ChangeState(owner.idleState);
    }

    public override void Exit()
    {
        /// [핵심] 상태를 나갈 때(사망 혹은 상태 전환) 실행 중인 로직을 모두 중단/제거합니다.
        if (explodeCoroutine != null)
        {
            owner.StopCoroutine(explodeCoroutine);
            explodeCoroutine = null;
        }

        if (currentIndicator != null)
        {
            Object.Destroy(currentIndicator);
        }
    }

    public override void Update() { }
    public override void PhysicsUpdate() { }
}
