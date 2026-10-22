using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossLaserState : BossBaseState
{
    private float chargeTime = 1.5f; // 기 모으는(예고) 시간
    private float laserDuration = 0.8f; // 실제 발사 유지 시간
    private Coroutine laserCoroutine;
    private GameObject laserObj;    // 생성된 레이저 프리팹 저장
    private LineRenderer lr;


    public BossLaserState(BossController owner) : base(owner) { }

    public override void Enter()
    {
        // 코루틴을 변수에 할당하여 제어 가능하게 만듭니다.
        laserCoroutine = owner.StartCoroutine(LaserSequence());
    }

    IEnumerator LaserSequence()
    {
        owner.RecordLaserTime();

        // 1. 발사 횟수 결정
        int shotCount = owner.isRageMode ? 3 : 1;

        // 레이지 모드일 때는 예고/발사 시간을 약간 단축하여 더 몰아붙일 수도 있습니다.
        float currentChargeTime = owner.isRageMode ? 0.8f : chargeTime;

        for (int i = 0; i < shotCount; i++)
        {
            // --- [준비 단계: 레이저 생성 및 초기화] ---
            laserObj = Object.Instantiate(owner.laserPrefab, owner.firePoint.position, Quaternion.identity, owner.firePoint);
            lr = laserObj.GetComponent<LineRenderer>();
            if (lr == null) yield break;

            lr.enabled = true;
            lr.startWidth = 0.05f;
            lr.endWidth = 0.05f;

            float timer = 0f;
            float lockOnStartTime = currentChargeTime - 0.2f; // 조준 고정 시간을 살짝 줄임

            // --- [조준 단계] ---
            while (timer < currentChargeTime)
            {
                timer += Time.deltaTime;
                Vector3 currentStart = owner.firePoint.position;

                if (timer < lockOnStartTime)
                {
                    // 실시간 추적
                    Vector2 direction = (owner.player.position - currentStart).normalized;
                    Vector2 endPos = (Vector2)currentStart + direction * 25f;
                    lr.SetPosition(0, currentStart);
                    lr.SetPosition(1, endPos);
                }
                else
                {
                    // 조준 고정 및 깜빡임
                    lr.SetPosition(0, currentStart);
                    lr.enabled = !lr.enabled;
                }
                yield return null;
            }

            // --- [발사 단계] ---
            lr.enabled = true;
            lr.startWidth = 0.6f;
            lr.endWidth = 0.6f;

            // 물리 판정 (BoxCast 추천)
            Vector3 fireStart = owner.firePoint.position;
            Vector2 fireDir = (lr.GetPosition(1) - lr.GetPosition(0)).normalized;

            // 0.6f 두께로 박스 캐스트 판정
            RaycastHit2D hit = Physics2D.BoxCast(fireStart, new Vector2(0.6f, 0.6f), 0f, fireDir, 25f, LayerMask.GetMask("Player"));
            Debug.DrawRay(fireStart, fireDir * 25f, Color.red, 0.5f);

            if (hit.collider != null)
            {
                PlayerHealth pHealth = hit.collider.GetComponent<PlayerHealth>();
                if (pHealth != null)
                {
                    float pushDirX = (owner.player.position.x > owner.transform.position.x) ? 1f : -1f;
                    HitData laserHit = new HitData
                    {
                        damage = 30, // 연사형이므로 데미지를 조금 낮추는 밸런스 조정
                        hitPosition = owner.transform.position,
                        useKnockback = true,
                        forceX = pushDirX * 20f,
                        forceY = 5f
                    };
                    pHealth.ProcessHit(laserHit);
                }
            }

            yield return new WaitForSeconds(laserDuration);

            // 한 발 쏘고 레이저 오브젝트 삭제
            if (laserObj != null) Object.Destroy(laserObj);

            // 연사 사이의 아주 짧은 간격
            if (i < shotCount - 1) yield return new WaitForSeconds(0.2f);
        }

        // 모든 발사가 끝나면 Idle로 이동
        owner.ChangeState(owner.idleState);
    }

    public override void Update() { }
    public override void PhysicsUpdate() { }
    public override void Exit()
    {
        // [핵심] 상태를 나갈 때(사망 포함) 실행 중인 코루틴을 멈추고 레이저를 지웁니다.
        if (laserCoroutine != null) owner.StopCoroutine(laserCoroutine);
        if (laserObj != null) Object.Destroy(laserObj);

        if (lr != null) lr.enabled = false;
    }
}
