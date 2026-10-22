using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSkyFallState : BossBaseState
{
    private int laserCount = 18;        // 레이저 기둥 총 개수
    private float fallInterval = 0.15f; // 레이저가 생성되는 시간 간격
    private float chargeTime = 1.2f;    // 각 레이저의 예고(얇은선) 시간
    private float laserDuration = 0.6f; // 발사 유지 시간

    private List<GameObject> activeLasers = new List<GameObject>(); // [추가] 생성된 레이저 관리용 리스트

    public BossSkyFallState(BossController owner) : base(owner) { }

    public override void Enter()
    {
        
        activeLasers.Clear(); // 진입 시 리스트 초기화

        owner.StartCoroutine(SequentialSkyFallSequence());
    }

    IEnumerator SequentialSkyFallSequence()
    {
        owner.anim.SetBool("isSkyFall", true);

        owner.RecordSkyFallTime(); // 쿨타임 시작

        yield return new WaitForSeconds(0.6f);

        // 1. 안전지대 위치 결정 (왼쪽 끝 혹은 오른쪽 끝)
        bool isSafeRight = Random.value > 0.5f;

        // 2. Transform에서 실시간 X 좌표 가져오기
        float left = owner.mapLeftX.position.x;
        float right = owner.mapRightX.position.x;
        float totalDist = right - left;

        float step = totalDist / (laserCount - 1);

        // 안전지대가 오른쪽이면 왼쪽에서 오른쪽으로, 반대면 오른쪽에서 왼쪽으로
        int startIdx = isSafeRight ? 0 : laserCount - 1;
        int endIdx = isSafeRight ? laserCount : -1;
        int dir = isSafeRight ? 1 : -1;

        for (int i = startIdx; isSafeRight ? (i < endIdx) : (i > endIdx); i += dir)
        {
            float targetX = left + (i * step);

            // 3. 안전지대 체크
            if (isSafeRight && targetX > right - owner.safeZoneWidth) break;
            if (!isSafeRight && targetX < left + owner.safeZoneWidth) break;

            owner.StartCoroutine(SingleLaserRoutine(targetX));
            yield return new WaitForSeconds(fallInterval);
        }
        owner.anim.SetBool("isSkyFall", false);

        yield return new WaitForSeconds(1.8f);

        owner.ChangeState(owner.idleState);
    }

    IEnumerator SingleLaserRoutine(float xPos)
    {
        // 레이저 프리팹 생성 (부모 설정 없이 맵에 고정)
        GameObject laser = Object.Instantiate(owner.laserPrefab, Vector3.zero, Quaternion.identity);
        activeLasers.Add(laser); // [추가] 리스트에 추가

        LineRenderer lr = laser.GetComponent<LineRenderer>();

        // [핵심] Exit()에 의해 레이저가 이미 파괴되었는지 확인
        if (laser == null || lr == null) yield break;

        // 1. 예고 단계 (얇은 세로선)
        float spawnY = owner.topLimitObj.position.y;
        float targetY = owner.bottomY;

        lr.SetPosition(0, new Vector3(xPos, spawnY, 0)); // 시작점
        lr.SetPosition(1, new Vector3(xPos, targetY, 0)); // 끝점

        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.enabled = true;

        yield return new WaitForSeconds(chargeTime);

        // 2. 발사 단계 (굵게 변화)
        lr.startWidth = 1.5f;
        lr.endWidth = 1.5f;

        Vector2 rayStart = new Vector2(xPos, spawnY); // TopLimit의 Y값
        Vector2 rayDir = Vector2.down;
        float rayDistance = Mathf.Abs(spawnY - targetY) + 2f; // 바닥까지 넉넉하게 닿도록 설정

        // 디버깅용: 씬 뷰에서 실제 레이캐스트 선을 그려봅니다 (빨간색)
        Debug.DrawRay(rayStart, rayDir * rayDistance, Color.red, 1.0f);

        // 물리 판정 (Raycast)
        RaycastHit2D hit = Physics2D.BoxCast(rayStart, new Vector2(1.5f, 0.1f), 0f, rayDir, rayDistance, LayerMask.GetMask("Player")); if (hit.collider != null)
        {
            PlayerHealth pHealth = hit.collider.GetComponent<PlayerHealth>();
            if (pHealth != null)
            {
                HitData data = new HitData
                {
                    damage = 30,
                    hitPosition = new Vector2(xPos, 0),
                    useKnockback = true,
                    forceX = 0f, // 진행 방향으로 살짝 밀어냄
                    forceY = 0f
                };
                pHealth.ProcessHit(data);
            }
        }

        yield return new WaitForSeconds(laserDuration);

        if (laser != null)
        {
            activeLasers.Remove(laser); // [추가] 소멸 전 리스트에서 제거
            Object.Destroy(laser);
        }
    }

    public override void Update() { }
    public override void PhysicsUpdate() { }
    public override void Exit() 
    {

        // 1. 현재 이 상태에서 돌아가는 모든 코루틴 중단
        owner.anim.SetBool("isSkyFall", false);
        owner.StopAllCoroutines();

        // [핵심] 보스가 죽거나 상태가 바뀔 때 남아있는 모든 레이저 파괴
        foreach (GameObject laser in activeLasers)
        {
            if (laser != null) Object.Destroy(laser);
        }
        activeLasers.Clear();
    }
}
