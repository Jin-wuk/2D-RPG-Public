using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// [추가] 모호한 참조 해결을 위한 명시적 선언
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

public class BossLaserStormState : BossBaseState
{
    private int totalLasers = 12;
    private float fireInterval = 0.8f;
    private float chargeTime = 1.5f;    // 추적 및 예고 시간
    private float laserDuration = 0.6f; // 발사 유지 시간
    private List<GameObject> activeLasers = new List<GameObject>();

    public BossLaserStormState(BossController owner) : base(owner) { }

    public override void Enter()
    {
        // 1. 보스 무적 및 애니메이션 설정
        owner.stats.isInvincible = true;
        owner.spriteRenderer.color = new Color(0.6f, 0.6f, 1f, 0.8f); // 무적 시각화

        activeLasers.Clear();
        owner.StartCoroutine(LaserStormSequence());
    }

    IEnumerator LaserStormSequence()
    {
        float leftX = owner.mapLeftX.position.x;
        float rightX = owner.mapRightX.position.x;
        float topY = owner.topLimitObj.position.y; // 천장 높이
        float bottomY = owner.bottomY; // 바닥 높이

        for (int i = 0; i < totalLasers; i++)
        {
            Vector3 spawnPos = Vector3.zero;

            // 0, 1, 2 중 랜덤하게 선택하여 생성 위치 결정
            int randomSide = Random.Range(0, 3);

            switch (randomSide)
            {
                case 0: // 왼쪽 벽면에서 생성
                    spawnPos = new Vector3(leftX, Random.Range(bottomY, topY), 0);
                    break;
                case 1: // 오른쪽 벽면에서 생성
                    spawnPos = new Vector3(rightX, Random.Range(bottomY, topY), 0);
                    break;
                case 2: // 천장에서 생성
                    spawnPos = new Vector3(Random.Range(leftX, rightX), topY, 0);
                    break;
            }


            // 2. 레이저 개별 루틴 실행
            owner.StartCoroutine(SingleTrackingLaserRoutine(spawnPos));

            yield return new WaitForSeconds(fireInterval);
        }

        // 전체 패턴 종료 대기 (모든 레이저가 끝날 충분한 시간)
        yield return new WaitForSeconds(chargeTime + laserDuration + 1.0f);
        owner.ChangeState(owner.idleState);
    }

    IEnumerator SingleTrackingLaserRoutine(Vector3 spawnPos)
    {
        // 3. 레이저 프리팹 생성 및 초기화
        GameObject laser = Object.Instantiate(owner.laserPrefab, spawnPos, Quaternion.identity);
        activeLasers.Add(laser);
        LineRenderer lr = laser.GetComponent<LineRenderer>();

        lr.enabled = true;
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;

        float timer = 0f;
        float lockOnStartTime = chargeTime - 0.4f; // 발사 0.4초 전 조준 고정
        Vector2 finalDir = Vector2.zero;

        // 4. 추적 및 조준 고정 단계 (제안하신 로직 적용)
        while (timer < chargeTime)
        {
            timer += Time.deltaTime;

            if (timer < lockOnStartTime)
            {
                // 실시간 추적
                finalDir = (Vector2)(owner.player.position - spawnPos).normalized;
                Vector2 endPos = (Vector2)spawnPos + finalDir * 30f;
                lr.SetPosition(0, spawnPos);
                lr.SetPosition(1, endPos);
            }
            else
            {
                // 조준 고정 알림 (깜빡임)
                lr.enabled = !lr.enabled;
            }
            yield return null;
        }

        // 5. 발사 단계
        lr.enabled = true;
        lr.startWidth = 0.8f;
        lr.endWidth = 0.8f;

        // 실제 데미지 판정 (BoxCast로 굵직하게)
        RaycastHit2D hit = Physics2D.BoxCast(spawnPos, new Vector2(0.8f, 0.1f), 0f, finalDir, 30f, LayerMask.GetMask("Player"));
        if (hit.collider != null)
        {
            PlayerHealth pHealth = hit.collider.GetComponent<PlayerHealth>();
            if (pHealth != null)
            {
                float pushDirX = (owner.player.position.x > spawnPos.x) ? 1f : -1f;
                HitData data = new HitData
                {
                    damage = 25,
                    hitPosition = spawnPos,
                    useKnockback = true,
                    forceX = pushDirX * 20f,
                    forceY = 2f
                };
                pHealth.ProcessHit(data);
            }
        }

        yield return new WaitForSeconds(laserDuration);

        if (laser != null)
        {
            activeLasers.Remove(laser);
            Object.Destroy(laser);
        }
    }

    public override void Update() { }
    public override void PhysicsUpdate() { }

    public override void Exit()
    {
        owner.StopAllCoroutines();

        // 2. 무적 상태 확실히 해제
        if (owner.stats != null)
        {
            owner.stats.isInvincible = false;
            Debug.Log("보스 무적 해제 완료");
        }

        // [수정] 무조건 흰색이 아니라, 레이지 모드 여부에 따라 색상 복구
        if (owner.isRageMode)
        {
            owner.spriteRenderer.color = owner.rageColor;
        }
        else
        {
            owner.spriteRenderer.color = Color.white;
        }

        foreach (GameObject laser in activeLasers)
        {
            if (laser != null) Object.Destroy(laser);
        }
        activeLasers.Clear();
    }
}
